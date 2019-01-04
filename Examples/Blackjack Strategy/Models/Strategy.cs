using Evolutionary;
using System.Collections.Generic;
using System.Linq;

namespace BlackjackStrategy.Models
{
    // encapsulates one complete strategy to play Blackjack
    class OverallStrategy
    {
        private Dictionary<string, ActionToTake> pairsStrategy = new Dictionary<string, ActionToTake>();
        private Dictionary<string, ActionToTake> softStrategy = new Dictionary<string, ActionToTake>();
        private Dictionary<string, ActionToTake> hardStrategy = new Dictionary<string, ActionToTake>();

        public OverallStrategy(CandidateSolution<bool, ProblemState> candidate)
        {
            // cycle over all of the possible upcards
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                // do pairs
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    AddPairStrategy(pairedCardRank, upcardRankName, action);
                }

                // then soft hands
                // we don't start with Ace, because that would be AA, which is handled in the pair zone
                // we also don't start with 10, since that's blackjack.  So 9 is our starting point
                for (int otherCard = 9; otherCard > 1; otherCard--)
                {
                    string otherCardRank = (otherCard == 11) ? "A" : otherCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    // first card is an ace, second card is looped over
                    playerHand.AddCard(new Card("AH")); // ace of hearts
                    playerHand.AddCard(new Card(otherCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    AddSoftStrategy(otherCardRank, upcardRankName, action);
                }

                // hard hands
                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // build player hand
                    Hand playerHand = new Hand();
                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;
                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    AddHardStrategy(hardTotal, upcardRankName, action);
                }
            }
        }

        private void SetupStateData(ProblemState stateData, Hand hand)
        {
            // prepare for testing
            stateData.PlayerHands.Clear();
            stateData.PlayerHands.Add(hand);
            stateData.VotesForDoubleDown = 0;
            stateData.VotesForHit = 0;
            stateData.VotesForStand = 0;
        }

        private ActionToTake GetAction(ProblemState stateData)
        {
            int votesForStand = stateData.VotesForStand;
            int votesForHit = stateData.VotesForHit;
            int votesForDouble = stateData.VotesForDoubleDown;
            int votesForSplit = stateData.VotesForSplit;

            List<ActionWithVotes> votes = new List<ActionWithVotes>();
            votes.Add(new ActionWithVotes(votesForDouble, ActionToTake.Double));
            votes.Add(new ActionWithVotes(votesForStand, ActionToTake.Stand));
            votes.Add(new ActionWithVotes(votesForHit, ActionToTake.Hit));
            votes.Add(new ActionWithVotes(votesForSplit, ActionToTake.Split));

            return votes.OrderByDescending(v => v.NumVotes).First().Action;
        }

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, string dealerUpcardRank, ActionToTake action)
        {
            pairsStrategy[pairRank + dealerUpcardRank] = action;
        }

        public void AddSoftStrategy(string secondaryCardRank, string dealerUpcardRank, ActionToTake action)
        {
            // secondary rank is the non-Ace
            softStrategy[secondaryCardRank + dealerUpcardRank] = action;
        }

        public void AddHardStrategy(int handTotal, string dealerUpcardRank, ActionToTake action)
        {
            // handTotal goes from 5 (since a total of 4 means a pair of 2s) to 20
            hardStrategy[handTotal + dealerUpcardRank] = action;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetAction(Hand hand, string dealerUpcardRank)
        {
            if (hand.IsPair())
            {
                string rank = hand.Cards[0].Rank;
                return pairsStrategy[rank + dealerUpcardRank];
            }

            if (hand.HasSoftAce())
            {
                string secondaryCardRank = hand.Cards
                    .Single(c => c.Rank != "A")
                    .Rank;
                return softStrategy[secondaryCardRank + dealerUpcardRank];
            }

            return hardStrategy[hand.HandValue() + dealerUpcardRank];
        }
    }
}

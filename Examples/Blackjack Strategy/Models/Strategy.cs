using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BlackjackStrategy.Models
{
    enum ActionToTake { Stand, Hit, Double, Split };

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
                    var action = GetActionFromCandidate(candidate.StateData);
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
                    var action = GetActionFromCandidate(candidate.StateData);
                    AddSoftStrategy(otherCardRank, upcardRankName, action);
                }

                // hard hands.  
                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // build player hand
                    Hand playerHand = new Hand();

                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;

                    // 20 is always TT, which is a pair, so we handle that by building a 3 card hand
                    if (hardTotal == 20)
                    {
                        playerHand.AddCard(new Card("TD")); // ten of diamonds
                        firstCardRank = 6;
                        secondCardRank = 4;
                    }

                    // we don't want pairs, so check for that
                    if (firstCardRank == secondCardRank)
                    {
                        firstCardRank++;
                        secondCardRank--;
                    }

                    Debug.Assert(firstCardRank != secondCardRank, "Build pair for hard hand");

                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(candidate.StateData);
                    AddHardStrategy(hardTotal, upcardRankName, action);
                }
            }
        }

        private void SetupStateData(ProblemState stateData, Hand hand)
        {
            // prepare for testing
            stateData.PlayerHand = hand;
            stateData.VotesForDoubleDown = 0;
            stateData.VotesForHit = 0;
            stateData.VotesForStand = 0;
            stateData.VotesForSplit = 0;
        }

        private ActionToTake GetActionFromCandidate(ProblemState stateData)
        {
            int votesForStand = stateData.VotesForStand;
            int votesForHit = stateData.VotesForHit;
            int votesForDouble = stateData.VotesForDoubleDown;
            int votesForSplit = stateData.VotesForSplit;

            List<Tuple<int, ActionToTake>> votes = new List<Tuple<int, ActionToTake>>();
            votes.Add(new Tuple<int, ActionToTake>(votesForDouble, ActionToTake.Double));
            votes.Add(new Tuple<int, ActionToTake>(votesForStand, ActionToTake.Stand));
            votes.Add(new Tuple<int, ActionToTake>(votesForHit, ActionToTake.Hit));
            votes.Add(new Tuple<int, ActionToTake>(votesForSplit, ActionToTake.Split));

            var result = votes.OrderByDescending(v => v.Item1).First().Item2;

            // make sure we've selected Split only when it's valid
            if ((result == ActionToTake.Split) && (stateData.PlayerHand.IsPair() == false)) {
                Debug.Assert(false, "Recommended action is split on a non-pair");
            }
            if ((result == ActionToTake.Double) && (stateData.PlayerHand.Cards.Count != 2))
            {
                Debug.Assert(false, "Recommended action is double with more than 2 cards");
            }

            return result;
        }

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, string dealerUpcardRank, ActionToTake action)
        {
            if (pairRank == "10") pairRank = "T";
            pairsStrategy[pairRank + dealerUpcardRank] = action;
        }

        public void AddSoftStrategy(string secondaryCardRank, string dealerUpcardRank, ActionToTake action)
        {
            // secondary rank is the non-Ace
            if (secondaryCardRank == "10" || secondaryCardRank == "J" || secondaryCardRank == "Q" || secondaryCardRank == "K")
                secondaryCardRank = "T";  // we only stored one value for the tens
            softStrategy[secondaryCardRank + dealerUpcardRank] = action;
        }

        public void AddHardStrategy(int handTotal, string dealerUpcardRank, ActionToTake action)
        {
            Debug.Assert(action != ActionToTake.Split, "Split found for non-pair");

            // handTotal goes from 5 (since a total of 4 means a pair of 2s) to 20
            hardStrategy[handTotal + dealerUpcardRank] = action;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetActionForHand(Hand hand, string dealerUpcardRank)
        {
            if (hand.HandValue() >= 21) return ActionToTake.Stand;

            if (hand.IsPair())
            {
                string rank = hand.Cards[0].Rank;
                if (rank == "10" || rank == "J" || rank == "Q" || rank == "K")
                    rank = "T";  // we only stored one value for the tens
                return pairsStrategy[rank + dealerUpcardRank];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == "A");
                int total = hand.Cards.Where(c => c.Rank != "A").Sum(c => c.RankValueHigh) + (howManyAces - 1);

                // and then collapse that total down into a single "other card" rank
                string rank = (total == 10) ? "T" : total.ToString();
                return softStrategy[rank + dealerUpcardRank];
            }

            return hardStrategy[hand.HandValue() + dealerUpcardRank];
        }
    }
}

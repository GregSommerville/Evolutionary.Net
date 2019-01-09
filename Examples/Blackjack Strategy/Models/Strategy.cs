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

        // given a dictionary of candidates keyed by a string that is the dealer upcard rank
        public OverallStrategy(Dictionary<string, CandidateSolution<bool, ProblemState>> solutionsByUpcards)
        {
            // cycle over all of the possible upcards
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerCard = new Card(upcardRankName, "D");

                var solution = solutionsByUpcards[upcardRankName];

                // do pairs
                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // find strategy 
                    SetupStateData(solution.StateData, playerHand, dealerCard);
                    solution.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(solution.StateData);
                    AddPairStrategy(pairedCardRank, action, dealerCard);
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
                    SetupStateData(solution.StateData, playerHand, dealerCard);
                    solution.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(solution.StateData);
                    AddSoftStrategy(otherCardRank, action, dealerCard);
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

                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // find strategy 
                    SetupStateData(solution.StateData, playerHand, dealerCard);
                    solution.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(solution.StateData);
                    AddHardStrategy(hardTotal, action, dealerCard);
                }
            }
        }

        // given a single candidate
        public OverallStrategy(CandidateSolution<bool, ProblemState> candidate)
        {
            // cycle over all of the possible upcards
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerCard = new Card(upcardRankName, "D");

                    // do pairs
                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand, dealerCard);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(candidate.StateData);
                    AddPairStrategy(pairedCardRank, action, dealerCard);
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
                    SetupStateData(candidate.StateData, playerHand, dealerCard);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(candidate.StateData);
                    AddSoftStrategy(otherCardRank, action, dealerCard);
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

                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand, dealerCard);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetActionFromCandidate(candidate.StateData);
                    AddHardStrategy(hardTotal, action, dealerCard);
                }
            }
        }

        private void SetupStateData(ProblemState stateData, Hand hand, Card dealerUpcard)
        {
            // prepare for testing
            stateData.PlayerHand = hand;
            stateData.DealerUpcard = dealerUpcard;

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

            // there are times when all votes = 0, in which case double or split could be erroneously selected.
            // this code handles that situation so they never get selected
            if (stateData.PlayerHand.IsPair() == false) votesForSplit = int.MinValue;
            if (stateData.PlayerHand.Cards.Count > 2) votesForDouble = int.MinValue;

            List<Tuple<int, ActionToTake>> votes = new List<Tuple<int, ActionToTake>>();
            votes.Add(new Tuple<int, ActionToTake>(votesForDouble, ActionToTake.Double));
            votes.Add(new Tuple<int, ActionToTake>(votesForStand, ActionToTake.Stand));
            votes.Add(new Tuple<int, ActionToTake>(votesForHit, ActionToTake.Hit));
            votes.Add(new Tuple<int, ActionToTake>(votesForSplit, ActionToTake.Split));

            var result = votes.OrderByDescending(v => v.Item1).First().Item2;
            return result;
        }

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, ActionToTake action, Card dealerUpcard)
        {
            pairsStrategy[CleanRank(pairRank) + CleanRank(dealerUpcard.Rank)] = action;
        }

        public void AddSoftStrategy(string secondaryCardRank, ActionToTake action, Card dealerUpcard)
        {
            // secondary rank is the non-Ace
            softStrategy[CleanRank(secondaryCardRank) + CleanRank(dealerUpcard.Rank)] = action;
        }

        public void AddHardStrategy(int handTotal, ActionToTake action, Card dealerUpcard)
        {
            // handTotal goes from 5 (since a total of 4 means a pair of 2s) to 20
            hardStrategy[handTotal + CleanRank(dealerUpcard.Rank)] = action;
        }

        private string CleanRank(string rank)
        {
            if (rank == "10" || rank == "J" || rank == "Q" || rank == "K")
                rank = "T";  // we only stored one value for the tens
            return rank;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetActionForHand(Hand hand, Card dealerUpcard)
        {
            if (hand.HandValue() >= 21) return ActionToTake.Stand;

            string upcardRank = CleanRank(dealerUpcard.Rank);

            if (hand.IsPair())
            {
                string rank = CleanRank(hand.Cards[0].Rank);
                return pairsStrategy[rank + upcardRank];
            }

            if (hand.HasSoftAce())
            {
                // we want the total other than the high ace
                int howManyAces = hand.Cards.Count(c => c.Rank == "A");
                int total = hand.Cards
                    .Where(c => c.Rank != "A")
                    .Sum(c => c.RankValueHigh) + 
                    (howManyAces - 1);

                string rank = (total == 10) ? "T" : total.ToString();
                return softStrategy[rank + upcardRank];
            }

            return hardStrategy[hand.HandValue() + upcardRank];
        }
    }
}

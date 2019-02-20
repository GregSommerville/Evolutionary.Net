using Evolutionary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class StrategyFactory
    {
        // This class converts from the genetic program of type CandidateSolution<bool, ProblemState> to a Strategy
        public static Strategy GetStrategyForGP(Dictionary<Card.Ranks, CandidateSolution<bool, ProblemState>> solutionsByUpcards)
        {
            Strategy result = new Strategy();
            foreach (var upcardRank in Card.ListOfRanks)
            {
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;

                AddStrategyForUpcard(upcardRank, result, solutionsByUpcards[upcardRank]);
            }

            return result;
        }

        public static Strategy GetStrategyForGP(CandidateSolution<bool, ProblemState> candidate)
        {
            Strategy result = new Strategy();

            foreach (var upcardRank in Card.ListOfRanks)
            {
                if (upcardRank == Card.Ranks.Jack || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.King)
                    continue;

                AddStrategyForUpcard(upcardRank, result, candidate);
            }

            return result;
        }

        private static void AddStrategyForUpcard(Card.Ranks upcardRank, Strategy result, CandidateSolution<bool, ProblemState> candidate)
        {
            Card dealerCard = new Card(upcardRank, Card.Suits.Diamonds);

            // do pairs
            for (var pairedRank = Card.Ranks.Ace; pairedRank >= Card.Ranks.Two; pairedRank--)
            {
                // build player hand
                Hand playerHand = new Hand();
                playerHand.AddCard(new Card(pairedRank, Card.Suits.Hearts));
                playerHand.AddCard(new Card(pairedRank, Card.Suits.Spades));

                // find strategy 
                SetupStateData(candidate.StateData, playerHand, dealerCard);
                candidate.Evaluate();

                // get the decision and store in the strategy object
                var action = GetActionFromCandidate(candidate.StateData);

                result.SetActionForPair(upcardRank, pairedRank, action);
            }

            // then soft hands
            // we don't start with Ace, because that would be AA, which is handled in the pair zone
            // we also don't start with 10, since that's blackjack.  So 9 is our starting point
            for (int otherCard = 9; otherCard > 1; otherCard--)
            {
                // build player hand
                Hand playerHand = new Hand();

                // first card is an ace, second card is looped over
                playerHand.AddCard(new Card(Card.Ranks.Ace, Card.Suits.Hearts));
                playerHand.AddCard(new Card((Card.Ranks)otherCard, Card.Suits.Spades));

                // find strategy 
                SetupStateData(candidate.StateData, playerHand, dealerCard);
                candidate.Evaluate();

                // get the decision and store in the strategy object
                var action = GetActionFromCandidate(candidate.StateData);
                result.SetActionForSoftHand(upcardRank, otherCard, action);
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
                    playerHand.AddCard(new Card(Card.Ranks.Ten, Card.Suits.Diamonds));
                    firstCardRank = 6;
                    secondCardRank = 4;
                }

                // we don't want pairs, so check for that
                if (firstCardRank == secondCardRank)
                {
                    firstCardRank++;
                    secondCardRank--;
                }

                playerHand.AddCard(new Card((Card.Ranks)firstCardRank, Card.Suits.Diamonds));
                playerHand.AddCard(new Card((Card.Ranks)secondCardRank, Card.Suits.Spades));

                // find strategy 
                SetupStateData(candidate.StateData, playerHand, dealerCard);
                candidate.Evaluate();

                // get the decision and store in the strategy object
                var action = GetActionFromCandidate(candidate.StateData);
                result.SetActionForHardHand(upcardRank, hardTotal, action);
            }
        }

        private static void SetupStateData(ProblemState stateData, Hand hand, Card dealerUpcard)
        {
            // prepare for testing
            stateData.PlayerHand = hand;
            stateData.DealerUpcard = dealerUpcard;

            stateData.VotesForDoubleDown = 0;
            stateData.VotesForHit = 0;
            stateData.VotesForStand = 0;
            stateData.VotesForSplit = 0;
        }

        private static ActionToTake GetActionFromCandidate(ProblemState stateData)
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
    }
}

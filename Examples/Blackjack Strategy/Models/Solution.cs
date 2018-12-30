using Evolutionary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using System.Data;
using System.Linq;

namespace BlackjackStrategy.Models
{
    enum ActionToTake {  Stand, Hit, Double, Split };
    class ActionWithVotes
    {
        public int NumVotes;
        public ActionToTake Action;
        public ActionWithVotes(int numVotes, ActionToTake action)
        {
            NumVotes = numVotes;
            Action = action;
        }
    }

    class Solution
    {
        // a place to store the best solution, once we find it
        public CandidateSolution<bool, ProblemState> BestSolution { get; set; }
        private Action<string> displayGenerationCallback;
        private string dealerUpcardRank;

        public void BuildProgram(
            int populationSize, 
            int crossoverPercentage, 
            double mutationPercentage, 
            int ElitismPercentage, 
            int tourneySize, 
            Action<string> currentStatusCallback,
            string dealerUpcardRankToUse)
        {
            displayGenerationCallback = currentStatusCallback;
            dealerUpcardRank = dealerUpcardRankToUse;

            // just some values, leave the rest as defaults
            var engineParams = new EngineParameters()
            {
                CrossoverRate = crossoverPercentage / 100F,
                ElitismRate = ElitismPercentage / 100F,
                IsLowerFitnessBetter = false,
                MutationRate = mutationPercentage / 100F,
                PopulationSize = populationSize,
                TourneySize = tourneySize
            };

            // create the engine.  each tree (and node within the tree) will return a bool.
            // we also indicate the type of our problem state data (used by terminal functions and stateful functions)
            var engine = new Engine<bool, ProblemState>(engineParams);

            // no constants for this problem
            
            // no variables for this solution - we can't pass in information about our hand 
            // via boolean variables, so we do it via some terminal functions instead

            // for a boolean tree, we use the standard operators
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a, b, c) => a || b || c, "Or3");
            engine.AddFunction((a, b) => a && b, "And");
            engine.AddFunction((a, b, c) => a && b && c, "And3");
            engine.AddFunction((a) => !a, "Not");

            // then add functions to indicate a strategy
            engine.AddStatefulFunction(HitIf, "HitIf");
            engine.AddStatefulFunction(StandIf, "StandIf");
            engine.AddStatefulFunction(DoubleIf, "DoubleIf");
            engine.AddStatefulFunction(SplitIf, "SplitIf");

            // terminal functions to look at game state
            // first, player holding ace?
            engine.AddTerminalFunction(HasAce, "HasAce");
            // player hand totals
            engine.AddTerminalFunction(HandVal4, "Has4");
            engine.AddTerminalFunction(HandVal5, "Has5");
            engine.AddTerminalFunction(HandVal6, "Has6");
            engine.AddTerminalFunction(HandVal7, "Has7");
            engine.AddTerminalFunction(HandVal8, "Has8");
            engine.AddTerminalFunction(HandVal9, "Has9");
            engine.AddTerminalFunction(HandVal10, "Has10");
            engine.AddTerminalFunction(HandVal11, "Has11");
            engine.AddTerminalFunction(HandVal12, "Has12");
            engine.AddTerminalFunction(HandVal13, "Has13");
            engine.AddTerminalFunction(HandVal14, "Has14");
            engine.AddTerminalFunction(HandVal15, "Has15");
            engine.AddTerminalFunction(HandVal16, "Has16");
            engine.AddTerminalFunction(HandVal17, "Has17");
            engine.AddTerminalFunction(HandVal18, "Has18");
            engine.AddTerminalFunction(HandVal19, "Has19");
            engine.AddTerminalFunction(HandVal20, "Has20");
            // num cards held
            engine.AddTerminalFunction(Holding2Cards, "Hold2");
            engine.AddTerminalFunction(Holding3Cards, "Hold3");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            BestSolution = engine.FindBestSolution();
        }

        //-------------------------------------------------------------------------
        // Now terminal functions to get information about the player's hand
        //-------------------------------------------------------------------------

        private  bool HasAce(ProblemState stateData)
        {
            foreach (var card in stateData.PlayerHand.Cards)
                if (card.Rank == "A") return true;
            return false;
        }

        private  bool HandVal4(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 4;
        }

        private  bool HandVal5(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 5;
        }

        private  bool HandVal6(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 6;
        }

        private  bool HandVal7(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 7;
        }

        private  bool HandVal8(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 8;
        }

        private  bool HandVal9(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 9;
        }

        private  bool HandVal10(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 10;
        }

        private  bool HandVal11(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 11;
        }

        private  bool HandVal12(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 12;
        }

        private  bool HandVal13(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 13;
        }

        private  bool HandVal14(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 14;
        }

        private  bool HandVal15(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 15;
        }

        private  bool HandVal16(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 16;
        }

        private  bool HandVal17(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 17;
        }

        private  bool HandVal18(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 18;
        }

        private  bool HandVal19(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 19;
        }

        private  bool HandVal20(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 20;
        }


        private  bool Holding2Cards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count == 2;
        }

        private  bool Holding3Cards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count == 3;
        }

        private  bool Holding4PlusCards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count >= 4;
        }

        //-------------------------------------------------------------------------
        // then functions to steer the strategy via storing to state
        //-------------------------------------------------------------------------
        private  bool HitIf(bool value, ProblemState state)
        {
            // pass through the value, but register the vote
            if (value) state.VotesForHit++;
            return value;
        }

        private  bool StandIf(bool value, ProblemState state)
        {
            // pass through the value, but register the vote
            if (value) state.VotesForStand++;
            return value;
        }

        private  bool DoubleIf(bool value, ProblemState state)
        {
            // double down only legal when holding 2 cards
            if (state.PlayerHand.Cards.Count == 2)
            {
                if (value) state.VotesForDoubleDown++;
            }
            return value;
        }

        private bool SplitIf(bool value, ProblemState state)
        {
            // only legal when holding a pair 
            if ((state.PlayerHand.Cards.Count == 2) &&
                (state.PlayerHand.Cards[0].Rank == state.PlayerHand.Cards[1].Rank))
            {
                if (value) state.VotesForSplit++;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private float EvaluateCandidate(CandidateSolution<bool, ProblemState> candidate)
        {
            int playerChips = 0;

            for (int handNum = 0; handNum < TestConditions.NumHandsToPlay; handNum++)
            {
                // for each hand, we generate a random deck.  Blackjack is often played with multiple decks to improve the house edge
                MultiDeck deck = new MultiDeck(TestConditions.NumDecks);
                // always use the designated dealer upcard (of hearts), so we need to remove from the deck so it doesn't get used twice
                deck.RemoveCard(dealerUpcardRank, "H");

                Hand dealerHand = new Hand();
                Hand playerHand = new Hand();
                playerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(new Card(dealerUpcardRank, "H"));
                playerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());

                // save the cards in state, and reset the votes for this hand
                candidate.StateData.PlayerHands.Clear();
                candidate.StateData.PlayerHands.Add(playerHand);

                // do the intial wager
                int totalBetAmount = TestConditions.BetSize;
                playerChips -= TestConditions.BetSize;

                // outer loop is for each hand the player holds.  Obviously this only happens when they've split a hand
                for (int handIndex = 0; handIndex < candidate.StateData.PlayerHands.Count; handIndex++)
                {
                    var currentHand = candidate.StateData.PlayerHands[handIndex];

                    // specify if we're looking at the second hand.  This controls the return value of PlayerHand
                    candidate.StateData.UsingFirstHand = (currentHand == candidate.StateData.PlayerHands[0]);

                    // loop until the hand is done
                    var currentHandState = TestConditions.GameState.PlayerDrawing;

                    // check for player having a blackjack, which is an instant win
                    if (playerHand.HandValue() == 21)
                    {
                        // if the dealer also has 21, then it's a tie
                        if (dealerHand.HandValue() != 21)
                        {
                            currentHandState = TestConditions.GameState.PlayerBlackjack;
                            playerChips += TestConditions.BlackjackPayoffSize;
                        }
                        else
                        {
                            // a tie means we just ignore it and drop through
                            currentHandState = TestConditions.GameState.HandComparison;
                        }
                    }

                    // check for dealer having blackjack, which is either instant loss or tie 
                    if (dealerHand.HandValue() == 21) currentHandState = TestConditions.GameState.HandComparison;

                    // player draws 
                    while (currentHandState == TestConditions.GameState.PlayerDrawing)
                    {
                        // get the decision
                        candidate.StateData.VotesForDoubleDown = 0;
                        candidate.StateData.VotesForHit = 0;
                        candidate.StateData.VotesForStand = 0;
                        candidate.StateData.VotesForSplit = 0;
                        candidate.Evaluate();   // throw away the result, because it's meaningless

                        // look at the votes to see what to do
                        var action = GetAction(candidate.StateData);
                        switch (action)
                        {
                            case ActionToTake.Hit:
                                // hit me
                                playerHand.AddCard(deck.DealCard());
                                // if we're at 21, we're done
                                if (playerHand.HandValue() == 21)
                                    currentHandState = TestConditions.GameState.DealerDrawing;
                                // did we bust?
                                if (playerHand.HandValue() > 21)
                                    currentHandState = TestConditions.GameState.PlayerBusted;
                                break;

                            case ActionToTake.Stand:
                                // if player stands, it's the dealer's turn to draw
                                currentHandState = TestConditions.GameState.DealerDrawing;
                                break;

                            case ActionToTake.Double:
                                // double down means bet another chip, and get one and only card card
                                playerChips -= TestConditions.BetSize;
                                totalBetAmount += TestConditions.BetSize;
                                playerHand.AddCard(deck.DealCard());
                                if (playerHand.HandValue() > 21)
                                    currentHandState = TestConditions.GameState.PlayerBusted;
                                else
                                    currentHandState = TestConditions.GameState.DealerDrawing;
                                break;

                            case ActionToTake.Split:
                                // do the split and add the hand to our collection
                                var newHand = new Hand();
                                newHand.AddCard(playerHand.Cards[1]);
                                playerHand.Cards[1] = deck.DealCard();
                                newHand.AddCard(deck.DealCard());
                                candidate.StateData.PlayerHands.Add(newHand);
                                // our extra bet
                                playerChips -= TestConditions.BetSize;
                                // we don't adjust totalBetAmount because each bet pays off individually, so the total is right 
                                //totalBetAmount += TestConditions.BetSize;
                                break;
                        }
                    }

                    // if the player busted, nothing to do, since chips have already been consumed.  Just go on to the next hand
                    // on the other hand, if the player hasn't busted, then we need to play the hand for the dealer
                    while (currentHandState == TestConditions.GameState.DealerDrawing)
                    {
                        // if player didn't bust or blackjack, dealer hits until they have 17+ (hits on soft 17)
                        if (dealerHand.HandValue() < 17)
                        {
                            dealerHand.AddCard(deck.DealCard());
                            if (dealerHand.HandValue() > 21)
                            {
                                currentHandState = TestConditions.GameState.DealerBusted;
                                playerChips += totalBetAmount * 2;  // the original bet and a matching amount
                            }
                        }
                        else
                        {
                            // dealer hand is 17+, so we're done
                            currentHandState = TestConditions.GameState.HandComparison;
                        }
                    }

                    if (currentHandState == TestConditions.GameState.HandComparison)
                    {
                        int playerHandValue = playerHand.HandValue();
                        int dealerHandValue = dealerHand.HandValue();

                        // if it's a tie, give the player his bet back
                        if (playerHandValue == dealerHandValue)
                        {
                            playerChips += totalBetAmount;
                        }
                        else
                        {
                            if (playerHandValue > dealerHandValue)
                            {
                                // player won
                                playerChips += totalBetAmount * 2;  // the original bet and a matching amount
                            }
                            else
                            {
                                // player lost, nothing to do since the chips have already been decremented
                            }
                        }
                    }
                }
            }

            return playerChips;
        }

        public static void DebugDisplayStrategy(CandidateSolution<bool, ProblemState> candidate, string prefixText)
        {
            string debug = 
                prefixText + 
                (String.IsNullOrWhiteSpace(prefixText) ? "" : "  ") +
                "  Plr: " + candidate.StateData.PlayerHand + 
                "  H: " + candidate.StateData.VotesForHit +
                "  S: " + candidate.StateData.VotesForStand +
                "  P: " + candidate.StateData.VotesForSplit + 
                "  D: " + candidate.StateData.VotesForDoubleDown;
            Debug.WriteLine(debug);
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private bool PerGenerationCallback(EngineProgress progress)
        {
            string summary = "Generation " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0");

            displayGenerationCallback(summary);
            Debug.WriteLine(summary);

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }

        public static ActionToTake GetAction(ProblemState stateData)
        {
            int votesForStand = stateData.VotesForStand;
            int votesForHit = stateData.VotesForHit;
            int votesForDouble = stateData.VotesForDoubleDown;
            int votesForSplit = stateData.VotesForSplit;

            List<ActionWithVotes> votes = new List<ActionWithVotes>();
            votes.Add(new ActionWithVotes(votesForDouble, ActionToTake.Double));
            votes.Add(new ActionWithVotes(votesForStand, ActionToTake.Stand));
            votes.Add(new ActionWithVotes(votesForHit, ActionToTake.Hit));
            votes.Add(new ActionWithVotes(votesForDouble, ActionToTake.Double));
            votes.Add(new ActionWithVotes(votesForSplit, ActionToTake.Split));

            return votes.OrderByDescending(v => v.NumVotes).First().Action;
        }
    }
}

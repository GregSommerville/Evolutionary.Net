using Evolutionary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;

namespace BlackjackStrategy.Models
{
    class Solution
    {
        // a place to store the best solution, once we find it
        public CandidateSolution<bool, ProblemState> BestSolution { get; set; }
        private Action<string> displayGenerationCallback;

        public void BuildProgram(int populationSize, int crossoverPercentage, 
            double mutationPercentage, int elitismPercentage, int tourneySize, Action<string> currentStatusCallback)
        {
            displayGenerationCallback = currentStatusCallback;

            var engineParams = new EngineParameters()
            {
                CrossoverRate = crossoverPercentage / 100F,
                ElitismPercentageOfPopulation = elitismPercentage,
                IsLowerFitnessBetter = false,
                MutationRate = mutationPercentage / 100F,
                PopulationSize = populationSize,
                TourneySize = tourneySize,
                NoChangeGenerationCountForTermination = 2, // terminate if we go N generations without an improvement of average fitness
                RandomTreeMinDepth = 5, // when first creating a random tree or subtree
                RandomTreeMaxDepth = 8
            };

            Debug.WriteLine("Starting test with muation " + engineParams.MutationRate + ", pop size " + engineParams.PopulationSize);

            // create the engine.  each tree (and node within the tree) will return a bool.
            // we also indicate the type of our problem state data (used by terminal functions)
            var engine = new Engine<bool, ProblemState>(engineParams);

            // no constants for this problem
            
            // no variables for this solution - we can't pass in information about our hand and 
            // the dealer upcard via boolean variables, so we do it via some terminal functions instead

            // for a boolean tree, we use the standard operators
            //engine.AddFunction((a, b) => a && b, "And");
            //engine.AddFunction((a, b, c) => a && b && c, "And3");
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a, b, c) => a || b || c, "Or3");
//            engine.AddFunction((a) => !a, "Not");

            // then add functions to indicate a strategy
            engine.AddStatefulFunction(HitIf, "HitIf");
            engine.AddStatefulFunction(StandIf, "StandIf");
            engine.AddStatefulFunction(DoubleIf, "DoubleIf");

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
            //engine.AddTerminalFunction(Holding2Cards, "Hold2");
            //engine.AddTerminalFunction(Holding3Cards, "Hold3");
            //engine.AddTerminalFunction(Holding4PlusCards, "HoldGE4");

            // then whatever the dealer upcard is
            engine.AddTerminalFunction(DealerShowsA, "DlrA");
            engine.AddTerminalFunction(DealerShows2, "Dlr2");
            engine.AddTerminalFunction(DealerShows3, "Dlr3");
            engine.AddTerminalFunction(DealerShows4, "Dlr4");
            engine.AddTerminalFunction(DealerShows5, "Dlr5");
            engine.AddTerminalFunction(DealerShows6, "Dlr6");
            engine.AddTerminalFunction(DealerShows7, "Dlr7");
            engine.AddTerminalFunction(DealerShows8, "Dlr8");
            engine.AddTerminalFunction(DealerShows9, "Dlr9");
            engine.AddTerminalFunction(DealerShowsT, "DlrT");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            BestSolution = engine.FindBestSolution();
        }

        //-------------------------------------------------------------------------
        // first, terminal functions to get information about the dealer upcard
        //-------------------------------------------------------------------------

        private bool DealerShowsA(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "A";
        }

        private bool DealerShows2(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "2";
        }

        private  bool DealerShows3(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "3";
        }

        private  bool DealerShows4(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "4";
        }

        private  bool DealerShows5(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "5";
        }

        private  bool DealerShows6(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "6";
        }

        private  bool DealerShows7(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "7";
        }

        private  bool DealerShows8(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "8";
        }

        private  bool DealerShows9(ProblemState stateData)
        {
            return stateData.DealerHand.Cards[0].Rank == "9";
        }

        private  bool DealerShowsT(ProblemState stateData)
        {
            var dealerCard = stateData.DealerHand.Cards[0];
            return dealerCard.Rank == "T" || dealerCard.Rank == "J" || dealerCard.Rank == "Q" || dealerCard.Rank == "K";
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
            // pass through the value, but register the vote
            if (value) state.VotesForDoubleDown++;
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
                Hand dealerHand = new Hand();
                Hand playerHand = new Hand();

                playerHand.AddCard(deck.DealCard());
                playerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());
                dealerHand.AddCard(deck.DealCard());

                // save the cards in state, and reset the votes for this hand
                candidate.StateData.DealerHand = dealerHand;
                candidate.StateData.PlayerHand = playerHand;

                // loop until the hand is done
                var currentHandState = TestConditions.GameState.PlayerDrawing;

                // do the intial wager
                int totalBetAmount = TestConditions.BetSize;
                playerChips -= TestConditions.BetSize;

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
                    candidate.Evaluate();   // throw away the result, because it's meaningless

                    // look at the votes to see what to do
                    string action = GetAction(candidate.StateData);

                    switch (action)
                    {
                        case "H":
                            // hit me
                            playerHand.AddCard(deck.DealCard());
                            // if we're at 21, we're done
                            if (playerHand.HandValue() == 21)
                                currentHandState = TestConditions.GameState.DealerDrawing;
                            // did we bust?
                            if (playerHand.HandValue() > 21)
                                currentHandState = TestConditions.GameState.PlayerBusted;
                            break;

                        case "S":
                            // if player stands, it's the dealer's turn to draw
                            currentHandState = TestConditions.GameState.DealerDrawing;
                            break;

                        case "D":
                            // double down means bet another chip, and get one and only card card
                            playerChips -= TestConditions.BetSize;
                            totalBetAmount += TestConditions.BetSize;
                            playerHand.AddCard(deck.DealCard());
                            if (playerHand.HandValue() > 21)
                                currentHandState = TestConditions.GameState.PlayerBusted;
                            else
                                currentHandState = TestConditions.GameState.DealerDrawing;
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

            return playerChips;
        }

        public static void DebugDisplayStrategy(CandidateSolution<bool, ProblemState> candidate, string prefixText)
        {
            string debug = 
                prefixText + 
                (String.IsNullOrWhiteSpace(prefixText) ? "" : "  ") +
                "Dlr: " + candidate.StateData.DealerHand.Cards[0].Rank + 
                "  Plr: " + candidate.StateData.PlayerHand + 
                "  H: " + candidate.StateData.VotesForHit +
                "  S: " + candidate.StateData.VotesForStand +
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

        public static string GetAction(ProblemState stateData)
        {
            int votesForStand = stateData.VotesForStand;
            int votesForHit = stateData.VotesForHit;
            int votesForDouble = stateData.VotesForDoubleDown;

            if ((votesForHit > votesForStand) && (votesForHit > votesForDouble)) return "H";
            if ((votesForDouble > votesForHit) && (votesForDouble > votesForStand)) return "D";
            return "S";
        }
    }
}

﻿using Evolutionary;
using System;
using System.Diagnostics;

namespace BlackjackStrategy.Models
{
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

            //----------------------------------------------
            // terminal functions to look at game state
            //----------------------------------------------

            // first, player holding ace?
            engine.AddTerminalFunction(HasSoftAce, "HasSoftAce");

            // specific pairs
            engine.AddTerminalFunction(HasPairTwos, "HasPair2");
            engine.AddTerminalFunction(HasPairThrees, "HasPair3");
            engine.AddTerminalFunction(HasPairFours, "HasPair4");
            engine.AddTerminalFunction(HasPairFives, "HasPair5");
            engine.AddTerminalFunction(HasPairSixes, "HasPair6");
            engine.AddTerminalFunction(HasPairSevens, "HasPair7");
            engine.AddTerminalFunction(HasPairEights, "HasPair8");
            engine.AddTerminalFunction(HasPairNines, "HasPair9");
            engine.AddTerminalFunction(HasPairTens, "HasPairT");
            engine.AddTerminalFunction(HasPairAces, "HasPairA");

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

        private  bool HasSoftAce(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce();
        }

        private bool HasPairOf(string rankNeeded, Hand hand)
        {
            return (hand.Cards.Count == 2) &&
                (hand.Cards[0].Rank == rankNeeded) &&
                (hand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairTwos(ProblemState stateData)
        {
            return HasPairOf("2", stateData.PlayerHand);
        }

        private bool HasPairThrees(ProblemState stateData)
        {
            return HasPairOf("3", stateData.PlayerHand);
        }

        private bool HasPairFours(ProblemState stateData)
        {
            return HasPairOf("4", stateData.PlayerHand);
        }

        private bool HasPairFives(ProblemState stateData)
        {
            return HasPairOf("5", stateData.PlayerHand);
        }

        private bool HasPairSixes(ProblemState stateData)
        {
            return HasPairOf("6", stateData.PlayerHand);
        }

        private bool HasPairSevens(ProblemState stateData)
        {
            return HasPairOf("7", stateData.PlayerHand);
        }

        private bool HasPairEights(ProblemState stateData)
        {
            return HasPairOf("8", stateData.PlayerHand);
        }

        private bool HasPairNines(ProblemState stateData)
        {
            return HasPairOf("9", stateData.PlayerHand);
        }

        private bool HasPairTens(ProblemState stateData)
        {
            // covers tens, jacks, queens and kings
            return (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].RankValueHigh == 10) &&
                (stateData.PlayerHand.Cards[1].RankValueHigh == 10);
        }

        private bool HasPairAces(ProblemState stateData)
        {
            return HasPairOf("A", stateData.PlayerHand);
        }

        // all the ones relating to hand total value
        private bool HandVal4(ProblemState stateData)
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


        // how many cards I've got
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
            // test every possible situation and store the candidate's suggested action in the strategy object
            OverallStrategy strategy = new OverallStrategy(candidate);

            // then test that strategy and return the total money lost/made
            var strategyTester = new StrategyTester(strategy);
            return strategyTester.GetStrategyScore(dealerUpcardRank);
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
    }
}
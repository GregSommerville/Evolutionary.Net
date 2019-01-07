using Evolutionary;
using System;
using System.Diagnostics;

namespace BlackjackStrategy.Models
{
    class SolutionSingle : SolutionBase
    {
        public CandidateSolution<bool, ProblemState> Solution { get; private set; }
        private float Fitness;
        private int NumGenerationsNeeded;
        private Action<string> displayGenerationCallback;

        public override OverallStrategy GetStrategy()
        {
            var strategy = new OverallStrategy(Solution);
            return strategy;
        }

        public override void BuildProgram(EngineParameters engineParams, Action<string> currentStatusCallback)
        {
            displayGenerationCallback = currentStatusCallback;

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

            // soft hands
            engine.AddTerminalFunction(AcePlus2, "AcePlus2");
            engine.AddTerminalFunction(AcePlus3, "AcePlus3");
            engine.AddTerminalFunction(AcePlus4, "AcePlus4");
            engine.AddTerminalFunction(AcePlus5, "AcePlus5");
            engine.AddTerminalFunction(AcePlus6, "AcePlus6");
            engine.AddTerminalFunction(AcePlus7, "AcePlus7");
            engine.AddTerminalFunction(AcePlus8, "AcePlus8");
            engine.AddTerminalFunction(AcePlus9, "AcePlus9");

            // pairs
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

            // hard hand totals
            engine.AddTerminalFunction(HandVal5, "Hard5");
            engine.AddTerminalFunction(HandVal6, "Hard6");
            engine.AddTerminalFunction(HandVal7, "Hard7");
            engine.AddTerminalFunction(HandVal8, "Hard8");
            engine.AddTerminalFunction(HandVal9, "Hard9");
            engine.AddTerminalFunction(HandVal10, "Hard10");
            engine.AddTerminalFunction(HandVal11, "Hard11");
            engine.AddTerminalFunction(HandVal12, "Hard12");
            engine.AddTerminalFunction(HandVal13, "Hard13");
            engine.AddTerminalFunction(HandVal14, "Hard14");
            engine.AddTerminalFunction(HandVal15, "Hard15");
            engine.AddTerminalFunction(HandVal16, "Hard16");
            engine.AddTerminalFunction(HandVal17, "Hard17");
            engine.AddTerminalFunction(HandVal18, "Hard18");
            engine.AddTerminalFunction(HandVal19, "Hard19");
            engine.AddTerminalFunction(HandVal20, "Hard20");

            // upcards
            engine.AddTerminalFunction(DealerShows2, "Dlr2");
            engine.AddTerminalFunction(DealerShows3, "Dlr3");
            engine.AddTerminalFunction(DealerShows4, "Dlr4");
            engine.AddTerminalFunction(DealerShows5, "Dlr5");
            engine.AddTerminalFunction(DealerShows6, "Dlr6");
            engine.AddTerminalFunction(DealerShows7, "Dlr7");
            engine.AddTerminalFunction(DealerShows8, "Dlr8");
            engine.AddTerminalFunction(DealerShows9, "Dlr9");
            engine.AddTerminalFunction(DealerShows10, "Dlr10");
            engine.AddTerminalFunction(DealerShowsA, "DlrA");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            Solution = engine.FindBestSolution();
            Fitness = Solution.Fitness;
            FinalStatus = "Final Score: " + Fitness.ToString() + "\n" +
                    "Generations needed to find: " + NumGenerationsNeeded;
        }

        //-------------------------------------------------------------------------
        // Now terminal functions to get information about the player's hand
        //-------------------------------------------------------------------------

        private  bool HasSoftAce(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce();
        }

        // since these are only used when dealing with a soft ace, we can simply subtract 11 for the high ace
        private bool AcePlus2(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 2;
        }
        private bool AcePlus3(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 3;
        }
        private bool AcePlus4(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 4;
        }
        private bool AcePlus5(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 5;
        }
        private bool AcePlus6(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 6;
        }
        private bool AcePlus7(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 7;
        }
        private bool AcePlus8(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 8;
        }
        private bool AcePlus9(ProblemState stateData)
        {
            return stateData.PlayerHand.HasSoftAce() && 
                (stateData.PlayerHand.HandValue() - 11) == 9;
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

        private bool DealerShows2(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "2";
        }
        private bool DealerShows3(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "3";
        }
        private bool DealerShows4(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "4";
        }
        private bool DealerShows5(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "5";
        }
        private bool DealerShows6(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "6";
        }
        private bool DealerShows7(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "7";
        }
        private bool DealerShows8(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "8";
        }
        private bool DealerShows9(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "9";
        }
        private bool DealerShows10(ProblemState stateData)
        {
            return stateData.DealerUpcard.RankValueHigh == 10;
        }
        private bool DealerShowsA(ProblemState stateData)
        {
            return stateData.DealerUpcard.Rank == "A";
        }

        //-------------------------------------------------------------------------
        // then functions to steer the strategy via storing to state
        //-------------------------------------------------------------------------
        private bool HitIf(bool value, ProblemState state)
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
            // we don't check for validity here, since it's handled when retrieving the action
            if (value) state.VotesForDoubleDown++;
            return value;
        }

        private bool SplitIf(bool value, ProblemState state)
        {
            // we don't check for validity here, since it's handled when retrieving the action
            if (value) state.VotesForSplit++;
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
            return strategyTester.GetStrategyScore(TestConditions.NumHandsToPlay);
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

            // keep track of how many gens we've searched
            NumGenerationsNeeded = progress.GenerationNumber;

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Evolutionary;

namespace BlackjackStrategy.Models
{
    class SolutionByUpcard : SolutionBase
    {
        public int TotalGenerations { get; set; }

        private Dictionary<Card.Ranks, CandidateSolution<bool, ProblemState>> solutions = 
            new Dictionary<Card.Ranks, CandidateSolution<bool, ProblemState>>();
        private Strategy FinalStrategy;
        private int NumGenerationsNeeded = 0;
        private Action<string, CandidateSolution<bool, ProblemState>> perGenerationCallback;
        private ProgramSettings settings;
        private Card.Ranks currentDealerUpcardRank;


        public override Strategy GetStrategy()
        {
            return FinalStrategy;
        }

        public override void BuildProgram(ProgramSettings settings, Action<string, CandidateSolution<bool, ProblemState>> callback)
        {
            this.settings = settings;
            this.perGenerationCallback = callback;

            //for (int upcardRank = 14; upcardRank > 1; upcardRank--)
            foreach (var upcardRank in Card.ListOfRanks)
            {
                // skip K, Q, J
                if (upcardRank == Card.Ranks.King  || upcardRank == Card.Ranks.Queen || upcardRank == Card.Ranks.Jack)
                    continue;

                Card dealerCard = new Card(upcardRank, Card.Suits.Diamonds);

                solutions[dealerCard.Rank] = FindStrategyForUpcard(dealerCard);
                SaveSolutionToDisk(dealerCard.Rank + "_upcard_solution.txt", solutions[dealerCard.Rank].ToString());
            }

            // convert the dictionary of GP solutions to a single strategy
            FinalStrategy = StrategyFactory.GetStrategyForGP(solutions);
            FinalStatus = "Generations needed to find: " + NumGenerationsNeeded;
        }

        private void SaveSolutionToDisk(string fileName, string solution)
        {
            File.WriteAllText(fileName, solution);
        }

        private CandidateSolution<bool, ProblemState> FindStrategyForUpcard(Card dealerCard)
        {
            currentDealerUpcardRank = dealerCard.Rank;

            // create the engine.  each tree (and node within the tree) will return a bool.
            // we also indicate the type of our problem state data (used by terminal functions and stateful functions)
            var engine = new Engine<bool, ProblemState>(settings.GPsettings);

            // for a boolean tree, we use the standard operators
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a, b, c) => a || b || c, "Or3");
            engine.AddFunction((a, b) => a && b, "And");
            engine.AddFunction((a, b, c) => a && b && c, "And3");
            engine.AddFunction((a) => !a, "Not");

            // then add functions to indicate a strategy
            engine.AddStatefulFunction(SplitIf, "SplitIf");
            engine.AddStatefulFunction(HitIf, "HitIf");
            engine.AddStatefulFunction(StandIf, "StandIf");
            engine.AddStatefulFunction(DoubleIf, "DoubleIf");

            // details of pair
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

            // terminal functions to indicate the other card's rank
            engine.AddTerminalFunction(AcePlus2, "AcePlus2");
            engine.AddTerminalFunction(AcePlus3, "AcePlus3");
            engine.AddTerminalFunction(AcePlus4, "AcePlus4");
            engine.AddTerminalFunction(AcePlus5, "AcePlus5");
            engine.AddTerminalFunction(AcePlus6, "AcePlus6");
            engine.AddTerminalFunction(AcePlus7, "AcePlus7");
            engine.AddTerminalFunction(AcePlus8, "AcePlus8");
            engine.AddTerminalFunction(AcePlus9, "AcePlus9");

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

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((p,b) => PerGenerationCallback(p,b));

            return engine.FindBestSolution();
        }

        //-------------------------------------------------------------------------
        // Now terminal functions to get information about the player's hand
        //-------------------------------------------------------------------------

        // since these are only used when dealing with a soft ace, we can simply subtract 11 for the high ace
        private bool AcePlus2(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 2;
        }
        private bool AcePlus3(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 3;
        }
        private bool AcePlus4(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 4;
        }
        private bool AcePlus5(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 5;
        }
        private bool AcePlus6(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 6;
        }
        private bool AcePlus7(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 7;
        }
        private bool AcePlus8(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 8;
        }
        private bool AcePlus9(ProblemState stateData)
        {
            return (stateData.PlayerHand.HandValue() - 11) == 9;
        }

        private bool HasPairOf(Card.Ranks rankNeeded, Hand hand)
        {
            return (hand.Cards.Count == 2) &&
                (hand.Cards[0].Rank == rankNeeded) &&
                (hand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairTwos(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Two, stateData.PlayerHand);
        }

        private bool HasPairThrees(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Three, stateData.PlayerHand);
        }

        private bool HasPairFours(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Four, stateData.PlayerHand);
        }

        private bool HasPairFives(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Five, stateData.PlayerHand);
        }

        private bool HasPairSixes(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Six, stateData.PlayerHand);
        }

        private bool HasPairSevens(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Seven, stateData.PlayerHand);
        }

        private bool HasPairEights(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Eight, stateData.PlayerHand);
        }

        private bool HasPairNines(ProblemState stateData)
        {
            return HasPairOf(Card.Ranks.Nine, stateData.PlayerHand);
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
            return HasPairOf(Card.Ranks.Ace, stateData.PlayerHand);
        }

        // all the ones relating to hand total value
        private bool HandVal5(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 5;
        }

        private bool HandVal6(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 6;
        }

        private bool HandVal7(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 7;
        }

        private bool HandVal8(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 8;
        }

        private bool HandVal9(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 9;
        }

        private bool HandVal10(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 10;
        }

        private bool HandVal11(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 11;
        }

        private bool HandVal12(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 12;
        }

        private bool HandVal13(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 13;
        }

        private bool HandVal14(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 14;
        }

        private bool HandVal15(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 15;
        }

        private bool HandVal16(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 16;
        }

        private bool HandVal17(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 17;
        }

        private bool HandVal18(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 18;
        }

        private bool HandVal19(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 19;
        }

        private bool HandVal20(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 20;
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

        private bool StandIf(bool value, ProblemState state)
        {
            // pass through the value, but register the vote
            if (value) state.VotesForStand++;
            return value;
        }

        private bool DoubleIf(bool value, ProblemState state)
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
            Strategy strategy = StrategyFactory.GetStrategyForGP(candidate);

            // then test that strategy and return the total money lost/made
            var strategyTester = new StrategyTester(strategy, settings.TestSettings);
            strategyTester.DealerUpcardRank = currentDealerUpcardRank;

            return strategyTester.GetStrategyScore(settings.TestSettings.NumHandsToPlay);
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private bool PerGenerationCallback(EngineProgress progress, CandidateSolution<bool, ProblemState> bestThisGeneration)
        {
            string summary =
                "Upcard " + currentDealerUpcardRank  +
                " gen: " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0");

            perGenerationCallback(summary, bestThisGeneration);
            Debug.WriteLine(summary);

            // keep track of how many gens we've searched
            NumGenerationsNeeded++;

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }
    }
}
using Evolutionary;
using System.Diagnostics;

namespace BlackjackStrategy.Models
{
    class Solution
    {
        // a place to store the best solution, once we find it
        public static CandidateSolution<bool, ProblemState> BestSolution { get; set; }

        public static void BuildProgram(int populationSize, int crossoverPercentage, 
            double mutationPercentage, int elitismPercentage, int tourneySize)
        {
            var engineParams = new EngineParameters()
            {
                CrossoverRate = crossoverPercentage / 100F,
                ElitismPercentageOfPopulation = elitismPercentage,
                IsLowerFitnessBetter = false,
                MutationRate = mutationPercentage / 100F,
                PopulationSize = populationSize,
                TourneySize = tourneySize,
                NoChangeGenerationCountForTermination = 12
            };

            // create the engine.  each tree (and node within the tree) will return a bool.
            // we also indicate the type of our problem state data (used by terminal functions)
            var engine = new Engine<bool, ProblemState>(engineParams);

            // constants are simple in this case since there are only two possibilities
            engine.AddConstant(false);
            engine.AddConstant(true);

            // no variables for this solution - we can't pass in information about our hand and 
            // the dealer upcard via boolean variables, so we do it via some terminal functions instead

            // for a boolean tree, we use the standard operators
            engine.AddFunction((a, b) => a && b, "And");
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a) => !a, "Not");
            engine.AddFunction((a, b, c) => a ? b : c, "If");         // if a, return b else c

            // terminal functions to look at game state - first, cards the player is holding
            engine.AddTerminalFunction(HasAce, "HasAce");
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

            // then whatever the dealer upcard is
            engine.AddTerminalFunction(DealerShowsA, "DealerA");
            engine.AddTerminalFunction(DealerShows2, "Dealer2");
            engine.AddTerminalFunction(DealerShows3, "Dealer3");
            engine.AddTerminalFunction(DealerShows4, "Dealer4");
            engine.AddTerminalFunction(DealerShows5, "Dealer5");
            engine.AddTerminalFunction(DealerShows6, "Dealer6");
            engine.AddTerminalFunction(DealerShows7, "Dealer7");
            engine.AddTerminalFunction(DealerShows8, "Dealer8");
            engine.AddTerminalFunction(DealerShows9, "Dealer9");
            engine.AddTerminalFunction(DealerShowsT, "DealerT");

            // then add terminal functions to indicate a strategy
            engine.AddTerminalFunction(VoteHit, "Hit");
            engine.AddTerminalFunction(VoteStand, "Stand");
            engine.AddTerminalFunction(VoteDouble, "DblDown");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            BestSolution = engine.FindBestSolution();
        }

        // our terminal functions fall into two categories: 
        // ones to get the game state, and ones to indicate which strategy to use.

        //-------------------------------------------------------------------------
        // first, terminal functions to get information about the dealer upcard
        //-------------------------------------------------------------------------

        private static bool DealerShowsA(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "A";
        }

        private static bool DealerShows2(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "2";
        }

        private static bool DealerShows3(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "3";
        }

        private static bool DealerShows4(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "4";
        }

        private static bool DealerShows5(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "5";
        }

        private static bool DealerShows6(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "6";
        }

        private static bool DealerShows7(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "7";
        }

        private static bool DealerShows8(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "8";
        }

        private static bool DealerShows9(ProblemState stateData)
        {
            return stateData.DealerCard.Rank == "9";
        }

        private static bool DealerShowsT(ProblemState stateData)
        {
            var dealerCard = stateData.DealerCard;
            return dealerCard.Rank == "T" || dealerCard.Rank == "J" || dealerCard.Rank == "Q" || dealerCard.Rank == "K";
        }

        //-------------------------------------------------------------------------
        // Now terminal functions to get information about the player's hand
        //-------------------------------------------------------------------------

        private static bool HasAce(ProblemState stateData)
        {
            var card1 = stateData.Card1;
            var card2 = stateData.Card1;
            return card1.Rank == "A" || card2.Rank == "A";
        }

        private static bool HandVal4(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 4);
        }

        private static bool HandVal5(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 5);
        }

        private static bool HandVal6(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 6);
        }

        private static bool HandVal7(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 7);
        }

        private static bool HandVal8(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 8);
        }

        private static bool HandVal9(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 9);
        }

        private static bool HandVal10(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 10);
        }

        private static bool HandVal11(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 11);
        }

        private static bool HandVal12(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 12);
        }

        private static bool HandVal13(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 13);
        }

        private static bool HandVal14(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 14);
        }

        private static bool HandVal15(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 15);
        }

        private static bool HandVal16(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 16);
        }

        private static bool HandVal17(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 17);
        }

        private static bool HandVal18(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 18);
        }

        private static bool HandVal19(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 19);
        }

        private static bool HandVal20(ProblemState stateData)
        {
            return HandTotalEquals(stateData, 20);
        }

        private static bool HandTotalEquals(ProblemState stateData, int lookingFor)
        {
            var card1 = stateData.Card1;
            var card2 = stateData.Card1;
            // RankValueHigh and RankValueLow are the same for all cards except Ace:
            // "A": RankValueHigh = 13 RankValueLow = 1
            // "K": RankValueHigh = 10, RankValueLow = 10  (and so forth)
            bool hasit = (
                ((card1.RankValueHigh + card2.RankValueHigh) == lookingFor) ||
                ((card1.RankValueLow + card2.RankValueLow) == lookingFor));
            return hasit;
        }

        //-------------------------------------------------------------------------
        // then terminal functions to steer the strategy
        //-------------------------------------------------------------------------
        private static bool VoteHit(ProblemState stateData)
        {
            stateData.VotesForHit++;
            return true;
        }

        private static bool VoteStand(ProblemState stateData)
        {
            stateData.VotesForStand++;
            return true;
        }

        private static bool VoteDouble(ProblemState stateData)
        {
            stateData.VotesForDoubleDown++;
            return true;
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private static float EvaluateCandidate(CandidateSolution<bool, ProblemState> candidate)
        {
            var state = candidate.StateData;    // the engine allocates the state data, so we don't have to

            // loop over X hands
            // randomly deal cards to player + dealer and save in state data
            // get the decision
            // if not busted or won, try again
            // after standing, deal out cards to see who wins
            return 0;
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private static bool PerGenerationCallback(EngineProgress progress)
        {
            Debug.WriteLine("Generation " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0.0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0.0"));

            // return true to keep going, false to halt the system
            return true;
        }
    }
}

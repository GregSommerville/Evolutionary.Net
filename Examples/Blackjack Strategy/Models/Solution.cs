using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

            // add the normal functions that take parameters
            engine.AddFunction((a, b) => a && b, "And");
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a) => !a, "Not");
            engine.AddFunction((a, b, c) => a ? b : c, "If");         // if a, return b else c

            // terminal functions to look at game state
            engine.AddTerminalFunction(HasAce, "HasAnAce");
            engine.AddTerminalFunction(HandVal4, "Has4");
            engine.AddTerminalFunction(DealerShowsAce, "DealerAce");

            // terminal functions to indicate a strategy
            engine.AddTerminalFunction(VoteHit, "Hit");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            BestSolution = engine.FindBestSolution();
        }

        //-------------------------------------------------------------------------
        // our terminal functions fall into two categories: 
        // ones to get the game state, and ones to indicate which strategy to use.
        // first, the functions to get state:
        //-------------------------------------------------------------------------
        private static bool DealerShowsAce(ProblemState stateData)
        {
            var dealerCard = stateData.DealerCard;
            return dealerCard.Rank == "A";
        }

        private static bool HasAce(ProblemState stateData)
        {
            var card1 = stateData.Card1;
            var card2 = stateData.Card1;
            return card1.Rank == "A" || card2.Rank == "A";
        }

        private static bool HandVal4(ProblemState stateData)
        {
            var card1 = stateData.Card1;
            var card2 = stateData.Card1;
            bool hasit = (
                ((card1.RankValueHigh + card2.RankValueHigh) == 4) ||
                ((card1.RankValueLow + card2.RankValueLow) == 4));
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

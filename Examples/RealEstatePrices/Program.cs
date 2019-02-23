using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Evolutionary;

namespace RealEstatePrices
{
    class Program
    {
        static void Main(string[] args)
        {
            // create the engine with params set for this problem
            var x = new EngineParameters()
            {
                IsLowerFitnessBetter = true,
                PopulationSize = 500,
                MaxGenerations = 150,
                SelectionStyle = SelectionStyle.Tourney,
                TourneySize = 3
            };
            var e = new Engine<double, StateData>(x);

            // give the engine a fitness function and a per-gen callback (so we can see the progress)
            e.AddFitnessFunction((c) => EvaluateCandidate(c));
            e.AddProgressFunction((p,b) => PerGenerationCallback(p, b));

            // then add various functions, consts and vars for the regression

            // "fixed acidity";"volatile acidity";"citric acid";"residual sugar";"chlorides";
            // "free sulfur dioxide";"total sulfur dioxide";"density";"pH";"sulphates";"alcohol";"quality"

            // find the solution
        }

        private static bool PerGenerationCallback(EngineProgress p, object b)
        {
            throw new NotImplementedException();
        }

        private static float EvaluateCandidate(object c)
        {
            var sampleData = new Dataset();

            throw new NotImplementedException();
        }
    }
}

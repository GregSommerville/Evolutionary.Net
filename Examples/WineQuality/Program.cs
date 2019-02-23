using Evolutionary;
using System;
using System.Collections.Generic;

namespace WineQuality
{
    class Program
    {
        const string VarnameFixAcidity = "FixAcid";
        const string VarnameVolAcidity = "VolAcid";
        const string VarnameCitric = "CitAcid";
        const string VarnameResSug = "ResSug";
        const string VarnameChlor = "Chlor";
        const string VarnameFreeSO2 = "FreeSO2";
        const string VarnameTotSO2 = "TotSO2";
        const string VarnameDensity = "Dens";
        const string VarnamepH = "pH";
        const string VarnameSulfates = "Sulfates";
        const string VarnameAlc = "Alc";

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

            // one variable per value on a row of the training data 
            e.AddVariable(VarnameFixAcidity);
            e.AddVariable(VarnameVolAcidity);
            e.AddVariable(VarnameCitric);
            e.AddVariable(VarnameResSug);
            e.AddVariable(VarnameChlor);
            e.AddVariable(VarnameFreeSO2);
            e.AddVariable(VarnameTotSO2);
            e.AddVariable(VarnameDensity);
            e.AddVariable(VarnamepH);
            e.AddVariable(VarnameSulfates);
            e.AddVariable(VarnameAlc);

            // some basic arithmatic building blocks
            e.AddFunction((a, b) => a + b, "Add");                    // addition
            e.AddFunction((a, b) => a - b, "Sub");                    // subtraction
            e.AddFunction((a, b) => a * b, "Mult");                    // multiplication
            e.AddFunction((a, b) => (b == 0) ? 1 : a / b, "Div");     // safe division 
            e.AddFunction((a) => -1 * a, "Neg");                      // negate
            e.AddFunction((a, b) => Math.Min(a, b), "Min");
            e.AddFunction((a, b) => Math.Max(a, b), "Max");

            // -1 to 12 by 1 seems like a nice range of constants
            for (int i = -1; i < 13; i++)
                e.AddConstant(i);

            // find the solution
            var solution = e.FindBestSolution();

            // then test how well it does
            var finalScore = EvaluateCandidate(solution, false);

            Console.WriteLine();
            Console.WriteLine("Final score: " + finalScore.ToString("0.0000"));
            Console.WriteLine();
            Console.WriteLine("Hit any key to exit");
            Console.ReadKey();
        }

        private static bool PerGenerationCallback(EngineProgress p, object b)
        {
            Console.WriteLine("Gen " + p.GenerationNumber.ToString("000") + 
                " avg: " + p.AvgFitnessThisGen.ToString("0.0000") + 
                " t: " + p.TimeForGeneration.TotalSeconds);

            // write out HTML row showing progress?

            return true;    // keep going
        }

        private static float EvaluateCandidate(CandidateSolution<double, StateData> candidate, bool useTrainingData = true)
        {
            var sampleData = new Dataset();

            // run through our test data and see how close the answer the genetic program
            // comes up with is to the training data.  Lower fitness scores are better than bigger scores
            double totalDifference = 0;
            while (true)
            {
                List<double> row;
                if (useTrainingData)
                    row = sampleData.GetRowOfTrainingData();
                else
                    row = sampleData.GetRowOfTestingData();
                if (row == null) break;

                // populate vars
                candidate.SetVariableValue(VarnameFixAcidity, row[0]);
                candidate.SetVariableValue(VarnameVolAcidity, row[1]);
                candidate.SetVariableValue(VarnameCitric, row[2]);
                candidate.SetVariableValue(VarnameResSug, row[3]);
                candidate.SetVariableValue(VarnameChlor, row[4]);
                candidate.SetVariableValue(VarnameFreeSO2, row[5]);
                candidate.SetVariableValue(VarnameTotSO2, row[6]);
                candidate.SetVariableValue(VarnameDensity, row[7]);
                candidate.SetVariableValue(VarnamepH, row[8]);
                candidate.SetVariableValue(VarnameSulfates, row[9]);
                candidate.SetVariableValue(VarnameAlc, row[10]);
                var actualAnswer = row[11];

                // and evaluate
                var result = candidate.Evaluate();

                // now figure the difference between the calculated value and the training data
                var diff = Math.Abs(result - actualAnswer);
                totalDifference += diff;
            }

            // since the genetic engine doesn't stop as long as the fitness scores are improving,
            // speed things up by truncating the precision to 4 digits after the decimal
            totalDifference = Math.Truncate(totalDifference * 10000F) / 10000F;
            return (float)totalDifference;
        }
    }
}

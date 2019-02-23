using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        static string printableSettings = "";

        static void Main(string[] args)
        {
            // create the engine with params set for this problem
            var engineParams = new EngineParameters()
            {
                IsLowerFitnessBetter = true,
                PopulationSize = 500,
                MinGenerations = 100,
                MaxGenerations = 350,
                StagnantGenerationLimit = 50,
                SelectionStyle = SelectionStyle.Tourney,
                TourneySize = 3,
                ElitismRate = 0,
                CrossoverRate = 1,
                MutationRate = 0.0
            };

            // all settings in one column
            printableSettings =
                "P: " + engineParams.PopulationSize + " " +
                "G: " + engineParams.MinGenerations + " - " + engineParams.MaxGenerations + " " +
                "Stgn: " + engineParams.StagnantGenerationLimit + " " +
                "Sel: " + engineParams.SelectionStyle + " " +
                "Trny: " + engineParams.TourneySize + " " +
                "Mut: " + engineParams.MutationRate + " " +
                "Elit: " + engineParams.ElitismRate + " ";

            var e = new Engine<double, StateData>(engineParams);

            // give the engine a fitness function and a per-gen callback (so we can see the progress)
            e.AddFitnessFunction((c) => EvaluateCandidate(c));
            e.AddProgressFunction((p) => PerGenerationCallback(p));

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
            e.AddFunction((a) => -1 * a, "Neg");                      // -1 * a
            e.AddFunction((a) => (a > 0) ? 1 / a : 0, "Inv");         // 1 / a
            e.AddFunction((a) => Math.Abs(a), "Abs");
            e.AddFunction((a) => a * a, "Sqr");
            e.AddFunction((a) => a * a * a, "Cube");
            e.AddFunction((a, b) => (a < b) ? a : b, "Min");
            e.AddFunction((a, b) => (a > b) ? a : b, "Max");

            e.AddConstant(1);
            e.AddConstant(10);
            e.AddConstant(100);
            e.AddConstant(1000);

            // find the solution
            var solution = e.FindBestSolution();

            // then test how well it does
            var finalScore = EvaluateCandidate(solution, false);

            Console.WriteLine();
            Console.WriteLine("Final score: " + finalScore.ToString("0.0000"));
            Console.WriteLine();
            Console.WriteLine(solution.ToString());
            Console.WriteLine();
            Console.WriteLine("Hit any key to exit");
            Console.ReadKey();
        }

        private static bool PerGenerationCallback(EngineProgress p)
        {
            Console.WriteLine("Gen " + p.GenerationNumber.ToString("000") + 
                " avg: " + p.AvgFitnessThisGen.ToString("0.0000") +
                " best: " + p.BestFitnessThisGen.ToString("0.0000") +
                " t: " + p.TimeForGeneration.TotalSeconds.ToString("0.00"));

            // save stats: date, gen#, best-this-gen, avg-this-gen, settings
            var writer = File.AppendText("per-gen-stats.csv");
            writer.WriteLine(
                "\"" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\"," +
                p.GenerationNumber + "," +
                p.BestFitnessThisGen.ToString("0") + "," +
                p.AvgFitnessThisGen.ToString("0") + "," +
                printableSettings);
            writer.Close();

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
                var diff = result - actualAnswer;
                totalDifference += diff * diff;

                if (!useTrainingData)
                    Console.WriteLine("Ans: " + actualAnswer.ToString(" 0") + " AI: " + result.ToString("0.00"));
            }

            totalDifference = Math.Sqrt(totalDifference);
            Debug.Assert(!double.IsNaN(totalDifference));

            // fitness function returns a float, so make sure we're within the valid range
            if (totalDifference > float.MaxValue) totalDifference = float.MaxValue;
            if (totalDifference < float.MinValue) totalDifference = float.MinValue;
            return (float)totalDifference;
        }
    }
}

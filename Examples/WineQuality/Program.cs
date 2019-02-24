using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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

        static string printableParams = "";
        static double bestOverallScoreSoFar = double.MaxValue,
                      bestAverageScoreSoFar = double.MaxValue;

        static void Main(string[] args)
        {
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
                MutationRate = 0.0,
                RandomTreeMinDepth = 5,
                RandomTreeMaxDepth = 10
            };

            printableParams =
                "P: " + engineParams.PopulationSize + " " +
                "G: " + engineParams.MinGenerations + " - " + 
                        engineParams.MaxGenerations + " " +
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
            e.AddFunction((a) => (a != 0) ? 1 / a : 0, "Inv");         // 1 / a
            e.AddFunction((a) => Math.Abs(a), "Abs");
            e.AddFunction((a) => a * a, "Sqr");
            e.AddFunction((a) => a * a * a, "Cube");
            e.AddFunction((a, b) => (a < b) ? a : b, "Min");
            e.AddFunction((a, b) => (a > b) ? a : b, "Max");

            e.AddConstant(10);
            e.AddConstant(100);
            e.AddConstant(1000);

            // find the solution
            var solution = e.FindBestSolution();
            ShowFinalResults(solution);

            Console.WriteLine();
            Console.WriteLine("Hit any key to exit");
            Console.ReadKey();
        }

        static void ShowFinalResults(CandidateSolution<double, StateData> solution)
        {
            // then test how well it does
            double finalScore;
            string testDetails;
            RunFinalTests(solution, out finalScore, out testDetails);
            Console.WriteLine();
            Console.WriteLine("Final score: " + finalScore.ToString("0.0000"));
            Console.WriteLine();
            Console.WriteLine(solution.ToString());

            var writer = File.AppendText("test-results.csv");
            writer.Write(testDetails);
            writer.Close();
        }

        static bool PerGenerationCallback(EngineProgress p)
        {
            string bestSuffix = " ", avgSuffix = " ";
            if (p.BestFitnessSoFar < bestOverallScoreSoFar)
            {
                bestOverallScoreSoFar = p.BestFitnessSoFar;
                bestSuffix = "*";
            }
            if (p.AvgFitnessThisGen < bestAverageScoreSoFar)
            {
                bestAverageScoreSoFar = p.AvgFitnessThisGen;
                avgSuffix = "*";
            }

            Console.WriteLine("Gen " + p.GenerationNumber.ToString().PadLeft(3) + 
                " avg: " + p.AvgFitnessThisGen.ToString("0.0000").PadLeft(20) + avgSuffix + 
                " best: " + p.BestFitnessThisGen.ToString("0.0000").PadLeft(20) + bestSuffix +
                " t: " + p.TimeForGeneration.TotalSeconds.ToString("0.00"));

            // save stats: date, gen#, best-this-gen, avg-this-gen, settings
            var writer = File.AppendText("per-gen-stats.csv");
            writer.WriteLine(
                "\"" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\"," +
                p.GenerationNumber + "," +
                p.BestFitnessThisGen.ToString("0") + "," +
                p.AvgFitnessThisGen.ToString("0") + "," +
                printableParams);
            writer.Close();

            return true;    // keep going
        }

        static float EvaluateCandidate(CandidateSolution<double, StateData> candidate)
        {
            var sampleData = new Dataset();

            // run through our test data and see how close the answer the genetic program
            // comes up with is to the training data.  Lower fitness scores are better than bigger scores
            double totalDifference = 0;
            while (true)
            {
                var row = sampleData.GetRowOfTrainingData();
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
            }

            totalDifference = Math.Sqrt(totalDifference);
            Debug.Assert(!double.IsNaN(totalDifference));

            // fitness function returns a float, so make sure we're within the valid range
            if (totalDifference > float.MaxValue) totalDifference = float.MaxValue;
            if (totalDifference < float.MinValue) totalDifference = float.MinValue;
            return (float)totalDifference;
        }

        static void RunFinalTests(CandidateSolution<double, StateData> candidate, out double finalScore, out string testDetails)
        {
            var sampleData = new Dataset();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Test results at " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());

            // run through our test data and see how close the answer the genetic program
            // comes up with is to the training data.  Lower fitness scores are better than bigger scores
            double totalDifference = 0;
            while (true)
            {
                var row = sampleData.GetRowOfTestingData();
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

                // now figure the difference between the calculated value and the training data
                var result = candidate.Evaluate();
                var diff = result - actualAnswer;
                totalDifference += diff * diff;
                Console.WriteLine("Ans: " + actualAnswer.ToString(" 0") + " AI: " + result.ToString("0.00"));
                sb.AppendLine(actualAnswer.ToString("0.0") + ", " + result.ToString("0.000"));
            }

            totalDifference = Math.Sqrt(totalDifference);

            // fitness function returns a float, so make sure we're within the valid range
            if (totalDifference > float.MaxValue) totalDifference = float.MaxValue;
            if (totalDifference < float.MinValue) totalDifference = float.MinValue;
            finalScore = (float)totalDifference;

            sb.AppendLine("\"Final score\", " + finalScore.ToString("0.000"));
            testDetails = sb.ToString();
        }
    }
}

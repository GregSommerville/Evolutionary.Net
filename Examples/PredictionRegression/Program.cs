﻿using Evolutionary;
using System;
using System.IO;
using System.Text;

namespace PredictionRegression
{
    class Program
    {
        static string printableParams = "";
        static double bestOverallScoreSoFar = double.MaxValue,
                      bestAverageScoreSoFar = double.MaxValue;

        static void Main(string[] args)
        {
            var ignoreColumns = new int[] { 0, 1, 13, 14 };
            int labelColumnIndex = 15 - ignoreColumns.Length;
            Dataset.LoadFromFile("bike-sharing-by-day.csv", labelColumnIndex , ignoreColumns);

            var engineParams = new EngineParameters()
            {
                IsLowerFitnessBetter = true,
                PopulationSize = 750,
                MinGenerations = 25,
                MaxGenerations = 2500,
                StagnantGenerationLimit = 25,
                SelectionStyle = SelectionStyle.Tourney,
                TourneySize = 3,
                ElitismRate = 0,
                CrossoverRate = 1,
                MutationRate = 0.0,
                RandomTreeMinDepth = 1,
                RandomTreeMaxDepth = 3
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
            for (int i = 0; i < Dataset.NumColumns; i++)
            {
                if (i != Dataset.LabelColumnIndex)
                    e.AddVariable("v" + i);
            }

            // some basic arithmatic building blocks
            e.AddFunction((a, b) => a + b, "Add");                    // addition
            e.AddFunction((a, b) => a - b, "Sub");                    // subtraction
            e.AddFunction((a, b) => a * b, "Mult");                    // multiplication
            e.AddFunction((a, b) => (b == 0) ? 1 : a / b, "Div");     // safe division 
            e.AddFunction((a, b) => (a < b) ? a : b, "Min");
            e.AddFunction((a, b) => (a > b) ? a : b, "Max");
            e.AddFunction((a, b) => (a + b) / 2, "Avg");
            e.AddFunction((a, b, c) => (a + b + c) / 3, "Avg3");
            e.AddFunction((a) => -1 * a, "Neg");                      // -1 * a
            e.AddFunction((a) => (a != 0) ? 1 / a : 0, "Inv");         // 1 / a
            e.AddFunction((a) => Math.Abs(a), "Abs");
            e.AddFunction((a) => a * a, "Sqr");
            e.AddFunction((a) => a * a * a, "Cube");

            e.AddConstant(0);
            e.AddConstant(2);
            e.AddConstant(3);
            e.AddConstant(5);
            e.AddConstant(7);
            e.AddConstant(11);
            e.AddConstant(13);
            e.AddConstant(17);
            e.AddConstant(19);
            e.AddConstant(23);

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
                "  avg: " + FormatNumber(p.AvgFitnessThisGen) + avgSuffix + 
                "  best: " + FormatNumber(p.BestFitnessThisGen) + bestSuffix +
                "  t: " + p.TimeForGeneration.TotalSeconds.ToString("0.00"));

            // save stats: date, gen#, best-this-gen, avg-this-gen, settings
            var writer = File.AppendText("per-gen-stats.csv");
            writer.WriteLine(
                "\"" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\"," +
                p.GenerationNumber + "," +
                p.BestFitnessThisGen.ToString() + "," +
                p.AvgFitnessThisGen.ToString() + "," +
                printableParams);
            writer.Close();

            return true;    // keep going
        }

        static string FormatNumber(double d)
        {
            string result = d.ToString("0.0000").PadLeft(20);
            if (result.Length > 20)
                result = d.ToString("E").PadLeft(20);
            return result;
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

                // populate vars with values from row of the training data, and then get the calculated value
                for (int i = 0; i < Dataset.NumColumns; i++)
                    if (i != Dataset.LabelColumnIndex)
                        candidate.SetVariableValue("v" + i, row[i]);
                var result = candidate.Evaluate();

                // now figure the difference between the calculated value and the training data
                var actualAnswer = row[Dataset.LabelColumnIndex];
                var diff = result - actualAnswer;
                totalDifference += diff * diff;
            }

            // sqrt of the summed squared diffences
            totalDifference = Math.Sqrt(totalDifference);

            // fitness function returns a float
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

                // populate vars with values from row of the training data 
                for (int i = 0; i < Dataset.NumColumns; i++)
                    if (i != Dataset.LabelColumnIndex)
                        candidate.SetVariableValue("v" + i, row[i]);
                var result = candidate.Evaluate();

                // now figure the difference between the calculated value and the training data
                var actualAnswer = row[Dataset.LabelColumnIndex];
                var diff = result - actualAnswer;
                totalDifference += diff * diff;
                Console.WriteLine("Ans: " + actualAnswer.ToString(" 0") + " AI: " + result.ToString("0.00"));
                sb.AppendLine(actualAnswer.ToString("0.0") + ", " + result.ToString("0.000"));
            }

            totalDifference = Math.Sqrt(totalDifference);
            finalScore = (float)totalDifference;
            sb.AppendLine("\"Final score\", " + finalScore.ToString("0.000"));
            testDetails = sb.ToString();
        }
    }
}
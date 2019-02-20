using BlackjackStrategy.Models;
using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BlackjackStrategy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // This parameters object is bound to the UI, for editing
        public ProgramSettings ProgramConfiguration { get; set; } = new ProgramSettings();

        private List<string> progressMsg = new List<string>();
        private Stopwatch stopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            propGrid.ExpandAllProperties();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            gaResultTB.Text = "Creating solution, please wait...";
            stopwatch.Restart();

            SetButtonsEnabled(false);

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncFindAndDisplaySolution());
        }

        private void btnShowKnown_Click(object sender, RoutedEventArgs e)
        {
            var strategy = new HandCodedStrategy();
            strategy.LoadStandardStrategy();
            DisplayStrategyGrids(strategy, "Classic Baseline Blackjack Strategy");
            DisplayStatistics(strategy);
        }

        private void AsyncFindAndDisplaySolution()
        {
            // reset the progress messages
            progressMsg = new List<string>();

            // one overall solution
            var solutionFinder = new SolutionByUpcard();  // new SolutionSingle();
            solutionFinder.BuildProgram(ProgramConfiguration, PerGenerationCallback);
            var strategy = solutionFinder.GetStrategy();

            int numGens = solutionFinder.TotalGenerations;
            DisplayStrategyGrids(strategy, "Best from " + numGens + " generations", numGens);
            DisplayStatistics(strategy, numGens);

            SetButtonsEnabled(true);
        }

        private void PerGenerationCallback(string status, CandidateSolution<bool, ProblemState> bestThisGeneration)
        {
            var strategy = StrategyFactory.GetStrategyForGP(bestThisGeneration);
            DisplayStrategyGrids(strategy, "");
            DisplayCurrentStatus(status);
        }

        private void DisplayStrategyGrids(StrategyBase strategy, string caption, int generationNumber = 0)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                string imgFilename = "";
                if (ProgramConfiguration.TestSettings.SaveImagePerGeneration && (generationNumber > 0))
                    imgFilename = "gen" + generationNumber;

                StrategyView.ShowPlayableHands(strategy, canvas, imgFilename, caption);
            }),
            DispatcherPriority.Background);
        }

        private void DisplayCurrentStatus(string status)
        {
            progressMsg.Insert(0, status);
            string allStatuses = String.Join("\n", progressMsg);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                gaResultTB.Text = allStatuses;
            }),
            DispatcherPriority.Background);
        }


        private void SetButtonsEnabled(bool enable)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                btnSolve.IsEnabled = enable;
                btnShowKnown.IsEnabled = enable;
            }),
            DispatcherPriority.Background);
        }

        private void DisplayStatistics(StrategyBase strategy, int totalGenerations = 0)
        {
            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                stopwatch.Stop();

                // test it and display scores
                var tester = new StrategyTester(strategy, ProgramConfiguration.TestSettings);

                double average, stdDev, coeffVariation;
                tester.GetStatistics(out average, out stdDev, out coeffVariation);

                string scoreResults =
                    "\nAverage score: " + average.ToString("0") +
                    "\nStandard Deviation: " + stdDev.ToString("0") +
                    "\nCoeff. of Variation: " + coeffVariation.ToString("0.0000");

                string output = (totalGenerations > 0) ? "Solution found in " + totalGenerations + " generations\n" : "" + 
                    "Elapsed: " +
                        stopwatch.Elapsed.Hours + "h " +
                        stopwatch.Elapsed.Minutes + "m " +
                        stopwatch.Elapsed.Seconds + "s " +
                        "\n\nTest Results:" + scoreResults;

                gaResultTB.Text = output;
            }),
            DispatcherPriority.Background);
        }
    }
}
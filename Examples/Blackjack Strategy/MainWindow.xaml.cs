using BlackjackStrategy.Models;
using Evolutionary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace BlackjackStrategy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> progressSoFar = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            var populationSize = (int)populationSizeSlider.Value;
            var crossoverPercentage = (int)crossoverPctSlider.Value;
            var mutationPercentage = Convert.ToDouble(txtMutationPct.Text);
            var elitismPercentage = (int)elitismPercentageSlider.Value;
            var tourneySize = (int)tourneySizeSlider.Value;

            gaResultTB.Text = "Creating solution, please wait...";

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncCall(populationSize, crossoverPercentage, mutationPercentage, elitismPercentage, tourneySize));
        }

        private void AsyncCall(int populationSize, int crossoverPercentage, double mutationPercentage, int elitismPercentage, int tourneySize)
        {
            var solutionFinder = new Solution();
            solutionFinder.BuildProgram(populationSize, crossoverPercentage, mutationPercentage, elitismPercentage, tourneySize, DisplayCurrentStatus);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                string solution = solutionFinder.BestSolution.ToString();

                gaResultTB.Text = "Found solution:\n\n" + solution;
                Debug.WriteLine(solution);

                ShowPlayableHands(solutionFinder.BestSolution);
            }),
            DispatcherPriority.Background);
        }

        private void DisplayCurrentStatus(string status)
        {
            progressSoFar.Insert(0, status);
            string allStatuses = String.Join("\n", progressSoFar);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                gaResultTB.Text = "Working...\n" + allStatuses;
            }),
            DispatcherPriority.Background);
        }

        private void ShowPlayableHands(CandidateSolution<bool, ProblemState> best)
        {
            // clear the screen
            canvas.Children.Clear();
            Debug.WriteLine("FINAL SOLUTION");

            // display a grid for hands without an ace.  One column for each possible dealer upcard
            AddColorBox(Colors.White, "", 0, 0);
            int x = 1;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string rankNeeded = (upcardRank == 11) ? "A" : upcardRank.ToString();
                AddColorBox(Colors.White, rankNeeded, x, 0);
                int y = 1;

                for (int hardTotal = 20; hardTotal > 3; hardTotal--)
                {
                    // add a white box with the total
                    AddColorBox(Colors.White, hardTotal.ToString(), 0, y);

                    var deck = new MultiDeck(TestConditions.NumDecks);

                    // build dealer hand
                    Hand dealerHand = new Hand();
                    dealerHand.AddCard(new Card(rankNeeded, "S"));

                    // build player hand
                    Hand playerHand = new Hand();
                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;
                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    Debug.Assert(playerHand.HandValue() == hardTotal);

                    // get strategy and display
                    best.StateData.DealerHand = dealerHand;
                    best.StateData.PlayerHand = playerHand;
                    best.StateData.VotesForDoubleDown = 0;
                    best.StateData.VotesForHit = 0;
                    best.StateData.VotesForStand = 0;

                    best.Evaluate();    // get the decision
                    Solution.DebugDisplayStrategy(best, "Final");

                    string action = Solution.GetAction(best.StateData);

                    // Now draw the box
                    switch (action)
                    {
                        case "H":
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case "S":
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case "D":
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }
            
            // and another for hands with an ace
        }

        private void AddColorBox(Color color, string label, int x, int y)
        {
            const int columnWidth = 30, rowHeight = 30;
            int startX = (((int)canvas.ActualWidth / 2) - (11 * columnWidth)) / 2;
            int startY = ((int)canvas.ActualHeight - (17 * rowHeight)) / 2;

            // the element is a border
            var box = new Border();
            box.BorderBrush = Brushes.Black;
            box.BorderThickness = new System.Windows.Thickness(1);
            box.Background = new SolidColorBrush(color);
            box.Width = columnWidth;
            box.Height = rowHeight;

            // and a label as a child
            var itemText = new TextBlock();
            itemText.HorizontalAlignment = HorizontalAlignment.Center;
            itemText.VerticalAlignment = VerticalAlignment.Center;
            itemText.Text = label;
            box.Child = itemText;

            canvas.Children.Add(box);
            Canvas.SetTop(box, startY + y * rowHeight);
            Canvas.SetLeft(box, startX + x * columnWidth);
        }
    }
}
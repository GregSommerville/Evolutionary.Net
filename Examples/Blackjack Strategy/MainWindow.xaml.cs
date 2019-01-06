using BlackjackStrategy.Models;
using Evolutionary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private string currentRank = "";
        private Dictionary<string, CandidateSolution<bool, ProblemState>> solutionByUpcard = new Dictionary<string, CandidateSolution<bool, ProblemState>>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSolve_Click(object sender, RoutedEventArgs e)
        {
            var populationSize = (int)populationSizeSlider.Value;
            var crossoverPercentage = (int)crossoverPctSlider.Value;
            var mutationPercentage = Convert.ToDouble(txtMutationPct.Text);
            var ElitismPercentage = (int)ElitismSlider.Value;
            var tourneySize = (int)tourneySizeSlider.Value;

            gaResultTB.Text = "Creating solution, please wait...";

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncCall(populationSize, crossoverPercentage, mutationPercentage, ElitismPercentage, tourneySize));
        }

        private void AsyncCall(int populationSize, int crossoverPercentage, double mutationPercentage, int ElitismPercentage, int tourneySize)
        {
            //// create a solution for each upcard 
            //for (var upcard = 2; upcard < 12; upcard++)
            //{
            //    // set the dealer upcard to use
            //    currentRank = upcard.ToString();
            //    if (upcard == 11) currentRank = "A";
                
                // reset the progress messages
                progressSoFar = new List<string>();

                // find the solution for this dealer upcard
                var solutionFinder = new Solution();
                solutionFinder.BuildProgram(
                    populationSize, 
                    crossoverPercentage, 
                    mutationPercentage,
                    ElitismPercentage, 
                    tourneySize, 
                    DisplayCurrentStatus);

            //    // save the solution in memory
            //    solutionByUpcard[currentRank] = solutionFinder.BestSolution;

            //    // and on disk
            //    SaveSolutionToDisk(
            //        "upcard" + currentRank + "solution.txt", 
            //        solutionByUpcard[currentRank].ToString()
            //        );
            //}

            // then display the final results
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //StringBuilder sb = new StringBuilder();
                //sb.AppendLine("Scores by dealer upcard:");
                //for (var upcard = 2; upcard < 12; upcard++)
                //{
                //    // set the dealer upcard to use
                //    currentRank = upcard.ToString();
                //    if (upcard == 11) currentRank = "A";

                //    sb.AppendLine(currentRank + " score: " + solutionByUpcard[currentRank].Fitness.ToString());
                //}
                //gaResultTB.Text = sb.ToString();

                var best = solutionFinder.BestSolution;

                gaResultTB.Text = "Solution found.  Final Score: " + best.Fitness.ToString() + "\n" +
                    "Generations needed to find: " + solutionFinder.NumGenerationsNeeded;

                ShowPlayableHands(solutionFinder.BestSolution);
            }),
            DispatcherPriority.Background);
        }

        private void SaveSolutionToDisk(string fileName, string solution)
        {
            File.WriteAllText(fileName, solution);
        }

        private void DisplayCurrentStatus(string status)
        {
            progressSoFar.Insert(0, status);
            string allStatuses = String.Join("\n", progressSoFar);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                gaResultTB.Text = allStatuses;
            }),
            DispatcherPriority.Background);
        }

        private void ShowPlayableHands(CandidateSolution<bool, ProblemState> solution)
        {
            var strategy = new OverallStrategy(solution);

            // clear the screen
            canvas.Children.Clear();

            // display a grid for non-paired hands without an ace.  One column for each possible dealer upcard
            AddColorBox(Colors.White, "", 0, 0);
            int x = 1, y = 0;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = 1;

                // get the strategy
                //var best = solutionByUpcard[upcardRankName];

                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // add a white box with the total
                    AddColorBox(Colors.White, hardTotal.ToString(), 0, y);

                    // build player hand
                    Hand playerHand = new Hand();
                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;
                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // and another for hands with an ace
            // display a grid for hands without an ace.  One column for each possible dealer upcard
            const int leftColumnForAces = 12;
            AddColorBox(Colors.White, "", leftColumnForAces, 0);
            x = leftColumnForAces + 1;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = 1;

                //var best = solutionByUpcard[upcardRankName];
                //var strategy = new OverallStrategy(best);

                // we don't start with Ace, because that would be AA, which is handled in the pair zone
                // we also don't start with 10, since that's blackjack.  So 9 is our starting point
                for (int otherCard = 9; otherCard > 1; otherCard--)
                {
                    string otherCardRank = (otherCard == 11) ? "A" : otherCard.ToString();

                    // add a white box with the player hand: "A-x"
                    AddColorBox(Colors.White, "A-" + otherCardRank, leftColumnForAces, y);

                    // build player hand
                    Hand playerHand = new Hand();
                    // first card is an ace, second card is looped over
                    playerHand.AddCard(new Card("AH")); // ace of hearts
                    playerHand.AddCard(new Card(otherCardRank, "S"));

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }

            // finally, a grid for pairs
            int startY = y + 1;
            AddColorBox(Colors.White, "", leftColumnForAces, 0);
            x = leftColumnForAces + 1;
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                Card dealerUpcard = new Card(upcardRankName, "D");

                AddColorBox(Colors.White, upcardRankName, x, 0);
                y = startY;

                //var best = solutionByUpcard[upcardRankName];
                //var strategy = new OverallStrategy(best);

                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // add a white box with the player hand: "x-x"
                    AddColorBox(Colors.White, pairedCardRank + "-" + pairedCardRank, leftColumnForAces, y);

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // get strategy and display
                    var action = strategy.GetActionForHand(playerHand, dealerUpcard);
                    switch (action)
                    {
                        case ActionToTake.Hit:
                            AddColorBox(Colors.Green, "H", x, y);
                            break;

                        case ActionToTake.Stand:
                            AddColorBox(Colors.Red, "S", x, y);
                            break;

                        case ActionToTake.Double:
                            AddColorBox(Colors.Yellow, "D", x, y);
                            break;

                        case ActionToTake.Split:
                            AddColorBox(Colors.LightBlue, "P", x, y);
                            break;
                    }
                    y++;
                }
                x++;
            }
        }

        private void AddColorBox(Color color, string label, int x, int y)
        {
            // easy to do constants when the screen isn't meant to resize
            const int 
                columnWidth = 38, 
                rowHeight = 28,
                startX = 20,
                startY = 20;

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
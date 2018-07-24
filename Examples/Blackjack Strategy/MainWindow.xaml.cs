using BlackjackStrategy.Models;
using System;
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

            gaResultTB.Text = "Creating solution...";

            // Finding the solution takes a while, so kick off a thread for it
            Task.Factory.StartNew(() => AsyncCall(populationSize, crossoverPercentage, mutationPercentage, elitismPercentage, tourneySize));
        }

        private void AsyncCall(int populationSize, int crossoverPercentage, double mutationPercentage, int elitismPercentage, int tourneySize)
        {
            Models.Solution.BuildProgram(populationSize, crossoverPercentage, mutationPercentage, elitismPercentage, tourneySize);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                string solution = Models.Solution.BestSolution.ToString();

                gaResultTB.Text = "Found solution:\n\n" + solution;
                Debug.WriteLine(solution);

                ShowPlayableHands();
            }),
            DispatcherPriority.Background);
        }

        private void ShowPlayableHands()
        {
            const int columnWidth = 43, rowHeight = 43;
            int startX = ((int)canvas.ActualWidth - (13 * columnWidth)) / 2;
            int startY = ((int)canvas.ActualHeight - (13 * rowHeight)) / 2;

            // clear the screen
            canvas.Children.Clear();

            // get boolean array that indicates whether to play each hand
            //var playableCards = Solution.GetPlayableCards();

            // now display on screen
            for (int outerRank = (int)Card.Ranks.Ace; outerRank >= (int)Card.Ranks.Two; outerRank--)
                for (int innerRank = (int)Card.Ranks.Ace; innerRank >= (int)Card.Ranks.Two; innerRank--)
                {
                    int x = (int)Card.Ranks.Ace - outerRank;
                    int y = (int)Card.Ranks.Ace - innerRank;

                    // should I play it?
                    bool highlight = true; // playableCards[x, y];   

                    // name of hand?
                    string handName = "";
                    if (x == y)
                    {
                        // pair
                        handName = Card.RankText(outerRank) + Card.RankText(outerRank);
                    }
                    else if (x > y)
                    {
                        // suited
                        handName = Card.RankText(innerRank) + Card.RankText(outerRank) + "s";
                    }
                    else
                    {
                        // offsuit
                        handName = Card.RankText(outerRank) + Card.RankText(innerRank) + "o";
                    }

                    // drawing on canvas

                    // the element is a border
                    var box = new Border();
                    box.BorderBrush = Brushes.Black;
                    box.BorderThickness = new System.Windows.Thickness(1);
                    if (highlight)
                        box.Background = new SolidColorBrush(Colors.Yellow);
                    else
                        box.Background = new SolidColorBrush(Colors.White);                    
                    box.Width = columnWidth;
                    box.Height = rowHeight;

                    // and a label as a child
                    var itemText = new TextBlock();
                    itemText.HorizontalAlignment = HorizontalAlignment.Center;
                    itemText.VerticalAlignment = VerticalAlignment.Center;
                    if (highlight)
                        itemText.Inlines.Add(new Bold(new Run(handName)));
                    else
                        itemText.Text = handName;
                    box.Child = itemText;

                    canvas.Children.Add(box);
                    Canvas.SetTop(box, startY + y * rowHeight);
                    Canvas.SetLeft(box, startX + x * columnWidth);
                }
        }
    }
}

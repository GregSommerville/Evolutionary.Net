using Evolutionary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Threading;
using System;
using System.Data;
using System.Linq;

namespace BlackjackStrategy.Models
{
    enum ActionToTake {  Stand, Hit, Double, Split };
    class ActionWithVotes
    {
        public int NumVotes;
        public ActionToTake Action;
        public ActionWithVotes(int numVotes, ActionToTake action)
        {
            NumVotes = numVotes;
            Action = action;
        }
    }

    class Solution
    {
        // a place to store the best solution, once we find it
        public CandidateSolution<bool, ProblemState> BestSolution { get; set; }
        private Action<string> displayGenerationCallback;
        private string dealerUpcardRank;

        public void BuildProgram(
            int populationSize, 
            int crossoverPercentage, 
            double mutationPercentage, 
            int ElitismPercentage, 
            int tourneySize, 
            Action<string> currentStatusCallback,
            string dealerUpcardRankToUse)
        {
            displayGenerationCallback = currentStatusCallback;
            dealerUpcardRank = dealerUpcardRankToUse;

            // just some values, leave the rest as defaults
            var engineParams = new EngineParameters()
            {
                CrossoverRate = crossoverPercentage / 100F,
                ElitismRate = ElitismPercentage / 100F,
                IsLowerFitnessBetter = false,
                MutationRate = mutationPercentage / 100F,
                PopulationSize = populationSize,
                TourneySize = tourneySize
            };

            // create the engine.  each tree (and node within the tree) will return a bool.
            // we also indicate the type of our problem state data (used by terminal functions and stateful functions)
            var engine = new Engine<bool, ProblemState>(engineParams);

            // no constants for this problem
            
            // no variables for this solution - we can't pass in information about our hand 
            // via boolean variables, so we do it via some terminal functions instead

            // for a boolean tree, we use the standard operators
            engine.AddFunction((a, b) => a || b, "Or");
            engine.AddFunction((a, b, c) => a || b || c, "Or3");
            engine.AddFunction((a, b) => a && b, "And");
            engine.AddFunction((a, b, c) => a && b && c, "And3");
            engine.AddFunction((a) => !a, "Not");

            // then add functions to indicate a strategy
            engine.AddStatefulFunction(HitIf, "HitIf");
            engine.AddStatefulFunction(StandIf, "StandIf");
            engine.AddStatefulFunction(DoubleIf, "DoubleIf");
            engine.AddStatefulFunction(SplitIf, "SplitIf");

            //----------------------------------------------
            // terminal functions to look at game state
            //----------------------------------------------

            // first, player holding ace?
            engine.AddTerminalFunction(HasAce, "HasAce");

            // specific pairs
            engine.AddTerminalFunction(HasPairTwos, "HasPair2");
            engine.AddTerminalFunction(HasPairThrees, "HasPair3");
            engine.AddTerminalFunction(HasPairFours, "HasPair4");
            engine.AddTerminalFunction(HasPairFives, "HasPair5");
            engine.AddTerminalFunction(HasPairSixes, "HasPair6");
            engine.AddTerminalFunction(HasPairSevens, "HasPair7");
            engine.AddTerminalFunction(HasPairEights, "HasPair8");
            engine.AddTerminalFunction(HasPairNines, "HasPair9");
            engine.AddTerminalFunction(HasPairTens, "HasPairT");
            engine.AddTerminalFunction(HasPairAces, "HasPairA");

            // player hand totals
            engine.AddTerminalFunction(HandVal4, "Has4");
            engine.AddTerminalFunction(HandVal5, "Has5");
            engine.AddTerminalFunction(HandVal6, "Has6");
            engine.AddTerminalFunction(HandVal7, "Has7");
            engine.AddTerminalFunction(HandVal8, "Has8");
            engine.AddTerminalFunction(HandVal9, "Has9");
            engine.AddTerminalFunction(HandVal10, "Has10");
            engine.AddTerminalFunction(HandVal11, "Has11");
            engine.AddTerminalFunction(HandVal12, "Has12");
            engine.AddTerminalFunction(HandVal13, "Has13");
            engine.AddTerminalFunction(HandVal14, "Has14");
            engine.AddTerminalFunction(HandVal15, "Has15");
            engine.AddTerminalFunction(HandVal16, "Has16");
            engine.AddTerminalFunction(HandVal17, "Has17");
            engine.AddTerminalFunction(HandVal18, "Has18");
            engine.AddTerminalFunction(HandVal19, "Has19");
            engine.AddTerminalFunction(HandVal20, "Has20");

            // num cards held
            engine.AddTerminalFunction(Holding2Cards, "Hold2");
            engine.AddTerminalFunction(Holding3Cards, "Hold3");

            // pass a fitness evaluation function and run
            engine.AddFitnessFunction((t) => EvaluateCandidate(t));

            // and add something so we can track the progress
            engine.AddProgressFunction((t) => PerGenerationCallback(t));

            BestSolution = engine.FindBestSolution();
        }

        //-------------------------------------------------------------------------
        // Now terminal functions to get information about the player's hand
        //-------------------------------------------------------------------------

        private  bool HasAce(ProblemState stateData)
        {
            foreach (var card in stateData.PlayerHand.Cards)
                if (card.Rank == "A") return true;
            return false;
        }


        private bool HasPairTwos(ProblemState stateData)
        {
            string rankNeeded = "2";
            return 
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairThrees(ProblemState stateData)
        {
            string rankNeeded = "3";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairFours(ProblemState stateData)
        {
            string rankNeeded = "4";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairFives(ProblemState stateData)
        {
            string rankNeeded = "5";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairSixes(ProblemState stateData)
        {
            string rankNeeded = "6";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairSevens(ProblemState stateData)
        {
            string rankNeeded = "7";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairEights(ProblemState stateData)
        {
            string rankNeeded = "8";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairNines(ProblemState stateData)
        {
            string rankNeeded = "9";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        private bool HasPairTens(ProblemState stateData)
        {
            // covers tens, jacks, queens and kings
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].RankValueHigh == 10) &&
                (stateData.PlayerHand.Cards[1].RankValueHigh == 10);
        }

        private bool HasPairAces(ProblemState stateData)
        {
            string rankNeeded = "A";
            return
                (stateData.PlayerHand.Cards.Count == 2) &&
                (stateData.PlayerHand.Cards[0].Rank == rankNeeded) &&
                (stateData.PlayerHand.Cards[1].Rank == rankNeeded);
        }

        // all the ones relating to hand total value
        private bool HandVal4(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 4;
        }

        private  bool HandVal5(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 5;
        }

        private  bool HandVal6(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 6;
        }

        private  bool HandVal7(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 7;
        }

        private  bool HandVal8(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 8;
        }

        private  bool HandVal9(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 9;
        }

        private  bool HandVal10(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 10;
        }

        private  bool HandVal11(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 11;
        }

        private  bool HandVal12(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 12;
        }

        private  bool HandVal13(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 13;
        }

        private  bool HandVal14(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 14;
        }

        private  bool HandVal15(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 15;
        }

        private  bool HandVal16(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 16;
        }

        private  bool HandVal17(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 17;
        }

        private  bool HandVal18(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 18;
        }

        private  bool HandVal19(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 19;
        }

        private  bool HandVal20(ProblemState stateData)
        {
            return stateData.PlayerHand.HandValue() == 20;
        }


        // how many cards I've got
        private  bool Holding2Cards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count == 2;
        }

        private  bool Holding3Cards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count == 3;
        }

        private  bool Holding4PlusCards(ProblemState stateData)
        {
            return stateData.PlayerHand.Cards.Count >= 4;
        }

        //-------------------------------------------------------------------------
        // then functions to steer the strategy via storing to state
        //-------------------------------------------------------------------------
        private  bool HitIf(bool value, ProblemState state)
        {
            // pass through the value, but register the vote
            if (value) state.VotesForHit++;
            return value;
        }

        private  bool StandIf(bool value, ProblemState state)
        {
            // pass through the value, but register the vote
            if (value) state.VotesForStand++;
            return value;
        }

        private  bool DoubleIf(bool value, ProblemState state)
        {
            // double down only legal when holding 2 cards
            if (state.PlayerHand.Cards.Count == 2)
            {
                if (value) state.VotesForDoubleDown++;
            }
            return value;
        }

        private bool SplitIf(bool value, ProblemState state)
        {
            // only legal when holding a pair 
            if ((state.PlayerHand.Cards.Count == 2) &&
                (state.PlayerHand.Cards[0].Rank == state.PlayerHand.Cards[1].Rank))
            {
                if (value) state.VotesForSplit++;
            }
            return value;
        }

        //-------------------------------------------------------------------------
        // each candidate gets evaluated here
        //-------------------------------------------------------------------------
        private float EvaluateCandidate(CandidateSolution<bool, ProblemState> candidate)
        {
            OverallStrategy strategy = CreateStrategyForCandidate(candidate);
            var strategyTester = new StrategyTester(strategy);
            return strategyTester.GetStrategyScore(dealerUpcardRank);
        }

        private void SetupStateData(ProblemState stateData, Hand hand)
        {
            // prepare for testing
            stateData.PlayerHands.Clear();
            stateData.PlayerHands.Add(hand);
            stateData.VotesForDoubleDown = 0;
            stateData.VotesForHit = 0;
            stateData.VotesForStand = 0;
        }

        private OverallStrategy CreateStrategyForCandidate(CandidateSolution<bool, ProblemState> candidate)
        {
            var result = new OverallStrategy();

            // cycle over all of the possible upcards
            for (int upcardRank = 2; upcardRank < 12; upcardRank++)
            {
                // do pairs
                string upcardRankName = (upcardRank == 11) ? "A" : upcardRank.ToString();
                for (int pairedCard = 11; pairedCard > 1; pairedCard--)
                {
                    string pairedCardRank = (pairedCard == 11) ? "A" : pairedCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    playerHand.AddCard(new Card(pairedCardRank, "H")); // X of hearts
                    playerHand.AddCard(new Card(pairedCardRank, "S")); // X of spades

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();    

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    result.AddPairStrategy(pairedCardRank, dealerUpcardRank, action);
                }

                // then soft hands
                // we don't start with Ace, because that would be AA, which is handled in the pair zone
                // we also don't start with 10, since that's blackjack.  So 9 is our starting point
                for (int otherCard = 9; otherCard > 1; otherCard--)
                {
                    string otherCardRank = (otherCard == 11) ? "A" : otherCard.ToString();

                    // build player hand
                    Hand playerHand = new Hand();
                    // first card is an ace, second card is looped over
                    playerHand.AddCard(new Card("AH")); // ace of hearts
                    playerHand.AddCard(new Card(otherCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    result.AddSoftStrategy(otherCardRank, dealerUpcardRank, action);
                }
                
                // hard hands
                for (int hardTotal = 20; hardTotal > 4; hardTotal--)
                {
                    // build player hand
                    Hand playerHand = new Hand();
                    // divide by 2 if it's even, else add one and divide by two
                    int firstCardRank = ((hardTotal % 2) != 0) ? (hardTotal + 1) / 2 : hardTotal / 2;
                    int secondCardRank = hardTotal - firstCardRank;
                    playerHand.AddCard(new Card(firstCardRank, "D"));
                    playerHand.AddCard(new Card(secondCardRank, "S"));

                    // find strategy 
                    SetupStateData(candidate.StateData, playerHand);
                    candidate.Evaluate();

                    // get the decision and store in the strategy object
                    var action = GetAction(candidate.StateData);
                    result.AddHardStrategy(hardTotal, dealerUpcardRank, action);
                }
            }

            return result;
        }


        public static void DebugDisplayStrategy(CandidateSolution<bool, ProblemState> candidate, string prefixText)
        {
            string debug = 
                prefixText + 
                (String.IsNullOrWhiteSpace(prefixText) ? "" : "  ") +
                "  Plr: " + candidate.StateData.PlayerHand + 
                "  H: " + candidate.StateData.VotesForHit +
                "  S: " + candidate.StateData.VotesForStand +
                "  P: " + candidate.StateData.VotesForSplit + 
                "  D: " + candidate.StateData.VotesForDoubleDown;
            Debug.WriteLine(debug);
        }

        //-------------------------------------------------------------------------
        // For each generation, we get information about what's going on
        //-------------------------------------------------------------------------
        private bool PerGenerationCallback(EngineProgress progress)
        {
            string summary = "Generation " + progress.GenerationNumber +
                " best: " + progress.BestFitnessThisGen.ToString("0") +
                " avg: " + progress.AvgFitnessThisGen.ToString("0");

            displayGenerationCallback(summary);
            Debug.WriteLine(summary);

            // return true to keep going, false to halt the system
            bool keepRunning = true;
            return keepRunning;
        }

        public static ActionToTake GetAction(ProblemState stateData)
        {
            int votesForStand = stateData.VotesForStand;
            int votesForHit = stateData.VotesForHit;
            int votesForDouble = stateData.VotesForDoubleDown;
            int votesForSplit = stateData.VotesForSplit;

            List<ActionWithVotes> votes = new List<ActionWithVotes>();
            votes.Add(new ActionWithVotes(votesForDouble, ActionToTake.Double));
            votes.Add(new ActionWithVotes(votesForStand, ActionToTake.Stand));
            votes.Add(new ActionWithVotes(votesForHit, ActionToTake.Hit));
            votes.Add(new ActionWithVotes(votesForSplit, ActionToTake.Split));

            return votes.OrderByDescending(v => v.NumVotes).First().Action;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Evolutionary
{
    public class Engine<T>
    {
        // default settings 
        private int TourneySize = 4;
        private int NoChangeGenerationCountForTermination = 12;
        private int ElitismPercentageOfPopulation = 10;
        private int PopulationSize = 200;
        private bool IsLowerFitnessBetter = true;
        private double CrossoverRate = 0.95;
        private double MutationRate = 0.01;

        // object to store all available node contents (functions, constants, variables)
        private EngineComponents<T> operatorsAvailable = new EngineComponents<T>();

        // declares the delegate type for the fitness function, and defines a pointer to our fitness function
        private delegate float FitnessFunctionPointer(CandidateSolution<T> tree);
        FitnessFunctionPointer myFitnessFunction;

        private List<CandidateSolution<T>> currentGeneration = new List<CandidateSolution<T>>();

        public Engine()
        {
        }

        public Engine(EngineParameters engineParams)
        {
            TourneySize = engineParams.TourneySize;
            NoChangeGenerationCountForTermination = engineParams.NoChangeGenerationCountForTermination;
            ElitismPercentageOfPopulation = engineParams.ElitismPercentageOfPopulation;
            PopulationSize = engineParams.PopulationSize;
            IsLowerFitnessBetter = engineParams.IsLowerFitnessBetter;
            CrossoverRate = engineParams.CrossoverRate;
            MutationRate = engineParams.MutationRate;
        }
    
        public CandidateSolution<T> FindBestSolution()
        {
            float bestFitnessScoreAllTime = (IsLowerFitnessBetter ? float.MaxValue : float.MinValue);
            CandidateSolution<T> bestTreeAllTime = null;
            int bestSolutionGenerationNumber = 0;

            // create an initial population of random trees, passing the possible functions, consts, and variables
            for (int p = 0; p < PopulationSize; p++)
            {
                CandidateSolution<T> tree = new CandidateSolution<T>(operatorsAvailable);
                tree.CreateRandom();
                currentGeneration.Add(tree);
            }

            // loop over generations
            int currentGenerationNumber = 0;
            while (true)
            {
                // for each tree, find and store the fitness score
                float bestFitnessScoreThisGeneration = (IsLowerFitnessBetter ? float.MaxValue : float.MinValue);
                CandidateSolution<T> bestTreeThisGeneration = null;
                float totalFitness = 0;
                foreach (var tree in currentGeneration)
                {
                    // calc the fitness by calling the user-supplied function via the delegate   
                    tree.Fitness = myFitnessFunction(tree);
                    totalFitness += tree.Fitness;

                    // find best of this generation, update best all-time if needed
                    bool isBestThisGeneration = false;
                    if (IsLowerFitnessBetter)
                        isBestThisGeneration = tree.Fitness < bestFitnessScoreThisGeneration;
                    else
                        isBestThisGeneration = tree.Fitness > bestFitnessScoreThisGeneration;

                    if (isBestThisGeneration)
                    {
                        bestFitnessScoreThisGeneration = tree.Fitness;
                        bestTreeThisGeneration = tree;

                        bool isBestEver = false;
                        if (IsLowerFitnessBetter)
                            isBestEver = bestFitnessScoreThisGeneration < bestFitnessScoreAllTime;
                        else
                            isBestEver = bestFitnessScoreThisGeneration > bestFitnessScoreAllTime;

                        if (isBestEver)
                        {
                            bestFitnessScoreAllTime = bestFitnessScoreThisGeneration;
                            bestTreeAllTime = tree.Clone();
                            bestSolutionGenerationNumber = currentGenerationNumber;
                        }
                    }
                }

                float averageFitness = totalFitness / PopulationSize;
                Debug.WriteLine("Generation " + currentGenerationNumber + 
                    " best: " + bestFitnessScoreThisGeneration.ToString("0.0") +
                    " avg: " + averageFitness.ToString("0.0"));

                // exit the loop if we're not making any progress
                if ((currentGenerationNumber - bestSolutionGenerationNumber) >= NoChangeGenerationCountForTermination)
                    break;

                List<CandidateSolution<T>> nextGeneration = new List<CandidateSolution<T>>();

                // Elitism
                int numElitesToAdd = (ElitismPercentageOfPopulation * PopulationSize) / 100;
                var theBest = currentGeneration.OrderBy(c => c.Fitness).Take(numElitesToAdd);
                foreach (var peakPerformer in theBest)
                {
                    nextGeneration.Add(peakPerformer);
                }

                // now create a new generation using fitness scores for selection, and crossover and mutation
                while (nextGeneration.Count < PopulationSize)
                {
                    // select parents
                    var parent1 = TournamentSelectParent();
                    var parent2 = TournamentSelectParent();

                    // cross them over to generate two new children
                    CandidateSolution<T> child1, child2;
                    CrossOverParents(parent1, parent2, out child1, out child2);

                    // Mutation
                    if (Randomizer.GetFloatFromZeroToOne() < MutationRate)
                        child1.Mutate();
                    if (Randomizer.GetFloatFromZeroToOne() < MutationRate)
                        child2.Mutate();

                    // then add to the new generation 
                    nextGeneration.Add(child1);
                    nextGeneration.Add(child2);
                }

                // move to the next generation
                currentGeneration = nextGeneration;
                currentGenerationNumber++;
            }

            return bestTreeAllTime;
        }

        private void CrossOverParents(CandidateSolution<T> parent1, CandidateSolution<T> parent2, out CandidateSolution<T> child1, out CandidateSolution<T> child2)
        {
            // are we crossing over, or just copying a parent directly?
            child1 = parent1.Clone();
            child2 = parent2.Clone();

            // Do we use exact copies of parents, or crossover?
            if (Randomizer.GetDoubleFromZeroToOne() < CrossoverRate)
            {
                NodeBaseType<T> randomC1node, randomC2node;
                // pick a random node from both parents and clone the tree below it.
                // We don't want root nodes, since that's not really crossover, so we disallow those
                do
                {
                    randomC1node = child1.SelectRandomNode();
                } while (randomC1node == child1.Root);
                do
                {
                    randomC2node = child2.SelectRandomNode();
                } while (randomC2node == child2.Root);

                // make copies of them 
                var newChild1 = randomC1node.Clone(null);
                var newChild2 = randomC2node.Clone(null);

                // create new children by swapping subtrees
                CandidateSolution<T>.SwapSubtrees(randomC1node, randomC2node, newChild1, newChild2);
            }
        }

        private CandidateSolution<T> TournamentSelectParent()
        {
            CandidateSolution<T> result = null;
            float bestFitness = float.MaxValue;
            if (IsLowerFitnessBetter == false)
                bestFitness = float.MinValue;

            for(int i = 0; i < TourneySize; i++)
            {
                int index = Randomizer.IntLessThan(PopulationSize);
                var randomTree = currentGeneration[index];
                var fitness = randomTree.Fitness;

                // each tree's fitness is driven by how long it takes to dock the rocket, so lower is better
                bool isFitnessBetter = false;
                if (IsLowerFitnessBetter)
                    isFitnessBetter = fitness < bestFitness;
                else
                    isFitnessBetter = fitness > bestFitness;
                if (isFitnessBetter)
                {
                    result = randomTree;
                    bestFitness = fitness;
                }
            }
            return result;
        }

        public void AddConstant(T value)
        {
            operatorsAvailable.Constants.Add(value);
        }

        public void AddVariable(string variableName)
        {
            operatorsAvailable.Variables[variableName] = new VariableMetadata<T>();
            operatorsAvailable.Variables[variableName].Name = variableName;
        }

        public void SetVariableValue(string varName, T varVal)
        {
            operatorsAvailable.Variables[varName].Value = varVal;
        }

        // zero parameter functions are a type of terminal, so they are stored separate from the other functions
        public void AddFunction(Func<T> function, string functionName)
        {
            operatorsAvailable.TerminalFunctions.Add(new FunctionMetaData<T>(function, 0, functionName));
        }

        // one parameter functions
        public void AddFunction(Func<T, T> function, string functionName)
        {
            operatorsAvailable.Functions.Add(new FunctionMetaData<T>(function, 1, functionName));
        }

        // two parameter functions
        public void AddFunction (Func<T, T, T> function, string functionName)
        {
            operatorsAvailable.Functions.Add(new FunctionMetaData<T>(function, 2, functionName));
        }

        // three parameter functions
        public void AddFunction(Func<T, T, T, T> function, string functionName)
        {
            operatorsAvailable.Functions.Add(new FunctionMetaData<T>(function, 3, functionName));
        }

        // And a reference to the fitness function
        public void AddFitnessFunction(Func<CandidateSolution<T>,float> fitnessFunction)
        {
            myFitnessFunction = new FitnessFunctionPointer(fitnessFunction);
        }
    }
}

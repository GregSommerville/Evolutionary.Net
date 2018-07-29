using System.Linq;
using System.Collections.Generic;

namespace Evolutionary
{
    public class CandidateSolution<T, S> where S : new()  // S (the state datatype) must have a parameterless constructor
    {
        private EngineComponents<T> primitiveSet;

        internal NodeBaseType<T, S> Root { get; set; }
        public int TreeMinDepth { get; }
        public int TreeMaxDepth { get; }
        public float Fitness { get; set; }
        public S StateData { get; }    // used by terminal functions
        public Dictionary<string, T> Variables { get; }

        internal CandidateSolution(EngineComponents<T> availableComponents, int minDepth, int maxDepth)
        {
            TreeMinDepth = minDepth;
            TreeMaxDepth = maxDepth;
            StateData = new S();
            Variables = new Dictionary<string, T>();

            // keep a reference to all of the functions, constants and variables
            primitiveSet = availableComponents;
        }

        public void CreateRandom()
        {
            // ramped half-and-half approach:
            // do grow or full approach, evenly split
            // full: choose function nodes for all levels except the maximum, which is always terminal
            // grow: randomly choose function or terminal, except for max depth which is always terminal
            bool buildFullTrees = Randomizer.GetFloatFromZeroToOne() < 0.5;
            int depth = Randomizer.IntBetween(TreeMinDepth, TreeMaxDepth);

            Root = AddRandomNode(null, depth, buildFullTrees);
        }

        internal NodeBaseType<T, S> AddRandomNode(NodeBaseType<T, S> parentNode, int levelsToCreate, bool buildFullTrees)
        {
            // we're either adding a terminal node (const or var), or a function node (which has children)
            bool addingTerminal = false;

            // if we're at the max depth, add a terminal node
            if (levelsToCreate == 0)
                addingTerminal = true;
            else
                if (levelsToCreate >= TreeMinDepth)
                    // make sure we're at least a certain depth
                    addingTerminal = false;
                else
                {
                    // we're somewhere between the min and max depth 
                    if (buildFullTrees)
                        addingTerminal = false;
                    else
                    {
                        // pick a random type, either function or terminal
                        int numFuncs = primitiveSet.Functions.Count;
                        int numTerminalFuncs = primitiveSet.TerminalFunctions.Count;
                        int numVars = primitiveSet.VariableNames.Count;
                        int numConsts = primitiveSet.Constants.Count;

                        int random = Randomizer.IntLessThan(numFuncs + numTerminalFuncs + numVars + numConsts);
                        if (random < numFuncs)
                            addingTerminal = false;
                        else
                            addingTerminal = true;
                    }
                }

            // Now that we know whether to add a function or terminal node, go ahead and do it
            if (addingTerminal)
            {
                var termNode = NewRandomTerminalNode();
                termNode.Parent = parentNode;
                return termNode;
            }
            else
            {
                var funcNode = NewRandomFunctionNode();
                funcNode.Parent = parentNode;

                int numKids = funcNode.NumArguments;
                for (int i = 0; i < numKids; i++)
                {
                    // recursively create descendent nodes
                    funcNode.Children.Add(AddRandomNode(funcNode, levelsToCreate - 1, buildFullTrees));
                }

                return funcNode;
            }
        }

        internal FunctionNode<T, S> NewRandomFunctionNode()
        {
            // create a function node, leaving the Children unpopulated
            int random = Randomizer.IntLessThan(primitiveSet.Functions.Count);
            return new FunctionNode<T, S>(primitiveSet.Functions[random]);
        }

        internal TerminalNode<T, S> NewRandomTerminalNode()
        {
            int numConsts = primitiveSet.Constants.Count;
            int numVars = primitiveSet.VariableNames.Count;
            int numTerminalFunctions = primitiveSet.TerminalFunctions.Count;
            int random = Randomizer.IntLessThan(numConsts + numVars + numTerminalFunctions);

            // create a new constant node, simply passing the value
            if (random < numConsts)
                return new ConstantNode<T, S>(primitiveSet.Constants[random]);

            // create a new variable node, passing a reference to a GPVariable<T> object
            int varIndex = random - numConsts;
            if (varIndex < numVars)
            {
                var selectedVarName = primitiveSet.VariableNames[varIndex];
                var newNode = new VariableNode<T, S>(selectedVarName, this);
                return newNode;
            }

            // terminal function node 
            int termFunctionIndex = random - (numVars + numConsts);
            var selectedTerminalFunction = primitiveSet.TerminalFunctions.ToArray()[termFunctionIndex];
            return new TerminalFunctionNode<T,S>(selectedTerminalFunction, this); // "this" is a reference to the candidate solution that owns the tree
        }
        public void SetVariableValue(string varName, T varValue)
        {
            // Called from the fitness function, this allows the user to set variable values
            Variables[varName] = varValue;
        }

        internal void Mutate()
        {
            // pick any node but the root (since that would be a replacement of the entire tree, not a mutation)
            NodeBaseType<T, S> randomNode = null;
            do
            {
                randomNode = SelectRandomNode();
            } while (randomNode == Root);

            // build a random subtree
            bool buildFullTrees = Randomizer.GetFloatFromZeroToOne() < 0.5F;
            int depth = Randomizer.IntBetween(TreeMinDepth, TreeMaxDepth);
            var newSubtree = AddRandomNode(null, depth, buildFullTrees);

            // replace the randomly selected node with the new random one 
            var parent = (FunctionNode<T, S>)randomNode.Parent;
            newSubtree.Parent = parent;
            // find the link from the parent to the old child, and replace it 
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] == randomNode)
                {
                    parent.Children[i] = newSubtree;
                    break;
                }
            }
        }

        internal NodeBaseType<T, S> SelectRandomNode()
        {
            // do one complete pass through the entire tree to randomly select
            int numNodesSeen = 1;
            return Root.SelectRandom(ref numNodesSeen);    
        }

        internal static void SwapSubtrees(
            CandidateSolution<T, S> candidate1, CandidateSolution<T, S> candidate2,
            NodeBaseType<T, S> selectedNode1, NodeBaseType<T, S> selectedNode2, 
            NodeBaseType<T, S> replacementNode1, NodeBaseType<T, S> replacementNode2)
        {
            // find the child we're going to replace
            var parent = (FunctionNode<T, S>)selectedNode1.Parent;
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] == selectedNode1)
                {
                    parent.Children[i] = replacementNode2;
                    replacementNode2.Parent = parent;
                    selectedNode1.Parent = null;
                    break;
                }
            }

            // find the child we're going to replace
            parent = (FunctionNode<T, S>)selectedNode2.Parent;
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] == selectedNode2)
                {
                    parent.Children[i] = replacementNode1;
                    replacementNode1.Parent = parent;
                    selectedNode2.Parent = null;
                    break;
                }
            }

            // now traverse down the new subtrees and point any variable or terminal function nodes to the candidate
            replacementNode1.SetCandidateRef(candidate2);
            replacementNode2.SetCandidateRef(candidate1);
        }

        public CandidateSolution<T,S> Clone()
        {
            // clone the tree object
            var newTree = new CandidateSolution<T,S>(primitiveSet, TreeMinDepth, TreeMaxDepth);

            // and all of the nodes in the tree
            newTree.Root = Root.Clone(null);
            return newTree;
        }

        public T Evaluate()
        {
            return Root.Evaluate();
        }

        public override string ToString()
        {
            return Root.ToString();
        }
    }
}

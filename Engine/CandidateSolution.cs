using System.Linq;

namespace Evolutionary
{
    public class CandidateSolution<T>
    {
        private EngineComponents<T> availableComponents;

        internal NodeBaseType<T> Root { get; set; }
        public int TreeMinDepth { get; }
        public int TreeMaxDepth { get; }
        public float Fitness { get; set; }

        internal CandidateSolution(EngineComponents<T> availableComponents)
        {
            TreeMinDepth = 3;
            TreeMaxDepth = 6;

            // keep a reference to all of the functions, constants and variables
            this.availableComponents = availableComponents;
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

        internal NodeBaseType<T> AddRandomNode(NodeBaseType<T> parentNode, int levelsToCreate, bool buildFullTrees)
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
                        int numFuncs = availableComponents.Functions.Count;
                        int numTerminalFuncs = availableComponents.TerminalFunctions.Count;
                        int numVars = availableComponents.Variables.Count;
                        int numConsts = availableComponents.Constants.Count;

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

        internal FunctionNode<T> NewRandomFunctionNode()
        {
            // create a function node, leaving the Children unpopulated
            int random = Randomizer.IntLessThan(availableComponents.Functions.Count);
            return new FunctionNode<T>(availableComponents.Functions[random]);
        }

        internal TerminalNode<T> NewRandomTerminalNode()
        {
            int numConsts = availableComponents.Constants.Count;
            int numVars = availableComponents.Variables.Count;
            int numTerminalFunctions = availableComponents.TerminalFunctions.Count;
            int random = Randomizer.IntLessThan(numConsts + numVars + numTerminalFunctions);

            // create a new constant node, simply passing the value
            if (random < numConsts)
                return new ConstantNode<T>(availableComponents.Constants[random]);

            // create a new variable node, passing a reference to a GPVariable<T> object
            int varIndex = random - numConsts;
            if (varIndex < numVars)
            {
                var selectedVariable = availableComponents.Variables.ToArray()[varIndex].Value;
                var newNode = new VariableNode<T>(selectedVariable);
                return newNode;
            }

            // terminal function node 
            int termFunctionIndex = random - (numVars + numConsts);
            var selectedTerminalFunction = availableComponents.TerminalFunctions.ToArray()[termFunctionIndex];
            return new TerminalFunctionNode<T>(selectedTerminalFunction);
            
        }
        public void SetVariableValue(string varName, T varValue)
        {
            // Each tree is passed to a user-written fitness function, and
            // that function will need to be able to set variable values.  
            // Even though the variables aren't stored in the tree, it's reasonable 
            // for a user to set values via the tree.  We used the stored reference to 
            // the engine's component list
            availableComponents.Variables[varName].Value = varValue;
        }

        internal void Mutate()
        {
            // pick any node but the root (since that would be a replacement of the entire tree, not a mutation)
            NodeBaseType<T> randomNode = null;
            do
            {
                randomNode = SelectRandomNode();
            } while (randomNode == Root);

            // build a random subtree
            bool buildFullTrees = Randomizer.GetFloatFromZeroToOne() < 0.5F;
            int depth = Randomizer.IntBetween(TreeMinDepth, TreeMaxDepth);
            var newSubtree = AddRandomNode(null, depth, buildFullTrees);

            // replace the randomly selected node with the new random one 
            var parent = (FunctionNode<T>)randomNode.Parent;
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

        internal NodeBaseType<T> SelectRandomNode()
        {
            // do one complete pass through the entire tree to randomly select
            int numNodesSeen = 1;
            return Root.SelectRandom(ref numNodesSeen);    
        }

        internal static void SwapSubtrees(NodeBaseType<T> selectedNode1, NodeBaseType<T> selectedNode2, NodeBaseType<T> replacementNode1, NodeBaseType<T> replacementNode2)
        {
            // find the child we're going to replace
            var parent = (FunctionNode<T>)selectedNode1.Parent;
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
            parent = (FunctionNode<T>)selectedNode2.Parent;
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
        }

        public CandidateSolution<T> Clone()
        {
            // clone the tree object
            var newTree = new CandidateSolution<T>(availableComponents);

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

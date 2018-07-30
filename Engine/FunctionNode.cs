using System;
using System.Collections.Generic;

namespace Evolutionary
{
    /// <summary>
    /// Contains all functions
    /// </summary>
    class FunctionNode<T, S> : NodeBaseType<T, S> where S : new()
    {
        // a function node always has N children, where N depends on the function
        public List<NodeBaseType<T, S>> Children { get; set; }
        private FunctionMetaData<T> functionMetadata = null;
        private CandidateSolution<T, S> ownerCandidate;

        public FunctionNode(FunctionMetaData<T> function, CandidateSolution<T, S> candidate)
        {
            functionMetadata = function;
            Children = new List<NodeBaseType<T, S>>(function.NumArguments);
            ownerCandidate = candidate;
        }

        public override T Evaluate()
        {
            if (functionMetadata.UsesState == false)
                switch(functionMetadata.NumArguments)
                {
                    case 1:
                        return ((Func<T, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate());
                    case 2:
                        return ((Func<T, T, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate());
                    case 3:
                        return ((Func<T, T, T, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate(), 
                            Children[2].Evaluate());
                    case 4:
                        return ((Func<T, T, T, T, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate(), 
                            Children[2].Evaluate(), 
                            Children[3].Evaluate());
                }
            else
                switch (functionMetadata.NumArguments)
                {
                    case 1:
                        return ((Func<T, S, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            ownerCandidate.StateData);
                    case 2:
                        return ((Func<T, T, S, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate(), 
                            ownerCandidate.StateData);
                    case 3:
                        return ((Func<T, T, T, S, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate(), 
                            Children[2].Evaluate(),
                            ownerCandidate.StateData);
                    case 4:
                        return ((Func<T, T, T, T, S, T>)functionMetadata.FunctionPtr)(
                            Children[0].Evaluate(), 
                            Children[1].Evaluate(), 
                            Children[2].Evaluate(), 
                            Children[3].Evaluate(),
                            ownerCandidate.StateData);
                }

            throw new Exception("Evaluate: Invalid number of arguments in a function node");
        }

        public override NodeBaseType<T, S> Clone(NodeBaseType<T, S> parentNode)
        {
            // clone the node
            var newNode = new FunctionNode<T, S>(functionMetadata, ownerCandidate);
            newNode.Parent = parentNode;

            // clone the children
            foreach (var child in Children)
                newNode.Children.Add(child.Clone(newNode));
            return newNode;
        }

        public override NodeBaseType<T, S> SelectRandom(ref int numNodesConsidered)
        {
            float selectPercentage = 1F / numNodesConsidered;

            // possibly pick myself
            NodeBaseType<T, S> selected = null;
            if (Randomizer.GetFloatFromZeroToOne() < selectPercentage)
                selected = this;

            // but also consider all of my children
            foreach (var child in Children)
            {
                numNodesConsidered++;
                var selectedFromChild = child.SelectRandom(ref numNodesConsidered);
                if (selectedFromChild != null)
                    selected = selectedFromChild;
            }

            return selected;
        }

        public override string ToString()
        {
            switch (functionMetadata.NumArguments)
            {
                case 1:
                    return functionMetadata.Name + "(" + Children[0].ToString() + ")";

                case 2:
                    return functionMetadata.Name + "(" + Children[0].ToString() + "," + Children[1].ToString() + ")";

                case 3:
                    return functionMetadata.Name + "(" + Children[0].ToString() + "," + Children[1].ToString() + "," + Children[2].ToString() + ")";

                case 4:
                    return functionMetadata.Name + "(" + Children[0].ToString() + "," + Children[1].ToString() + "," + Children[2].ToString() + "," + Children[3].ToString() + ")";
            }
            throw new Exception("ToString: Invalid number of arguments in FunctionNode");
        }

        public override void SetCandidateRef(CandidateSolution<T, S> candidate)
        {
            ownerCandidate = candidate;
            // and get the descendants
            foreach (var child in Children)
                child.SetCandidateRef(candidate);
        }

        public int NumArguments
        {
            get
            {
                return functionMetadata.NumArguments;
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace Evolutionary
{
    /// <summary>
    /// Contains all functions
    /// </summary>
    class FunctionNode<T> : NodeBaseType<T>
    {
        // a function node always has N children, where N depends on the function
        public List<NodeBaseType<T>> Children { get; set; }
        private FunctionMetaData<T> functionMetadata = null;

        public FunctionNode(FunctionMetaData<T> function)
        {
            functionMetadata = function;
            Children = new List<NodeBaseType<T>>(function.NumArguments);
        }

        public override T Evaluate()
        {
            switch(functionMetadata.NumArguments)
            {
                case 1:
                    return ((Func<T, T>)functionMetadata.FunctionPtr)(Children[0].Evaluate());
                case 2:
                    return ((Func<T, T, T>)functionMetadata.FunctionPtr)(Children[0].Evaluate(), Children[1].Evaluate());
                case 3:
                    return ((Func<T, T, T, T>)functionMetadata.FunctionPtr)(Children[0].Evaluate(), Children[1].Evaluate(), Children[2].Evaluate());
            }
            throw new Exception("Evaluate: Invalid number of arguments in a function node");
        }

        public override NodeBaseType<T> Clone(NodeBaseType<T> parentNode)
        {
            // clone the node
            var newNode = new FunctionNode<T>(functionMetadata);
            newNode.Parent = parentNode;

            // clone the children
            foreach (var child in Children)
                newNode.Children.Add(child.Clone(newNode));
            return newNode;
        }

        public override NodeBaseType<T> SelectRandom(ref int numNodesConsidered)
        {
            float selectPercentage = 1F / numNodesConsidered;

            // possibly pick myself
            NodeBaseType<T> selected = null;
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
            }
            throw new Exception("ToString: Invalid number of arguments in FunctionNode");
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

namespace Evolutionary
{
    class ConstantNode<T, S> : TerminalNode<T, S> where S : new()
    {
        private T constantValue;

        public ConstantNode(T value)
        {
            constantValue = value;
        }

        public override NodeBaseType<T, S> Clone(NodeBaseType<T, S> parentNode)
        {
            var newNode = new ConstantNode<T, S>(constantValue);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return constantValue;
        }

        public override void SetCandidateRef(CandidateSolution<T, S> candidate)
        {
            // no need to store a reference to the candidate for a constant
        }

        public override string ToString()
        {
            return constantValue.ToString();
        }
    }
}

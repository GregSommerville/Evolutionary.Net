namespace Evolutionary
{
    class VariableNode<T, S> : TerminalNode<T, S> where S : new()
    {
        // pointer to the variable information
        private VariableMetadata<T> varPtr;
        private CandidateSolution<T, S> ownerCandidate;

        public VariableNode(VariableMetadata<T> variable)
        {
            this.varPtr = variable;
        }

        public override NodeBaseType<T, S> Clone(NodeBaseType<T, S> parentNode)
        {
            var newNode = new VariableNode<T, S>(varPtr);
            newNode.Parent = parentNode;

            return newNode;
        }

        public override T Evaluate()
        {
            return varPtr.Value;
        }

        public override void SetCandidateRef(CandidateSolution<T, S> candidate)
        {
            ownerCandidate = candidate;
        }

        public override string ToString()
        {
            return varPtr.Name;
        }
    }
}

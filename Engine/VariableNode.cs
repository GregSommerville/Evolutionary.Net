namespace Evolutionary
{
    class VariableNode<T, S> : TerminalNode<T, S> where S : new()
    {
        // variable values are stored in the candidate
        private CandidateSolution<T, S> ownerCandidate;
        private string variableName;

        public VariableNode(string varName, CandidateSolution<T, S> candidate)
        {
            variableName = varName;
            ownerCandidate = candidate;
        }

        public override NodeBaseType<T, S> Clone(NodeBaseType<T, S> parentNode)
        {
            var newNode = new VariableNode<T, S>(variableName, ownerCandidate);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return ownerCandidate.Variables[variableName];
        }

        public override void SetCandidateRef(CandidateSolution<T, S> candidate)
        {
            ownerCandidate = candidate;
        }

        public override string ToString()
        {
            return variableName;
        }
    }
}

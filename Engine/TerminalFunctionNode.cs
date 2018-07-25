using System;

namespace Evolutionary
{
    class TerminalFunctionNode<T,S> : TerminalNode<T, S> where S : new()
    {
        private FunctionMetaData<T> functionMetadata = null;
        private CandidateSolution<T, S> ownerCandidate;

        public TerminalFunctionNode(FunctionMetaData<T> function, CandidateSolution<T, S> candidate)
        {
            functionMetadata = function;
            ownerCandidate = candidate;
        }

        public override NodeBaseType<T, S> Clone(NodeBaseType<T, S> parentNode)
        {
            // clone the node
            var newNode = new TerminalFunctionNode<T, S>(functionMetadata, ownerCandidate);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return ((Func<S,T>)functionMetadata.FunctionPtr)(ownerCandidate.StateData);
        }

        public override void SetCandidateRef(CandidateSolution<T, S> candidate)
        {
            ownerCandidate = candidate;
        }

        public override string ToString()
        {
            return functionMetadata.Name + "()";
        }
    }
}

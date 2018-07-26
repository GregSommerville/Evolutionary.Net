namespace Evolutionary
{
    abstract class NodeBaseType<T,S> where S : new()
    {
        // everybody has a parent
        public NodeBaseType<T,S> Parent { get; set; }

        // everyone needs to implement these
        public abstract T Evaluate();
        public abstract NodeBaseType<T,S> Clone(NodeBaseType<T, S> parentNode);
        public abstract NodeBaseType<T, S> SelectRandom(ref int numNodesConsidered);
        public abstract void SetCandidateRef(CandidateSolution<T, S> candidate);
        public abstract override string ToString();
    }
}

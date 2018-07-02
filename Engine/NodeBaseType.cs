namespace Evolutionary
{
    /// <summary>
    /// Base class for our Terminal and Function node types
    /// </summary>
    abstract class NodeBaseType<T>
    {
        // everybody has a parent
        public NodeBaseType<T> Parent { get; set; }

        // everyone needs to implement these
        public abstract T Evaluate();
        public abstract NodeBaseType<T> Clone(NodeBaseType<T> parentNode);
        public abstract NodeBaseType<T> SelectRandom(ref int numNodesConsidered);
        public abstract override string ToString();
    }
}

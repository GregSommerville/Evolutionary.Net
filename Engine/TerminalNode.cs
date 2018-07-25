namespace Evolutionary
{
    /// <summary>
    /// Base class for variable and constant nodes
    /// </summary>
    abstract class TerminalNode<T, S> : NodeBaseType<T, S> where S : new()
    {
        // Both types of terminal nodes (Variable and Constant) do the same thing here
        public override NodeBaseType<T, S> SelectRandom(ref int numNodesConsidered)
        {
            float selectPercentage = 1F / numNodesConsidered;

            if (Randomizer.GetFloatFromZeroToOne() < selectPercentage)
                return this;
            return null;
        }
    }
}

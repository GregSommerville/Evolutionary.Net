namespace Evolutionary
{
    /// <summary>
    /// Base class for variable and constant nodes
    /// </summary>
    abstract class TerminalNode<T> : NodeBaseType<T>
    {
        // Both types of terminal nodes (Variable and Constant) do the same thing here
        public override NodeBaseType<T> SelectRandom(ref int numNodesConsidered)
        {
            float selectPercentage = 1F / numNodesConsidered;

            if (Randomizer.GetFloatFromZeroToOne() < selectPercentage)
                return this;
            return null;
        }
    }
}

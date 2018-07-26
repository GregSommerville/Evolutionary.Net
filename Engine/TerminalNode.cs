namespace Evolutionary
{
    abstract class TerminalNode<T, S> : NodeBaseType<T, S> where S : new()
    {
        // Algorithm "R"
        public override NodeBaseType<T, S> SelectRandom(ref int numNodesConsidered)
        {
            float selectPercentage = 1F / numNodesConsidered;
            if (Randomizer.GetFloatFromZeroToOne() < selectPercentage)
                return this;
            return null;
        }
    }
}

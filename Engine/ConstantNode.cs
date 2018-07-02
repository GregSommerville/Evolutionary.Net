namespace Evolutionary
{
    class ConstantNode<T> : TerminalNode<T>
    {
        private T constantValue;

        public ConstantNode(T value)
        {
            constantValue = value;
        }

        public override NodeBaseType<T> Clone(NodeBaseType<T> parentNode)
        {
            var newNode = new ConstantNode<T>(constantValue);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return constantValue;
        }

        public override string ToString()
        {
            return constantValue.ToString();
        }
    }
}

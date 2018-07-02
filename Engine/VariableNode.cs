namespace Evolutionary
{
    class VariableNode<T> : TerminalNode<T>
    {
        // pointer to the variable information
        private VariableMetadata<T> varPtr;

        public VariableNode(VariableMetadata<T> variable)
        {
            this.varPtr = variable;
        }

        public override NodeBaseType<T> Clone(NodeBaseType<T> parentNode)
        {
            var newNode = new VariableNode<T>(varPtr);
            newNode.Parent = parentNode;

            return newNode;
        }

        public override T Evaluate()
        {
            return varPtr.Value;
        }

        public override string ToString()
        {
            return varPtr.Name;
        }
    }
}

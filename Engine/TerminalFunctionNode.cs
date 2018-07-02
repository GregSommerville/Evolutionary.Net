using System;

namespace Evolutionary
{
    class TerminalFunctionNode<T> : TerminalNode<T>
    {
        private FunctionMetaData<T> functionMetadata = null;

        public TerminalFunctionNode(FunctionMetaData<T> function)
        {
            functionMetadata = function;
        }

        public override NodeBaseType<T> Clone(NodeBaseType<T> parentNode)
        {
            // clone the node
            var newNode = new TerminalFunctionNode<T>(functionMetadata);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return ((Func<T>)functionMetadata.FunctionPtr)();
        }

        public override string ToString()
        {
            return functionMetadata.Name + "()";
        }
    }
}

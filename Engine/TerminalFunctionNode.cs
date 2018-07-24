using System;

namespace Evolutionary
{
    class TerminalFunctionNode<T,S> : TerminalNode<T>
    {
        private FunctionMetaData<T> functionMetadata = null;
        private S stateDataRef = default(S);

        public TerminalFunctionNode(FunctionMetaData<T> function, S stateData)
        {
            functionMetadata = function;
            stateDataRef = stateData;
        }

        public override NodeBaseType<T> Clone(NodeBaseType<T> parentNode)
        {
            // clone the node
            var newNode = new TerminalFunctionNode<T, S>(functionMetadata, stateDataRef);
            newNode.Parent = parentNode;
            return newNode;
        }

        public override T Evaluate()
        {
            return ((Func<S,T>)functionMetadata.FunctionPtr)(stateDataRef);
        }

        public override string ToString()
        {
            return functionMetadata.Name + "()";
        }
    }
}

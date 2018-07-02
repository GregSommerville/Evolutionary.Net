using System;

namespace Evolutionary
{
    class FunctionMetaData<T>
    {
        public int NumArguments { get; set; }
        public Delegate FunctionPtr { get; set; }
        public string Name { get; set; }

        public FunctionMetaData(Delegate functionPtr, int numArgs, string functionName)
        {
            NumArguments = numArgs;
            FunctionPtr = functionPtr;
            Name = functionName;
        }
    }
}

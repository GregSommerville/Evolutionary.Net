using System;

namespace Evolutionary
{
    class FunctionMetaData<T>
    {
        public int NumArguments { get; set; }
        public Delegate FunctionPtr { get; set; }
        public string Name { get; set; }
        public bool UsesState { get; set; }  // does state information get passed to the function?

        public FunctionMetaData(Delegate functionPtr, int numArgs, string functionName, bool usesState = false)
        {
            NumArguments = numArgs;
            FunctionPtr = functionPtr;
            Name = functionName;
            UsesState = usesState;
        }
    }
}

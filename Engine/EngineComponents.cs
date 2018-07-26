using System.Collections.Generic;

namespace Evolutionary
{
    // stores all of the components used in this engine
    internal class EngineComponents<T>
    {
        public List<FunctionMetaData<T>> Functions { get; set; }
        public List<FunctionMetaData<T>> TerminalFunctions { get; set; }
        public List<T> Constants { get; set; }
        public List<string> VariableNames { get; set; }

        public EngineComponents()
        {
            Functions = new List<FunctionMetaData<T>>();
            TerminalFunctions = new List<FunctionMetaData<T>>();
            Constants = new List<T>();
            VariableNames = new List<string>();
        }
    }

}

using Evolutionary;
using System;

namespace BlackjackStrategy.Models
{
    abstract class SolutionBase
    {
        public string FinalStatus { get; set; }

        public abstract void BuildProgram(EngineParameters engineParams, Action<string> currentStatusCallback);
        public abstract OverallStrategy GetStrategy();
    }
}

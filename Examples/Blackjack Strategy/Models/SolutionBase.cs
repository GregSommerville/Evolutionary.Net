using Evolutionary;
using System;

namespace BlackjackStrategy.Models
{
    abstract class SolutionBase
    {
        public string FinalStatus { get; set; }

        public abstract void BuildProgram(ProgramSettings settings, Action<string, CandidateSolution<bool, ProblemState>> currentStatusCallback);
        public abstract Strategy GetStrategy();
    }
}

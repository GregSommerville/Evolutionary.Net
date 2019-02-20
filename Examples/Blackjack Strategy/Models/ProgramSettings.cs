using Evolutionary;

namespace BlackjackStrategy.Models
{
    public class ProgramSettings
    {
        public TestConditions TestSettings { get; set; } = new TestConditions();
        public EngineParameters GPsettings { get; set; } = new EngineParameters();

        public ProgramSettings()
        {
            GPsettings.CrossoverRate = 1;
            GPsettings.ElitismRate = 0;
            GPsettings.IsLowerFitnessBetter = false;
            GPsettings.MaxGenerations = 100;
            GPsettings.MinGenerations = 1;
            GPsettings.MutationRate = 0;
            GPsettings.PopulationSize = 250;
            GPsettings.SelectionStyle = SelectionStyle.Tourney;
            GPsettings.StagnantGenerationLimit = 10;
            GPsettings.TourneySize = 3;
        }
    }
}

using System.ComponentModel;

namespace Evolutionary
{
    [DisplayName("Genetic Engine Settings")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class EngineParameters
    {
        [Description("If using Tourney Selection, how many to select")]
        public int? TourneySize { get; set; } = EngineParameterDefaults.TourneySize;

        [Description("Min number of generations it must run")]
        public int? MinGenerations { get; set; } = EngineParameterDefaults.MinGenerations;

        [Description("Max number of generations it can run")]
        public int? MaxGenerations { get; set; } = EngineParameterDefaults.MaxGenerations;

        [Description("Execution stops after this # gens with no improvement in best or average scores")]
        public int? StagnantGenerationLimit { get; set; } = EngineParameterDefaults.StagnantGenerationLimit;

        [Description("From 0.0 to 1.0, percentage of the best scoring candidates moved to the next generation")]
        public double? ElitismRate { get; set; } = EngineParameterDefaults.ElitismRate;

        [Description("Number of candidates per generation")]
        public int? PopulationSize { get; set; } = EngineParameterDefaults.PopulationSize;

        [Description("Controls whether lower or higher is better")]
        public bool? IsLowerFitnessBetter { get; set; } = EngineParameterDefaults.IsLowerFitnessBetter;

        [Description("From 0.0 to 1.0, percentage of candidates that are crossed over")]
        public double? CrossoverRate { get; set; } = EngineParameterDefaults.CrossoverRate;

        [Description("From 0.0 to 1.0, percentage of candidates that are mutated")]
        public double? MutationRate { get; set; } = EngineParameterDefaults.MutationRate;

        [Description("Min levels for new subtrees")]
        public int? RandomTreeMinDepth { get; set; } = EngineParameterDefaults.RandomTreeMinDepth;

        [Description("Max levels for new subtrees")]
        public int? RandomTreeMaxDepth { get; set; } = EngineParameterDefaults.RandomTreeMaxDepth;

        [Description("How parents are selected for crossover")]
        public SelectionStyle? SelectionStyle { get; set; } = EngineParameterDefaults.SelectionStyle;

        // so it looks right in the property grid
        public override string ToString()
        {
            return "";
        }
    }

    // Default values for the above
    public sealed class EngineParameterDefaults
    {
        public static int TourneySize = 3;
        public static int MinGenerations = 25;
        public static int MaxGenerations = 75;
        public static int StagnantGenerationLimit = 10;
        public static double ElitismRate = 0.1;
        public static int PopulationSize = 500;
        public static bool IsLowerFitnessBetter = true;
        public static double CrossoverRate = 1.00;
        public static double MutationRate = 0.01;
        public static int RandomTreeMinDepth = 4;
        public static int RandomTreeMaxDepth = 7;
        public static SelectionStyle SelectionStyle = SelectionStyle.Tourney;
    }
}

﻿namespace Evolutionary
{
    public sealed class EngineParameters
    {
        public int? TourneySize { get; set; }
        public int? MinGenerations { get; set; }
        public int? MaxGenerations { get; set; }
        public int? StagnantGenerationLimit { get; set; }
        public double? ElitismRate { get; set; }
        public int? PopulationSize { get; set; }
        public bool? IsLowerFitnessBetter { get; set; }
        public double? CrossoverRate { get; set; }
        public double? MutationRate { get; set; }
        public int? RandomTreeMinDepth { get; set; }
        public int? RandomTreeMaxDepth { get; set; }
        public SelectionStyle? SelectionStyle { get; set; }
    }

    // Default values for the above
    public sealed class EngineParameterDefaults
    {
        public static int TourneySize = 4;
        public static int MinGenerations = 25;
        public static int MaxGenerations = 75;
        public static int StagnantGenerationLimit = 10;
        public static double ElitismRate = 0.1;
        public static int PopulationSize = 200;
        public static bool IsLowerFitnessBetter = true;
        public static double CrossoverRate = 0.95;
        public static double MutationRate = 0.02;
        public static int RandomTreeMinDepth = 4;
        public static int RandomTreeMaxDepth = 7;
        public static SelectionStyle SelectionStyle = SelectionStyle.Tourney;
    }
}

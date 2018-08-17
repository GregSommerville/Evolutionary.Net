﻿namespace Evolutionary
{
    public sealed class EngineParameters
    {
        public int TourneySize { get; set; }
        public int MinGenerations { get; set; }
        public int MaxGenerations { get; set; }
        public int NoChangeGenerationCountForTermination { get; set; }
        public int ElitismPercentageOfPopulation { get; set; }
        public int PopulationSize { get; set; }
        public bool IsLowerFitnessBetter { get; set; }
        public double CrossoverRate { get; set; }
        public double MutationRate { get; set; }
        public int RandomTreeMinDepth { get; set; }
        public int RandomTreeMaxDepth { get; set; }
    }
}

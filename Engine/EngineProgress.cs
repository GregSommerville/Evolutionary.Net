﻿namespace Evolutionary
{
    public class EngineProgress
    {
        public int GenerationNumber { get; set; }
        public float BestFitnessThisGen { get; set; }
        public float AvgFitnessThisGen { get; set; }
        public float BestFitnessSoFar { get; set; }
    }
}

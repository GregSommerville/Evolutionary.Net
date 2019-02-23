using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WineQuality
{
    using DataRow = List<double>;

    class Dataset
    {
        const double PercentageForTraining = 0.666;
        const string FileName = "wine-quality.csv";

        static List<DataRow> sampleData;

        List<int> indexesForTraining, indexesForTesting;
        int currentTrainingIndex, currentTestingIndex, numTrainingItems, numTestingItems;

        static Dataset()
        {
            // this static constructor gets called once to initialize before any instances are created
            LoadDataForTrainingAndTesting();
        }

        public Dataset()
        {
            // instance constructor gives each caller a unique split of training/testing data
            SplitData();
        }

        void SplitData()
        {
            indexesForTraining = new List<int>();
            indexesForTesting = new List<int>();

            // thread-safe random object, since the seed value is not the default (time-based)
            var random = new Random(Guid.NewGuid().GetHashCode());

            var numItems = sampleData.Count;
            for (int i = 0; i < numItems; i++)
                if (random.NextDouble() < PercentageForTraining)
                    indexesForTraining.Add(i);
                else
                    indexesForTesting.Add(i);

            numTrainingItems = indexesForTraining.Count;
            numTestingItems = numItems - numTrainingItems;
            currentTrainingIndex = 0;
            currentTestingIndex = 0;
        }

        public DataRow GetRowOfTrainingData()
        {
            if (currentTrainingIndex >= numTrainingItems)
                return null;

            return sampleData[indexesForTraining[currentTrainingIndex++]];
        }

        public DataRow GetRowOfTestingData()
        {
            if (currentTestingIndex >= numTestingItems)
                return null;

            return sampleData[indexesForTesting[currentTestingIndex++]];
        }

        static void LoadDataForTrainingAndTesting()
        {
            /*  Dataset is from:
              P. Cortez, A. Cerdeira, F. Almeida, T. Matos and J. Reis. 
              Modeling wine preferences by data mining from physicochemical properties.
              In Decision Support Systems, Elsevier, 47(4):547-553. ISSN: 0167-9236.
              Available at: [@Elsevier] http://dx.doi.org/10.1016/j.dss.2009.05.016
            */

            sampleData = new List<DataRow>();

            // the CSV file is actually semi-colon delimited (not comma), and fields are the following:
            // "fixed acidity";"volatile acidity";"citric acid";"residual sugar";"chlorides";"free sulfur dioxide";"total sulfur dioxide";"density";"pH";"sulphates";"alcohol";"quality"
            var lines = File.ReadAllLines(FileName);
            for (int lineNum = 1; lineNum < lines.Length; lineNum++)
            {
                // skipping line 0 (the header), parse all numbers and store
                var parts = lines[lineNum].Split(";".ToCharArray());
                var decimalValues = parts.Select(p => double.Parse(p)).ToList();
                sampleData.Add(decimalValues);
            }            
        }
    }
}

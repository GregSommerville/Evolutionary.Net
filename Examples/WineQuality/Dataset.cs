using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WineQuality
{
    using DataRow = List<double>;

    class Dataset
    {
        const double PercentageForTraining = 0.75;
        const string FileName = "wine-quality.csv";

        static List<DataRow> sampleData;
        static int numItems;

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
            
            numTrainingItems = (int)(numItems * PercentageForTraining);
            numTestingItems = numItems - numTrainingItems;

            for (int i = 0; i < numTrainingItems; i++)
                indexesForTraining.Add(i);
            for (int i = numTrainingItems; i < numItems; i++)
                indexesForTesting.Add(i);

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
            numItems = sampleData.Count;

            NormalizeData();
        }

        static void NormalizeData()
        {
            int numCols = sampleData.First().Count;
            for (int col = 0; col < numCols - 2; col++)
            {
                // find min and max for the column
                double minVal = double.MaxValue, maxVal = double.MinValue;
                for (int i = 0; i < numItems; i++)
                {
                    double val = sampleData[i][col];
                    if (val < minVal) minVal = val;
                    if (val > maxVal) maxVal = val;
                }
                var valueRange = maxVal - minVal;

                // then normalize the column
                for (int i = 0; i < numItems; i++)
                    sampleData[i][col] = (sampleData[i][col] - minVal) / valueRange;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PredictionRegression
{
    using DataRow = List<double>;

    class Dataset
    {
        public static int LabelColumnIndex { get; set; }
        public static int NumColumns { get; set;  }

        const double PercentageForTraining = 0.75;

        static List<DataRow> sampleData;
        static int numItems;

        List<int> indexesForTraining, indexesForTesting;
        int currentTrainingIndex, currentTestingIndex, numTrainingItems, numTestingItems;

        public static void LoadFromFile(string filename, int colIndexForLabel, int[] ignoreColumns, string delimiter = ",")
        {
            LoadDataForTrainingAndTesting(filename, delimiter, ignoreColumns);
            LabelColumnIndex = colIndexForLabel;
        }

        public Dataset()
        {
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

        static void LoadDataForTrainingAndTesting(string fileName, string delimiter, int[] ignoreColumns)
        {
            sampleData = new List<DataRow>();            
            var splitOn = delimiter.ToCharArray();
            var lines = File.ReadAllLines(fileName);

            // skipping line 0 (the header)
            for (int lineNum = 1; lineNum < lines.Length; lineNum++)
            {
                // break line into parts and discard ignoreColumns
                var parts = lines[lineNum].Split(splitOn).ToList();
                foreach (int colIndex in ignoreColumns.OrderByDescending(i => i))
                    parts.RemoveAt(colIndex);

                // parse and save
                var decimalValues = parts.Select(p => double.Parse(p)).ToList();
                sampleData.Add(decimalValues);
            }
            numItems = sampleData.Count;
            NumColumns = sampleData.First().Count;

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

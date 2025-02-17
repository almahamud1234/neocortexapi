using NeoCortexApi;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace AnomalyDetectionSample
{

    public class HTMTrainingManager
    {
        public void ExecuteHTMModelTraining(string trainingFolderPath, string predictionFolderPath, out Predictor trainedPredictor)
        {


            CsvSequenceFolder trainingReader = new CsvSequenceFolder(trainingFolderPath);
            var trainingSequences = trainingReader.ExtractSequencesFromFolder();

            // Read numerical sequences from CSV files in the specified prediction folder
            CsvSequenceFolder predictionReader = new CsvSequenceFolder(predictionFolderPath);
            var predictionSequences = predictionReader.ExtractSequencesFromFolder();

            // Combine sequences from both training and prediction folders
            List<List<double>> combinedSequences = new List<List<double>>(trainingSequences);
            combinedSequences.AddRange(predictionSequences);

            // Start multi-sequence learning experiment to generate predictor model
            MultiSequenceLearning learningAlgorithm = new MultiSequenceLearning();
            trainedPredictor = learningAlgorithm.Run(htmInput);

            // HTM model training completed

            stopwatch.Stop();

        }
    }
}

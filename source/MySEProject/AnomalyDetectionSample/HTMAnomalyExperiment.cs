using NeoCortexApi;

namespace AnomalyDetectionSample
{
    public class HTMAnomalyExperiment
    {
        private readonly string _trainingCSVFolderPath;
        private readonly string _predictingCSVFolderPath;
        private static double _totalAccuracy = 0.0;
        private static int _iterationCount = 0;
        private readonly double _tolerance;

        public HTMAnomalyExperiment(string trainingFolderPath = "anomaly_training", string predictingFolderPath = "anomaly_predicting", double tolerance = 0.1)
        {
            _tolerance = tolerance;
            var projectBaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            _trainingCSVFolderPath = Path.Combine(projectBaseDirectory!, trainingFolderPath);
            _predictingCSVFolderPath = Path.Combine(projectBaseDirectory!, predictingFolderPath);
        }

        public void ExecuteExperiment()
        {
            HTMTrainingManager htmModel = new HTMTrainingManager();

            if (!Directory.Exists(_trainingCSVFolderPath) || !Directory.Exists(_predictingCSVFolderPath))
            {
                Console.WriteLine("Training or predicting folder does not exist.");
                return;
            }

            htmModel.ExecuteHTMModelTraining(_trainingCSVFolderPath, _predictingCSVFolderPath, out Predictor predictor);
            Console.WriteLine("Starting the anomaly detection experiment...");

            CsvSequenceFolder testSequencesReader = new CsvSequenceFolder(_predictingCSVFolderPath);
            var inputSequences = testSequencesReader.ExtractSequencesFromFolder();
            var trimmedInputSequences = CsvSequenceFolder.TrimSequences(inputSequences);

            predictor.Reset();

            string outputFilePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName, "output", $"anomaly_output_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            List<string> experimentResults = new List<string>();

            foreach (var sequence in trimmedInputSequences)
            {
                experimentResults.AddRange(DetectAnomalyOnSequence(predictor, sequence.ToArray(), _tolerance));
            }

            File.WriteAllLines(outputFilePath, experimentResults);
            StoredOutputValues.totalAvgAccuracy = _totalAccuracy / Math.Max(_iterationCount, 1);

            Console.WriteLine("Experiment results have been written to: " + outputFilePath);
        }

        private List<string> DetectAnomalyOnSequence(Predictor predictor, double[] sequence, double tolerance)
        {
            if (sequence.Length < 2)
                throw new ArgumentException($"Sequence must contain at least two values. Sequence: [{string.Join(",", sequence)}]");

            foreach (double value in sequence)
            {
                if (double.IsNaN(value))
                    throw new ArgumentException($"Sequence contains non-numeric values. Sequence: [{string.Join(",", sequence)}]");
            }

            var resultOutputLines = new List<string>
            {
                "------------------------------",
                "",
                $"Testing the sequence for anomaly detection: {string.Join(", ", sequence)}.",
                ""
            };

            double currentAccuracy = 0.0;

            for (int i = 0; i < sequence.Length; i++)
            {
                var currentItem = sequence[i];
                var predictionResult = predictor.Predict(currentItem);

                resultOutputLines.Add($"Current element in the testing sequence: {currentItem}");

                if (predictionResult.Any())
                {
                    var tokens = predictionResult.First().PredictedInput.Split('_');
                    var tokens2 = predictionResult.First().PredictedInput.Split('-');
                    var similarity = predictionResult.First().Similarity;

                    if (i < sequence.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = sequence[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());

                        var anomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = anomalyScore / nextItem;

                        if (deviation <= tolerance)
                        {
                            resultOutputLines.Add($"No anomaly detected in the next element. HTM Engine found similarity: {similarity}%.");
                            currentAccuracy += similarity;
                        }
                        else
                        {
                            resultOutputLines.Add($"****Anomaly detected**** in the next element. HTM Engine predicted: {predictedNextItem} with similarity: {similarity}%, actual value: {nextItem}.");
                            i++; // Skip the anomalous element
                            resultOutputLines.Add("Skipping to the next element in the testing sequence.");
                            currentAccuracy += similarity;
                        }
                    }
                    else
                    {
                        resultOutputLines.Add("End of sequence. Further anomaly testing cannot be continued.");
                    }
                }
                else
                {
                    resultOutputLines.Add("Nothing predicted from HTM Engine. Anomaly detection failed.");
                }
            }

            double averageSequenceAccuracy = currentAccuracy / sequence.Length;
            resultOutputLines.Add($"Average accuracy for this sequence: {averageSequenceAccuracy}%.");
            resultOutputLines.Add("------------------------------");

            _totalAccuracy += averageSequenceAccuracy;
            _iterationCount++;

            WriteOutput(predictor, sequence, tolerance);
            return resultOutputLines;
        }

        private void WriteOutput(Predictor predictor, double[] sequence, double tolerance)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Testing the sequence for anomaly detection: {string.Join(", ", sequence)}.");
            bool startFromFirst = true;

            double firstItem = sequence[0];
            double secondItem = sequence[1];
            var secondItemRes = predictor.Predict(secondItem);

            Console.WriteLine($"First element in the testing sequence from input list: {firstItem}");

            if (secondItemRes.Any())
            {
                var tokens = secondItemRes.First().PredictedInput.Split('_');
                var tokens2 = secondItemRes.First().PredictedInput.Split('-');
                var similarity = secondItemRes.First().Similarity;
                var predictedFirstItem = double.Parse(tokens2.Last());
                var firstAnomalyScore = Math.Abs(predictedFirstItem - firstItem);
                var firstDeviation = firstAnomalyScore / firstItem;

                if (firstDeviation <= tolerance)
                {
                    Console.WriteLine($"No anomaly detected in the first element. HTM Engine found similarity: {similarity}%. Starting check from beginning of the list.");
                    startFromFirst = true;
                }
                else
                {
                    Console.WriteLine($"****Anomaly detected**** in the first element. HTM Engine predicted: {predictedFirstItem} with similarity: {similarity}%, actual value: {firstItem}. Moving to the next element.");
                    startFromFirst = false;
                }
            }
            else
            {
                Console.WriteLine("Anomaly detection cannot be performed for the first element. Starting check from beginning of the list.");
                startFromFirst = true;
            }

            int checkCondition = startFromFirst ? 0 : 1;

            for (int i = checkCondition; i < sequence.Length; i++)
            {
                var currentItem = sequence[i];
                var res = predictor.Predict(currentItem);
                Console.WriteLine($"Current element in the testing sequence from input list: {currentItem}");

                if (res.Any())
                {
                    var tokens = res.First().PredictedInput.Split('_');
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var similarity = res.First().Similarity;

                    if (i < sequence.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = sequence[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());

                        var anomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = anomalyScore / nextItem;

                        if (deviation <= tolerance)
                        {
                            Console.WriteLine($"No anomaly detected in the next element. HTM Engine found similarity: {similarity}%.");
                        }
                        else
                        {
                            Console.WriteLine($"****Anomaly detected**** in the next element. HTM Engine predicted: {predictedNextItem} with similarity: {similarity}%, actual value: {nextItem}.");
                            i++; // Skip the anomalous element
                            Console.WriteLine("As anomaly was detected, skipping to the next element in our testing sequence.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("End of input list. Further anomaly testing cannot be continued.");
                    }
                }
                else
                {
                    Console.WriteLine("Nothing predicted from HTM Engine. Anomaly detection failed.");
                }
            }
        }
    }
}

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
        List<double[]> allTestingData = new List<double[]>();
        List<double[]> allLearnedData = new List<double[]>();
        List<List<int>> allAnomalyIndices = new List<List<int>>();

        /// <summary>
        /// Constructor to initialize paths for training and predicting data and set the anomaly tolerance level.
        /// </summary>
        public HTMAnomalyExperiment(string trainingFolderPath = "training_sequence", string predictingFolderPath = "predicting_sequence", double tolerance = 0.2)
        {
            _tolerance = tolerance;
            var projectBaseDirectory = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            _trainingCSVFolderPath = Path.Combine(projectBaseDirectory!, trainingFolderPath);
            _predictingCSVFolderPath = Path.Combine(projectBaseDirectory!, predictingFolderPath);
        }

        /// <summary>
        /// Executes the anomaly detection experiment using HTM model.
        /// </summary>
        public void ExecuteExperiment()
        {
            HTMTrainingManager htmModel = new HTMTrainingManager();

            // Check if directories exist before proceeding
            if (!Directory.Exists(_trainingCSVFolderPath) || !Directory.Exists(_predictingCSVFolderPath))
            {
                Console.WriteLine("Training or predicting folder does not exist.");
                return;
            }

            // Train the HTM model
            htmModel.ExecuteHTMModelTraining(_trainingCSVFolderPath, _predictingCSVFolderPath, out Predictor predictor);
            Console.WriteLine("Starting the anomaly detection experiment...");

            CsvSequenceFolder testSequencesReader = new CsvSequenceFolder(_predictingCSVFolderPath);
            var inputSequences = testSequencesReader.ExtractSequencesFromFolder();
            var trimmedInputSequences = CsvSequenceFolder.TrimSequences(inputSequences);

            predictor.Reset();

            string outputFilePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName, "output", $"anomaly_output_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            List<string> experimentResults = new List<string>();

            // Store learned data for visualization
            foreach (var sequence in inputSequences)
            {
                allLearnedData.Add(sequence.ToArray());
            }

            // Process testing sequences and detect anomalies
            foreach (var sequence in trimmedInputSequences)
            {
                allTestingData.Add(sequence.ToArray());
                var (log, anomalies) = DetectAnomalyOnSequence(predictor, sequence.ToArray(), _tolerance);
                experimentResults.AddRange(log);
                allAnomalyIndices.Add(anomalies);
            }

            // Save experiment results to file
            File.WriteAllLines(outputFilePath, experimentResults);
            StoredOutputValues.totalAvgAccuracy = _totalAccuracy / Math.Max(_iterationCount, 1);

            // Generate visualizations
            AnomalyVisualizer.CreateGraphForAnomalies(allTestingData, allAnomalyIndices);
            SequenceVisualizer.CreateGraphForSequences(allLearnedData, allTestingData);
            SequenceVisualizer.CreateBothSequenceWithAnomalies(allLearnedData, allTestingData, allAnomalyIndices);

            Console.WriteLine("Experiment results have been written to: " + outputFilePath);
        }

        /// <summary>
        /// Detects anomalies in a given sequence using the trained predictor.
        /// </summary>
        public Tuple<List<string>, List<int>> DetectAnomalyOnSequence(Predictor predictor, double[] sequence, double tolerance)
        {
            // Validate sequence length
            if (sequence.Length < 2)
                throw new ArgumentException($"Sequence must contain at least two values. Sequence: [{string.Join(",", sequence)}]");

            // Validate numerical values in sequence
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
            var anomalousIndex = new List<int>();

            // Iterate through sequence and detect anomalies
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
                            anomalousIndex.Add(i + 1);
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

            // Compute average accuracy for the sequence
            double averageSequenceAccuracy = currentAccuracy / sequence.Length;
            resultOutputLines.Add($"Average accuracy for this sequence: {averageSequenceAccuracy}%.");
            resultOutputLines.Add("------------------------------");

            _totalAccuracy += averageSequenceAccuracy;
            _iterationCount++;

            // Write output of sequence of data for anomalies using the HTM Engine predictor.
            WriteOutput(predictor, sequence, tolerance);

            return Tuple.Create(resultOutputLines, anomalousIndex);
        }

        /// <summary>
        /// Analyzes a given sequence of data for anomalies using the HTM Engine predictor.
        /// </summary>
        /// <param name="predictor">The predictor model used for anomaly detection.</param>
        /// <param name="sequence">The input sequence of numerical values.</param>
        /// <param name="tolerance">The threshold for detecting anomalies.</param>
        private void WriteOutput(Predictor predictor, double[] sequence, double tolerance)
        {
            Console.WriteLine("------------------------------");
            Console.WriteLine($"Testing the sequence for anomaly detection: {string.Join(", ", sequence)}.");

            // Flag to determine if checking should start from the first element
            bool startFromFirst = true;

            // Extract the first and second elements of the sequence
            double firstItem = sequence[0];
            double secondItem = sequence[1];
            var secondItemRes = predictor.Predict(secondItem);

            Console.WriteLine($"First element in the testing sequence from input list: {firstItem}");

            // Checking for prediction results of the second item
            if (secondItemRes.Any())
            {
                var tokens2 = secondItemRes.First().PredictedInput.Split('-');
                var similarity = secondItemRes.First().Similarity;
                var predictedFirstItem = double.Parse(tokens2.Last());
                var firstAnomalyScore = Math.Abs(predictedFirstItem - firstItem);
                var firstDeviation = firstAnomalyScore / firstItem;

                // Compare the deviation with the tolerance threshold
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

            // Determine the starting index for checking anomalies
            int checkCondition = startFromFirst ? 0 : 1;

            for (int i = checkCondition; i < sequence.Length; i++)
            {
                var currentItem = sequence[i];
                var res = predictor.Predict(currentItem);
                Console.WriteLine($"Current element in the testing sequence from input list: {currentItem}");

                // Checking for prediction results
                if (res.Any())
                {
                    var tokens2 = res.First().PredictedInput.Split('-');
                    var similarity = res.First().Similarity;

                    // Ensure there is a next item for comparison
                    if (i < sequence.Length - 1)
                    {
                        int nextIndex = i + 1;
                        double nextItem = sequence[nextIndex];
                        double predictedNextItem = double.Parse(tokens2.Last());

                        var anomalyScore = Math.Abs(predictedNextItem - nextItem);
                        var deviation = anomalyScore / nextItem;

                        // Compare the deviation with the tolerance threshold
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

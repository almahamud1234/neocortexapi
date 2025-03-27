using XPlot.Plotly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    public class SequenceVisualizer
    {
        /// <summary>
        /// Creates and saves a graph comparing actual and learned sequences.
        /// </summary>
        /// <param name="allLearnedData">List of predicted (learned) sequences.</param>
        /// <param name="allTestingData">List of actual testing sequences.</param>
        public static void CreateGraphForSequences(List<double[]> allLearnedData, List<double[]> allTestingData)
        {
            List<Scatter> allGraphs = new List<Scatter>();

            for (int i = 0; i < allTestingData.Count; i++)
            {
                double[] data = allTestingData[i];
                double[] learnedData = allLearnedData[i];

                // Plot actual data sequence
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),
                    y = data,
                    mode = "lines",
                    name = $"Actual Sequence {i + 1}",
                    line = new Line { color = "blue" } // Solid blue line for actual data
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),
                    y = learnedData,
                    mode = "lines",
                    name = $"Predicted Sequence {i + 1}",
                    line = new Line { color = "green", dash = "dashdot" } // Green dashed line for predicted data
                };

                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);
            }

            // Create and configure the plot
            var chart = Chart.Plot(allGraphs);
            chart.WithTitle("Comparison of Actual and Testing Sequences");
            chart.WithXTitle("X-axis (Index in Sequence)");
            chart.WithYTitle("Y-axis (Value of Sequence)");

            // Define output directory and file path
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "output", "graph");
            Directory.CreateDirectory(outputDirectory); 

            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }

        /// <summary>
        /// Creates and saves a graph comparing actual, learned sequences, and highlights anomalies.
        /// </summary>
        /// <param name="allLearnedData">List of predicted (learned) sequences.</param>
        /// <param name="allTestingData">List of actual testing sequences.</param>
        /// <param name="allAnomalyIndices">List of anomaly indices for each sequence.</param>
        public static void CreateBothSequenceWithAnomalies(List<double[]> allLearnedData, List<double[]> allTestingData, List<List<int>> allAnomalyIndices)
        {
            List<Scatter> allGraphs = new List<Scatter>();
            List<Scatter> allAnomalies = new List<Scatter>();

            for (int i = 0; i < allTestingData.Count; i++)
            {
                double[] data = allTestingData[i];
                double[] learnedData = allLearnedData[i];
                List<int> anomalyIndices = allAnomalyIndices[i];

                // Plot actual data sequence
                var actualGraph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),
                    y = data,
                    mode = "lines",
                    name = $"Testing Sequence {i + 1}",
                    line = new Line { color = "blue" }
                };

                // Plot predicted (learned) sequence
                var learnedGraph = new Scatter
                {
                    x = Enumerable.Range(0, learnedData.Length).ToArray(),
                    y = learnedData,
                    mode = "lines",
                    name = $"Learned Sequence {i + 1}",
                    line = new Line { color = "green", dash = "dashdot" } // Dashed line for learned data
                };

                allGraphs.Add(actualGraph);
                allGraphs.Add(learnedGraph);

                // Plot anomalies as red markers
                var anomalyGraph = new Scatter
                {
                    x = anomalyIndices.Select(idx => (double)idx).ToArray(),
                    y = anomalyIndices.Select(idx => data[idx]).ToArray(),
                    mode = "markers",
                    name = $"Anomalies in Sequence {i + 1}",
                    marker = new Marker { color = "red", size = 8 }
                };

                allAnomalies.Add(anomalyGraph);
            }

            // Create and configure the plot
            var chart = Chart.Plot(allGraphs.Concat(allAnomalies));
            chart.WithTitle("View for Anomalies in Actual and Testing Sequences");
            chart.WithXTitle("X-axis (Index in Sequence)");
            chart.WithYTitle("Y-axis (Value of Sequence)");

            // Define output directory and file path
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "output", "graph");
            Directory.CreateDirectory(outputDirectory); // Ensure directory exists

            string filePath = Path.Combine(outputDirectory, $"Actual_Testing_Sequence_anomaly_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser
            chart.Show();
        }
    }
}

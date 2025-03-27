using XPlot.Plotly;

namespace AnomalyDetectionSample
{
    public class AnomalyVisualizer
    {
        /// <summary>
        /// Generates and saves a graph visualizing anomalies in given data sequences.
        /// </summary>
        /// <param name="allData">List of double arrays representing different data sequences.</param>
        /// <param name="allAnomalyIndices">List of integer lists containing indices of anomalies for each sequence.</param>
        public static void CreateGraphForAnomalies(List<double[]> allData, List<List<int>> allAnomalyIndices)
        {
            // Lists to store the main graphs and anomalies separately
            List<Scatter> allGraphs = new List<Scatter>();
            List<Scatter> allAnomalies = new List<Scatter>();

            for (int i = 0; i < allData.Count; i++)
            {
                double[] data = allData[i]; // Retrieve the current data sequence
                List<int> anomalyIndices = allAnomalyIndices[i]; // Retrieve the corresponding anomaly indices

                // Create the main data plot
                var graph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(), // X-axis values (index positions)
                    y = data, // Y-axis values (data points)
                    mode = "lines", // Line plot mode
                    name = "Testing Sequence" + (i + 1) // Label for the sequence
                };

                // Create the anomaly plot
                var anomalies = new Scatter
                {
                    x = anomalyIndices.ToArray(), // X-axis values (anomaly positions)
                    y = anomalyIndices.Select(index => data[index]).ToArray(), // Y-axis values (actual anomaly values)
                    mode = "markers", // Marker mode for anomalies
                    name = "Anomalies in sequence" + (i + 1), // Label for anomalies
                    marker = new Marker { color = "red" } // Red color for anomalies
                };

                // Add plots to the respective lists
                allGraphs.Add(graph);
                allAnomalies.Add(anomalies);
            }

            // Combine all plots and create the final chart
            var chart = Chart.Plot(allGraphs.Concat(allAnomalies));
            chart.WithTitle("Anomalies in Testing sequences");
            chart.WithXTitle("X-axis (Anomaly Indexes inside Sequence)");
            chart.WithYTitle("Y-axis (Value of Sequence)");

            // Define the output directory and file path
            string projectRoot = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.FullName;
            string outputDirectory = Path.Combine(projectRoot!, "output", "graph");
            Directory.CreateDirectory(outputDirectory); // Ensure the directory exists

            // Generate a timestamped file name for the output HTML file
            string filePath = Path.Combine(outputDirectory, $"Testing_Sequence_Anomaly_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            // Save the chart as an HTML file
            File.WriteAllText(filePath, chart.GetHtml());

            // Display the chart in a browser window
            chart.Show();
        }
    }
}

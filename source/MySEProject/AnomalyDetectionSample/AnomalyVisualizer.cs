using XPlot.Plotly;

namespace AnomalyDetectionSample
{
    public class AnomalyVisualizer
    {
        public static void CreateGraphForAnomalies(List<double[]> allData, List<List<int>> allAnomalyIndices)
        {
            List<Scatter> allGraphs = new List<Scatter>();
            List<Scatter> allAnomalies = new List<Scatter>();

            for (int i = 0; i < allData.Count; i++)
            {
                double[] data = allData[i];
                List<int> anomalyIndices = allAnomalyIndices[i];

                var graph = new Scatter
                {
                    x = Enumerable.Range(0, data.Length).ToArray(),
                    y = data,
                    mode = "lines",
                    name = "Sequence" + (i+1)
                };

                var anomalies = new Scatter
                {
                    x = anomalyIndices.ToArray(),
                    y = anomalyIndices.Select(index => data[index]).ToArray(),
                    mode = "markers",
                    name = "Anomalies in sequence" + (i+1),
                    marker = new Marker { color = "red" }
                };

                allGraphs.Add(graph);
                allAnomalies.Add(anomalies);
            }

            var chart = Chart.Plot(allGraphs.Concat(allAnomalies));
            chart.WithTitle("Anomalies in sequences");
            chart.WithXTitle("X-axis(Anomaly Indexes inside Sequence)");
            chart.WithYTitle("Y-axis(Value of Sequence)");
            chart.Show();
        }

    }
}

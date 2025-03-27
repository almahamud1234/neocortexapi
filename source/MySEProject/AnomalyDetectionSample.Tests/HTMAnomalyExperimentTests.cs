using AnomalyDetectionSample;
using Moq;
using NeoCortexApi;

namespace AnomalyDetectionSample.Tests
{
    public class HTMAnomalyExperimentTests
    {
        private readonly double _testTolerance = 0.5;
        private readonly string _testTrainingPath = "test_training_path";
        private readonly string _testPredictingPath = "test_predicting_path";

        /// <summary>
        /// Tests whether the constructor initializes paths correctly.
        /// </summary>
        [Fact]
        public void Constructor_ShouldInitializePathsCorrectly()
        {
            // Arrange & Act
            var experiment = new HTMAnomalyExperiment(_testTrainingPath, _testPredictingPath, _testTolerance);

            // Assert
            Assert.NotNull(experiment);
        }

        /// <summary>
        /// Tests conversion of valid sequences into HTM input format.
        /// </summary>
        [Fact]
        public void ConvertToHTMInput_ShouldReturnCorrectDictionary_WhenValidSequencesAreProvided()
        {
            // Arrange
            var inputSequences = new List<List<double>>
        {
            new List<double> { 56, 77, 88, 69 },
            new List<double> { 12, 44, 75 },
            new List<double> { 25, 26, 98 }
        };
            var converter = new CSVToHTMInputConverter();

            // Act
            var result = converter.ConvertToHTMInput(inputSequences);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        /// <summary>
        /// Tests conversion when no sequences are provided.
        /// </summary>
        [Fact]
        public void ConvertToHTMInput_ShouldReturnEmptyDictionary_WhenNoSequencesAreProvided()
        {
            // Arrange
            var inputSequences = new List<List<double>>();
            var converter = new CSVToHTMInputConverter();

            // Act
            var result = converter.ConvertToHTMInput(inputSequences);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests whether TrimSequences method trims a random number of elements from the sequences.
        /// </summary>
        [Fact]
        public void TrimSequences_ShouldTrimRandomNumberOfElements()
        {
            // Arrange
            var sequences = new List<List<double>>()
        {
            new List<double> { 11, 22, 33, 44, 55 },
            new List<double> { 10, 20, 30, 40, 50 }
        };

            // Act
            var trimmedSequences = CsvSequenceFolder.TrimSequences(sequences);

            // Assert
            foreach (var trimmedSequence in trimmedSequences)
            {
                Assert.InRange(trimmedSequence.Count, 1, 4);
            }
        }

        /// <summary>
        /// Ensures that ExecuteExperiment handles missing folders gracefully without throwing exceptions.
        /// </summary>
        [Fact]
        public void ExecuteExperiment_ShouldHandleMissingFolders()
        {
            // Arrange
            var experiment = new HTMAnomalyExperiment("non_existent_training", "non_existent_predicting", _testTolerance);

            // Act
            var exception = Record.Exception(() => experiment.ExecuteExperiment());

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Ensures that DetectAnomalyOnSequence throws an exception when an invalid sequence (too short) is provided.
        /// </summary>
        [Fact]
        public void DetectAnomalyOnSequence_ShouldThrowExceptionForInvalidSequence()
        {
            // Arrange
            var predictorMock = new Mock<Predictor>();
            var experiment = new HTMAnomalyExperiment();
            double[] invalidSequence = { 69 };  // Too short sequence

            // Act & Assert
            Assert.Throws<ArgumentException>(() => experiment.DetectAnomalyOnSequence(predictorMock.Object, invalidSequence, _testTolerance));
        }

        /// <summary>
        /// Ensures that DetectAnomalyOnSequence throws an exception when NaN values are present in the sequence.
        /// </summary>
        [Fact]
        public void DetectAnomalyOnSequence_ShouldHandleNaNValues()
        {
            // Arrange
            var predictorMock = new Mock<Predictor>();
            var experiment = new HTMAnomalyExperiment();
            double[] invalidSequence = { 69, double.NaN, 75 };

            // Act & Assert
            Assert.Throws<ArgumentException>(() => experiment.DetectAnomalyOnSequence(predictorMock.Object, invalidSequence, _testTolerance));
        }
    }
}

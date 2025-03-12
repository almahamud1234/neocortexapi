using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Utility class for reading, processing, and displaying numerical sequences from CSV files in a specified folder.
    /// </summary>
    public class CsvSequenceFolder
    {
        private readonly string _folderPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvSequenceFolder"/> class with the provided folder path.
        /// Validates if the folder exists before proceeding.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing the CSV files.</param>
        /// <exception cref="ArgumentException">Thrown when folderPath is null or empty.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified folder does not exist.</exception>
        public CsvSequenceFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {folderPath}");
            }

            _folderPath = folderPath;
        }

        /// <summary>
        /// Reads all CSV files in the specified folder and extracts numerical sequences from them.
        /// </summary>
        /// <returns>A list of numerical sequences extracted from the CSV files.</returns>
        public List<List<double>> ExtractSequencesFromFolder()
        {
            var folderSequences = new List<List<double>>();
            var fileEntries = Directory.GetFiles(_folderPath, "*.csv");

            if (!fileEntries.Any())
            {
                Console.WriteLine("No CSV files found in the directory.");
                return folderSequences;
            }

            foreach (var fileName in fileEntries)
            {
                try
                {
                    var csvLines = File.ReadAllLines(fileName);
                    var sequencesInFile = ParseCsvLines(csvLines, fileName);
                    folderSequences.AddRange(sequencesInFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file {fileName}: {ex.Message}");
                }
            }

            return folderSequences;
        }

        /// <summary>
        /// Parses the content of a CSV file into numerical sequences.
        /// Ignores any non-numeric values and logs warnings.
        /// </summary>
        /// <param name="csvLines">The lines read from a CSV file.</param>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <returns>A list of numerical sequences extracted from the CSV file.</returns>
        private List<List<double>> ParseCsvLines(string[] csvLines, string fileName)
        {
            var sequences = new List<List<double>>();

            foreach (var line in csvLines)
            {
                var columns = line.Split(',');
                var sequence = new List<double>();

                foreach (var column in columns)
                {
                    if (double.TryParse(column, out double value))
                    {
                        sequence.Add(value);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Non-numeric value found in file {fileName}. Skipping invalid value: {column}");
                    }
                }

                if (sequence.Count > 0)
                {
                    sequences.Add(sequence);
                }
            }

            return sequences;
        }

        /// <summary>
        /// Displays all extracted sequences from CSV files in the console.
        /// </summary>
        public void DisplayCsvSequences()
        {
            var sequences = ExtractSequencesFromFolder();

            if (sequences.Count == 0)
            {
                Console.WriteLine("No sequences to display.");
                return;
            }

            for (int i = 0; i < sequences.Count; i++)
            {
                Console.WriteLine($"Sequence {i + 1}: {string.Join(" ", sequences[i])}");
            }
        }

        /// <summary>
        /// Trims a random number of elements (between 1 and 4) from the beginning of each sequence.
        /// Ensures sequences with fewer than 5 elements remain unchanged.
        /// </summary>
        /// <param name="sequences">The list of sequences to trim.</param>
        /// <returns>A new list of sequences with trimmed elements.</returns>
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            var random = new Random();
            var trimmedSequences = new List<List<double>>();

            foreach (var sequence in sequences)
            {
                // Ensure sequence has enough elements to trim
                int numElementsToRemove = sequence.Count > 4 ? random.Next(1, 5) : 0;
                trimmedSequences.Add(sequence.Skip(numElementsToRemove).ToList());
            }

            return trimmedSequences;
        }
    }
}
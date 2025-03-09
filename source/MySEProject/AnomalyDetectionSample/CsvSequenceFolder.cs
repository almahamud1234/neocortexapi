using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnomalyDetectionSample
{
    /// <summary>
    /// Utility class for reading and processing CSV files from a specified folder.
    /// </summary>
    public class CsvSequenceFolder
    {
        private readonly string _folderPath;

        /// <summary>
        /// Initializes a new instance of the CsvSequenceFolder class with the provided folder path.
        /// </summary>
        /// <param name="folderPath">The path to the folder containing the CSV files.</param>
        public CsvSequenceFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
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
        /// Reads all CSV files in the specified folder and returns their contents as a list of sequences.
        /// </summary>
        /// <returns>A list of sequences contained in the CSV files present in the folder.</returns>
        public List<List<double>> ExtractSequencesFromFolder()
        {
            var folderSequences = new List<List<double>>();
            var fileEntries = Directory.GetFiles(_folderPath, "*.csv");

            if (fileEntries.Length == 0)
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
        /// Parses the CSV lines into a list of sequences of doubles.
        /// </summary>
        /// <param name="csvLines">Lines read from the CSV file.</param>
        /// <param name="fileName">The name of the file being processed.</param>
        /// <returns>A list of sequences from the CSV file.</returns>
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
        /// Outputs the sequences extracted from the CSV files in the specified folder to the console.
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
                Console.Write($"Sequence {i + 1}: ");
                Console.WriteLine(string.Join(" ", sequences[i]));
            }
        }

        /// <summary>
        /// Trims a random number of elements (between 1 and 4) from the beginning of each sequence in a list of sequences.
        /// </summary>
        /// <param name="sequences">The list of sequences to trim.</param>
        /// <returns>A new list of trimmed sequences.</returns>
        public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
            var random = new Random();
            var trimmedSequences = new List<List<double>>();

            foreach (var sequence in sequences)
            {
                if (sequence.Count > 4)
                {
                    int numElementsToRemove = random.Next(1, 5);
                    var trimmedSequence = sequence.Skip(numElementsToRemove).ToList();
                    trimmedSequences.Add(trimmedSequence);
                }
                else
                {
                    trimmedSequences.Add(new List<double>(sequence)); // Copy sequence if it's too small to trim
                }
            }

            return trimmedSequences;
        }
    }
}

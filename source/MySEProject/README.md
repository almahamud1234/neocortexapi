# ML 24/25-03 Implement Anomaly Detection Sample

## Introduction:

HTM (Hierarchical Temporal Memory) is a machine learning algorithm, which uses a hierarchical network of nodes to process time-series data in a distributed way. Each node, or column, can be trained to learn and recognize patterns in input data. This can be used in identifying anomalies/deviations from normal patterns. It is a promising approach for anomaly detection and prediction in a variety of applications. In our project, we are going to use the MultiSequenceLearning class in NeoCortex API to implement an anomaly detection system, such that numerical sequences are read from multiple CSV files inside a folder, train our HTM Engine, and use the trained engine for learning patterns and detect anomalies.  

## Requirements

To run this project, we need.
* [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* Nuget package: [NeoCortexApi Version= 1.1.4](https://www.nuget.org/packages/NeoCortexApi/)

For code debugging, we recommend using Visual Studio IDE/Visual Studio Code. This project can be run on [github codespaces](https://github.com/features/codespaces) as well.

## Usage

To run this project, 

* Install .NET SDK. Then using the code editor/IDE of your choice, create a new console project and place all the C# codes inside your project folder. 
* Add/reference Nuget package NeoCortexApi v1.1.4 to this project.
* Place numerical sequence CSV Files (datasets) under relevant folders respectively. All the folders should be inside the project folder. More details are given below.

Our project is based on NeoCortex API. More details [here](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/gettingStarted.md).

## Details

To train our HTM Engine, we used the [MultiSequenceLearning](https://github.com/ddobric/neocortexapi/blob/master/source/Samples/NeoCortexApiSample/MultisequenceLearning.cs) class in the NeoCortex API. Firstly, we will read and train the HTM Engine using the data from both our training (learning) and predicting (predictive) folders, which are present as numerical sequences in CSV files in the 'training_sequence' and 'predicting_sequence' folders inside the project directory. We will read numerical sequence data from the prediction folder for testing purposes, remove the first few elements (thus effectively turning the data into a subsequence of the original sequence; we have already inserted anomalies at random indexes into this data), and then use it to detect anomalies.

Please take note that all files inside the folders are read with the.csv extension, and exception handlers are set up in case the file format is incorrect.

Our HTM Engine has been trained using the MultiSequenceLearning class in the NeoCortex API. The first step in training the HTM Engine will be 
reading and utilizing data from our  `training_sequence` (learning) and `predicting_sequence` (predictive) folders, which are both present as numerical sequences in 
JSON files in the `predicting_sequence` and `training_sequence` folders inside the project directory. 

# Data format:
For this project, we used two datasets from a weather dataset for Bangladesh. We took temperature of 20 days for different hours.

Below we give our data sequences where the sequences are in JSON files. We keep our dataset in two individual folders which are `training_files` (for training data where 4 files) and `predicting_files` (for predicting data where also 4 files).  

For example, an hourly sequence of weather temperature of a csv file within training [folder](https://github.com/almahamud1234/neocortexapi/tree/MatrixMasters/source/MySEProject/AnomalyDetectionSample/training_sequence).

```
38, 40, 46, 29, 33, 27, 42, 47, 30, 31 
46, 36, 39, 34, 43, 28, 32, 25, 45, 26 
44, 36, 37, 41, 30, 47, 28, 33, 27, 42 
37, 46, 26, 47, 44, 36, 29, 42, 38, 27 
45, 47, 28, 35, 29, 37, 31, 40, 45, 25
```
Our predicting datasets located in the `predicting_files` [folder](https://github.com/almahamud1234/neocortexapi/tree/MatrixMasters/source/MySEProject/AnomalyDetectionSample/predicting_sequence). sample of one data set is

```
30, 18, 42, 19, 79, 20, 44, 16, 25, 17 
19, 21, 3, 22, 41, 24, 39, 16, 34, 15 
43, 32, 27, 35, 66, 30, 41, 23, 18, 31
42, 16, 30, 45, 22, 28, 76, 36, 39, 25 
37, 42, 26, 24, 56, 41, 45, 28, 16, 32
```
   
### Encoding:

Encoding of our input data is very important, such that it can be processed by our HTM Engine. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/Encoders.md). 

As we are going to train and test data between the range of integer values between 0-100 with no periodicity, we are using the following settings. Minimum and maximum values are set to 0 and 100 respectively, as we are expecting all the values to be in this range only. In other used cases, these values need to be changed.

```csharp

int inputBits = 121;
int numColumns = 1210;
.......................
.......................
double max = 100;

Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 21},
                ...........
                { "MinVal", 0.0},
                ...........
                { "MaxVal", max}
            };
 ```
 
 Complete settings:
 
 ```csharp

Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 21},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "integer"},
                { "ClipInput", false},
                { "MaxVal", max}
            };
```

### HTM Configuration:

We have used the following configuration. More on [this](https://github.com/ddobric/neocortexapi/blob/master/source/Documentation/SpatialPooler.md#parameter-desription)

```csharp
{
                Random = new ThreadSafeRandom(42),

                CellsPerColumn = 25,
                GlobalInhibition = true,
                LocalAreaDensity = -1,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                //InhibitionRadius = 15,

                MaxBoost = 10.0,
                DutyCyclePeriod = 25,
                MinPctOverlapDutyCycles = 0.75,
                MaxSynapsesPerSegment = (int)(0.02 * numColumns),

                ActivationThreshold = 15,
                ConnectedPermanence = 0.5,

                // Learning is slower than forgetting in this case.
                PermanenceDecrement = 0.25,
                PermanenceIncrement = 0.15,

                // Used by punishing of segments.
                PredictedSegmentDecrement = 0.1
};
```

### Multisequence learning

The [multisequencelearning](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs) class file's [RunExperiment](https://github.com/almahamud1234/neocortexapi/blob/a05b009aaa68c6f3c7961583e2c247ee943d98a9/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L74) method provides an example of how multisequence learning functions. As a summary,

* The system initialization process begins with the initialization of connection memory and HTM configuration. Next, the HTM Classifier, Cortex Layer, and Homeostatic Plasticity Controller are initialized. 

* Finally, Temporal Memory and Spatial Pooler are set up to complete the initializatio

```csharp
.....
TemporalMemory tm = new TemporalMemory();
SpatialPoolerMT sp = new SpatialPoolerMT(hpc);
.....
```
* The cortical layer is then integrated with spatial pooler memory, which is trained for the maximum number of cycles.

```csharp
.....
layer1.HtmModules.Add("sp", sp);
int maxCycles = 3500;
for (int i = 0; i < maxCycles && isInStableState == false; i++)
.....
`````
* Temporal memory is then introduced to the cortical layer to learn every input sequence.

```csharp
.....
layer1.HtmModules.Add("tm", tm);
foreach (var sequenceKeyPair in sequences){
.....
}
.....
```
* The HTM classifier and trained cortical layer are finally returned. More [here](https://github.com/almahamud1234/neocortexapi/blob/a05b009aaa68c6f3c7961583e2c247ee943d98a9/source/MySEProject/AnomalyDetectionSample/multisequencelearning.cs#L298)


## Execution of the project

To run this project, use the following class/methods given in [Program.cs].

Our project is carried out as follows.
 
* At the beginning, we use the `ExtractSequencesFromFolder` method of the [`CsvSequenceFolder`](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/CsvSequenceFolder.cs) class to read all files inside a folder. These classes maintain a list of numerical sequences from the read data for repeated future use. To handle non-numeric data, exception handling is incorporated within certain classes. Additionally, the `TrimSequences` technique allows data trimming by removing one to four components (numbers 1 through 4) from the start, returning a numeric sequence.

```csharp
 public List<List<double>> ExtractSequencesFromFolder()
        {
         ....  
          return folderSequences;
        }

public static List<List<double>> TrimSequences(List<List<double>> sequences)
        {
        ....
          return trimmedSequences;
        }
```

* Next, the `ConvertToHTMInput` method of the [`CSVToHTMInputConverter`](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/CSVToHTMInputConverter.cs) class is used to convert all the read sequences into a format suitable for HTM training.

```csharp
Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>();
for (int i = 0; i < sequences.Count; i++)
    {
     // Unique key created and added to dictionary for HTM Input                
     string key = "S" + (i + 1);
     List<double> value = sequences[i];
     dictionary.Add(key, value);
    }
     return dictionary;
```
After that, the `ExecuteHTMModelTraining` method of the [`HTMTrainingManager`](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/HTMTrainingManager.cs) class is used to train the model using the converted sequences. The numerical data sequences from both the training (for learning) and predicting folders are combined before training the HTM engine. This class then returns the trained model object, which serves as the predictor.

```csharp
.....
MultiSequenceLearning learning = new MultiSequenceLearning();
predictor = learning.Run(htmInput);
.....
.....
List<List<double>> combinedSequences = new List<List<double>>(sequences1);
combinedSequences.AddRange(sequences2);
.....
```
In the end, we use the [`HTMAnomalyExperiment`](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/HTMAnomalyExperiment.cs) class to detect anomalies in sequences read from files inside the predicting folder. All the previously explained classes—CSV file reading (using `CsvSequenceFolder`), combining and converting them for HTM training (using `CSVToHTMInputConverter`), and training the HTM engine (via `HTMTrainingManager`)—are utilized here. The same `CsvSequenceFolder` class is used to read files for the predicting sequences. The `TrimSequences` method is then applied to trim sequences for anomaly testing, as explained earlier.

```csharp
.....
 allTestingData.Add(sequence.ToArray());
var (log, anomalies) = DetectAnomalyOnSequence(predictor, sequence.ToArray(), _tolerance);
experimentResults.AddRange(log);
allAnomalyIndices.Add(anomalies);
.....
```
The path to the training and predicting folders is set as the default and passed through the constructor, or it can be manually set within the class.

```csharp
.....
_trainingCSVFolderPath = Path.Combine(projectBaseDirectory, trainingFolderPath);
_predictingCSVFolderPath = Path.Combine(projectBaseDirectory, predictingFolderPath);
.....
```
In the end, the `DetectAnomaly` method is used to detect anomalies in our trimmed sequences one by one, utilizing the trained HTM model predictor.
 
```csharp
foreach (List<double> list in triminputtestseq)
       {
         .....
         double[] lst = list.ToArray();
         DetectAnomaly(myPredictor, lst);
       }
```
Exception handling is implemented to manage errors thrown by the `DetectAnomaly` method, such as passing non-numeric values or when the number of elements in the list is less than two.

The [`DetectAnomaly`](https://github.com/almahamud1234/neocortexapi/blob/a05b009aaa68c6f3c7961583e2c247ee943d98a9/source/MySEProject/AnomalyDetectionSample/HTMAnomalyExperiment.cs#L68) method, which is the main method of the `ExtractSequencesFromFolder` class, detects anomalies in our data. It traverses each value in the list one by one in a sliding window manner, using the trained model predictor to predict the next element for comparison. An anomaly score is used to quantify the comparison, and if the prediction exceeds a certain tolerance level, it is flagged as an anomaly.

We can obtain our prediction in a list of results in the format of `NeoCortexApi.Classifiers.ClassifierResult`1[System.String]` from our trained model predictor using the following method:

```csharp
var res = predictor.Predict(item);
```

Normally output from HTM is in the following format when we pass a numerical value 14 for example:

```csharp
S3_11-5-12-10-14-13 - 100
S1_5-6-16-10-4-11-7 - 5
.....
```
The first line has the best prediction which the HTM model predicts, with accuracy. We can easily derive the predicted value, which will come after 14 (in this case, it is 13). The string operations are used to get these values. Later we are going to use this to determine anomalies.
```csharp
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
```

We are using AnomalyScore, which is nothing, but the absolute value of the ratio of differences between HTM´s predicted number and actual number. If the ratio exceeds tolerancevalue, we mark it as an anomaly, otherwise, it is not. When an anomaly is detected, we skip that element in the list (we did not pass that value to HTM in the loop).

We use accuracyPerList to record accuracy per numerical sequence tested. recordAccuracy is collected from inside each loop run, which indicates HTM model´s accuracy. We also add index positions of anomalies to anomalyIndices, when we encounter indices of anomalies in loop. totalAccuracy and listCount is used to calculate average accuracy of the whole experiment.

```csharp
StoredOutputValues.totalAvgAccuracy = _totalAccuracy / Math.Max(_iterationCount, 1);
```

The SequenceVisualizer [class](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample/SequenceVisualizer.cs) is used to plot graphs of sequences of data and their anomalies. This class used to showing the visual result for detecting anomalies comparing to training sequences and the predicting sequence.

```csharp
public static void CreateGraphForSequences(List<double[]> allLearnedData, List<double[]> allTestingData)
.....
allGraphs.Add(actualGraph);
allGraphs.Add(learnedGraph);
.....
var chart = Chart.Plot(allGraphs);
```

## Unit Tests

We have developed a unit tests project to test different functionality of this anomaly project. During implemeted the unit tests, we ensured the behavior of detection of anomalies in the project. Project files for the units test can be found [here](https://github.com/almahamud1234/neocortexapi/blob/MatrixMasters/source/MySEProject/AnomalyDetectionSample.Tests/HTMAnomalyExperimentTests.cs)

![Image](https://github.com/user-attachments/assets/e74a54b5-fe1d-4a40-b50b-b84692decdc9)

## Results

Once the experiment concludes, the anomaly detection results are persisted to the screen, along with HTM accuracy for each individual number sequences and overall HTM accuracy for the whole experiment. We have uploaded the anomaly results of our data in this repository for reference. output result of combined numerical sequence data from training folder (without anomalies) and predicting folder (with anomalies) can be found [here](https://github.com/almahamud1234/neocortexapi/tree/MatrixMasters/source/MySEProject/AnomalyDetectionSample/output)

# Graphical view

We shown the graphical view of the training and predicting sequences along with anomalies. After successfully running the projects, we are generating the plots in html format in the browser as well as saving the files in the output graph [folder](https://github.com/almahamud1234/neocortexapi/tree/MatrixMasters/source/MySEProject/AnomalyDetectionSample/output/graph). We generated 2 kind of plots

1. Training and preicting sequence plot
![Image](https://github.com/user-attachments/assets/0926d42e-52fd-4abb-a505-f331dc5e245f)

3. Training and preicting sequence with anomalies plot
![Image](https://github.com/user-attachments/assets/24943024-cb4c-498a-bbf3-8043b8e9190d)

We found Anomaly detection results for Testing the sequence: 30, 18, 42, 19, 79, 20, 44, 16, 25, 17 

FNR = FN / (FN + TP) = 0/ (0+2) = 0
FPR = FP / (FP + TN) = 2/ (2+5) = 0.29
Where, FN = 0, FP = 2, TN = 5, TP = 2.

After running our sample project, we analyzed the output folder of this experiment and got the following average results:

• Average FNR of the experiment: 0.22
• Average FPR of the experiment: 0.28

We can observe that the False Negative Rate(FNR) is in our output (0.22). It is desired that the false negative rate should be as lower as possible in an anomaly detection program. Lower false positive rate is also desirable, but not absolutely essential.

Although, it depends on a number of factors, like quantity (the more, the better) and quality of data, and hyperparameters used to tune and train model; more data should be used for training, and hyperparameters should be further tuned to find the most optimal setting for training to get the best results. We were using less amount of numerical sequences as data to demonstrate our sample project due to time and computational constraints, but that can be improved if we use better resources, like cloud.

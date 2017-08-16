using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VisistsRecordsFilesProcessor
{
    public class FilesProcessor
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(FilesProcessor));

        private readonly string dataFolder;
        private readonly string alreadyProcessedListPath;
        private readonly string backendAddress;
        private readonly char csvFileSeparator;
        private readonly CancellationToken ct;

        public event Action StopComplete;

        public FilesProcessor(string dataFolder, string backendAddress, char csvFileSeparator, CancellationToken ct)
        {
            this.dataFolder = dataFolder;
            this.alreadyProcessedListPath = Path.Combine(dataFolder, "processed.txt");

            this.backendAddress = backendAddress;
            this.csvFileSeparator = csvFileSeparator;
            this.ct = ct;
        }

        public void Run()
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                while (true)
                {
                    ct.ThrowIfCancellationRequested();

                    // List new CSV files in the directory, ignore files already processed
                    // PROS: we don't process the same files over and over
                    // CONS: we must create a new file we a different name to add new records (or remove the corresponding line from the processed.txt file)

                    var alreadyProcessed = ReadProcessedFilesList();

                    var files = Directory.GetFiles(dataFolder, "*.csv").Except(alreadyProcessed);

                    foreach (var file in files)
                    {
                        ct.ThrowIfCancellationRequested();

                        var processor = new FileProcessor(file, csvFileSeparator, backendAddress, ct);
                        processor.ProcessFile().Wait();

                        // Mark the file as processed so that it is not picked up later
                        File.AppendAllLines(alreadyProcessedListPath, new string[1] { file });
                    }

                    ct.ThrowIfCancellationRequested();

                    // The main program loop will run every 5 seconds and watch for new files
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
            catch (OperationCanceledException)
            {
                logger.Warn("Terminating loop as requested");
            }
            catch (Exception ex)
            {
                logger.Error("Caught exception", ex);
            }
            finally
            {
                StopComplete?.Invoke();
            }
        }

        private string[] ReadProcessedFilesList()
        {
            if (!File.Exists(alreadyProcessedListPath))
                return new string[0];
            else
                return File.ReadAllLines(alreadyProcessedListPath);
        }
    }
}

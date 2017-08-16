using Flurl.Http;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VisistsRecordsFilesProcessor
{
    internal class FileProcessor
    {
        private readonly ILog logger = LogManager.GetLogger(nameof(FileProcessor));

        private readonly string filePath;
        private readonly char csvFileSeparator;
        private readonly string backendAddress;
        private readonly CancellationToken ct;

        public FileProcessor(string filePath, char csvFileSeparator, string backendAddress, CancellationToken ct)
        {
            this.filePath = filePath;
            this.csvFileSeparator = csvFileSeparator;
            this.backendAddress = backendAddress;
            this.ct = ct;
        }

        public async Task ProcessFile()
        {
            try
            {
                var records = ReadFile();

                if (records.Count > 0)
                    await UploadRecords(records);
                else
                    logger.Warn($"No valid website visits record extracted from file {filePath}");
            }
            catch (Exception ex)
            {
                logger.Error("Caught exception", ex);
            }
        }

        private List<WebsiteVisitRecord> ReadFile()
        {
            logger.Info($"Reading file {filePath}");

            List<WebsiteVisitRecord> records = new List<WebsiteVisitRecord>();

            string[] lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                try
                {
                    string[] parts = line.Split(csvFileSeparator);

                    if (parts.Length != 3)
                        throw new ArgumentOutOfRangeException();

                    if (!DateTime.TryParse(parts[0], out DateTime date))
                        throw new ArgumentException(nameof(date));

                    string website = parts[1];
                    if (string.IsNullOrEmpty(website))
                        throw new ArgumentException(nameof(website));

                    if (!int.TryParse(parts[2], out int visits))
                        throw new ArgumentException(nameof(visits));

                    var record = new WebsiteVisitRecord(website, date, visits);

                    logger.Info($"Extracted record from file: {record}");

                    records.Add(record);
                }
                catch (ArgumentOutOfRangeException)
                {
                    logger.Error($"Failed to read line '{line}': expected three parts separated by {csvFileSeparator}");
                }
                catch (ArgumentException ex)
                {
                    logger.Error($"Failed to read line '{line}': missing or invalid data {ex.ParamName}");
                }
            }

            return records;
        }

        private async Task UploadRecords(List<WebsiteVisitRecord> records)
        {
            var response = await backendAddress.PostJsonAsync(records, ct);

            if (response.IsSuccessStatusCode)
                logger.Info($"Successfully uploaded {records.Count} website visits records to the backend");
            else
                logger.Error($"Website visits records upload failed with status {response.StatusCode}: {response.Content}");
        }
    }
}

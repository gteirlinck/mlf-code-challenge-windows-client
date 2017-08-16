using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisistsRecordsFilesProcessor;

namespace WindowsClientConsoleApp
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(nameof(Program));

        static void Main(string[] args)
        {
            XmlConfigurator.Configure();

            logger.Info("Starting data watcher");

            Run();

            logger.Info("Exiting data watcher");
        }

        private static void Run()
        {
            string dataFolder = ConfigurationManager.AppSettings["DataFolder"];
            string backendEndpoint = ConfigurationManager.AppSettings["BackendEndpoint"];
            char csvFileSeparator = ConfigurationManager.AppSettings["CsvFileSeparator"][0];

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(2));

            FilesProcessor filesProcessor = new FilesProcessor(dataFolder, backendEndpoint, csvFileSeparator, cts.Token);
            filesProcessor.Run();
        }
    }
}

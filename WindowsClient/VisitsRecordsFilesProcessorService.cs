using log4net;
using log4net.Config;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using VisistsRecordsFilesProcessor;

namespace WindowsClient
{
    public partial class VisitsRecordsFilesProcessorService : ServiceBase
    {
        private static ILog logger = LogManager.GetLogger(nameof(VisitsRecordsFilesProcessorService));

        private CancellationTokenSource stopRequestedCts;
        private AutoResetEvent stopCompleteEvent;

        public VisitsRecordsFilesProcessorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            XmlConfigurator.Configure();

            string dataFolder = ConfigurationManager.AppSettings["DataFolder"];
            string backendEndpoint = ConfigurationManager.AppSettings["BackendEndpoint"];
            char csvFileSeparator = ConfigurationManager.AppSettings["CsvFileSeparator"][0];

            logger.Info("Starting data watcher service");

            try
            {
                stopRequestedCts = new CancellationTokenSource();
                stopCompleteEvent = new AutoResetEvent(false);

                FilesProcessor filesProcessor = new FilesProcessor(dataFolder, backendEndpoint, csvFileSeparator, stopRequestedCts.Token);
                filesProcessor.StopComplete += () => stopCompleteEvent.Set();

                Task.Run(() =>
                {
                    filesProcessor.Run();
                });
            }
            catch (Exception ex)
            {
                logger.Fatal("Caught fatal unhandled exception. The process will now exit", ex);

                OnStop();
            }
        }

        protected override void OnStop()
        {
            logger.Info("Stopping data watcher service");

            stopRequestedCts?.Cancel();

            logger.Info("Waiting for the service to stop");

            stopCompleteEvent?.WaitOne();

            logger.Info("Exiting data watcher service");
        }
    }
}

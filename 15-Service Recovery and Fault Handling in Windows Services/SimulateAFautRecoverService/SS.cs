using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimulateAFautRecoverService
{
    public partial class SS : ServiceBase
    {

        private string logFilePath = @"C:\ServiceLogs\RecoveryLog.txt";
        public SS()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            LogEvent("Service Started");

            // Start a background task with fault handling
            Thread workerThread = new Thread(WorkerTask);
            workerThread.Start();
        }

        protected override void OnStop()
        {
            LogEvent("Service Stopped");
        }

        private void WorkerTask()
        {
            try
            {
                // Simulate work
                while (true)
                {
                    LogEvent("Service is running...");
                    Thread.Sleep(5000);

                    // Simulate a failure
                    throw new Exception("Simulated error for testing recovery.");
                }
            }
            catch (Exception ex)
            {
                LogEvent($"Error: {ex.Message}");
                // Exit the process to simulate failure
                Environment.Exit(1);
            }
        }

        private void LogEvent(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
    }
}

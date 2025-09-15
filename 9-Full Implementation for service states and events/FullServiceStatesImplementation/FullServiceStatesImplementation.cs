using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FullServiceStatesImplementation
{
    public partial class FullServiceStatesImplementation : ServiceBase
    {

        string logDirectory;
        string logFilePath;
        public FullServiceStatesImplementation()
        {
            InitializeComponent();
            CanPauseAndContinue = true; //The service supports pausing and resuming operations.

            // Enable support for OnShutdown
            CanShutdown = true; // The service is notified when the system shuts down.


            // Read log directory path from App.config
            //The service reads the log directory path from an external configuration file (App.config) for flexibility.
            logDirectory = ConfigurationManager.AppSettings["LogDirectory"];


            // Validate and create directory if it doesn't exist
            if (string.IsNullOrWhiteSpace(logDirectory))
            {
                throw new ConfigurationErrorsException("LogDirectory is not specified in the configuration file.");
            }

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            logFilePath = Path.Combine(logDirectory, "ServiceStateLog.txt");


            Process process = Process.GetCurrentProcess();
            process.PriorityClass = ProcessPriorityClass.RealTime;
           

        }


        private  void LogServiceEvent(string message)
        {

          
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
            File.AppendAllText(logFilePath, logMessage);
        }


        protected override void OnStart(string[] args)
        {
            LogServiceEvent("Service Started");

        }

        protected override void OnStop()
        {
            LogServiceEvent("Service Stoped");
        }

        protected override void OnContinue()
        {
            LogServiceEvent("Servie Resumed");
        }

        protected override void OnPause()
        {
            LogServiceEvent("Service Paused");
        }

        protected override void OnShutdown()
        {
            LogServiceEvent("Service Shutdown due to system shutdown");
        }


        [Conditional("DEBUG")]
        public  static void SimulateService()
        {

            Console.WriteLine("FSSI Service Run In the Debug Mode  ...");
            FullServiceStatesImplementation fullServiceStatesImplementation = new FullServiceStatesImplementation();
            Console.WriteLine("Please Enter any key to start the service ... ");
            Console.ReadKey();
            Console.WriteLine("Service is Started : ");
            fullServiceStatesImplementation.OnStart(null);
            Console.WriteLine("Please Enter any key to Pause the service ... ");
            Console.ReadKey();
            fullServiceStatesImplementation.OnPause();

            Console.WriteLine("Please Enter any key to Resume The service ... ");
            Console.ReadKey();


            fullServiceStatesImplementation.OnContinue();

            Console.WriteLine("Please Enter any key to Stop the service ... ");
            Console.ReadKey();
            fullServiceStatesImplementation.OnStop();



           

            
        }
    }
}

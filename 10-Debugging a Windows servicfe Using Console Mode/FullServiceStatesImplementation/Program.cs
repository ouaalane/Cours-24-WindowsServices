using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FullServiceStatesImplementation
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {



            // this fonction will run the debug mode
            FullServiceStatesImplementation.SimulateService();

            if (!Environment.UserInteractive)
            {

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new FullServiceStatesImplementation()
                };
                ServiceBase.Run(ServicesToRun);
            }
            
        }
    }
}

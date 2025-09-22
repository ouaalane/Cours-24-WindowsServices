using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace FileMonitoringService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller processInstaller;
        private ServiceInstaller serviceInstaller;

        public ProjectInstaller()
        {
            InitializeComponent();

            // Service account
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            // Service configuration
            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "FileMonitoringService",
                DisplayName = "File Monitoring Service",
                StartType = ServiceStartMode.Automatic,
                Description = "This is my file monitoring service.",
                ServicesDependedOn = new string[] { "RpcSs", "EventLog", "LanmanWorkstation" } // Dependencies
            };

            /*
             FileSystemWatcher indirectly relies on core services like RPC, Event Log, and Workstation/Server Services for network operations.
             
             Adding these services as dependencies ensures smooth operation of your Windows Service when using FileSystemWatcher, 
             especially in scenarios involving network shares or remote drives.
             
             */

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);



        }
    }
}

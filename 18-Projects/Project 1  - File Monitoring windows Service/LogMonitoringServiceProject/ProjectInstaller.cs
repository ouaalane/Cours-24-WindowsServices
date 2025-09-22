using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace LogMonitoringServiceProject
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {

        ServiceProcessInstaller _ServiceProcessInstaller;
        ServiceInstaller _ServiceInstaller;

        
        public ProjectInstaller()
        {
            InitializeComponent();


            _ServiceInstaller = new ServiceInstaller
            {
                ServiceName = "FileMonitoringService",
                DisplayName = "File Monitoring Service",
                StartType = ServiceStartMode.Automatic,
                Description = "This Service is able to monitor a specific folder for new files procces them by reneming with guid and move them to destination flder ",
                ServicesDependedOn = new string[] { "RpcSs", "EventLog", "LanmanWorkstation" } // Dependencies
            };

            _ServiceProcessInstaller = new ServiceProcessInstaller
            { 
                Account = ServiceAccount.LocalService,
               
                
            };


            Installers.Add(_ServiceProcessInstaller);
            Installers.Add(_ServiceInstaller);
        }
    }
}

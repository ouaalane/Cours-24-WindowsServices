using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace MyFirstWinService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {



        private ServiceProcessInstaller _serviceProcessInstaller;
        private ServiceInstaller _serviceInstaller;
        public ProjectInstaller()
        {


            InitializeComponent();


            _serviceProcessInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };


            _serviceInstaller = new ServiceInstaller
            {
                ServiceName = "MyFirstWinService", // must match your servicename in your service base class  
                DisplayName = "My First Windows Service",
                StartType = ServiceStartMode.Manual
            };






            Installers.Add(_serviceProcessInstaller);
            Installers.Add(_serviceInstaller);




        }
    }
}

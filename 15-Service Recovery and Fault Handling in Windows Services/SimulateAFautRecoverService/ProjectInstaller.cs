using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SimulateAFautRecoverService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public ProjectInstaller()
        {
            processInstaller = new ServiceProcessInstaller
            {
                Account = ServiceAccount.LocalSystem
            };

            serviceInstaller = new ServiceInstaller
            {
                ServiceName = "SS",
                DisplayName = "SS",
                StartType = ServiceStartMode.Automatic
            };

            Installers.Add(processInstaller);
            Installers.Add(serviceInstaller);
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            // Configure recovery: restart service after 1 minute if it crashes
            ConfigureServiceRecovery(serviceInstaller.ServiceName);
        }

        private void ConfigureServiceRecovery(string serviceName)
        {
            string args = $"failure \"{serviceName}\" reset= 86400 actions= restart/60000/restart/60000/restart/60000";

            ProcessStartInfo psi = new ProcessStartInfo("sc.exe", args)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }
    }
}
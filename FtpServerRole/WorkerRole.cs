using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.Common;
using SuperSocket.SocketEngine.Configuration;
using System.Configuration;

namespace FtpServerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("FtpServerRole entry point called", "Information");

            while (true)
            {
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            LogUtil.Setup();
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 100;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;

            if (!SocketServerManager.Initialize(serverConfig, ResolveServerConfig))
            {
                Trace.WriteLine("Failed to initialize SuperSocket!", "Error");
                return false;
            }

            if (!SocketServerManager.Start())
            {
                Trace.WriteLine("Failed to start SuperSocket!", "Error");
                return false;
            }

            return base.OnStart();
        }

        private IServerConfig ResolveServerConfig(IServerConfig serverConfig)
        {
            var config = new ServerConfig();
            serverConfig.CopyPropertiesTo(config);

            RoleInstanceEndpoint instanceEndpoint;
            if (!RoleEnvironment.CurrentRoleInstance.InstanceEndpoints.TryGetValue(serverConfig.Name + "Endpoint", out instanceEndpoint))
            {
                Trace.WriteLine(string.Format("Failed to find Input Endpoint configuration {0}!", serverConfig.Name + "Endpoint"), "Error");
                return serverConfig;
            }

            var ipEndpoint = instanceEndpoint.IPEndpoint;
            config.Ip = ipEndpoint.Address.ToString();
            config.Port = ipEndpoint.Port;

            List<int> dataPorts = new List<int>();

            int dataPortIndex = 0;

            while (true)
            {
                var dataPortKey = "FTPDataPort" + dataPortIndex++;

                RoleInstanceEndpoint dataEndpoint;
                if (!RoleEnvironment.CurrentRoleInstance.InstanceEndpoints.TryGetValue(dataPortKey, out dataEndpoint))
                    break;

                dataPorts.Add(dataEndpoint.IPEndpoint.Port);
            }

            if (dataPorts.Count > 0)
                config.Options["dataPort"] = string.Join(",", dataPorts.Select(p => p.ToString()).ToArray());

            return config;
        }

        public override void OnStop()
        {
            SocketServerManager.Stop();
            base.OnStop();
        }
    }
}

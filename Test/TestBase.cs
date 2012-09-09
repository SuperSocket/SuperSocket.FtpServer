using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;

namespace SuperSocket.Ftp.Test
{
    public abstract class TestBase
    {
        private IBootstrap m_BootStrap;

        protected IBootstrap Bootstrap
        {
            get { return m_BootStrap; }
        }

        private Encoding m_Encoding = new UTF8Encoding();

        [TearDown]
        public void ClearBootstrap()
        {
            if (m_BootStrap != null)
            {
                m_BootStrap.Stop();
                m_BootStrap = null;
            }
        }

        protected IConfigurationSource CreateBootstrap(string configFile)
        {
            var fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = Path.Combine(@"Config", configFile);

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var configSource = config.GetSection("socketServer") as IConfigurationSource;

            m_BootStrap = BootstrapFactory.CreateBootstrap(configSource);

            return configSource;
        }

        protected IConfigurationSource SetupBootstrap(string configFile)
        {
            var configSource = CreateBootstrap(configFile);

            Assert.IsTrue(m_BootStrap.Initialize());

            var result = m_BootStrap.Start();

            Assert.AreEqual(StartResult.Success, result);

            return configSource;
        }
    }
}

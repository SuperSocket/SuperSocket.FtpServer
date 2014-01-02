using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.FtpClient;
using System.Text;
using NUnit.Framework;
using SuperSocket.SocketBase;
using System.Net;

namespace SuperSocket.Ftp.Test
{
    [TestFixture]
    public class BasicTest : TestBase
    {
        [Test]
        public void TestStartStop()
        {
            var config = SetupBootstrap("Basic.config");
            Bootstrap.Stop();
        }


        [Test]
        public void TestWorkFlow()
        {
            var config = SetupBootstrap("Basic.config");

            using(var client = new FtpClient())
            {
                client.Host = "localhost";
                client.InternetProtocolVersions = FtpIpVersion.IPv4;
                client.Credentials = new NetworkCredential("kerry", "123456");
                client.DataConnectionType = FtpDataConnectionType.PASV;
                client.Connect();
                Assert.AreEqual(true, client.IsConnected);
                var workDir = client.GetWorkingDirectory();
                Assert.AreEqual("/", workDir);
                Console.WriteLine("EncryptionMode: {0}", client.EncryptionMode);
                foreach (var item in client.GetListing(workDir, FtpListOption.Size))
                {
                    Console.WriteLine(item.Name);
                }
            }
        }
    }
}

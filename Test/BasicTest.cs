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
        public void TestConnection()
        {
            var config = SetupBootstrap("Basic.config");

            using(var client = new FtpClient("kerry", "123456", "localhost"))
            {
                client.Connect();
                Assert.AreEqual(true, client.Connected);
            }
        }
    }
}

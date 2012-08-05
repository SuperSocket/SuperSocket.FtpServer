using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Raccent.Ftp.FtpService.Console
{
    [ServiceContract]
    public interface IStatusReporter
    {
        //[OperationContract]
        //int GetCurrentConnectionCount();

        [OperationContract]
        int GetOnlineUserCount();

        [OperationContract]
        string Ping();
    }
}

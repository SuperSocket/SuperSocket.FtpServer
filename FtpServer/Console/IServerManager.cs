using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Raccent.Ftp.FtpService.Console
{
    [ServiceContract]
    public interface IServerManager
    {
        [OperationContract]
        void Start();

        [OperationContract]
        void Stop();

        [OperationContract]
        void Restart();
    }
}

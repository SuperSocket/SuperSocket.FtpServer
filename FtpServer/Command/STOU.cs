using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class STOU : STOR
    {
        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            string param = requestInfo.Body;
            if (string.IsNullOrEmpty(requestInfo.Body))
                param = Guid.NewGuid().ToString();

            base.ExecuteCommand(session, new StringRequestInfo(requestInfo.Key, param, new string[]{ }));
        }
    }
}

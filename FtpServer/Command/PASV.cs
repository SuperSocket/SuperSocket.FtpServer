using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class PASV : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            int port = DataConnection.GetPassivePort(session);

            if (port > 0)
            {
                string address = ((IPEndPoint)session.LocalEndPoint).Address.ToString().Replace('.', ',') + "," + (port >> 8) + "," + (port & 0xFF);
                session.CurrentDataConnectionPort = port;
                session.Send(Resource.PassiveEnter_227, address);
            }
            else
            {
                session.Send(Resource.DataConnectionCannotOpen_420);
            }
        }

        #endregion
    }
}

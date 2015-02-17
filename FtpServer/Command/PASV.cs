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

            DataConnection dataConnection;

            if (DataConnection.TryOpenDataConnection(session, out dataConnection))
            {
                var port = dataConnection.Port;

                var localAddress = session.AppServer.ExternalLocalAddress;

                if(string.IsNullOrEmpty(localAddress))
                    localAddress = ((IPEndPoint)session.LocalEndPoint).Address.ToString();

                string address = localAddress.Replace('.', ',') + "," + (port >> 8) + "," + (port & 0xFF);
                session.DataConnection = dataConnection;
                session.Send(FtpCoreResource.PassiveEnter_227, address);
            }
            else
            {
                session.Send(FtpCoreResource.DataConnectionCannotOpen_420);
            }
        }

        #endregion
    }
}

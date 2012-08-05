using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class PORT : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string address = requestInfo.GetFirstParam();

            string[] arrAddress = new string[0];

            if (!string.IsNullOrEmpty(address))
            {
                arrAddress = address.Split(',');
            }

            if (arrAddress == null || arrAddress.Length != 6)
            {
                session.SendParameterError();
                return;
            }

            string ip = arrAddress[0] + "." + arrAddress[1] + "." + arrAddress[2] + "." + arrAddress[3];
            int port = (Convert.ToInt32(arrAddress[4]) << 8) | Convert.ToInt32(arrAddress[5]);

            if (DataConnection.TrySocketPort(session, port))
            {
                session.CurrentDataConnectionPort = port;
                session.Send(Resource.PortOk_220);
                return;
            }
            else
            {
                session.Send(Resource.PortInvalid_552);
                return;
            }
        }

        #endregion
    }
}

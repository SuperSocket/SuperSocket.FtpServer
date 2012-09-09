using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class DELE : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string filename = requestInfo.Body;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            if (session.AppServer.FtpServiceProvider.DeleteFile(session.Context, filename))
            {
                session.Send(Resource.DeleteOk_250);
            }
            else
            {
                session.Send(session.Context.Message);
            }
        }

        #endregion
    }
}

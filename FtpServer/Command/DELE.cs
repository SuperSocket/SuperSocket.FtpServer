using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class DELE : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string filename = requestInfo.Data;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            if (session.AppServer.FtpServiceProvider.DeleteFile(session.Context, filename))
            {
                session.SendResponse(Resource.DeleteOk_250);
            }
            else
            {
                session.SendResponse(session.Context.Message);
            }
        }

        #endregion
    }
}

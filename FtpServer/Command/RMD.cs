using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class RMD : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string foldername = requestInfo.Data;

            if (string.IsNullOrEmpty(foldername))
            {
                session.SendParameterError();
                return;
            }

            if (session.AppServer.FtpServiceProvider.RemoveFolder(session.Context, foldername))
            {
                session.SendResponse(Resource.RemoveOk_250, session.Context.CurrentPath + "/" + foldername);
            }
            else
            {
                session.SendResponse(session.Context.Message);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class RMD : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string foldername = requestInfo.Body;

            if (string.IsNullOrEmpty(foldername))
            {
                session.SendParameterError();
                return;
            }

            if (session.AppServer.FtpServiceProvider.RemoveFolder(session.Context, foldername))
            {
                session.Send(Resource.RemoveOk_250, session.Context.CurrentPath + "/" + foldername);
            }
            else
            {
                session.Send(session.Context.Message);
            }
        }

        #endregion
    }
}

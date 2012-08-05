using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class MKD : StringCommandBase<FtpSession>
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

            if (session.AppServer.FtpServiceProvider.CreateFolder(session.Context, foldername))
            {
                session.Send(Resource.MakeDirOk_250, session.Context.CurrentPath + "/" + foldername);
            }
            else
            {
                session.Send(session.Context.Message);
            }
        }

        #endregion
    }
}

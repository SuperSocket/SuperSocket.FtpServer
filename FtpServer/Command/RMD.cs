using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class RMD : FtpCommandBase
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
                session.Send(FtpCoreResource.RemoveOk_250, CombinePath(session.Context.CurrentPath, foldername));
            }
            else
            {
                session.Send(session.Context.Message);
            }
        }

        #endregion

        private string CombinePath(string dir, string name)
        {
            if (dir.EndsWith("/"))
                return dir + name;
            else
                return dir + "/" + name;
        }
    }
}

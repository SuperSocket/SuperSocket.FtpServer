using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class CWD : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string path = requestInfo.Body;

            if (string.IsNullOrEmpty(path))
            {
                session.SendParameterError();
                return;
            }

            long folderID;

            if (session.AppServer.FtpServiceProvider.IsExistFolder(session.Context, path, out folderID))
            {
                session.Context.CurrentPath = path;
                session.Context.CurrentFolderID = folderID;
                session.Send(Resource.ChangeWorkDirOk_250, path);
            }
            else
            {
                if (session.Context.Status == FtpStatus.Error)
                    session.Send(session.Context.Message);
                else
                    session.Send(Resource.NotFound_550);
            }
        }

        #endregion
    }
}

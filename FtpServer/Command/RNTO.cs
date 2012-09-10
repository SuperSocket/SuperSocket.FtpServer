using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class RNTO : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            string newfileName = requestInfo.Body;

            if (string.IsNullOrEmpty(newfileName))
            {
                session.SendParameterError();
                return;
            }

            if (session.Context.RenameItemType == ItemType.File)
            {
                if (!session.AppServer.FtpServiceProvider.RenameFile(session.Context, session.Context.RenameFor, newfileName))
                {
                    session.Send(session.Context.Message);
                    return;
                }
            }
            else
            {
                if (!session.AppServer.FtpServiceProvider.RenameFolder(session.Context, session.Context.RenameFor, newfileName))
                {
                    session.Send(session.Context.Message);
                    return;
                }
            }

            session.Send(FtpCoreResource.RenameToOk_250);
        }

        #endregion
    }
}

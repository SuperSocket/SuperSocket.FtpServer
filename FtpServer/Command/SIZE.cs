using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class SIZE : StringCommandBase<FtpSession>
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

            long size = session.AppServer.FtpServiceProvider.GetFileSize(session.Context, filename);

            if (session.Context.Status == FtpStatus.Error)
                session.SendResponse(session.Context.Message);
            else
                session.SendResponse(Resource.SizeOk_213, size);
        }

        #endregion
    }
}

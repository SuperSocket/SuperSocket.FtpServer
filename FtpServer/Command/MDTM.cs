using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class MDTM : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            string filename = requestInfo.Data;

            if (string.IsNullOrEmpty(filename))
            {
                session.SendParameterError();
                return;
            }

            DateTime mdfTime = session.AppServer.FtpServiceProvider.GetModifyTime(session.Context, filename);

            if (session.Context.Status == FtpStatus.Error)
            {
                session.SendResponse(session.Context.Message);
            }
            else
            {
                session.SendResponse(string.Format(Resource.FileOk_213, mdfTime.ToString("yyyyMMddhhmmss")));
            }
        }

        #endregion
    }
}

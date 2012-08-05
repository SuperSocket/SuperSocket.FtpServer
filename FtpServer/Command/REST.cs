using System;
using System.Collections.Generic;
using System.Text;
using Raccent.Ftp.FtpCommon;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class REST : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            long offset = requestInfo.Data.ToLong();

            session.Context.Offset = offset;

            session.SendResponse(Resource.RestartOk_350, offset);
        }

        #endregion
    }
}

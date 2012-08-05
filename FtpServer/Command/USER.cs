using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class USER : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (session.Logged)
            {
                session.SendResponse(Resource.AlreadyLoggedIn_230);
                return;
            }

            string username = requestInfo.Data;

            if (string.IsNullOrEmpty(username))
            {
                session.SendParameterError();
                return;
            }
            else if (string.Compare(username, "anonymous", StringComparison.OrdinalIgnoreCase) == 0)
            {
                session.SendResponse(Resource.RequirePasswor_331, username);
                session.Context.UserName = username;
                return;
            }
            else
            {
                session.SendResponse(Resource.RequirePasswor_331, username);
                session.Context.UserName = username;
                return;
            }
        }

        #endregion
    }
}

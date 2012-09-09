using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class USER : StringCommandBase<FtpSession>
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (session.Logged)
            {
                session.Send(Resource.AlreadyLoggedIn_230);
                return;
            }

            string username = requestInfo.Body;

            if (string.IsNullOrEmpty(username))
            {
                session.SendParameterError();
                return;
            }
            else if (string.Compare(username, "anonymous", StringComparison.OrdinalIgnoreCase) == 0)
            {
                session.Send(Resource.RequirePasswor_331, username);
                session.Context.UserName = username;
                return;
            }
            else
            {
                session.Send(Resource.RequirePasswor_331, username);
                session.Context.UserName = username;
                return;
            }
        }

        #endregion
    }
}

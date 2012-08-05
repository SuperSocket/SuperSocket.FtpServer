using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Raccent.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class LIST : StringCommandBase<FtpSession>
    {
        #region ICommand<SocketSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            List<ListItem> list = session.AppServer.FtpServiceProvider.GetList(session.Context);

            if (session.Context.Status == FtpStatus.Error)
            {
                session.SendResponse(session.Context.Message);
                return;
            }

            if (!session.Context.User.IncreaseConnection())
            {
                session.SendResponse(Resource.ReachedLoginLimit_421);
                return;
            }

            DataConnection dataConn = session.CreateDataConnection();

            if (dataConn.RunDataConnection())
            {
                session.SendResponse(Resource.DataConnectionAccepted_150);

                try
                {
                    dataConn.SendResponse(session.Context, list);
                }
                catch (Exception)
                {
                    session.SendResponse(Resource.DataConnectionCannotOpen_420);
                    return;
                }

                session.Context.User.DecreaseConnection();
                session.CloseDataConnection(dataConn);
                session.SendResponse(Resource.DataTransferComplete_226);
            }
            else
            {
                session.Context.User.DecreaseConnection();
                session.SendResponse(Resource.DataConnectionCannotOpen_420);
            }
        }

        #endregion
    }
}

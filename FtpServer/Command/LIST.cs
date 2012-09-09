using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class LIST : FtpCommandBase
    {
        #region ICommand<SocketSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            if (!session.Logged)
                return;

            List<ListItem> list = session.AppServer.FtpServiceProvider.GetList(session.Context);

            if (session.Context.Status == FtpStatus.Error)
            {
                session.Send(session.Context.Message);
                return;
            }

            if (!session.Context.User.IncreaseConnection())
            {
                session.Send(Resource.ReachedLoginLimit_421);
                return;
            }

            DataConnection dataConn = session.CreateDataConnection();

            if (dataConn.RunDataConnection())
            {
                session.Send(Resource.DataConnectionAccepted_150);

                try
                {
                    dataConn.SendResponse(session.Context, list);
                }
                catch (Exception)
                {
                    session.Send(Resource.DataConnectionCannotOpen_420);
                    return;
                }

                session.Context.User.DecreaseConnection();
                session.CloseDataConnection(dataConn);
                session.Send(Resource.DataTransferComplete_226);
            }
            else
            {
                session.Context.User.DecreaseConnection();
                session.Send(Resource.DataConnectionCannotOpen_420);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Raccent.Ftp.FtpService.Storage;
using SuperSocket.Common;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace Raccent.Ftp.FtpService.Command
{
    public class STOR : StringCommandBase<FtpSession>
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

            if (!session.Context.User.IncreaseConnection())
            {
                session.SendResponse(Resource.ReachedLoginLimit_421);
                return;
            }

            DataConnection dataConn = session.CreateDataConnection();

            if (dataConn.RunDataConnection())
            {
                Stream stream = dataConn.GetStream(session.Context);

                try
                {
                    session.SendResponse(Resource.DataConnectionAccepted_150);

                    if (session.AppServer.FtpServiceProvider.StoreFile(session.Context, filename, stream))
                        session.SendResponse(Resource.DataTransferComplete_226);
                    else
                        session.SendResponse(session.Context.Message);
                }
                catch (SocketException)
                {
                    session.SendResponse(Resource.DataConnectionError_426);
                }
                catch (Exception e)
                {
                    session.Logger.Error(e);
                    session.SendResponse(Resource.OuputFileError_551);
                }
                finally
                {
                    session.Context.User.DecreaseConnection();
                    session.CloseDataConnection(dataConn);
                }
            }
        }

        #endregion
    }
}

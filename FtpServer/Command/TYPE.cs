using System;
using System.Collections.Generic;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Ftp.FtpService.Command
{
    public class TYPE : FtpCommandBase
    {
        #region StringCommandBase<FtpSession> Members

        public override void ExecuteCommand(FtpSession session, StringRequestInfo requestInfo)
        {
            char typeCode = ' ';

            string newType = requestInfo.GetFirstParam();

            if (!string.IsNullOrEmpty(newType) && newType.Length > 0)
            {
                typeCode = newType[0];
            }

            switch (typeCode)
            {
                case ('A'):
                    session.Context.TransferType = TransferType.A;
                    break;
                case ('E'):
                    session.Context.TransferType = TransferType.E;
                    break;
                case ('I'):
                    session.Context.TransferType = TransferType.I;
                    break;
                case ('L'):
                    session.Context.TransferType = TransferType.L;
                    break;
                default:
                    session.SendParameterError();
                    return;
            }

            session.Send(FtpCoreResource.TypeOk_220, typeCode);
        }

        #endregion
    }
}

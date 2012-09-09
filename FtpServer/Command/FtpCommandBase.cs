using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;

namespace SuperSocket.Ftp.FtpService.Command
{
    public abstract class FtpCommandBase : StringCommandBase<FtpSession>
    {
        /// <summary>
        /// Gets a value indicating whether this command is an extension command.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is ext command; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsExtCommand
        {
            get { return false; }
        }
    }
}

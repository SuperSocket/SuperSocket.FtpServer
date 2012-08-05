using System;
using System.Collections.Generic;
using System.Text;

namespace SuperSocket.Ftp.FtpService.Storage
{
    public enum RenameResult
    {
        UnknownError,
        SameNameFolderExist,
        SameNameFileExist,
        Success
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Raccent.Ftp.FtpService
{
    [DataContract]
    public enum CreateUserResult
    {
        [EnumMember]
        UserNameAlreadyExist,

        [EnumMember]
        Success,

        [EnumMember]
        UnknownError
    }
}

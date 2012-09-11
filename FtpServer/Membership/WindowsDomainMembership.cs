using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace SuperSocket.Ftp.FtpService.Membership
{
    public class WindowsDomainMembership
    {
        public bool ValidateUser(string username, string password)
        {
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "YOURDOMAIN"))
            {
                // validate the credentials
                return pc.ValidateCredentials(username, password);
            }
        }
    }
}

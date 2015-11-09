namespace SuperSocket.Ftp.FtpService.Membership
{
#if !MONO

    using System.DirectoryServices.AccountManagement;

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
#endif
}

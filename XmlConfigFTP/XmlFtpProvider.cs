using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Raccent.Ftp.FtpCommon;
using Raccent.Ftp.FtpService;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;
using System.IO;

namespace Raccent.Ftp.XmlConfigFTP
{
    public class XmlFtpProvider : FtpServiceProviderBase
    {
        private string m_UserSettingFile;

        private Dictionary<string, FtpUser> m_UserDict = new Dictionary<string, FtpUser>(StringComparer.OrdinalIgnoreCase);
        
        public override bool Init(IAppServer<FtpSession> server, IServerConfig config)
        {
            if (!base.Init(server, config))
                return false;

            var m_UserSettingFile = config.Options.GetValue("userSetting");

            if (string.IsNullOrEmpty(m_UserSettingFile))
            {
                server.Logger.LogError("No user setting file defined!");
                return false;
            }

            List<FtpUser> users;

            if (!File.Exists(m_UserSettingFile))
            {
                users = new List<FtpUser>();
                users.Add(new FtpUser
                {
                    UserName = "anonymous",
                    Password = "*",
                    Root = @"C:\",
                    MaxConnections = 5
                });
                users.Add(new FtpUser
                {
                    UserName = "kerry",
                    Password = "123456",
                    Root = @"C:\",
                    MaxConnections = 5
                });

                XmlSerializerUtil.Serialize(m_UserSettingFile, users);
            }
            else
            {
                try
                {
                    users = XmlSerializerUtil.Deserialize<List<FtpUser>>(m_UserSettingFile);
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error("Failed to deserialize FtpUser list file!", e);
                    return false;
                }
            }


            foreach (var u in users)
            {
                m_UserDict[u.UserName] = u;
            }

            return true;
        }

        public override AuthenticationResult Authenticate(string username, string password, out FtpUser user)
        {
            lock (SyncRoot)
            {
                if (!m_UserDict.TryGetValue(username, out user))
                    return AuthenticationResult.NotExist;

                if ("*".Equals(user.Password))
                    return AuthenticationResult.Success;

                if (string.CompareOrdinal(password, user.Password) != 0)
                    return AuthenticationResult.PasswordError;

                return AuthenticationResult.Success;
            }
        }

        public override void UpdateUsedSpaceForUser(FtpUser user)
        {
            //Do nothing
        }

        public override List<FtpUser> GetAllUsers()
        {
            lock (SyncRoot)
            {
                return m_UserDict.Values.ToList();
            }
        }

        public override FtpUser GetFtpUserByID(long userID)
        {
            throw new NotImplementedException();
        }

        public override FtpUser GetFtpUserByUserName(string username)
        {
            lock (SyncRoot)
            {
                FtpUser user;
                m_UserDict.TryGetValue(username, out user);
                return user;
            }
        }

        public override bool UpdateFtpUser(FtpUser user)
        {
            //Do nothing
            return true;
        }

        public override CreateUserResult CreateFtpUser(FtpUser user)
        {
            //Do nothing
            return CreateUserResult.UnknownError;
        }

        public override bool ChangePassword(long userID, string password)
        {
            throw new NotImplementedException();
        }

        public override long GetUserRealUsedSpace(FtpContext context, string username)
        {
            string userStorageRoot = context.User.Root;
            return GetDirectorySize(userStorageRoot);
        }

        public override string GetTempDirectory(string sessionID)
        {
            return string.Empty;
        }

        public override void ClearTempDirectory(FtpContext context)
        {
            //Do nothing
        }
    }
}

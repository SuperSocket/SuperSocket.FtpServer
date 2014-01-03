using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using SuperSocket.Common;
using SuperSocket.Ftp.FtpCommon;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;

namespace SuperSocket.Ftp.FtpService
{
    public abstract class FtpServiceProviderBase
    {
        protected object SyncRoot = new object();

        public string Name
        {
            get { return "FtpServiceProvider"; }
        }

        protected FtpServer AppServer { get; private set; }

        public virtual bool Init(FtpServer server, IServerConfig config)
        {
            AppServer = server;

            var dataPortNode = config.Options.GetValue("dataPort");

            if (string.IsNullOrEmpty(dataPortNode))
            {
                server.Logger.Error("Parameter 'dataPort' is required!");
                return false;
            }

            string[] range = dataPortNode.Split(',', ';', ':');

            for (int i = 0; i < range.Length; i++)
            {
                string portField = range[i];

                if (string.IsNullOrEmpty(portField))
                    continue;

                string[] arrPorts = portField.Split('-');

                if (arrPorts == null || arrPorts.Length < 1 || arrPorts.Length > 2)
                {
                    server.Logger.Error("Invalid 'dataPort' value in parameter!");
                    return false;
                }

                if (arrPorts.Length == 1)
                {
                    int port;

                    if (!int.TryParse(arrPorts[0], out port))
                    {
                        server.Logger.Error("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    m_DataPorts.Add(port);
                }
                else if (arrPorts.Length == 2)
                {
                    int portStart, portEnd;

                    if (!int.TryParse(arrPorts[0], out portStart))
                    {
                        server.Logger.Error("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (!int.TryParse(arrPorts[1], out portEnd))
                    {
                        server.Logger.Error("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (portEnd <= portStart)
                    {
                        server.Logger.Error("Invalid 'dataPort' value in parameter!");
                        return false;
                    }

                    if (portStart < 0)
                        portStart = 1;

                    if (portEnd > 65535)
                        portEnd = 65535;

                    for (int seq = portStart; seq < portEnd + 1; seq++)
                    {
                        m_DataPorts.Add(seq);
                    }
                }
            }

            return true;
        }

        public virtual void OnServiceStop()
        {

        }

        private List<int> m_DataPorts = new List<int>();

        private Random m_PortIndexCreator = new Random();

        public virtual int GetRandomPort()
        {
            lock (SyncRoot)
            {
                return m_DataPorts[m_PortIndexCreator.Next(0, m_DataPorts.Count - 1)];
            }
        }

        /// <summary>
        /// Authenticates the specified username and password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public abstract AuthenticationResult Authenticate(string username, string password, out FtpUser user);

        /// <summary>
        /// Updates the used space for user.
        /// </summary>
        /// <param name="user">The user.</param>
        public abstract void UpdateUsedSpaceForUser(FtpUser user);

        /// <summary>
        /// Gets all users.
        /// </summary>
        /// <returns></returns>
        public abstract List<FtpUser> GetAllUsers();

        /// <summary>
        /// Gets the FTP user by ID.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <returns></returns>
        public abstract FtpUser GetFtpUserByID(long userID);

        /// <summary>
        /// Gets the FTP user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public abstract FtpUser GetFtpUserByUserName(string username);


        /// <summary>
        /// Updates the FTP user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public abstract bool UpdateFtpUser(FtpUser user);

        /// <summary>
        /// Creates the FTP user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public abstract CreateUserResult CreateFtpUser(FtpUser user);


        /// <summary>
        /// Changes the user's password.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public abstract bool ChangePassword(long userID, string password);


        public virtual List<ListItem> GetList(FtpContext context)
        {
            string dir = GetStoragePath(context, context.CurrentPath);

            return GetList(context, dir);
        }

        protected List<ListItem> GetList(FtpContext context, string dir)
        {
            List<ListItem> list = new List<ListItem>();

            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir);

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo file = new FileInfo(files[i]);
                        ListItem item = new ListItem();

                        item.ItemType = ItemType.File;
                        item.LastModifiedTime = file.LastWriteTime;
                        item.Length = file.Length;
                        item.Name = file.Name;
                        item.OwnerName = context.UserName;
                        item.Permission = "-rwx------";
                        list.Add(item);
                    }
                }

                string[] folders = Directory.GetDirectories(dir);

                if (folders != null)
                {
                    for (int i = 0; i < folders.Length; i++)
                    {
                        DirectoryInfo folder = new DirectoryInfo(folders[i]);
                        ListItem item = new ListItem();

                        item.ItemType = ItemType.Folder;
                        item.LastModifiedTime = folder.LastWriteTime;
                        item.Length = 0;
                        item.Name = folder.Name;
                        item.OwnerName = context.UserName;
                        item.Permission = "drwx------";

                        list.Add(item);
                    }
                }

                return list;
            }
            else
            {
                AppServer.Logger.ErrorFormat("{0} was not found.", dir);
                context.SetError(FtpCoreResource.NotFound_550);
                return null;
            }
        }

        public virtual bool DeleteFile(FtpContext context, string filename)
        {
            string filepath = GetStoragePath(context, filename);

            if (File.Exists(filepath))
            {
                FileInfo file = new FileInfo(filepath);

                long fileLength = file.Length;

                try
                {
                    File.Delete(filepath);
                    context.ChangeSpace(0 - fileLength);
                    return true;
                }
                catch (UnauthorizedAccessException uae)
                {
                    AppServer.Logger.Error(uae);
                    context.SetError(FtpCoreResource.PermissionDenied_550);
                    return false;
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error(e);
                    context.SetError(FtpCoreResource.DeleteFailed_450, filename);
                    return false;
                }
            }
            else
            {
                context.SetError(FtpCoreResource.NotFound_550);
                return false;
            }
        }

        public virtual DateTime GetModifyTime(FtpContext context, string filename)
        {
            string filepath = GetStoragePath(context, filename);

            if (File.Exists(filepath))
            {
                try
                {
                    FileInfo file = new FileInfo(filepath);
                    return file.LastWriteTime;
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error(e);
                    context.SetError(FtpCoreResource.FileUnavailable_550);
                    return DateTime.MinValue;
                }
            }
            else
            {
                context.SetError(FtpCoreResource.NotFound_550);
                return DateTime.MinValue;
            }
        }

        public virtual long GetFileSize(FtpContext context, string filename)
        {
            string filepath = GetStoragePath(context, filename);

            if (File.Exists(filepath))
            {
                try
                {
                    FileInfo file = new FileInfo(filepath);
                    return file.Length;
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error(e);
                    context.SetError(FtpCoreResource.FileUnavailable_550);
                    return 0;
                }
            }
            else
            {
                context.SetError(FtpCoreResource.NotFound_550);
                return 0;
            }
        }

        public virtual bool RenameFile(FtpContext context, string oldPath, string newPath)
        {
            oldPath = GetStoragePath(context, oldPath);
            newPath = GetStoragePath(context, newPath);

            try
            {
                File.Move(oldPath, newPath);
                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.SetError(FtpCoreResource.RenameToFailed_553);
                return false;
            }
        }

        public virtual bool RenameFolder(FtpContext context, string oldPath, string newPath)
        {
            oldPath = GetStoragePath(context, oldPath);
            newPath = GetStoragePath(context, newPath);

            try
            {
                Directory.Move(oldPath, newPath);
                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.SetError(FtpCoreResource.RenameDirectoryFailed_553);
                return false;
            }
        }

        public virtual bool IsExistFolder(FtpContext context, string path)
        {
            string dir = GetStoragePath(context, path);

            try
            {
                bool result = Directory.Exists(dir);

                if (!result)
                {
                    AppServer.Logger.Error("The directory: " + dir + " was not found");
                }

                return result;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.SetError(FtpCoreResource.FileSystemError_450);
                return false;
            }
        }

        public virtual bool IsExistFile(FtpContext context, string filepath)
        {
            string path = GetStoragePath(context, filepath);

            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.SetError(FtpCoreResource.FileSystemError_450);
                return false;
            }
        }

        public virtual bool StoreFile(FtpContext context, string filename, Stream stream)
        {
            return StoreFile(context, filename, stream, new StoreOption());
        }

        protected bool StoreFile(FtpContext context, string filename, Stream stream, StoreOption option)
        {
            int bufLen = 1024 * 10;
            byte[] buffer = new byte[bufLen];
            int read = 0;
            long totalRead = 0;

            int speed = context.User.MaxUploadSpeed;

            FileStream fs = null;

            try
            {
                string filePath = GetStoragePath(context, filename);

                if (context.Offset > 0 && option.AppendOriginalFile) //Append
                {
                    FileInfo file = new FileInfo(filePath);

                    if (context.Offset != file.Length)
                    {
                        context.Status = FtpStatus.Error;
                        context.Message = "Invalid offset";
                        return false;
                    }

                    fs = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Write, bufLen);
                }
                else
                {
                    if (File.Exists(filePath))
                    {
                        FileInfo file = new FileInfo(filePath);
                        var fileLength = file.Length;
                        File.Delete(filename);
                        context.ChangeSpace(0 - fileLength);
                    }

                    fs = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Write, bufLen);
                }


                DateTime dtStart;
                TimeSpan ts;
                int usedMs = 0, predictMs = 0;

                dtStart = DateTime.Now;

                while ((read = stream.Read(buffer, 0, bufLen)) > 0)
                {
                    fs.Write(buffer, 0, read);
                    totalRead += read;
                    context.ChangeSpace(read);

                    if (speed > 0) // if speed <=0, then no speed limitation
                    {
                        ts = DateTime.Now.Subtract(dtStart);
                        usedMs = (int)ts.TotalMilliseconds;
                        predictMs = read / speed;

                        if (predictMs > usedMs) //Speed control
                        {
                            Thread.Sleep(predictMs - usedMs);
                        }

                        dtStart = DateTime.Now;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.Status = FtpStatus.Error;
                context.Message = "Store file error";
                return false;
            }
            finally
            {
                option.TotalRead = totalRead;

                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }
        }

        public virtual bool CreateFolder(FtpContext context, string foldername)
        {
            string dir = GetStoragePath(context, foldername);

            if (Directory.Exists(dir) || File.Exists(dir))
            {
                context.SetError(FtpCoreResource.DirectoryAlreadyExist_550, foldername);
                return false;
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(dir);
                    return true;
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error(e);
                    context.SetError(FtpCoreResource.MakeDirFailed_550, foldername);
                    return false;
                }
            }
        }

        public virtual bool RemoveFolder(FtpContext context, string foldername)
        {
            string dir = GetStoragePath(context, foldername);

            if (Directory.Exists(dir))
            {
                try
                {
                    Directory.Delete(dir);
                    return true;
                }
                catch (Exception e)
                {
                    AppServer.Logger.Error(e);
                    context.SetError(FtpCoreResource.RemoveDirectoryFailed_550, foldername);
                    return false;
                }
            }
            else
            {
                context.SetError(FtpCoreResource.NotFound_550);
                return false;
            }
        }

        public virtual bool ReadFile(FtpContext context, string filename, Stream stream)
        {
            int bufLen = 1024 * 10;
            byte[] buffer = new byte[bufLen];
            int read = 0;

            FileStream fs = null;

            try
            {
                string filePath = GetStoragePath(context, Path.Combine(context.CurrentPath, filename));

                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufLen);

                if (context.Offset > 0)
                    fs.Seek(context.Offset, SeekOrigin.Begin);

                DateTime dtStart;
                TimeSpan ts;
                int usedMs = 0, predictMs = 0;
                int speed = context.User.MaxDownloadSpeed;

                dtStart = DateTime.Now;

                while ((read = fs.Read(buffer, 0, bufLen)) > 0)
                {
                    stream.Write(buffer, 0, read);

                    if (speed > 0) // if speed <=0, then no speed limitation
                    {
                        ts = DateTime.Now.Subtract(dtStart);
                        usedMs = (int)ts.TotalMilliseconds;
                        predictMs = read / speed;

                        if (predictMs > usedMs) //Speed control
                            Thread.Sleep(predictMs - usedMs);

                        dtStart = DateTime.Now;
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                AppServer.Logger.Error(e);
                context.SetError("Failed to delete file");
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs.Dispose();
                    fs = null;
                }
            }

        }

        public abstract long GetUserRealUsedSpace(FtpContext context, string username);

        public abstract string GetTempDirectory(string sessionID);

        public abstract void ClearTempDirectory(FtpContext context);

        protected string GetStoragePath(FtpContext context, string virtualPath)
        {
            if (!virtualPath.StartsWith("/"))
            {
                if (context.CurrentPath != "/")
                    virtualPath = context.CurrentPath + "/" + virtualPath;
                else
                    virtualPath = context.CurrentPath + virtualPath;
            }

            //Windows path
            if (Path.DirectorySeparatorChar == '\\')
            {
                virtualPath = SuperSocket.Ftp.FtpCommon.StringUtil.ReverseSlash(virtualPath, '/');
            }

            return GetStoragePathInternal(context, virtualPath);
        }

        protected virtual string GetStoragePathInternal(FtpContext context, string virtualPath)
        {
            virtualPath = virtualPath.TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(context.User.Root, virtualPath);
        }

        protected long GetDirectorySize(string dir)
        {
            long total = 0;

            string[] arrFiles = Directory.GetFiles(dir);

            if (arrFiles != null && arrFiles.Length > 0)
            {
                for (int i = 0; i < arrFiles.Length; i++)
                {
                    FileInfo file = new FileInfo(arrFiles[i]);
                    total += file.Length;
                }
            }

            string[] arrDirs = Directory.GetDirectories(dir);

            if (arrDirs != null && arrDirs.Length > 0)
            {
                for (int i = 0; i < arrDirs.Length; i++)
                {
                    total += GetDirectorySize(arrDirs[i]);
                }
            }

            return total;
        }
    }
}

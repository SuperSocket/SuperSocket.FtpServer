using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using SuperSocket.Ftp.FtpService.Storage;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Config;


namespace SuperSocket.Ftp.FtpService
{
    class DataConnection
    {
        private static Random m_Random = new Random();

        private string m_Address = string.Empty;

        public string Address
        {
            get { return m_Address; }
            set { m_Address = value; }
        }

        private int m_Port = 0;

        public int Port
        {
            get { return m_Port; }
            set { m_Port = value; }
        }

        public Socket Client { get; private set; }

        public SslProtocols SecureProtocol { get; set; }

        private FtpSession m_Session;

        private bool m_IsClosed = false;

        private Stream m_Stream;

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Stream GetStream(FtpContext context)
        {
            if (m_Stream == null)
            {
                InitStream(context);
            }

            return m_Stream;
        }

        private void InitStream(FtpContext context)
        {
            switch (SecureProtocol)
            {
                case (SslProtocols.Tls):
                case (SslProtocols.Ssl3):
                case (SslProtocols.Ssl2):
                    SslStream sslStream = new SslStream(new NetworkStream(Client), false);
                    sslStream.AuthenticateAsServer(m_Session.AppServer.Certificate, false, SslProtocols.Default, true);
                    m_Stream = sslStream as Stream;
                    break;
                default:
                    m_Stream = new NetworkStream(Client);
                    break;
            }
        }
        
        public DataConnection(FtpSession session, int port) //PORT
        {
            m_Session = session;
            m_Address = session.Config.Ip;
            SecureProtocol = session.Context.DataSecureProtocol;
            m_Port = port;
        }

        public bool RunDataConnection()
        {
            Socket listener = null;

            try
            {
                IPAddress address;

                if (string.IsNullOrEmpty(m_Address) || "Any".Equals(m_Address, StringComparison.OrdinalIgnoreCase))
                    address = IPAddress.Any;
                else
                    address = IPAddress.Parse(m_Address);

                IPEndPoint endPoint = new IPEndPoint(address, m_Port);
                listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(100);

                SocketAsyncEventArgs acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(acceptEventArgs_Completed);
                if (!listener.AcceptAsync(acceptEventArgs))
                    ProcessAccept(acceptEventArgs);

                if (this.Client == null)
                {
                    //Wait 60000 milliseconds = 1 minute
                    int poolCount = 600;

                    while (poolCount > 0)
                    {
                        Thread.Sleep(100);
                        poolCount--;

                        if (this.Client != null)
                            break;
                    }
                }

                if (this.Client != null)
                {
                    InitStream(m_Session.Context);
                    return true;
                }
                else
                {
                    //Timeout, the client didn't connect server in time
                    return false;
                }
            }
            catch (Exception e)
            {
                m_Session.Logger.Error("Create dataconnection failed, " + m_Address + ", " + m_Port, e);
                return false;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Close();
                    listener = null;
                }
            }
        }

        void acceptEventArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        void ProcessAccept(SocketAsyncEventArgs e)
        {
            this.Client = e.AcceptSocket;
        }

        private static bool TrySocketPort(IPAddress address, int tryPort)
        {
            Socket listener = null;

            try
            {
                IPEndPoint endPoint = new IPEndPoint(address, tryPort);
                listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(100);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (listener != null)
                {
                    listener.Close();
                    listener = null;
                }
            }
        }

        internal static bool TrySocketPort(FtpSession session, int port)
        {
            IPAddress address;

            if (string.IsNullOrEmpty(session.Config.Ip) || "Any".Equals(session.Config.Ip, StringComparison.OrdinalIgnoreCase))
                address = IPAddress.Any;
            else
                address = IPAddress.Parse(session.Config.Ip);

            return TrySocketPort(address, port);
        }

        internal static int GetPassivePort(FtpSession session)
        {
            int tryPort = session.AppServer.FtpServiceProvider.GetRandomPort();
            int previousPort = tryPort;
            int tryTimes = 0;

            IPAddress address;

            if (string.IsNullOrEmpty(session.Config.Ip) || "Any".Equals(session.Config.Ip, StringComparison.OrdinalIgnoreCase))
                address = IPAddress.Any;
            else
                address = IPAddress.Parse(session.Config.Ip);

            while (!TrySocketPort(address, tryPort))
            {
                tryTimes++;
                if (tryTimes > 5)
                {
                    return 0;
                }

                tryPort = session.AppServer.FtpServiceProvider.GetRandomPort();

                if (previousPort == tryPort)
                {
                    return 0;
                }
            }

            return tryPort;
        }

        private const string DELIM = " ";
        private const string NEWLINE = "\r\n";

        public void SendResponse(FtpContext context, List<ListItem> list)
        {
            Stream stream = GetStream(context);

            if (list == null || list.Count <= 0)
                return;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i].Permission);

                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);

                sb.Append("1");
                sb.Append(DELIM);
                sb.Append("user");
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append(DELIM);
                sb.Append("group");

                sb.Append(DELIM);

                sb.Append(GetFixedLength(list[i].Length));

                sb.Append(DELIM);

                sb.Append(GetListTimeString(list[i].LastModifiedTime));

                sb.Append(DELIM);

                sb.Append(list[i].Name);

                sb.Append(NEWLINE);

                byte[] data = context.Charset.GetBytes(sb.ToString());
                stream.Write(data, 0, data.Length);

                sb.Remove(0, sb.Length);
            }

            stream.Flush();
        }

        private string GetListTimeString(DateTime time)
        {
            if (time.Year == DateTime.Now.Year)
                return time.ToString("MMM dd hh:mm");
            else
                return time.ToString("MMM dd yyyy");
        }

        private string GetFixedLength(long length)
        {
            string size = length.ToString();

            size = size.PadLeft(11, ' ');

            return size;
        }

        public virtual void Close()
        {
            if (Client != null && !m_IsClosed)
            {
                try
                {
                    Client.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    m_Session.Logger.Error(e);
                }

                try
                {
                    Client.Close();
                }
                catch (Exception e)
                {
                    m_Session.Logger.Error(e);
                }
                finally
                {
                    Client = null;
                    m_Session = null;
                    m_IsClosed = true;
                    OnClose();
                }
            }
        }

        /// <summary>
        /// Called when [close].
        /// </summary>
        protected virtual void OnClose()
        {
            m_IsClosed = true;
        }
    }
}

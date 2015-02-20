# SuperSocket.FtpServer


SuperSocket FTP Server is a FTP server written in C# language 100% base on **[SuperSocket](http://docs.supersocket.net/ "SuperSocket")**.

## How to use it? ##

1. Place the files of SuperSocket.FtpServer together with the SuperSocket's assemblies:

	* SuperSocket.SocketService.exe
	* SuperSocket.SocketService.exe.config
	* SuperSocket.SocketEngine.dll
	* SuperSocket.SocketBase.dll
	* SuperSocket.Common.dll
	* log4net.dll
	* Config\log4net.config
	* **SuperSocket.Ftp.FtpCommon.dll**
	* **SuperSocket.Ftp.FtpService.dll**


2. FTP server configuration, **SuperSocket.SocketService.exe.config**


		<?xml version="1.0" encoding="utf-8" ?>
		<configuration>
		    <configSections>
		      <section name="superSocket" type="SuperSocket.SocketEngine.Configuration.SocketServiceConfig, SuperSocket.SocketEngine"/>
		    </configSections>
		    <appSettings>
		        <add key="ServiceName" value="SuperSocket.FtpServer"/>
		    </appSettings>
		    <superSocket>
		        <servers>
		            <server name="SuperSocketFTP"
		                    serverType="SuperSocket.Ftp.FtpService.FtpServer, SuperSocket.Ftp.FtpService"
		                    ip="Any" port="21"
		                    maxConnectionNumber="100"
		                    dataPort="4000-4004">
		            </server>
		        </servers>
		    </superSocket>
		</configuration>


	*Other configuration attributes:*

		externalLocalAddress: ftp external access IP address
		logCommand: whether log each ftp command


3. FTP users configuration, **Config\FtpUser.xml**

		<?xml version="1.0" encoding="utf-8"?>
		<ArrayOfFtpUser xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
			<FtpUser>
				<UserName>kerry</UserName>
				<Password>password</Password>
				<Root>D:\ftproot</Root>
			</FtpUser>
			<FtpUser>
				<UserName>tony</UserName>
				<Password>password</Password>
				<Root>D:\ftproot\tony</Root>
			</FtpUser>
		</ArrayOfFtpUser>


4. Start the FTP server

	* Run the server directly: SuperSocket.SocketService.exe -r
	* Install the server as windows service: SuperSocket.SocketService.exe -i


You can find more guide from the documents of SuperSocket:
[http://docs.supersocket.net/v1-6/en-US/Start-SuperSocket-by-Configuration](http://docs.supersocket.net/v1-6/en-US/Start-SuperSocket-by-Configuration)










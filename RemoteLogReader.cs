using System;
using System.IO;
using System.Runtime.InteropServices;

namespace agentspy.net
{
    class RemoteLogReader : IDisposable
    {
        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public ResourceScope dwScope = 0;
            public ResourceType dwType = 0;
            public ResourceDisplayType dwDisplayType = 0;
            public ResourceUsage dwUsage = 0;
            public string lpLocalName = null;
            public string lpRemoteName = null;
            public string lpComment = null;
            public string lpProvider = null;
        };

        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        };

        public enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        };

        public enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        };

        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        };

        const int CONNECT_INTERACTIVE = 0x00000008;
        const int CONNECT_PROMPT = 0x00000010;
        const int CONNECT_REDIRECT = 0x00000080;
        const int CONNECT_UPDATE_PROFILE = 0x00000001;
        const int CONNECT_COMMANDLINE = 0x00000800;
        const int CONNECT_CMD_SAVECRED = 0x00001000;

        const int CONNECT_LOCALDRIVE = 0x00000100;

        const string LOGPATH = @"c$\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE";
        const string AgentLog = "VSTTAgentProcess.log";
        private const string UserName = "tfsbuild3_test";
        private const string Password = "adm$pwd$4$SystemInt";

        private readonly string mySharedFolder;
        private readonly StreamReader myStreamReader;

        public RemoteLogReader(string computer)
        {
            this.mySharedFolder = Path.Combine(@"\\" + computer, LOGPATH);
            var nr = new NETRESOURCE
            {
                dwType = ResourceType.RESOURCETYPE_DISK,
                lpLocalName = null,
                lpRemoteName = this.mySharedFolder,
                lpProvider = null
            };

            int result = WNetUseConnection(IntPtr.Zero, nr, Password, UserName, 0, null, null, null);
            //int result = WNetUseConnection(IntPtr.Zero, nr, "", "", CONNECT_INTERACTIVE | CONNECT_PROMPT, null, null, null);
            if (result != 0)
            {
                Console.WriteLine("Error {0} at connecting.", result);
                return;
            }
            var fileStream = new FileStream(Path.Combine(this.mySharedFolder, AgentLog), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.myStreamReader = new StreamReader(fileStream);
        }

        public string ReadLine()
        {
            return this.myStreamReader != null ? this.myStreamReader.ReadLine() : string.Empty;
        }

        public bool EndOfStream { get { return this.myStreamReader == null || this.myStreamReader.EndOfStream; } }

        public void Dispose()
        {
            if (myStreamReader != null ) myStreamReader.Dispose();
            var ret = WNetCancelConnection2(mySharedFolder, CONNECT_UPDATE_PROFILE, true);
            if (0 != ret)
            {
                Console.WriteLine("return value at closing the connection {0}", ret);
            }
        }
    }
}
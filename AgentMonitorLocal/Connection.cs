using aamcommon;

namespace AgentMonitorLocal_prototype
{
    public class Connection
    {
        public string Name { get; set; }
        public string UserHostAddress { get; set; }
        public TestAgentStatusRecord Record { get; set; }

        public Connection(string address)
        {
            UserHostAddress = address;
        }

        public Connection(string name, string address)
        {
            Name = name;
            UserHostAddress = address;
        }

    }
}

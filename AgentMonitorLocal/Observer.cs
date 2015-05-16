using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aamcommon;

namespace AgentMonitorLocal_prototype
{
    class Observer
    {
        private static readonly List<string> TestAgents = new List<string>() { "EH5DE01T0126PC", "EH5DE01T0162PC", "EH5DE01T0159PC" };
        private List<TestAgentStatusRecord> Records = new List<TestAgentStatusRecord>();
        private HttpInterface myHttpInterface = new HttpInterface();

        public Observer()
        {
            myHttpInterface.SendStatusRequests(TestAgents.ToArray());
            myHttpInterface.StatusUpdateReceived += (json) => StatusUpdateHandler(json);
        }

        private void StatusUpdateHandler(string json)
        {
            //var record = Records.SingleOrDefault(r => r[Field.AgentName])
        }

    }
}

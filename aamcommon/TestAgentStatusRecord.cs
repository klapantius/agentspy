﻿
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("aamcommon_test")]

namespace aamcommon
{
    public enum StateMachine
    {
        Queuing,
        Deploying,
        Deployed,
        RunSetupScript,
        InitializeDataCollectors,
        Synchronizing,
        Starting,
        Running,
        Completing,
        Waiting,
        CleanupDataCollectors,
        RunCleanupScript,
        Cleanup,
        RunCompleted,
        Online,
        Aborting,
        Stopping,
    }

    public enum AgentStatus { NA, Offline, Setup, TestExecution, Cleanup, Online, Finished = Online, Error }
    public enum Field { LogType, Controller, AgentName, Status, Build, Assembly, TC, Error, TimeStamp, JobId }

    /// <summary>
    /// a collection of status fields. The values may be reached via the indexer.
    /// It also has a StatusString property for the easyer communication between Agent- and Monitor Site.
    /// </summary>
    public class TestAgentStatusRecord
    {
        private static readonly Dictionary<AgentStatus, List<Field>> NeededFields = new Dictionary
            <AgentStatus, List<Field>>()
        {
            {AgentStatus.NA, new List<Field>()},
            {AgentStatus.Offline, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller}},
            {AgentStatus.Online, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller}},
            {AgentStatus.Setup, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller, Field.Build, Field.Assembly}},
            {AgentStatus.TestExecution, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller, Field.Build, Field.Assembly, Field.TC}},
            {AgentStatus.Cleanup, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller, Field.Build, Field.Assembly}},
            {AgentStatus.Error, new List<Field>() {Field.TimeStamp, Field.AgentName, Field.Controller, Field.Error}},
        };

        #region changed event
        public delegate void ChangedEventHandler(object sender);
        public event ChangedEventHandler Changed;

        protected virtual void OnChanged()
        {
            if (myPublish && Changed != null)
                Changed(this);
        }

        #endregion

        public const string FieldValueSeparator = ":";
        public const string FieldFieldSeparator = ",";

        private string activeJob;
        private readonly IDictionary<Field, string> myFields = new ConcurrentDictionary<Field, string>();
        private AgentStatus myStatus { get { return (AgentStatus)Enum.Parse(typeof(AgentStatus), myFields[Field.Status], true); } }
        private bool myPublish;

        public string this[Field f]
        {
            get { return myFields.ContainsKey(f) ? myFields[f] : string.Empty; }
            private set { myFields[f] = value; }
        }

        public TestAgentStatusRecord()
        {
            Enum.GetNames(typeof(Field)).ToList().ForEach(f => myFields.Add((Field)Enum.Parse(typeof(Field), f), string.Empty));
            Update(string.Empty, new Dictionary<Field, string>() {{Field.Status, AgentStatus.NA.ToString()}}, false);
        }

        /// <summary>
        /// updates the whole record: as well the fields as the status string
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="fieldsToBeUpdated"></param>
        public void Update(string jobid, IDictionary<Field, string> fieldsToBeUpdated, bool publish)
        {
            myPublish = publish;
            UpdateFields(fieldsToBeUpdated);
            OnChanged();
        }

        private void UpdateFields(IDictionary<Field, string> fieldsToBeUpdated)
        {
            // apply incoming changes
            foreach (var field in fieldsToBeUpdated)
            {
                myFields[field.Key] = field.Value;
            }

            // remove not needed fields
            foreach (var field in Enum.GetValues(typeof(Field)).Cast<Field>().Where(f => f != Field.Status && NeededFields[myStatus].All(nf => nf != f)))
            {
                myFields[field] = string.Empty;
            }
        }

        public string StatusString
        {
            get
            {
                return myStatus.ToString();
            }
            internal set
            {
                Update("0", new Dictionary<Field, string>() {{ Field.Status, value }}, false);
            }
        }

        public string AsJson
        {
            get
            {
                //return JsonConvert.SerializeObject(myFields, typeof (Dictionary<Field, string>), Formatting.None,
                //    new JsonSerializerSettings()
                //    {
                //        NullValueHandling = NullValueHandling.Ignore,
                //        DefaultValueHandling = DefaultValueHandling.Ignore,
                //    });
                return JsonConvert.SerializeObject(myFields, Formatting.None);
            }
            set
            {
                var fieldsToBeUpdated = (Dictionary<Field, string>)JsonConvert.DeserializeObject(value, typeof(Dictionary<Field, string>));
                Update("0", fieldsToBeUpdated, true); 
            }
        }

        public string FieldValue(Field f, string format)
        {
            return myFields.ContainsKey(f) && !string.IsNullOrEmpty(myFields[f]) ?
                string.Format(format, myFields[f]) :
                string.Empty;
        }

        public override string ToString()
        {
            var status = AgentStatus.NA;
            var trace = string.Empty;
            try
            {
                if (!myFields.ContainsKey(Field.Status)) return "*";
                var parsed = Enum.TryParse(myFields[Field.Status], false, out status);
                switch (status)
                {
                    case AgentStatus.NA:
                        return string.Format("no information ({0} - {1})", myFields[Field.Status], parsed);
                    case AgentStatus.Setup:
                    case AgentStatus.Cleanup:
                    case AgentStatus.TestExecution:
                        return string.Format("{4} - {0} is running{1}{2}{3}.", status,
                            FieldValue(Field.Build, " for build {0}"),
                            FieldValue(Field.Assembly, " assembly {0}"),
                            FieldValue(Field.TC, " test case {0}"),
                            FieldValue(Field.TimeStamp, "{0}"));
                    case AgentStatus.Offline:
                    case AgentStatus.Online:
                        return status.ToString();
                    case AgentStatus.Error:
                        return myFields.ContainsKey(Field.Error) ? myFields[Field.Error] : "unkown error";
                }
                return base.ToString();

            }
            catch
            {
                return string.Format("*** error at status \"{0}\" - trace: {1} ***", status, trace);
            }
        }

    }
}

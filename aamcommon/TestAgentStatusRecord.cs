
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
    }

    public enum AgentStatus { NA, Offline, Setup, TestExecution, Cleanup, Online, Error }
    public enum Field { LogType, Status, Build, Assembly, TC, Error, LastUpdated, JobId }

    /// <summary>
    /// a collection of status fields. The values may be reached via the indexer.
    /// It also has a StatusString property for the easyer communication between Agent- and Monitor Site.
    /// </summary>
    public class TestAgentStatusRecord
    {
        #region changed event
        public delegate void ChangedEventHandler(object sender, string e);
        public event ChangedEventHandler Changed;

        protected virtual void OnChanged(string e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion

        public const string FieldValueSeparator = ":";
        public const string FieldFieldSeparator = ";";

        private string activeJob;
        private readonly Dictionary<Field, string> myFields = new Dictionary<Field, string>();

        public string this[Field f]
        {
            get { return myFields.ContainsKey(f) ? myFields[f] : string.Empty; }
            private set { myFields[f] = value; }
        }

        public TestAgentStatusRecord()
        {
            Enum.GetNames(typeof(Field)).ToList().ForEach(f => myFields.Add((Field)Enum.Parse(typeof(Field), f), string.Empty));
            Update(string.Empty, new Dictionary<Field, string>()
            {
                {Field.Status, AgentStatus.NA.ToString()}
            });
        }

        /// <summary>
        /// updates the whole record: as well the fields as the status string
        /// </summary>
        /// <param name="jobid"></param>
        /// <param name="fieldsToBeUpdated"></param>
        public void Update(string jobid, Dictionary<Field, string> fieldsToBeUpdated)
        {
            UpdateFields(fieldsToBeUpdated);
            StatusString = string.Join(FieldFieldSeparator, myFields.
                Where(f => !string.IsNullOrEmpty(f.Value)).
                Select(f => string.Join(FieldValueSeparator, new[] { f.Key.ToString(), f.Value })).
                ToArray());
            //OnChanged(ToString());
        }

        private void UpdateFields(Dictionary<Field, string> fieldsToBeUpdated)
        {
            myFields.Clear();
            foreach (var field in fieldsToBeUpdated)
            {
                myFields[field.Key] = field.Value;
            }
        }

        private string myStatusString;
        public string StatusString
        {
            get { return myStatusString; }
            set
            {
                myStatusString = value;
                var fieldsToBeUpdated = new Dictionary<Field, string>();
                foreach (var items in myStatusString.Split(FieldFieldSeparator.ToCharArray()).Select(fvp => fvp.Split(FieldValueSeparator.ToCharArray())))
                {
                    Field key;
                    Enum.TryParse(items[0], true, out key);
                    var val = items[1];
                    fieldsToBeUpdated.Add(key, val);
                }
                UpdateFields(fieldsToBeUpdated);
                OnChanged(myStatusString);
            }
        }

        public override string ToString()
        {
            AgentStatus status;
            Enum.TryParse(myFields[Field.Status], false, out status);
            switch (status)
            {
                case AgentStatus.NA:
                    return "no information";
                case AgentStatus.Setup:
                case AgentStatus.Cleanup:
                case AgentStatus.TestExecution:
                    return string.Format("{0} is running{1}.", status,
                        myFields.ContainsKey(Field.Build) ? string.Format(" for build {0}{1}", myFields[Field.Build],
                        myFields.ContainsKey(Field.Assembly) ? string.Format(" assembly {0}{1}", myFields[Field.Assembly],
                        myFields.ContainsKey(Field.TC) ? string.Format(" test case {0}", myFields[Field.TC]) : "") : "") : "");
                case AgentStatus.Offline:
                case AgentStatus.Online:
                    return status.ToString();
                case AgentStatus.Error:
                    return myFields[Field.Error];
            }
            return base.ToString();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using aamcommon;

namespace aamws
{
    public enum JobStatus { Na, Setup, TestExecution, Cleanup, Finished }

    public class Job : IJob
    {
        public static readonly Dictionary<JobStatus, List<JobStatus>> EnabledStatusTransitions =
            new Dictionary<JobStatus, List<JobStatus>>()
            {
                {JobStatus.Na, new List<JobStatus>() {JobStatus.Setup, JobStatus.TestExecution, JobStatus.Cleanup, JobStatus.Finished}},
                {JobStatus.Setup, new List<JobStatus>() {JobStatus.TestExecution, JobStatus.Cleanup}},
                {JobStatus.TestExecution, new List<JobStatus>() {JobStatus.Cleanup}},
                {JobStatus.Cleanup, new List<JobStatus>() {JobStatus.Finished}},
                {JobStatus.Finished, new List<JobStatus>() {JobStatus.Setup}},
            };

        public string Id { get; private set; }

        private readonly Dictionary<Field, string> myFields = new Dictionary<Field, string>();

        public JobStatus Status
        {
            get { return (JobStatus)Enum.Parse(typeof(JobStatus), myFields[aamcommon.Field.Status]); }
            private set
            {
                if (EnabledStatusTransitions.ContainsKey(Status) &&
                    EnabledStatusTransitions[Status].All(s => s != value))
                {
                    myFields[aamcommon.Field.Error] = string.Format("Unexpected status transition: {0} => {1}", Status, value);
                }
                myFields[aamcommon.Field.Status] = value.ToString();
            }
        }

        public Job(string id)
        {
            Id = id;
            myFields[aamcommon.Field.Status] = JobStatus.Na.ToString();
        }

        public string Field(Field fieldName)
        {
            return myFields.ContainsKey(fieldName) ? myFields[fieldName] : string.Empty;
        }

        public void Update(Dictionary<Field, string> fieldsToBeUpdated)
        {
            foreach (var update in fieldsToBeUpdated)
            {
                if (update.Key == aamcommon.Field.Status)
                {
                    Status = (JobStatus)Enum.Parse(typeof(JobStatus), update.Value);
                }
                else
                {
                    myFields[update.Key] = update.Value;
                }
            }
        }

        public string Error
        {
            get { throw new System.NotImplementedException(); }
        }

    }
}

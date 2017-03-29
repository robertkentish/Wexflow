using System;
using System.Runtime.Serialization;

namespace Wexflow.Core.Service.Contracts
{
    public enum LaunchType
    {
        Startup,
        Trigger,
        Periodic
    }

    [DataContract]
    public class WorkflowInfo:IComparable
    {
        [DataMember]
        public int Id { get; private set; }
        [DataMember]
        public Guid InstanceId { get; private set; }
        [DataMember]
        public string Name { get; private set; }
        [DataMember]
        public LaunchType LaunchType { get; private set; }
        [DataMember]
        public bool IsEnabled { get; private set; }
        [DataMember]
        public string Description { get; private set; }
        [DataMember]
        public bool IsRunning { get; set; }
        [DataMember]
        public bool IsPaused { get; set; }

        public WorkflowInfo(int id, Guid instanceId, string name, LaunchType launchType, bool isEnabled, string desc, bool isRunning, bool isPaused)
        {
            Id = id;
            InstanceId = instanceId;
            Name = name;
            LaunchType = launchType;
            IsEnabled = isEnabled;
            Description = desc;
            IsRunning = isRunning;
            IsPaused = isPaused;
        }

        public int CompareTo(object obj)
        {
            var wfi = (WorkflowInfo)obj;
            return wfi.Id.CompareTo(Id);
        }

        public int CompareInstanceTo(object obj)
        {
            var wfi = (WorkflowInfo)obj;
            return wfi.InstanceId.CompareTo(InstanceId);
        }
    }
}

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Wexflow.Tasks.ABBYYUpload
{
    internal class Batch
    {
        [DataMember(Name = "Project", IsRequired = true)]
        public string Project { get; set; }

        [DataMember(Name = "Batchtype")]
        public string Batchtype { get; set; }

        [DataMember]
        public Dictionary<string, string> RegistrationParameters { get; set; }

        [DataMember(Name = "Files", IsRequired = true)]
        public string[] Files { get; set; }
    }
}

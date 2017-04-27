using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Wexflow.Core.Service.Contracts
{
    [DataContract]
    public class UploadFile
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public int FileLength { get; set; }

        [DataMember]
        public string FileName { get; set; }

    }
}

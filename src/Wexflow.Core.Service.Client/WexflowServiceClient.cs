using Wexflow.Core.Service.Contracts;
using System.Net;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Wexflow.Core.Service.Client
{
    public class WexflowServiceClient
    {
        public string Uri { get; private set; }

        public WexflowServiceClient(string uri)
        {
            Uri = uri.TrimEnd('/');
        }

        public WorkflowInfo[] GetWorkflows()
        {
            string uri = Uri + "/workflows";
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public WorkflowInfo[] GetRunningWorkflows()
        {
            string uri = Uri + "/workflowinstances";
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var workflows = JsonConvert.DeserializeObject<WorkflowInfo[]>(response);
            return workflows;
        }

        public Guid StartWorkflow(int id)
        {
            string uri = Uri + "/start/" + id;
            var webClient = new WebClient();
            var wfInstanceID = webClient.UploadString(uri, string.Empty);
            return Guid.Parse(wfInstanceID.Replace("\"", ""));
        }

        public void StopWorkflow(Guid id)
        {
            string uri = Uri + "/stop/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void SuspendWorkflow(Guid id)
        {
            string uri = Uri + "/suspend/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public void ResumeWorkflow(Guid id)
        {
            string uri = Uri + "/resume/" + id;
            var webClient = new WebClient();
            webClient.UploadString(uri, string.Empty);
        }

        public WorkflowInfo GetWorkflow(int id)
        {
            string uri = Uri + "/workflow/" + id;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public WorkflowInfo GetWorkflowInstance(Guid id)
        {
            string uri = Uri + "/workflowinstance/" + id;
            var webClient = new WebClient();
            var response = webClient.DownloadString(uri);
            var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            return workflow;
        }

        public void AddFilesToWorkflowInstance(Guid id, string filename)
        {
            string uri = Uri + "/workflowinstance/" + id + "/files";
            var webClient = new WebClient();

            using (Stream stream = File.OpenRead(filename))
            {
                var data = new MemoryStream();
                stream.CopyTo(data);
                var response = webClient.UploadData(uri, data.GetBuffer());
                // var workflow = JsonConvert.DeserializeObject<WorkflowInfo>(response);
            }

        }
    }
}

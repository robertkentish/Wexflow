using System.Linq;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;
using System.ServiceModel.Web;
using System;
using System.IO;

namespace Wexflow.Clients.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class WexflowService : IWexflowService
    {
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflows")]
        public WorkflowInfo[] GetWorkflows()
        {
            return WexflowWindowsService.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.Id, wf.InstanceId, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused)).ToArray();
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflowinstances")]
        public WorkflowInfo[] GetRunningWorkflows()
        {
            return WexflowWindowsService.WexflowEngine.RunningWorkflows.Select(i => new WorkflowInfo(i.Value.Id, i.Value.InstanceId, i.Value.Name, (LaunchType)i.Value.LaunchType, i.Value.IsEnabled, i.Value.Description, i.Value.IsRunning, i.Value.IsPaused)).ToArray();
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "start/{id}")]
        public Guid StartWorkflow(string id)
        {
            return WexflowWindowsService.WexflowEngine.StartWorkflow(int.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "stop/{id}")]
        public void StopWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.StopWorkflow(Guid.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "suspend/{id}")]
        public void SuspendWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.PauseWorkflow(Guid.Parse(id));
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "resume/{id}")]
        public void ResumeWorkflow(string id)
        {
            WexflowWindowsService.WexflowEngine.ResumeWorkflow(Guid.Parse(id));
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflow/{id}")]
        public WorkflowInfo GetWorkflow(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflow(int.Parse(id));
            if (wf == null)
                return null;

            return new WorkflowInfo(wf.Id, wf.InstanceId, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused);
        }

        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflowinstance/{id}")]
        public WorkflowInfo GetWorkflowInstance(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflowInstance(Guid.Parse(id));
            if (wf == null)
                return null;

            return new WorkflowInfo(wf.Id, wf.InstanceId, wf.Name, (LaunchType)wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused);
        }

        [WebInvoke(Method = "POST",
            ResponseFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare,
            UriTemplate = "workflowinstance/{id}/files")]
        public void AddFilesToWorkflowInstance(string id, Stream file)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflowInstance(Guid.Parse(id));
            if (wf == null)
                return;

            var uploadPath = Path.Combine(@"C:\Wexflow\Temp\UploadFiles", id.ToString());
            Directory.CreateDirectory(uploadPath);
            var uploadFilename = Path.Combine(uploadPath, "test.txt");

            int length = 0;
            using (FileStream writer = new FileStream(uploadFilename, FileMode.Create))
            {
                int readCount;
                var buffer = new byte[8192];
                while ((readCount = file.Read(buffer, 0, buffer.Length)) != 0)
                {
                    writer.Write(buffer, 0, readCount);
                    length += readCount;
                }
            }

            var newFile = new Core.FileInf(uploadFilename, wf.Id);
            wf.FilesPerTask[1].Add(newFile);
        }

        [WebInvoke(Method = "POST",
           ResponseFormat = WebMessageFormat.Json,
           BodyStyle = WebMessageBodyStyle.Bare,
           UriTemplate = "resumeworkflowinstance/{id}")]
        public void ResumeWorkflowFromTask(string id)
        {
            var wf = WexflowWindowsService.WexflowEngine.GetWorkflowInstance(Guid.Parse(id));
            if (wf == null)
                return;

            //wf.ResumeFromWait(status);

        }
    }
}

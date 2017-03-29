using System.Linq;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;
using System.ServiceModel.Web;
using System;

namespace Wexflow.Clients.WindowsService
{
    [ServiceBehavior(IncludeExceptionDetailInFaults=true)]
    public class WexflowService:IWexflowService
    {
        [WebInvoke(Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "workflows")]
        public WorkflowInfo[] GetWorkflows()
        {
            return WexflowWindowsService.WexflowEngine.Workflows.Select(wf => new WorkflowInfo(wf.Id, wf.InstanceId, wf.Name, (LaunchType) wf.LaunchType, wf.IsEnabled, wf.Description, wf.IsRunning, wf.IsPaused)).ToArray();
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
    }
}

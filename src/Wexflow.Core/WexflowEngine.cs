using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using System.Threading;

namespace Wexflow.Core
{
    public class WexflowEngine
    {
        public string SettingsFile { get; private set; }
        public string WorkflowsFolder { get; private set; }
        public string TempFolder { get; private set; }
        public string XsdPath { get; private set; }
        public Workflow[] Workflows { get; private set; }
        public Dictionary<Guid,Workflow> RunningWorkflows { get; private set; }

        public WexflowEngine(string settingsFile) 
        {
            SettingsFile = settingsFile;
            LoadSettings();
            LoadWorkflows();
            RunningWorkflows = new Dictionary<Guid, Workflow>();
        }

        void LoadSettings()
        {
            var xdoc = XDocument.Load(SettingsFile);
            WorkflowsFolder = GetWexflowSetting(xdoc, "workflowsFolder");
            TempFolder = GetWexflowSetting(xdoc, "tempFolder");
            if (!Directory.Exists(TempFolder)) Directory.CreateDirectory(TempFolder);
            XsdPath = GetWexflowSetting(xdoc, "xsd");
        }

        string GetWexflowSetting(XDocument xdoc, string name)
        {
            return xdoc.XPathSelectElement(string.Format("/Wexflow/Setting[@name='{0}']", name)).Attribute("value").Value;    
        }

        void LoadWorkflows()
        { 
            var workflows = new List<Workflow>();
            foreach (string file in Directory.GetFiles(WorkflowsFolder))
            {
                try
                {
                    var workflow = new Workflow(file, TempFolder, XsdPath);
                    workflows.Add(workflow);
                    Logger.InfoFormat("Workflow loaded: {0}", workflow);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("An error occured while loading the workflow : {0} Please check the workflow configuration. Error: {1}", file, e.Message);
                }
            }
            Workflows = workflows.ToArray();
        }

        public void Run()
        {
            foreach (Workflow workflow in Workflows)
            {
                if (workflow.IsEnabled)
                {
                    if (workflow.LaunchType == LaunchType.Startup)
                    {
                        var wfInstance = SpawnWorkflow(workflow.JobId);
                        wfInstance.Start();    
                    }
                    else if (workflow.LaunchType == LaunchType.Periodic)
                    {
                        Action<object> callback = o =>
                        {
                            var wf = (Workflow)o;
                            if (!wf.IsRunning)
                            {
                                var wfInstance = SpawnWorkflow(wf.JobId);
                                wfInstance.Start();
                            }
                        };
                        
                        var timer = new WexflowTimer(new TimerCallback(callback), workflow, workflow.Period);
                        timer.Start();
                    }
                }
            }
        }

        public Workflow GetWorkflow(int workflowId)
        {
            return Workflows.FirstOrDefault(wf => wf.Id == workflowId);
        }

        public Workflow SpawnWorkflow(int workflowId)
        {
            var workflow = GetWorkflow(workflowId);
            var wfInstance = workflow.CreateInstance();

            RunningWorkflows.Add(wfInstance.InstanceId, wfInstance);

            return wfInstance;
        }

        public Workflow GetWorkflowInstance(Guid wfInstanceId)
        {
            if(RunningWorkflows.ContainsKey(wfInstanceId))
            {
                return RunningWorkflows[wfInstanceId];
            }
            else
            {
                return null;
            }
        }

        public Guid StartWorkflow(int workflowId)
        {
            var wf = SpawnWorkflow(workflowId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", workflowId);
                return Guid.Empty;
            }
            else 
            {
                if (wf.IsEnabled) wf.Start();
            }

            return wf.InstanceId;
        }

        public void StopWorkflow(Guid wfInstanceId)
        {
            var wf = GetWorkflowInstance(wfInstanceId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", wfInstanceId);
            }
            else
            {
                if (wf.IsEnabled) wf.Stop();
            }
        }

        public void PauseWorkflow(Guid wfInstanceId)
        {
            var wf = GetWorkflowInstance(wfInstanceId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", wfInstanceId);
            }
            else
            {
                if (wf.IsEnabled) wf.Pause();
            }
        }

        public void ResumeWorkflow(Guid wfInstanceId)
        {
            var wf = GetWorkflowInstance(wfInstanceId);

            if (wf == null)
            {
                Logger.ErrorFormat("Workflow {0} not found.", wfInstanceId);
            }
            else
            {
                if (wf.IsEnabled) wf.Resume();
            }
        }

    }
}

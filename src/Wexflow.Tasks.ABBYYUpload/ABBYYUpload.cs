using System;
using Wexflow.Core;
using System.Xml.Linq;
using System.IO;
using System.Threading;
using System.Net;
using System.Collections.Generic;
using System.Linq;

namespace Wexflow.Tasks.ABBYYUpload
{
    public class ABBYYUpload : Wexflow.Core.Task
    {
        public string ServerAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Project { get; set; }
        public string BatchType { get; set; }
        public string BatchName { get; set; }
        public List<KeyValuePair<string, string>> RegistrationParameters { get; set; }

        public ABBYYUpload(XElement xe, Workflow wf) : base(xe, wf)
        {
            ServerAddress = GetSetting("server");
            Username = GetSetting("username");
            Password = GetSetting("password");
            Project = GetSetting("project");
            BatchType = GetSetting("batchType");
            BatchName = GetSetting("batchName");
            RegistrationParameters = new List<KeyValuePair<string, string>>();
        }

        public override TaskStatus Run()
        {
            Info("Executing ABBYY Workflow Step");

            //bool success = true;
            //bool atLeastOneSucceed = false;

            var files = SelectFiles();
            for (int i = files.Length - 1; i > -1; i--)
            {
                var file = files[i];

                try
                {
                    if (file.Path == null || !File.Exists(file.Path))
                    {
                        Error($"File not found error. ({file.Path})");
                        throw new FileNotFoundException();
                    }

                    // Package up our data to send to ABBYY
                    var flexiCapture = new AbbyyFlexicapture(ServerAddress, new NetworkCredential(Username, Password));
                    if (!flexiCapture.ProjectNames.Contains(Project))
                    {
                        Error($"Project referred to in JSON does not exist on server: {Project}");
                        throw new ArgumentException("Project referred to in JSON does not exist on server.", Project);
                    }

                    Debug("Setting batch and registration parameters");
                    flexiCapture.SetProject(Project);
                    flexiCapture.SetBatchType(BatchType);
                    flexiCapture.RegistrationParameter = RegistrationParameters.ToDictionary(k => k.Key, v => v.Value);

                    flexiCapture.Files = new string[] { file.Path };

                    Info("Parameters set and batch submitted. About to Process");
                    flexiCapture.ProcessBatch(BatchName);

                }
                catch (ThreadAbortException)
                {
                    throw;
                }

            }

            var status = Status.Waiting;

            //if (!success && atLeastOneSucceed)
            //{
            //    status = Status.Warning;
            //}
            //else if (!success)
            //{
            //    status = Status.Error;
            //}

            Info("Task finished.");
            return new TaskStatus(status, false);

        }
    }
}

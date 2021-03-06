﻿using System;
using System.ServiceModel;
using Wexflow.Core.Service.Contracts;

namespace Wexflow.Clients.WindowsService
{
    [ServiceContract(Namespace = "http://wexflow/")]
    public interface IWexflowService
    {
        [OperationContract]
        WorkflowInfo[] GetWorkflows();

        [OperationContract]
        WorkflowInfo[] GetRunningWorkflows();

        [OperationContract]
        Guid StartWorkflow(string id);

        [OperationContract]
        void StopWorkflow(string id);

        [OperationContract]
        void SuspendWorkflow(string id);

        [OperationContract]
        void ResumeWorkflow(string id);

        [OperationContract]
        WorkflowInfo GetWorkflow(string id);

        [OperationContract]
        WorkflowInfo GetWorkflowInstance(string id);
    }
}

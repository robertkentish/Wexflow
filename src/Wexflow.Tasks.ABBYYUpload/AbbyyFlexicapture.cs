using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using File = System.IO.File;
using Wexflow.Tasks.ABBYYUpload.FlexiCaptureEngine;

namespace Wexflow.Tasks.ABBYYUpload
{
    public class AbbyyFlexicapture
    {
        #region Declarations
        private readonly FlexiCaptureWebServiceApiVersion3 _service;
        private int _currentSessionId;
        private int _currentProjectId;
        private int _currentBatchTypeId;
        // private string _project;
        // private string _batchType;
        private Dictionary<string, string> _registrationParameter;
        private string[] _file;
        private readonly int _userId;
        private int _batchId;
        private Document[] _docs;
        private const int RoleType = 12; // TODO: Figure out valid values and what they mean
        private const int StationType = 10;
        #endregion

        #region Properties
        public Project[] Projects => GetProjects();

        public IEnumerable<string> ProjectNames => GetProjectNames();

        public BatchType[] BatchTypes => GetBatchTypes();

        public Document[] Documents => _docs;

        public int CurrentProjectId
        {
            get
            {
                return _currentProjectId;
            }
            set
            {
                var ids = GetProjectIDs();
                if (ids.Contains(value))
                {
                    _currentProjectId = value;
                }
                else
                {
                    _currentProjectId = -1;
                }
            }
        }

        public string CurrentProjectGuid
        {
            get
            {
                return _currentProjectId == -1 ? "" : Projects.Where(item => item.Id == _currentProjectId).Select(item => item.Guid).First();
            }
        }

        public Dictionary<string, string> RegistrationParameter
        {
            get
            {
                return _registrationParameter;
            }

            set
            {
                _registrationParameter = value;
            }
        }
        #endregion

        #region Events

        public event BatchCompleteHandler BatchComplete;
        public EventArgs E = null;
        public delegate void BatchCompleteHandler(AbbyyFlexicapture af, EventArgs e);


        #endregion

        #region Constructors

        public AbbyyFlexicapture(string serverIpOrName, string username, string password, string domain = "") : this(serverIpOrName, new NetworkCredential(username, password, domain))
        {
        }

        public AbbyyFlexicapture(IPAddress ipaddress, string username, string password, string domain = "") : this(ipaddress.ToString(), new NetworkCredential(username, password, domain))
        {
        }

        public AbbyyFlexicapture(IPAddress ipaddress, NetworkCredential credentials) : this(ipaddress.ToString(), credentials)
        {
        }

        public AbbyyFlexicapture(string serverIpOrName, NetworkCredential credentials)
        {
            _registrationParameter = null;
            var cred = credentials;
            _service = new FlexiCaptureWebServiceApiVersion3() { Credentials = cred };
            _currentSessionId = -1;
            _currentProjectId = -1;
            _service.Url = FlexiCaptureUrl.AbbyyFlexicaptureServerUrl(serverIpOrName);
            _userId = GetUserId(cred.UserName);
            _service.ProcessBatchCompleted += _service_ProcessBatchCompleted;
            OpenSession();
        }

        private void _service_ProcessBatchCompleted(object sender, AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Accessor Methods


        /// <summary>
        ///  Processes Batch
        /// </summary>
        /// <param name="batchName">optional: Name the Batch for tracking</param>
        public int ProcessBatch(string batchName = "")
        {
            // Initialise variables

            var batch = new FlexiCaptureEngine.Batch();
            _batchId = -1;

            // preset batch name to date and time if no name given

            if (string.IsNullOrEmpty(batchName))
            {
                batchName = _currentProjectId + DateTime.Today.ToString("_yy-MM-dd-HH-mm-ss-tt");
            }

            // set batchname

            batch.Name = batchName;
            batch.BatchTypeId = _currentBatchTypeId;


            // set pre-registration parameters
            if (_registrationParameter != null)
            {
                int n = 0;
                batch.Properties = new RegistrationProperty[_registrationParameter.Count()];
                foreach (var parameter in _registrationParameter)
                {
                    batch.Properties[n] = new RegistrationProperty() { Name = parameter.Key, Value = parameter.Value };
                    n++;
                }
            }

            // check if we have a project, else jump out with a -1

            if (_currentSessionId <= 0 || CurrentProjectId <= 0 || _userId <= 0) return _batchId;

            // create new batch and get ID
            var projectId = _service.OpenProject(_currentSessionId, CurrentProjectGuid);
            _batchId = _service.AddNewBatch(_currentSessionId, projectId, batch, _userId);



            // upload files
            if (_service.OpenBatch(_currentSessionId, _batchId))
            {
                foreach (var file in _file)
                {
                    var uploadFile = LoadFile(file);
                    _service.AddNewImage(_currentSessionId, _batchId, uploadFile);
                }


                //// set pre-registration parameters
                //if (_registrationParameter != null)
                //{
                //    foreach (var parameter in _registrationParameter)
                //    {
                //        _service.SetSettingValue(_userId, _currentProjectId, _currentBatchTypeId, parameter.Key, parameter.Value);
                //    }
                //}



                _service.CloseBatch(_currentSessionId, _batchId);
                _service.ProcessBatch(_currentSessionId, _batchId);


                //Document[] batchDocs = _service.GetDocuments(_currentSessionId, _batchId);
                //int[] docIds = new int[batchDocs.Count()];
                //for (int i=0; i<batchDocs.Count(); i++)
                //    docIds[i] = batchDocs[i].Id;

                //var newTaskId = _service.CreateTask(_currentSessionId, _batchId, 100, 0, "API Task", 0, docIds, false);


                ////var taskId = _service.GetTask(_currentSessionId, _currentProjectId, 100, false, false);
                //_service.OpenTask(_currentSessionId, newTaskId);
                //_service.OpenBatch(_currentSessionId, _batchId);
                //Document[] documents = _service.GetTaskDocuments(newTaskId);
                //_service.CloseTask(_currentSessionId, newTaskId, 200);
                //_service.CloseBatch(_currentSessionId, _batchId);

            }

            return _batchId;
        }

        #endregion

        #region Private Methods
        private BatchType[] GetBatchTypes()
        {
            if (_currentProjectId == -1) return null;
            var batchTypes = _service.GetBatchTypes(_currentProjectId);

            return batchTypes;
        }

        private Project[] GetProjects()
        {
            Project[] projects = new Project[0];
            try
            {
                projects = _service.GetProjects(); // will throw exception
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                if (projects != null)
                    return projects;
            }

            return projects;
        }

        private IEnumerable<string> GetProjectNames()
        {
            return Projects.Select(project => project.Name).ToArray();
        }

        private IEnumerable<int> GetProjectIDs()
        {
            return Projects.Select(project => project.Id).ToArray();
        }

        private static FlexiCaptureEngine.File LoadFile(string fileName)
        {
            var file = new FlexiCaptureEngine.File();

            using (var stream = File.Open(fileName, FileMode.Open))
            {
                file.Name = fileName;
                file.Bytes = new byte[stream.Length];
                stream.Read(file.Bytes, 0, file.Bytes.Length);
            }

            return file;
        }

        private int GetUserId(string username)
        {
            var userId = _service.FindUser(username);

            if (userId < 1)
            {
                return -1;
            }
            else
            {
                return userId;
            }
        }

        private int GetBatchTypeId(string batchType)
        {
            foreach (var batch in GetBatchTypes())
            {
                if (batch.Name == batchType)
                {
                    return batch.Id;
                }
            }
            return -1;
        }

        public void SetProject(string projectName)
        {
            _currentProjectId = (int)GetProjects().FirstOrDefault(p => p.Name == projectName)?.Id;
        }

        public void SetBatchType(string batchTypeName)
        {
            _currentBatchTypeId = (GetBatchTypeId(batchTypeName) == -1 ? 1 : GetBatchTypeId(batchTypeName));
        }

        public string[] Files
        {
            get
            {
                return _file;
            }

            set
            {
                _file = value;
            }
        }


        /// <summary>
        /// Opens a Session if none is open
        /// </summary>
        private void OpenSession(int roleType = RoleType, int stationType = StationType)
        {
            if (_currentSessionId != -1) return;
            _currentSessionId = _service.OpenSession(roleType, stationType); // will throw exception
        }

        /// <summary>
        /// Closes the open Session
        /// </summary>
        private void CloseSession()
        {
            _service.CloseSession(_currentSessionId);
            _currentSessionId = -1;
        }

        private void OnBatchComplete(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            CreateOutput();
            _service.DeleteBatch(_currentSessionId, _batchId);
            _service.CloseProject(_currentSessionId, _currentProjectId);
            CloseSession();
            BatchComplete?.Invoke(this, asyncCompletedEventArgs);
        }

        private void CreateOutput()
        {
            _docs = _service.GetDocuments(_currentSessionId, _batchId);
            if (_docs == null) return;

            foreach (var doc in _docs)
            {
                _service.LoadDocumentResult(_currentSessionId, _batchId, doc.Id, "Output.xml");
            }
        }

        #endregion
    }
}

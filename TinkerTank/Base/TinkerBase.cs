using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinkerTank;
using Enumerations;

namespace Base
{
    public abstract partial class TinkerBase
    {

        protected ComponentStatus _status = ComponentStatus.UnInitialised;
        private string _name;
        protected MeadowApp _appRoot;
        private string _statusText = string.Empty;
        private int _errorCount = 0;
        private int _errorTriggerCount = 4;
        private bool _disabled = false;
        private AutomaticErrorResponse _errorResponse = AutomaticErrorResponse.DisableComponent;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public ComponentStatus Status
        {
            get => _status;
            set => _status = value;
        }

        public string StatusText
        {
            get => _statusText;
            set => _statusText = value;
        }

        public int ErrorCount
        {
            get => _errorCount;
            set => _errorCount = value;
        }

        public int ErrorTriggerCount
        {
            get => _errorTriggerCount;
            set => _errorTriggerCount = value;
        }

        public bool Disabled
        {
            get => _disabled;
            set => _disabled = value;
        }

        public AutomaticErrorResponse ErrorResponse
        {
            get => _errorResponse;
            set => _errorResponse = value;
        }

    }
}

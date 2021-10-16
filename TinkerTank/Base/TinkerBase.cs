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
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace Base
{
    public class TinkerBase : ObservableObject
    {

        protected ComponentStatus _status = ComponentStatus.UnInitialised;
        protected MeadowApp _appRoot;
        private string _statusText = string.Empty;
        private AutomaticErrorResponse _errorResponse = AutomaticErrorResponse.DisableComponent;

        public ComponentStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string StatusText 
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public AutomaticErrorResponse ErrorResponse
        {
            get => _errorResponse;
            set => SetProperty(ref _errorResponse, value);
        }
    }
}

﻿using Meadow;
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
    public class TinkerBase
    {

        protected ComponentStatus _status = ComponentStatus.UnInitialised;
        protected MeadowApp _appRoot;
        private string _statusText = string.Empty;
        private AutomaticErrorResponse _errorResponse = AutomaticErrorResponse.DisableComponent;

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

        public AutomaticErrorResponse ErrorResponse
        {
            get => _errorResponse;
            set => _errorResponse = value;
        }
    }
}

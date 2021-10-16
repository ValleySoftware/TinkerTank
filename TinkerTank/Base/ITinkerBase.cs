using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enumerations;

namespace Base
{
    public interface ITinkerBase
    {
        void RefreshStatus();
        void Test();
        void ErrorEncountered();
    }
}

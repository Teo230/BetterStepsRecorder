using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace BetterStepsRecorder.WPF.Services
{
    internal class ExportService : IExportService
    {
        public ExportService() { }

        public bool Test() => true;
    }

    internal interface IExportService
    {
        bool Test();
    }
}

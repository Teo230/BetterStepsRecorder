using BetterStepsRecorder.WPF.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterStepsRecorder.WPF
{
    internal class MainWindowViewModel
    {
        IExportService _exportService;

        public MainWindowViewModel(IExportService exportService) 
        {
            _exportService = exportService;

            _exportService.Test();
        }
    }
}

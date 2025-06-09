using BetterStepsRecorder.WPF.Components;
using BetterStepsRecorder.WPF.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterStepsRecorder.WPF
{
    internal class MainWindowViewModel
    {
        IDialogCoordinator _dialogCoordinator { get; }

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator) 
        {
            _dialogCoordinator = dialogCoordinator;
        }

        #region Methods

        private void OpenGitHubRepo()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Mentaleak/BetterStepsRecorder",
                UseShellExecute = true
            });
        }

        #endregion

        #region Commands

        private ICommand _openHelpDialog;
        public ICommand OpenHelpDialog
        {
            get
            {
                _openHelpDialog ??= new RelayCommands(async obj =>
                    {
                        var result = await _dialogCoordinator.ShowMessageAsync(this, 
                            "Help",
                            "Welcome to the Better Steps Recorder help menu.\r\nThis tool helps you record steps and take screenshots efficiently.\r\nFor more details and instructions, visit our GitHub repository.",
                            MessageDialogStyle.AffirmativeAndNegative,
                            new MetroDialogSettings()
                            {
                                NegativeButtonText = "GitHub"
                            });

                        if (result == MessageDialogResult.Negative) OpenGitHubRepo();
                    }, obj => true);
                return _openHelpDialog;
            }
        }


        #endregion
    }
}

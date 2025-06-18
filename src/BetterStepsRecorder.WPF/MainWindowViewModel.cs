using BetterStepsRecorder.WPF.Components.PropertyGrid;
using BetterStepsRecorder.WPF.Services;
using BetterStepsRecorder.WPF.Utilities;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BetterStepsRecorder.WPF
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Props

        private bool _recording = false;
        public bool Recording
        {
            get => _recording;
            set
            {
                if (_recording != value)
                {
                    _recording = value;
                    NotifyPropertyChanged(nameof(Recording));
                }
            }
        }

        private string _selectedScreenshot;
        public string SelectedScreenshot
        {
            get => _selectedScreenshot;
            set
            {
                if (_selectedScreenshot != value)
                {
                    _selectedScreenshot = value;
                    NotifyPropertyChanged(nameof(SelectedScreenshot));
                }
            }
        }

        private ObservableCollection<ScreenshotInfo> _steps = new ObservableCollection<ScreenshotInfo>();
        public ObservableCollection<ScreenshotInfo> Steps
        {
            get => _steps;
            set
            {
                if (_steps != value)
                {
                    _steps = value;
                    NotifyPropertyChanged(nameof(Steps));
                }
            }
        }

        private ScreenshotInfo _selectedStep;
        public ScreenshotInfo SelectedStep
        {
            get => _selectedStep;
            set
            {
                if (_selectedStep != value)
                {
                    _selectedStep = value;
                    NotifyPropertyChanged(nameof(SelectedStep));
                    SelectedScreenshot = _selectedStep?.ScreenshotBase64 ?? string.Empty;
                }
            }
        }

        #endregion

        IDialogCoordinator _dialogCoordinator { get; }
        IScreenCaptureService _screenCaptureService { get; }

        public MainWindowViewModel(
            IDialogCoordinator dialogCoordinator,
            IScreenCaptureService screenCaptureService)
        {
            _dialogCoordinator = dialogCoordinator;
            _screenCaptureService = screenCaptureService;
        }

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

        private ICommand _startRecording;
        public ICommand StartRecording
        {
            get
            {
                _startRecording ??= new RelayCommands(async obj =>
                {
                    StartCaptureScreen();
                    Recording = true;
                }, obj => true);
                return _startRecording;
            }
        }

        private ICommand _stopRecording;
        public ICommand StopRecording
        {
            get
            {
                _stopRecording ??= new RelayCommands(async obj =>
                {
                    StopCaptureScreen();
                    Recording = false;
                }, obj => true);
                return _stopRecording;
            }
        }

        #endregion

        #region Methods

        private void OpenGitHubRepo()
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Mentaleak/BetterStepsRecorder",
                UseShellExecute = true
            });
        }

        private void StartCaptureScreen()
        {
            _screenCaptureService.OnScreenshotCaptured += _screenCaptureService_OnScreenshotCaptured;
            _screenCaptureService.StartCapturing();
        }

        private void StopCaptureScreen()
        {
            _screenCaptureService.OnScreenshotCaptured -= _screenCaptureService_OnScreenshotCaptured;
            _screenCaptureService.StopCapturing();
        }

        private void _screenCaptureService_OnScreenshotCaptured(object? sender, ScreenshotInfo screenshotInfo)
        {
            screenshotInfo.ElementName = "Step " + (Steps.Count + 1);
            Steps.Add(screenshotInfo);
            SelectedScreenshot = screenshotInfo.ScreenshotBase64;
            SelectedStep = Steps.Last();
        }

        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

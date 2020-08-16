using Avalonia.Controls;
using GameUpdater.Services;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace GameUpdater.ViewModels
{
    public class ManageGameViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _parentViewModel;
        public ManageGameViewModel(MainWindowViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
        }
        
        public void CheckFiles()
        {
            var checkFilesViewModel = new CheckFilesViewModel();
            checkFilesViewModel.OnCheckFilesFinished += o => _parentViewModel.ClosePopup();
            _parentViewModel.OpenPopup(checkFilesViewModel);
        }

        public void Uninstall()
        {
            var messageBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams
                {
                    ContentTitle = "Uninstall",
                    ContentMessage = "Are you sure, that you want to uninstall the game?",
                    ButtonDefinitions = ButtonEnum.YesNo,
                    Icon = Icon.Warning,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                });
            
            messageBoxStandardWindow.Show().ContinueWith(task =>
            {
                if (task.Result != ButtonResult.Yes) return;
                new Uninstall().PerformUninstall();
            });
        }
    }
}
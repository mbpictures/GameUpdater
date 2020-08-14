using Avalonia.Controls;
using GameUpdater.Services;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace GameUpdater.ViewModels
{
    public class ManageGameViewModel : ViewModelBase
    {
        public delegate void Check(object sender);

        public event Check OnCheckFiles;
        
        public void CheckFiles()
        {
            OnCheckFiles?.Invoke(this);    
        }

        public void Uninstall()
        {
            var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow(
                new MessageBoxStandardParams()
                {
                    ContentTitle = "Uninstall",
                    ContentMessage = $"Are you sure, that you want to uninstall the game?",
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
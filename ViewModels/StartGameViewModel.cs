using System.Diagnostics;

namespace GameUpdater.ViewModels
{
    public class StartGameViewModel : ViewModelBase
    {

        public void StartGame()
        {
            Process.Start("notepad.exe");
        }
    }
}
using System.Diagnostics;
using GameUpdater.Services;

namespace GameUpdater.ViewModels
{
    public class StartGameViewModel : ViewModelBase
    {

        public void StartGame()
        {
            Process.Start(new IniParser().Read("GameExe", "General"));
        }
    }
}
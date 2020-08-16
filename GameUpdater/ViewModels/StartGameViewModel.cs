using System.Diagnostics;
using GameUpdater.Services;

namespace GameUpdater.ViewModels
{
    public class StartGameViewModel : ViewModelBase
    {

        public void StartGame()
        {
            Process.Start(IniLoader.Instance.Read("GameExe", "General"));
        }
    }
}
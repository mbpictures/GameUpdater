using System.Diagnostics;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
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
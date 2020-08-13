using GameUpdater.Views;

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
            
        }
    }
}
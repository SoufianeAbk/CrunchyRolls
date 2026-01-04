using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// Basis ViewModel klasse met INotifyPropertyChanged
    /// Erft van ObservableObject voor automatische property notifications
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private bool isBusy = false;

        /// <summary>
        /// Omgekeerde IsBusy waarde (voor binding)
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        // ===== VIRTUELE METHODES =====

        /// <summary>
        /// Opgeruimd wanneer ViewModel verwijderd wordt
        /// </summary>
        public virtual void Dispose()
        {
            Debug.WriteLine($"🗑️ {this.GetType().Name} verwijderd");
        }
    }
}
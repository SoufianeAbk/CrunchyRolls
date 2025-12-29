using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CrunchyRolls.Core.ViewModels
{
    /// <summary>
    /// Basis ViewModel klasse met INotifyPropertyChanged
    /// Alle viewmodels erven hiervan af
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        // ===== PROPERTIES =====

        private string _title = string.Empty;
        private bool _isBusy = false;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Omgekeerde IsBusy waarde (voor binding)
        /// </summary>
        public bool IsNotBusy => !IsBusy;

        // ===== EVENTS =====

        public event PropertyChangedEventHandler? PropertyChanged;

        // ===== PROPERTY CHANGED HELPERS =====

        /// <summary>
        /// Property waarde instellen en PropertyChanged event triggeren
        /// </summary>
        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// PropertyChanged event uitlokken
        /// </summary>
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using iBot_GUI.Annotations;
using iBot_GUI.Controls;
using IBot.Facades.Core;
using IBot.Facades.Core.Settings;

namespace iBot_GUI.Pages.Settings
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl, INotifyPropertyChanged
    {
        private Dictionary<string, SettingsBase> _settings;
        public Dictionary<string, SettingsBase> Settings
        {
            get { return _settings; }
            set
            {
                if (Equals(value, _settings))
                    return;
                _settings = value;
                OnPropertyChanged(nameof(Settings));
            }
        }

        public SettingsPage()
        {
            Settings = new Dictionary<string, SettingsBase>();

            InitializeComponent();

            var conSettings = SettingsManager.GetConnectionSettings();
            var genSettings = SettingsManager.GetGeneralSettings();

            if (genSettings.Success)
                Settings.Add("General Settings", genSettings.Value);

            if (conSettings.Success)
                Settings.Add("Connection Settings", conSettings.Value);

            OnPropertyChanged(nameof(Settings));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

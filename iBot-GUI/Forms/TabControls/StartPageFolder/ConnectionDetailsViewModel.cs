using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace iBot_GUI.Forms.TabControls.StartPageFolder
{
    public class ConnectionDetailsViewModel : INotifyPropertyChanged
    {
        private readonly ConnectionDetails _model;       

        public ConnectionDetailsViewModel(ConnectionDetails model)
        {
            _model = model;
        }

        public ICollection<string> ChannelList => _model.ChannelList;

        public string Nick
        {
            get { return _model.Nick; }
            set
            {
                if (_model.Nick == value)
                    return;
                _model.Nick = value;

                UpdateSettings();

                OnPropertyChanged("Nick");
            }
        }

        public string Token
        {
            get { return _model.Token; }
            set
            {
                if (_model.Token == value)
                    return;
                _model.Token = value;

                UpdateSettings();

                OnPropertyChanged("Token");
            }
        }

        public void AddChannel(string channel)
        {            
            _model.ChannelList.Add(channel);

            UpdateSettings();

            OnPropertyChanged("ChannelList");
        }

        public void RemoveChannel(string s)
        {
            _model.ChannelList.Remove(s);

            UpdateSettings();

            OnPropertyChanged("ChannelList");
        }

        private void UpdateSettings()
        {
            IBot.Misc.SettingsManager.SaveConnectionSettings(Nick, Token, ChannelList.ToArray());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected internal void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
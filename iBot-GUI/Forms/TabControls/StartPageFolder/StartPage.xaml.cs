using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using IBot.Misc;

namespace iBot_GUI.Forms.TabControls.StartPageFolder
{
    /// <summary>
    ///     Interaktionslogik für StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        private readonly ConnectionDetailsViewModel _connectionDetails;

        public StartPage()
        {
            InitializeComponent();

            var t = SettingsManager.GetConnectionSettings();

            _connectionDetails =
                new ConnectionDetailsViewModel(new ConnectionDetails(new ObservableCollection<string>(t.ChannelList),
                    t.TwitchApiKey, t.Username));

            ConnectionDetailsTab.DataContext = _connectionDetails;
            TokenBox.Password = _connectionDetails.Token;

            RemoveChannelButton.IsEnabled = EnableRemocewChannelButton();
        }

        private bool EnableRemocewChannelButton()
        {
            return _connectionDetails.ChannelList.Count > 0;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Add_Channel(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChannelTextBox.Text.Trim()))
                return;

            _connectionDetails.AddChannel(ChannelTextBox.Text);
            ChannelTextBox.Text = "";

            RemoveChannelButton.IsEnabled = EnableRemocewChannelButton();
        }

        private void Remove_Channel(object sender, RoutedEventArgs e)
        {
            var s = ChannelList.SelectedItem as string;

            if (string.IsNullOrEmpty(s?.Trim()))
                return;

            _connectionDetails.RemoveChannel(s);

            RemoveChannelButton.IsEnabled = EnableRemocewChannelButton();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var passwordBox = e.Parameter as PasswordBox;
            if (passwordBox == null) return;

            var password = passwordBox.Password;
            _connectionDetails.Token = password;
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
    }

    public static class Command
    {
        public static readonly RoutedUICommand OkCommand = new RoutedUICommand("Ok Command", "OkCommand", typeof(StartPage));
    }
}
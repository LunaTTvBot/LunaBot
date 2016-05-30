using System;
using IBot.Facades.Events.Args.User;

namespace iBot_GUI.Pages.Start
{
    public partial class StartPage
    {
        public StartPage()
        {
            InitializeComponent();
            IBot.Facades.Events.UserEvents.UserPublicMessageEvent += ExtentInformationBox;
        }

        private void ExtentInformationBox(object sender, PublicMessageEventArgs e)
        {
            if(Dispatcher.CheckAccess()) {
                AppendText(e);
            } else {
                Dispatcher.Invoke(() => AppendText(e));
            }
        }

        private void AppendText(PublicMessageEventArgs publicMessageEventArgsFacade) {
            InfoBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " | " +
                               publicMessageEventArgsFacade.UserName + "#" +
                               publicMessageEventArgsFacade.Channel + " >> " +
                               publicMessageEventArgsFacade.Message + "\r\n");

            if(InfoBox.VerticalOffset + InfoBox.ActualHeight > InfoBox.ExtentHeight)
                InfoBox.ScrollToEnd();
        }
    }
}
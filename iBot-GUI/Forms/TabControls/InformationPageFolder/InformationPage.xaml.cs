using System;
using IBot.Facades.Events;
using IBot.Facades.Events.CustomArgs;

namespace iBot_GUI.Forms.TabControls.InformationPageFolder
{
    public partial class InformationPage
    {
        public InformationPage()
        {
            InitializeComponent();
            CommandManagerFacade.CommandCalledEvent += ExtentInformationBox;
            MessageEventFacade.UserPublicMessageEvent += ExtentInformationBox;
        }

        private void ExtentInformationBox(object sender, PublicMessageEventArgsFacade publicMessageEventArgsFacade)
        {
            if(Dispatcher.CheckAccess()) {
                AppendText(publicMessageEventArgsFacade);
            } else {
                Dispatcher.Invoke(() => AppendText(publicMessageEventArgsFacade));
            }
        }

        private void AppendText(PublicMessageEventArgsFacade publicMessageEventArgsFacade) {
            InfoBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " | " +
                               publicMessageEventArgsFacade.User + "#" +
                               publicMessageEventArgsFacade.Channel +  " >> " +
                               publicMessageEventArgsFacade.Msg + "\r\n");

            if(InfoBox.VerticalOffset + InfoBox.ActualHeight > InfoBox.ExtentHeight)
                InfoBox.ScrollToEnd();
        }

        private void AppendText(CommandCalledEventArgsFacade commandCalledEventArgsFacade)
        {
            InfoBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + " | " +
                               commandCalledEventArgsFacade.User + " -> " +
                               commandCalledEventArgsFacade.CommandName + " (" +
                               commandCalledEventArgsFacade.CommandType + ") >> " +
                               commandCalledEventArgsFacade.Msg + "\r\n");

            if (InfoBox.VerticalOffset + InfoBox.ActualHeight > InfoBox.ExtentHeight)
                InfoBox.ScrollToEnd();
        }

        private void ExtentInformationBox(object sender, CommandCalledEventArgsFacade commandCalledEventArgsFacade)
        {
            if (Dispatcher.CheckAccess())
            {
                AppendText(commandCalledEventArgsFacade);
            }
            else
            {
                Dispatcher.Invoke(() => AppendText(commandCalledEventArgsFacade));
            }
        }
    }
}
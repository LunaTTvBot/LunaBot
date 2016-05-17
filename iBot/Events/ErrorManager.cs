using System.Threading;
using IBot.Misc;
using IBot.Events.CustomEventArgs;
using System.Text.RegularExpressions;
using System;

namespace IBot.Events
{
    public static class ErrorManager
    {
        private const string LoginErrorPattern = @"^:" + GlobalTwitchPatterns.TwitchHostNamePattern + @"\sNOTICE\s\*\s:(.*)$";

        private static readonly Regex LoginErrorRegEx = new Regex(LoginErrorPattern);

        public static event EventHandler<ErrorEventArgs> UserLoginErrorEvent;

        private static void OnUserLoginErrorEvent(ErrorEventArgs e) => UserLoginErrorEvent?.Invoke(typeof(ErrorManager), e);

        public static void RemoveEventHandlers()
        {

        }

        public static void BindToMessageEvent()
        {
            IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += CheckAndRaiseErrorEvents;
        }

        private static void CheckAndRaiseErrorEvents(object sender, MessageEventArgs eArgs)
        {
            var m = LoginErrorRegEx.Match(eArgs.Message);
            if (!m.Success) return;

            OnUserLoginErrorEvent(new ErrorEventArgs(m.Groups[1].Value));
        }
    }
}

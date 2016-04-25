using System;
using System.Diagnostics;
using System.Threading;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.ComstumEventArgs;
using IRCConnectionTest.Misc;
using System.IO;

namespace IRCConnectionTest
{
    internal class App
    {
        private IrcConnection _connection;

        public void StartApp()
        {
            var settingsFileName = "settings.json";
            AppSettings settings;

            try
            {
                settings = AppSettings.Load(settingsFileName);
            }
            catch (FileNotFoundException)
            {
                settings = AppSettings.LoadLocal(settingsFileName);
            }

            _connection = new IrcConnection(
                settings.Username,
                settings.TwitchApiKey,
                settings.Nickname,
                settings.Url,
                settings.Port,
                ConnectionType.BotCon
            );

            if (_connection.Connect())
            {
                settings.Save(settingsFileName);
                Console.WriteLine("##### Connected! #####");

                _connection.RaiseMessageEvent += ConnectionOnRaiseMessageEvent;                

                _connection.Join("reiskornchen");

                RegisterChannelEvents();
                RegisterUserEvents();
            }

            // Console.ReadLine();
        }

        private static void RegisterUserEvents()
        {
            UserEventManager.UserJoinEvent += EventManagerOnUserJoinEvent;
            UserEventManager.UserPartEvent += EventManagerOnUserJoinEvent;
            UserEventManager.UserPublicMessageEvent += EventManagerOnUserPublicMessageEvent;
            UserEventManager.UserWhisperMessageEvent += EventManagerOnUserWhisperMessageEvent;
            UserEventManager.UserStateEvent += UserEventManagerOnUserStateEvent;
        }

        private static void RegisterChannelEvents()
        {
            // ChannelEventManager.UserJoinEvent += EventManagerOnUserJoinEvent;
            ChannelEventManager.UserPartEvent += EventManagerOnUserJoinEvent;
            ChannelEventManager.RoomStateAllEvent += ChannelEventManagerOnRoomStateAllEvent;
            ChannelEventManager.OperatorGrantedEvent += ChannelEventManagerOnOperatorGrantedEvent;
            ChannelEventManager.OperatorRevokedEvent += ChannelEventManagerOnOperatorGrantedEvent;
        }

        private static void ChannelEventManagerOnOperatorGrantedEvent(object sender, OperatorModeEventArgs eArgs)
        {
            Trace.WriteLine($"Operator {eArgs.OpType}: {eArgs.User}@{eArgs.Channel}");
        }

        private static void ChannelEventManagerOnRoomStateAllEvent(object sender, RoomStateAllEventArgs eArgs)
        {
            Trace.WriteLine($"ROOMSTATE received #{eArgs.Channel}");
        }

        private static void UserEventManagerOnUserStateEvent(object sender, UserStateEventArgs eArgs) {
            Trace.WriteLine($"USERSTATE received #{eArgs.Channel}");
        }

        private static void EventManagerOnUserWhisperMessageEvent(object sender, UserWhisperMessageEventArgs eArgs)
        {
            Trace.WriteLine($"User {eArgs.UserName} > {eArgs.ToUserName}: {eArgs.Message}");
        }

        private static void EventManagerOnUserPublicMessageEvent(object sender, UserPublicMessageEventArgs eArgs)
        {
            var name = eArgs.Tags != null && eArgs.Tags.DisplayName != ""
                ? eArgs.Tags.DisplayName
                : eArgs.UserName;

            Trace.WriteLine($"{name}#{eArgs.Channel}: {eArgs.Message}");
        }

        private static void EventManagerOnUserJoinEvent(object sender, UserEventArgs eArgs)
        {
            Trace.WriteLine($"User {eArgs.Type}ed: {eArgs.UserName} - {eArgs.Channel}");
        }

        private static void ConnectionOnRaiseMessageEvent(object sender, MessageEventArgs eArgs)
        {
            Console.WriteLine(eArgs.Message);
        }
    }
}
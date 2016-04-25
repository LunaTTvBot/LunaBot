using System;
using System.Diagnostics;
using System.Threading;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.ComstumEventArgs;
using IRCConnectionTest.Misc;
using System.IO;
using System.Runtime.Serialization;

namespace IRCConnectionTest
{
    internal class App
    {
        private IrcConnection _connection;
        public static string BotChannel = "niksdaboy";

        public void StartApp()
        {
            var settingsFileName = "settings.json";
            AppSettings settings;

            if (!AppSettings.TryLoad(settingsFileName, out settings))
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
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(UserList).TypeHandle);
                settings.Save(settingsFileName);
                Console.WriteLine("##### Connected! #####");

                _connection.RaiseMessageEvent += ConnectionOnRaiseMessageEvent;                

                _connection.Join(BotChannel);

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
            Console.WriteLine($"Operator {eArgs.OpType}: {eArgs.User}@{eArgs.Channel}");
        }

        private static void ChannelEventManagerOnRoomStateAllEvent(object sender, RoomStateAllEventArgs eArgs)
        {
            Console.WriteLine($"ROOMSTATE received #{eArgs.Channel}");
        }

        private static void UserEventManagerOnUserStateEvent(object sender, UserStateEventArgs eArgs) {
            Console.WriteLine($"USERSTATE received #{eArgs.Channel}");
        }

        private static void EventManagerOnUserWhisperMessageEvent(object sender, UserWhisperMessageEventArgs eArgs)
        {
            Console.WriteLine($"User {eArgs.UserName} > {eArgs.ToUserName}: {eArgs.Message}");
        }

        private static void EventManagerOnUserPublicMessageEvent(object sender, UserPublicMessageEventArgs eArgs)
        {
            var name = eArgs.Tags != null && eArgs.Tags.DisplayName != ""
                ? eArgs.Tags.DisplayName
                : eArgs.UserName;

            Console.WriteLine($"{name}#{eArgs.Channel}: {eArgs.Message}");
        }

        private static void EventManagerOnUserJoinEvent(object sender, UserEventArgs eArgs)
        {
            Console.WriteLine($"User {eArgs.Type}ed: {eArgs.UserName} - {eArgs.Channel}");
        }

        private static void ConnectionOnRaiseMessageEvent(object sender, MessageEventArgs eArgs)
        {
            Trace.WriteLine(eArgs.Message);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IRCConnectionTest.Events;
using IRCConnectionTest.Events.Commands;
using IRCConnectionTest.Events.CustomEventArgs;
using IRCConnectionTest.Misc;
using IRCConnectionTest.Plugins;
using IRCConnectionTest.Resources;

namespace IRCConnectionTest
{
    internal class App
    {
        public static List<string> BotChannelList;
        private IrcConnection _connection;

        public void StartApp()
        {
            Console.OutputEncoding = Encoding.UTF8;

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            const string settingsFileName = "settings.json";
            AppSettings settings;

            if (!AppSettings.TryLoad(settingsFileName, out settings))
            {
                settings = AppSettings.LoadLocal(settingsFileName);
            }

            BotChannelList = settings.ChannelList;

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
                var consoleAssembly = Assembly.GetExecutingAssembly();
                var pluginTypes = GetTypesByInterface<IPlugin>(consoleAssembly);

                // List<IPlugin> plugins = new List<IPlugin>();
                foreach (var pluginType in pluginTypes)
                {
                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    // plugins.Add(plugin);
                    plugin?.Execute();
                }

                CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
                {
                    RegEx = @"!test\s?(.*)",
                    Name = "Test",
                    Action = (command, matches, mArgs) =>
                    {
                        IrcConnection.Write(ConnectionType.BotCon, mArgs.Channel,
                            $"Ja, Test ({matches.Groups[1].Value})!");
                    }
                });

                RuntimeHelpers.RunClassConstructor(typeof(UserList).TypeHandle);
                settings.Save(settingsFileName);
                Console.WriteLine(app.app_connected);
                _connection.RaiseMessageEvent += ConnectionOnRaiseMessageEvent;
                BotChannelList.ForEach(channel => _connection.Join(channel));
                RegisterChannelEvents();
                RegisterUserEvents();
            }

            Console.ReadLine();
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
            // ChannelEventManager.UserPartEvent += EventManagerOnUserJoinEvent;
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

        private static void UserEventManagerOnUserStateEvent(object sender, UserStateEventArgs eArgs)
        {
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

        public static List<Type> GetTypesByInterface<T>(Assembly assembly)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            return assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(T).Name) != null)
                .ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IBot.Core;
using IBot.Core.Settings;
using IBot.Events;
using IBot.Events.Commands;
using IBot.Resources;
using NLog;
using IrcConnection = IBot.Core.IrcConnection;

namespace IBot
{
    internal class App
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static List<string> BotChannelList;

        public void StartApp()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

            RuntimeHelpers.RunClassConstructor(typeof(UserList).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(PluginManager).TypeHandle);

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("de-DE");
            var settings = SettingsManager.GetSettings<ConnectionSettings>();

            BotChannelList = settings.ChannelList;

            IrcConnectionManager.RegisterMessageHandler(ConnectionType.BotCon, (sender, args) => Logger.Debug("bot: " + args.Message));
            IrcConnectionManager.RegisterMessageHandler(ConnectionType.ChatCon, (sender, args) => Logger.Debug("chat: " + args.Message));

            BotChannelList.ForEach(channel =>
            {
                IrcConnectionManager.RegisterOnConnectedHandler(ConnectionType.BotCon, (sender, args) => args.Connection.Join(channel));
                IrcConnectionManager.RegisterOnConnectedHandler(ConnectionType.ChatCon, (sender, args) => args.Connection.Join(channel));
            });

            IrcConnectionManager.RegisterConnection(
                user: settings.BotUsername,
                password: settings.BotTwitchApiKey,
                nickname: settings.BotNickname,
                url: settings.Url,
                caps: new[] { TwitchCaps.Commands, TwitchCaps.Membership, TwitchCaps.Tags },
                secure: settings.BotSecure,
                port: settings.BotSecure ? settings.SslPort : settings.Port,
                type: ConnectionType.BotCon                                                
                );

            IrcConnectionManager.RegisterConnection(
                user: settings.OwnerUsername,
                password: settings.OwnerTwitchApiKey,
                nickname: settings.OwnerNickname,
                url: settings.Url,
                caps: null,
                secure: settings.OwnerSecure,
                port: settings.OwnerSecure ? settings.SslPort : settings.Port,
                type: ConnectionType.ChatCon
                );

            if (IrcConnectionManager.ConnectAll())
            {
                UserDatabaseManager.Initialise();

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

                Console.WriteLine(app.app_connected);
                RegisterChannelEvents();
                RegisterUserEvents();
            }

            Console.ReadLine();
        }

        private static void RegisterUserEvents()
        {
            UserEventManager.UserStateEvent +=
                (sender, args) => Console.WriteLine($"USERSTATE received #{args.Channel}");
            UserEventManager.UserJoinEvent +=
                (sender, args) => Console.WriteLine($"User {args.Type}ed: {args.UserName} - {args.Channel}");
            UserEventManager.UserPartEvent +=
                (sender, args) => Console.WriteLine($"User {args.Type}ed: {args.UserName} - {args.Channel}");
            UserEventManager.UserWhisperMessageEvent +=
                (sender, args) => Console.WriteLine($"User {args.UserName} > {args.ToUserName}: {args.Message}");

            UserEventManager.UserPublicMessageEvent +=
                (sender, args) =>
                    Console.WriteLine(
                        $"{(string.IsNullOrEmpty(args.Tags?.DisplayName) ? args.UserName : args.Tags.DisplayName)}#{args.Channel}: {args.Message}");
        }

        private static void RegisterChannelEvents()
        {
            ChannelEventManager.RoomStateAllEvent +=
                (sender, args) => Console.WriteLine($"ROOMSTATE received #{args.Channel}");
            ChannelEventManager.OperatorGrantedEvent +=
                (sender, args) => Console.WriteLine($"Operator {args.OpType}: {args.User}@{args.Channel}");
            ChannelEventManager.OperatorRevokedEvent +=
                (sender, args) => Console.WriteLine($"Operator {args.OpType}: {args.User}@{args.Channel}");
        }

        public static List<Type> GetTypesByInterface<T>(Assembly assembly)
        {
            if (!typeof(T).IsInterface)
                throw new ArgumentException("T must be an interface");

            return assembly.GetTypes()
                .Where(x => x.GetInterface(typeof(T).Name) != null)
                .ToList();
        }

        public void StopApp()
        {
        }
    }
}
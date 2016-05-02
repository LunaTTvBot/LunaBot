using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using IBot.Events;
using IBot.Events.Commands;
using IBot.Events.CustomEventArgs;
using IBot.Misc;
using IBot.Plugins;
using IBot.Resources;

namespace IBot
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
                _connection.RaiseMessageEvent += (sender, args) => Trace.WriteLine(args.Message);
                BotChannelList.ForEach(channel => _connection.Join(channel));
                RegisterChannelEvents();
                RegisterUserEvents();
            }

            Console.ReadLine();
        }

        private static void RegisterUserEvents()
        {
            UserEventManager.UserStateEvent += (sender, args) => Console.WriteLine($"USERSTATE received #{args.Channel}");
            UserEventManager.UserJoinEvent += (sender, args) => Console.WriteLine($"User {args.Type}ed: {args.UserName} - {args.Channel}");
            UserEventManager.UserPartEvent += (sender, args) => Console.WriteLine($"User {args.Type}ed: {args.UserName} - {args.Channel}");
            UserEventManager.UserWhisperMessageEvent += (sender, args) => Console.WriteLine($"User {args.UserName} > {args.ToUserName}: {args.Message}");

            UserEventManager.UserPublicMessageEvent +=
                (sender, args) => Console.WriteLine($"{(String.IsNullOrEmpty(args.Tags?.DisplayName) ? args.UserName : args.Tags.DisplayName)}#{args.Channel}: {args.Message}");
        }

        private static void RegisterChannelEvents()
        {
            ChannelEventManager.RoomStateAllEvent += (sender, args) => Console.WriteLine($"ROOMSTATE received #{args.Channel}");
            ChannelEventManager.OperatorGrantedEvent += (sender, args) => Console.WriteLine($"Operator {args.OpType}: {args.User}@{args.Channel}");
            ChannelEventManager.OperatorRevokedEvent += (sender, args) => Console.WriteLine($"Operator {args.OpType}: {args.User}@{args.Channel}");
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

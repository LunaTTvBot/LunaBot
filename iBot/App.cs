using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using IBot.Events;
using IBot.Events.Commands;
using IBot.Misc;
using IBot.Plugins;
using IBot.Resources;

namespace IBot
{
    internal class App
    {
        public static List<string> BotChannelList;

        public void StartApp()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("de-DE");

            var settings = SettingsManager.GetConnectionSettings();

            BotChannelList = settings.ChannelList;

            RuntimeHelpers.RunClassConstructor(typeof(CommandManager).TypeHandle);            

            ConnectionManager.BotConnectedEvent += (s, a) =>
            {                
                Console.WriteLine(app.app_connected);

                var consoleAssembly = Assembly.GetExecutingAssembly();
                var pluginTypes = GetTypesByInterface<IPlugin>(consoleAssembly);

                foreach (var pluginType in pluginTypes)
                {
                    var plugin = Activator.CreateInstance(pluginType) as IPlugin;
                    plugin?.Execute();
                }

                RuntimeHelpers.RunClassConstructor(typeof(UserList).TypeHandle);

                RegisterChannelEvents();
                RegisterUserEvents();
            };

            ConnectionManager.BotDisconnectedEvent += (s, a) =>
            {
                Console.WriteLine(app.app_disconnected);

                UserEventManager.RemoveEventHandlers();

            };
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
            ConnectionManager.DisconnectFromBotAccount();
        }
    }
}
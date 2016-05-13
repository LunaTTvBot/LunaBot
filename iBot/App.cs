using System;
using System.Runtime.CompilerServices;
using IBot.Events;
using IBot.Misc;
using IBot.Resources;

namespace IBot
{
    internal class App
    {
        public void StartApp()
        {
            RuntimeHelpers.RunClassConstructor(typeof(CommandManager).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(EventManager).TypeHandle);

            ConnectionManager.BotConnectedEvent += (s, a) =>
            {
                Console.WriteLine(app.app_connected);
                PluginManager.BindEvents();

                //RegisterChannelEvents();
                //RegisterUserEvents();
            };

            ConnectionManager.BotDisconnectedEvent += (s, a) => { Console.WriteLine(app.app_disconnected); };
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

        public void StopApp()
        {
            ConnectionManager.DisconnectFromBotAccount();
        }
    }
}
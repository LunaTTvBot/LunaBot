using System;
using System.Linq;
using System.Text.RegularExpressions;
using IBot.Events.CustomEventArgs;
using IBot.Events.Misc;
using IBot.Misc;

namespace IBot.Events
{
    internal static class ChannelEventManager
    {
        public const string UserJoinPartPattern =
            @"^" + GlobalTwitchPatterns.TwichUserBasePattern +
            @"(JOIN|PART)\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")$";

        public const string RoomStateBasePattern =
            @"^@(.*)\s:" + GlobalTwitchPatterns.TwitchHostNamePattern +
            @"\sROOMSTATE\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")$";

        public const string SlowModePattern = @"slow=([\d]+)";
        public const string SubscriberModePattern = @"subs-only=([1|0])";
        public const string R9KModePattern = @"r9k=([1|0])";
        public const string BroadcasterLanguagePattern = @"broadcaster-lang=([\w\d-_]*)";

        public const string AllModePattern =
            BroadcasterLanguagePattern +
            @";" + R9KModePattern +
            @";" + SlowModePattern +
            @";" + SubscriberModePattern;

        public const string OperatorPattern =
            @"^:" + GlobalTwitchPatterns.JtvPattern +
            @"\sMODE\s" +
            @"\#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")" +
            @"\s(\+o|-o)\s" +
            @"(" + GlobalTwitchPatterns.TwitchUserNamePattern + @")$";

        public const string UserListPattern =
            @"^:(" + GlobalTwitchPatterns.TwitchUserNamePattern + @")" +
            @"." + GlobalTwitchPatterns.TwitchHostNamePattern +
            @"\s353\s" + GlobalTwitchPatterns.TwitchUserNamePattern +
            @"\s=\s#(" + GlobalTwitchPatterns.TwitchChannelNamePattern + @")" +
            @"\s:(.*)";

        private static readonly Regex RegExJoinPart = new Regex(UserJoinPartPattern);
        private static readonly Regex RegExRoomState = new Regex(RoomStateBasePattern);
        private static readonly Regex RegExSlowMod = new Regex(SlowModePattern);
        private static readonly Regex RegExSubMod = new Regex(SubscriberModePattern);
        private static readonly Regex RegExR9KMod = new Regex(R9KModePattern);
        private static readonly Regex RegExLangMod = new Regex(BroadcasterLanguagePattern);
        private static readonly Regex RegExAllMod = new Regex(AllModePattern);
        private static readonly Regex RegExOpMod = new Regex(OperatorPattern);
        private static readonly Regex RegExUserList = new Regex(UserListPattern);

        static ChannelEventManager()
        {                     
            IrcConnection.GetIrcConnection(ConnectionType.BotCon).RaiseMessageEvent += ConnectionOnRaiseMessageEvent;
        }

        public static void ConnectionOnRaiseMessageEvent(object sender, MessageEventArgs eArgs)
        {
            RaiseUserJoinPartEvent(eArgs.Message);
            RaiseRoomStateEvents(eArgs.Message);
            RaiseOperatorEvents(eArgs.Message);
            RaiseUserListEvent(eArgs.Message);
        }

        public static event EventHandler<UserEventArgs> UserJoinEvent;
        public static event EventHandler<UserEventArgs> UserPartEvent;
        public static event EventHandler<RoomStateAllEventArgs> RoomStateAllEvent;
        public static event EventHandler<RoomStateSlowModeEventArgs> RoomStateSlowModeEvent;
        public static event EventHandler<RoomStateSubscriberModeEventArgs> RoomStateSubscriberModeEvent;
        public static event EventHandler<RoomStateR9KModeEventArgs> RoomStateR9KModeEvent;
        public static event EventHandler<RoomStateLangModeEventArgs> RoomStateLangModeEvent;
        public static event EventHandler<OperatorModeEventArgs> OperatorGrantedEvent;
        public static event EventHandler<OperatorModeEventArgs> OperatorRevokedEvent;
        public static event EventHandler<UserListEventArgs> UserListEvent;

        private static void OnUserJoinEvent(UserEventArgs e) => UserJoinEvent?.Invoke(typeof(ChannelEventManager), e);
        private static void OnUserPartEvent(UserEventArgs e) => UserPartEvent?.Invoke(typeof(ChannelEventManager), e);
        private static void OnOperatorGrantedEvent(OperatorModeEventArgs e) => OperatorGrantedEvent?.Invoke(null, e);
        private static void OnOperatorRevokedEvent(OperatorModeEventArgs e) => OperatorRevokedEvent?.Invoke(null, e);

        private static void OnRoomStateAllEvent(RoomStateAllEventArgs e) => RoomStateAllEvent?.Invoke(null, e);

        private static void OnRoomStateSlowModeEvent(RoomStateSlowModeEventArgs e)
            => RoomStateSlowModeEvent?.Invoke(null, e);

        private static void OnRoomStateSubscriberModeEvent(RoomStateSubscriberModeEventArgs e)
            => RoomStateSubscriberModeEvent?.Invoke(null, e);

        private static void OnRoomStateR9KModeEvent(RoomStateR9KModeEventArgs e)
            => RoomStateR9KModeEvent?.Invoke(null, e);

        private static void OnRoomStateLangModeEvent(RoomStateLangModeEventArgs e)
            => RoomStateLangModeEvent?.Invoke(null, e);

        private static void OnUserListEvent(UserListEventArgs e) => UserListEvent?.Invoke(null, e);

        private static void RaiseUserListEvent(string message)
        {
            var match = RegExUserList.Match(message);
            if (!match.Success) return;

            var userArray = match.Groups[3].Value.Split(' ');           
            OnUserListEvent(new UserListEventArgs(userArray.ToList(), match.Groups[1].Value, match.Groups[2].Value));
        }

        private static void RaiseOperatorEvents(string message)
        {
            var match = RegExOpMod.Match(message);
            if(!match.Success) return;

            if(match.Groups[2].Value == "+o") {
                OnOperatorGrantedEvent(new OperatorModeEventArgs(match.Groups[1].Value, match.Groups[3].Value, OperatorType.Granted));
            } else {
                OnOperatorRevokedEvent(new OperatorModeEventArgs(match.Groups[1].Value, match.Groups[3].Value, OperatorType.Revoked));
            }
        }

        private static void RaiseRoomStateEvents(string message)
        {
            var bMatch = RegExRoomState.Match(message);
            if (!bMatch.Success) return;

            var match = RegExAllMod.Match(bMatch.Groups[1].Value);
            if (match.Success)
            {
                var tags = ParseRoomStateTags(bMatch.Groups[1].Value);
                OnRoomStateAllEvent(new RoomStateAllEventArgs(tags, bMatch.Groups[2].Value));

                return;
            }

            match = RegExSlowMod.Match(bMatch.Groups[1].Value);
            if (match.Success)
            {
                OnRoomStateSlowModeEvent(
                    new RoomStateSlowModeEventArgs(
                        match.Groups[1].Value != "0",
                        Convert.ToInt32(match.Groups[1].Value),
                        bMatch.Groups[2].Value
                        )
                    );

                return;
            }

            match = RegExSubMod.Match(bMatch.Groups[1].Value);
            if (match.Success)
            {
                OnRoomStateSubscriberModeEvent(
                    new RoomStateSubscriberModeEventArgs(
                        match.Groups[1].Value == "1",
                        bMatch.Groups[2].Value
                        )
                    );

                return;
            }

            match = RegExR9KMod.Match(bMatch.Groups[1].Value);
            if (match.Success)
            {
                OnRoomStateR9KModeEvent(
                    new RoomStateR9KModeEventArgs(
                        match.Groups[1].Value == "1",
                        bMatch.Groups[2].Value
                        )
                    );

                return;
            }

            match = RegExLangMod.Match(bMatch.Groups[1].Value);
            if (match.Success)
            {
                OnRoomStateLangModeEvent(
                    new RoomStateLangModeEventArgs(
                        match.Groups[1].Value != "",
                        match.Groups[1].Value,
                        bMatch.Groups[2].Value
                        )
                    );
            }
        }

        private static RoomStateTags ParseRoomStateTags(string tags)
        {
            var match = RegExAllMod.Match(tags);
            return !match.Success
                ? null
                : new RoomStateTags(Convert.ToInt64(match.Groups[3].Value), match.Groups[4].Value == "1",
                    match.Groups[2].Value == "1", match.Groups[1].Value);
        }

        private static void RaiseUserJoinPartEvent(string message)
        {
            var match = RegExJoinPart.Match(message);
            if (!match.Success) return;

            if (match.Groups[2].Value == "JOIN")
                OnUserJoinEvent(new UserEventArgs(match.Groups[1].Value, match.Groups[3].Value, UserEventType.Join));
            else
                OnUserPartEvent(new UserEventArgs(match.Groups[1].Value, match.Groups[3].Value, UserEventType.Part));
        }
    }
}
using System;
using IBot.Events;
using IBot.Facades.Events.Args.User;
using IBot.Facades.Events.Tags;

namespace IBot.Facades.Events
{
    public class UserEvents
    {
        static UserEvents()
        {
            UserEventManager.UserPublicMessageEvent +=
                (sender, args) => OnUserPublicMessageEvent(new PublicMessageEventArgs(args.UserName, args.Channel, args.Message, new UserMessageTags(args.Tags)));

            UserEventManager.UserWhisperMessageEvent +=
                (sender, args) => OnUserWhisperMessageEvent(new PrivateMessageEventArgs(args.UserName, args.ToUserName, args.Message, new UserMessageTags(args.Tags)));

            UserEventManager.UserStateEvent +=
                (sender, args) => OnUserStateEvent(new UserStateEventArgs(args.Channel, new UserStateTags(args.Tags)));
        }

        public static event EventHandler<PublicMessageEventArgs> UserPublicMessageEvent;
        private static void OnUserPublicMessageEvent(PublicMessageEventArgs e) => UserPublicMessageEvent?.Invoke(null, e);

        public static event EventHandler<PrivateMessageEventArgs> UserWhisperMessageEvent;
        private static void OnUserWhisperMessageEvent(PrivateMessageEventArgs e) => UserWhisperMessageEvent?.Invoke(null, e);        

        public static event EventHandler<UserStateEventArgs> UserStateEvent;               
        private static void OnUserStateEvent(UserStateEventArgs e) => UserStateEvent?.Invoke(null, e);
    }
}
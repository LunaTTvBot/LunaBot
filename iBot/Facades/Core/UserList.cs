using System;
using System.Collections.Generic;
using System.Linq;
using IBot.Core;
using IBot.Events.Args.UserList;
using IBot.Facades.TranslationExtensions;
using CoreUser = IBot.Models.User;
using CoreList = IBot.Core.UserList;
using CorePermissions = IBot.Core.PermissionManager;

namespace IBot.Facades.Core
{
    public static class UserList
    {
        public static event EventHandler<Events.Args.UserJoinEventArgs> OnUserJoined;
        public static event EventHandler<Events.Args.UserPartedEventArgs> OnUserParted;
        public static event EventHandler OnUserListChanged;

        public static List<User> GetUsers(string channel) => CoreList.GetUserList(channel)
                                                                     .Select(u => u.ToFacadeUser())
                                                                     .ToList();

        public static List<User> GetFollowers(string channel) => CoreList.GetUserList(channel)
                                                                         .Where(u => CorePermissions.GetRights(u.Username).HasFlag(Rights.Follower))
                                                                         .Select(u => u.ToFacadeUser())
                                                                         .ToList();

        public static List<User> GetSubscribers(string channel) => CoreList.GetUserList(channel)
                                                                           .Where(u => CorePermissions.GetRights(u.Username).HasFlag(Rights.Subscriber))
                                                                           .Select(u => u.ToFacadeUser())
                                                                           .ToList();

        static UserList()
        {
            CoreList.UserJoined += CoreListOnUserJoined;
            CoreList.UserParted += CoreListOnUserParted;
            CoreList.UserListUpdated += CoreListOnUserListUpdated;
        }

        private static void CoreListOnUserJoined(object sender, UserJoinEventArgs eventArgs)
        {
            OnUserJoined?.Invoke(null, new Events.Args.UserJoinEventArgs(eventArgs.JoinedUser.ToFacadeUser(), eventArgs.JoinTime));
        }

        private static void CoreListOnUserParted(object sender, UserPartedEventArgs eventArgs)
        {
            OnUserParted?.Invoke(null, new Events.Args.UserPartedEventArgs(eventArgs.PartedUser.ToFacadeUser(), eventArgs.PartTime));
        }

        private static void CoreListOnUserListUpdated(object sender, EventArgs eventArgs)
        {
            OnUserListChanged?.Invoke(null, new EventArgs());
        }

        public static void Initialise() {}
    }
}

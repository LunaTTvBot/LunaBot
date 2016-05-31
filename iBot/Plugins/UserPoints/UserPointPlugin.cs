using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using IBot.Core;
using IBot.Events;
using IBot.Events.Args.Users;
using IBot.Events.Commands;
using IBot.Models;
using IBot.Resources.Plugins.Points;
using NLog;

namespace IBot.Plugins.UserPoints
{
    // Loaded via PluginManager
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UserPointPlugin : IPlugin
    {
        private const string PointPropertyName = "UserPoints_Value";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly object TransactionLock = new object();
        private static readonly Dictionary<string, long> PointDictionary = new Dictionary<string, long>();
        private static Timer _pointAwardTimer;

        public string PluginName => "User Point Plugin";

        public void Init()
        {
            if (SettingsManager.GetSettings<PointSettings>().StartOnApplicationStartup)
                Start();

            RegisterCommands();
        }

        private void RegisterCommands()
        {
            var settings = SettingsManager.GetSettings<PointSettings>();

            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand
            {
                Name = "CheckOwnPoints",
                Description = string.Format(PointLocale.CheckOwnPointsDescription, settings.PointNamePlural),
                RegEx = string.IsNullOrWhiteSpace(settings.CommandRegex)
                            ? settings.PointNamePlural
                            : settings.CommandRegex,
                RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled,
                Action = ShowOwnPointsCommand
            });

            CommandManager.RegisterWhisperCommand(new WhisperCommand()
            {
                Name = "CheckUsersPoints",
                Description = string.Format(PointLocale.CheckUserPointsDescription, settings.PointNamePlural),
                RegEx = @"!check\s(\w+)",
                RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled,
                Action = CheckUserPointsCommand
            });

            CommandManager.RegisterPublicChannelCommand(new PublicChannelCommand()
            {
                Name = "TransferPoints",
                Description = string.Format(PointLocale.TransferPointsDescription, settings.PointNamePlural),
                RegEx = @"!transfer\s(\w+)\s([0-9]+)",
                RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled,
                Action = TransferPointsToUserCommand
            });

            CommandManager.RegisterWhisperCommand(new WhisperCommand()
            {
                Name = "GivePoints",
                Description = string.Format(PointLocale.TransferPointsDescription),
                RegEx = @"!give\s(\w+)\s([0-9]+)",
                RegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled,
                Action = GivePointsToUserCommand
            });
        }

        private void GivePointsToUserCommand(WhisperCommand publicChannelCommand, Match match, UserWhisperMessageEventArgs eArgs)
        {
            var settings = SettingsManager.GetSettings<PointSettings>();
            var currentChannel = SettingsManager.GetOwnerChannel();

            var targetUser = new User(match.Groups[1].Value) { ChannelName = currentChannel };
            var mod = new User(eArgs.UserName) { ChannelName = currentChannel };
            var amount = Convert.ToInt64(match.Groups[2].Value);

            AddPoints(targetUser, amount);
            IrcConnection.Write(ConnectionType.BotCon, AnswerType.Private, mod.Username,
                                string.Format(PointLocale.GivePointsSuccess, targetUser.Username, amount, settings.PointNamePlural));

            Logger.Trace("{0} gave {1} {2} points", mod.Username, targetUser.Username, amount);
        }

        private void TransferPointsToUserCommand(PublicChannelCommand publicChannelCommand, Match match, UserPublicMessageEventArgs eArgs)
        {
            var settings = SettingsManager.GetSettings<PointSettings>();
            var currentChannel = SettingsManager.GetOwnerChannel();

            var targetUser = new User(match.Groups[1].Value) { ChannelName = currentChannel };
            var user = new User(eArgs.UserName) { ChannelName = currentChannel };
            var amount = Convert.ToInt64(match.Groups[2].Value);

            if (!UserHasPoints(user, amount))
            {
                IrcConnection.Write(ConnectionType.BotCon, AnswerType.Private, user.Username,
                                    string.Format(PointLocale.NotEnoughPoints, settings.PointNamePlural));

                Logger.Trace("{0} couldn't give {1} {2} points, not enough points", user.Username, targetUser.Username, amount);

                return;
            }

            if (RemovePoints(user, amount))
            {
                AddPoints(targetUser, amount);
                IrcConnection.Write(ConnectionType.BotCon, eArgs.Channel,
                                    string.Format(PointLocale.UserGaveUserPoints, user.Username, targetUser.Username, amount, settings.PointNamePlural));

                Logger.Trace("{0} gave {1} {2} points", user.Username, targetUser.Username, amount);
            }
            else
            {
                IrcConnection.Write(ConnectionType.BotCon, AnswerType.Private, user.Username,
                                    String.Format(PointLocale.GenericTransferError, settings.PointNamePlural));

                Logger.Trace("something went wrong while removing '{0}' points from '{1}' to give to '{2}'", amount, user.Username, targetUser.Username);
            }
        }

        private static void CheckUserPointsCommand(WhisperCommand command, Match match, UserWhisperMessageEventArgs eArgs)
        {
            var settings = SettingsManager.GetSettings<PointSettings>();
            var currentChannel = SettingsManager.GetOwnerChannel();

            var requestedUser = new User(match.Groups[1].Value) { ChannelName = currentChannel };
            var mod = new User(eArgs.UserName) { ChannelName = currentChannel };

            if (!PermissionManager.GetRights(mod).HasFlag(Rights.Moderator))
            {
                IrcConnection.Write(ConnectionType.BotCon, AnswerType.Private, mod.Username, PointLocale.NoPermission);

                Logger.Trace("{0} called a Mod-only command ({1}) without sufficient rights", mod.Username, command.Name);
                return;
            }

            var points = GetPoints(requestedUser);

            IrcConnection.Write(ConnectionType.BotCon, AnswerType.Private, mod.Username,
                                String.Format(PointLocale.PointCheckSuccess, requestedUser.Username, points, settings.PointNamePlural));

            Logger.Trace("{0} checked {1}'s points", mod.Username, requestedUser.Username);
        }

        private static void ShowOwnPointsCommand(PublicChannelCommand command, Match matches, UserPublicMessageEventArgs eArgs)
        {
            var settings = SettingsManager.GetSettings<PointSettings>();

            var user = new User(eArgs.UserName) { ChannelName = eArgs.Channel };
            IrcConnection.Write(ConnectionType.BotCon, eArgs.Channel,
                                String.Format(PointLocale.CheckOwnPointsSuccess, eArgs.UserName, GetPoints(user), settings.PointNamePlural));

            Logger.Trace("{0} looked at its own points", user.Username);
        }

        private static void InitialiseTimer()
        {
            if (_pointAwardTimer != null)
            {
                if (_pointAwardTimer.Enabled)
                    _pointAwardTimer.Stop();

                _pointAwardTimer = null;
            }

            _pointAwardTimer = new Timer
            {
                AutoReset = false,
                Interval = SettingsManager.GetSettings<PointSettings>().PointAwardIntervalSeconds * 1000
            };
            _pointAwardTimer.Elapsed += PointAwardTimerOnElapsed;
        }

        private static void PointAwardTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            foreach (var user in UserList.GetUserList(SettingsManager.GetOwnerChannel()))
            {
                var settings = SettingsManager.GetSettings<PointSettings>();
                var basePoints = settings.PointsAwardedPerInterval;
                var multiplier = settings.PointsMultiplierViewer;

                if (PermissionManager.GetRights(user).HasFlag(Rights.Owner))
                    multiplier = settings.PointsMultiplierOwner;
                else if (PermissionManager.GetRights(user).HasFlag(Rights.Moderator))
                    multiplier = settings.PointsMultiplierMod;
                else if (PermissionManager.GetRights(user).HasFlag(Rights.Subscriber))
                    multiplier = settings.PointsMultiplierSub;
                else if (PermissionManager.GetRights(user).HasFlag(Rights.Follower))
                    multiplier = settings.PointsMultiplierFollower;

                var awardedPoints = basePoints * multiplier;

                AddPoints(user, awardedPoints);
            }
            _pointAwardTimer.Enabled = true;
        }

        public static void Start()
        {
            InitialiseTimer();
            _pointAwardTimer?.Start();
        }

        public static void Stop()
        {
            if (_pointAwardTimer == null)
                return;

            if (_pointAwardTimer.Enabled)
                _pointAwardTimer.Stop();
            _pointAwardTimer = null;
        }

        private static bool ChangeAmount(User user, long change, bool checkBalanceBeforeAction)
        {
            try
            {
                if (checkBalanceBeforeAction && !UserHasPoints(user, change))
                    return false;

                lock (TransactionLock)
                {
                    if (!PointDictionary.ContainsKey(user.Id))
                        AddUser(user);

                    var newValue = PointDictionary[user.Id] += change;
                    user.Set(PointPropertyName, newValue);
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }

        private static void AddUser(User user)
        {
            lock (TransactionLock)
            {
                if (PointDictionary.ContainsKey(user.Id))
                    return;

                PointDictionary.Add(user.Id, user.Get<long>(PointPropertyName));
            }
        }

        public static long GetPoints(User user)
        {
            try
            {
                lock (PointDictionary)
                {
                    var savedUser = PointDictionary.FirstOrDefault(u => u.Key == user.Id);

                    return savedUser.Key != null
                               ? savedUser.Value
                               : user.Get<long>(PointPropertyName);
                }
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return default(long);
            }
        }

        public static bool UserHasPoints(User user, long amount)
        {
            try
            {
                return user.Get<long>(PointPropertyName) >= amount;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                return false;
            }
        }

        public static void AddPoints(User user, long amount)
        {
            ChangeAmount(user, Math.Abs(amount), false);
            Logger.Trace("user {0} received {1} points", user.Id, amount);
        }

        public static bool RemovePoints(User user, long amount)
        {
            var success = ChangeAmount(user, amount * -1, true);
            Logger.Trace("user {0} lost {1} points", user.Id, amount);

            return success;
        }
    }
}

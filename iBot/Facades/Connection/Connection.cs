using System.Linq;
using IBot.Core;

namespace IBot.Facades.Connection
{
    public static class Connection
    {
        public static Result<string> WritePublicChatMessage(string message, string channel)
            => WriteMessage(message.Trim(), channel, ConnectionType.ChatCon, AnswerType.Public);

        public static Result<string> WritePrivateChatMessage(string message, string user)
            => WriteMessage(message.Trim(), user, ConnectionType.ChatCon, AnswerType.Private);

        public static Result<string> WritePublicBotMessage(string message, string channel)
            => WriteMessage(message.Trim(), channel, ConnectionType.BotCon, AnswerType.Public);

        public static Result<string> WritePrivateBotMessage(string message, string user)
            => WriteMessage(message.Trim(), user, ConnectionType.BotCon, AnswerType.Private);

        private static Result<string> WriteMessage(string message, string target, ConnectionType con, AnswerType type)
        {
            var msg = message.Trim();
            var result = new Result<string>("Message could not be sent.", 400, msg);

            var connection = IrcConnectionManager.GetConnection(con);
            if (connection == null)
                return result;

            if (type == AnswerType.Public)
            {
                if (!connection.GetChannels().Contains(target))
                    return result;
            }

            IrcConnection.Write(con, type, target, msg);
            return new Result<string>("Message has been sent successfully.", 0, msg);
        }
    }
}
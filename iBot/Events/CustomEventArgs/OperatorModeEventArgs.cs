namespace IBot.Events.CustomEventArgs
{
    internal class OperatorModeEventArgs
    {
        public OperatorModeEventArgs(string channel, string user, OperatorType opType)
        {
            Channel = channel;
            User = user;
            OpType = opType;
        }

        public string Channel { get; }
        public string User { get; }
        public OperatorType OpType { get; }
    }

    internal enum OperatorType
    {
        Granted,
        Revoked
    }
}
using System;

namespace IBot.Core
{
    [Flags]
    internal enum Rights
    {
        Owner = 1 << 0,
        Moderator = 1 << 1,
        Subscriber = 1 << 2,
        Follower = 1 << 3,
        Viewer = 1 << 4
    }
}

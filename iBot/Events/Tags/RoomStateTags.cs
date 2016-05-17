namespace IBot.Events.Tags
{
    internal class RoomStateTags
    {
        public RoomStateTags(long slowTime, bool subsOnly, bool r9K, string lang)
        {
            SlowTime = slowTime;
            SubsOnly = subsOnly;
            R9K = r9K;
            Lang = lang;
        }

        public long SlowTime { get; }
        public bool SubsOnly { get; }
        public bool R9K { get; }
        public string Lang { get; }
    }
}
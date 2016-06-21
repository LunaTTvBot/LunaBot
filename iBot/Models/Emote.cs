namespace IBot.Models
{
    internal class Emote
    {
        public Emote(int id, int start, int end)
        {
            Id = id;
            Start = start;
            End = end;
        }

        public int Id { get; }
        public int Start { get; }
        public int End { get; }
        public int Length => End - Start;
    }
}

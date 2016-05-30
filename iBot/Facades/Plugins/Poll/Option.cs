using CoreOption = IBot.Plugins.Poll.PollOption;

namespace IBot.Facades.Plugins.Poll
{
    public class Option
    {
        public Option(string name, int id)
        {
            Name = name;
            Id = id;
        }

        internal Option(CoreOption opt)
        {
            Name = opt.Name;
            Id = opt.Id;
        }

        public string Name { get; }
        public int Id { get; }
    }
}
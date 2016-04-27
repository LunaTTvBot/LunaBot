using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IRCConnectionTest.Misc;

namespace IRCConnectionTest
{
    internal class Channel
    {
        public Channel()
        {
            Name = "";
            Users = new ObservableCollection<User>();
            Users.CollectionChanged += UsersOnCollectionChanged;
        }

        public Channel(string name) : this()
        {
            Name = name;
        }

        [Key]
        public string Name { get; set; }

        public ObservableCollection<User> Users { get; set; }

        private void UsersOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
            => DatabaseContext.Save();

        public static Channel Get(string name)
        {
            Channel channel = null;

            var db = DatabaseContext.Get();
            lock (db)
            {
                channel = db.Channels.FirstOrDefault(c => c.Name == name);

                if (channel != null)
                {
                    channel.Load();
                    return channel; 
                }

                channel = new Channel(name);
                db.Channels.Add(channel);
                db.SaveChangesAsync();
            }

            return channel;
        }

        private void Load()
        {
            var db = DatabaseContext.Get();

            db.Entry(db.Channels.Find(Name)).Collection(c => c.Users).Load();
        }

        public static IEnumerable<Channel> GetAll()
        {
            var channels = DatabaseContext.Get().Channels.ToList();
            foreach (var channel in channels)
                channel.Load();

            return channels;
        }
    }
}

using System;
using System.Collections.Generic;
using IBot.Models;

namespace Tools
{
    internal static class EmoteTools
    {
        public static List<Emote> ParseEmotes(string tagEmotes)
        {
            if (string.IsNullOrWhiteSpace(tagEmotes))
                return new List<Emote>();

            var emotes = tagEmotes.Split('/');
            var list = new List<Emote>();

            foreach (var emote in emotes)
            {
                var parts = emote.Split(',');
                var lastId = 0;

                foreach (var part in parts)
                {
                    string[] startEnd;
                    int id;
                    if (part.Contains(":"))
                    {
                        var elements = part.Split(':');
                        id = Convert.ToInt32(elements[0]);
                        startEnd = elements[1].Split('-');
                        lastId = id;
                    }
                    else
                    {
                        id = Convert.ToInt32(lastId);
                        startEnd = part.Split('-');
                    }
                    var start = Convert.ToInt32(startEnd[0]);
                    var end = Convert.ToInt32(startEnd[1]);
                    list.Add(new Emote(id, start, end));
                }
            }

            return list;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static double EmotePercentageOfMessage(string message, string emoteTags)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(emoteTags))
                return 0.0d;

            var builder = new StringBuilder();

            var emotes = ParseEmotes(emoteTags).OrderBy(e => e.Start).ToList();

            var messageIndex = 0;
            for (int emoteIndex = 0; emoteIndex < emotes.Count(); emoteIndex++)
            {
                var emote = emotes[emoteIndex];
                builder.Append(message.Substring(messageIndex, emote.Start - messageIndex));

                // unicode replacement character, we use this char to mark an emote
                builder.Append("\ufffd\ufffd");

                messageIndex = emote.End + 1;
            }

            var filteredMessage = builder.ToString();
            var noEmoteMessage = filteredMessage.Replace("\ufffd", "");

            var fullLength = filteredMessage.Length;
            var noEmoteLength = noEmoteMessage.Length;

            var textPercentage = (100.0 / fullLength) * noEmoteLength;
            var emotePercentage = 100.0 - textPercentage;

            return emotePercentage;
        }
    }
}

using Rocket.API;
using System.Xml.Serialization;

namespace Arechi.CallVote
{
    public class CallVoteConfig : IRocketPluginConfiguration
    {
        public string Color;
        public bool FinishVoteEarly, NotifyCooldownOver, AutoVoteCaller;
        public int VoteTimer;
        public int VoteCooldown;
        public int RequiredPercent;
        public int MuteTime;

        [XmlArrayItem("Vote")]
        [XmlArray(ElementName = "Votes")]
        public Vote[] Votes;

        public void LoadDefaults()
        {
            Color = "yellow";
            AutoVoteCaller = true;
            FinishVoteEarly = true;
            NotifyCooldownOver = false;
            VoteTimer = 50;
            VoteCooldown = 50;
            RequiredPercent = 60;
            MuteTime = 5;

            Votes = new Vote[]
            {
                new Vote("Day", "d", true),
                new Vote("Night", "n", true),
                new Vote("Rain", "r", true),
                new Vote("Airdrop", "a", true),
                new Vote("AirdropAll", "aall", true),
                new Vote("HealAll", "h", true),
                new Vote("VehicleAll", "v", true),
                new Vote("ItemAll", "i", true),
                new Vote("Unlock", "u", true),
                new Vote("Kick", "k", true),
                new Vote("Mute", "m", true),
                new Vote("Spy", "s", true),
                new Vote("Custom", "c", true)
            };
        }
    }

    public sealed class Vote
    {
        [XmlAttribute("Name")]
        public string Name;

        [XmlAttribute("Alias")]
        public string Alias;

        [XmlAttribute("Enabled")]
        public bool Enabled;

        public Vote(string name, string alias, bool enabled)
        {
            Name = name;
            Alias = alias;
            Enabled = enabled;
        }

        public Vote()
        {
            Name = "";
            Alias = "";
            Enabled = true;
        }
    }
}
using Rocket.API;
using System.Xml.Serialization;

namespace Arechi.CallVote
{
    public class Config : IRocketPluginConfiguration
    {
        public string Color;
        public bool FinishVoteEarly, NotifyCooldownOver, AutoVoteCaller;
        public int VoteTimer, VoteCooldown, RequiredPercent, MuteTime, MinimumPlayers;

        [XmlArray(ElementName = "Votes"), XmlArrayItem(ElementName = "Vote")]
        public Vote[] Votes;

        public void LoadDefaults()
        {
            Color = "yellow";
            AutoVoteCaller = true;
            FinishVoteEarly = true;
            NotifyCooldownOver = false;
            MinimumPlayers = 0;
            VoteTimer = 60;
            VoteCooldown = 300;
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
                new Vote("Custom", "c", true),
                new Vote("MaxSkills", "ms", true)
            };
        }
    }
}
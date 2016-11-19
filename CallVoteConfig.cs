using Rocket.API;

namespace Arechi.CallVote
{
    public class CallVoteConfig : IRocketPluginConfiguration
    {
        public string Color;
        public bool DayNightVote, RainVote, AirdropVote;
        public int VoteTimer;
        public int VoteCooldown;
        public int RequiredPercent;

        public void LoadDefaults()
        {
            Color = "yellow";
            DayNightVote = true;
            RainVote = true;
            AirdropVote = true;
            VoteTimer = 50;
            VoteCooldown = 50;
            RequiredPercent = 60;
        }
    }
}
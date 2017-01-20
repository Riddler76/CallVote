using Rocket.API;

namespace Arechi.CallVote
{
    public class CallVoteConfig : IRocketPluginConfiguration
    {
        public string Color;
        public bool DayVote, NightVote, RainVote, AirdropVote, AirdropAllVote, HealAllVote, VehicleAllVote, KickVote, SpyVote, CustomVote, FinishVoteEarly, NotifyCooldownOver;
        public int VoteTimer;
        public int VoteCooldown;
        public int RequiredPercent;

        public void LoadDefaults()
        {
            Color = "yellow";
            FinishVoteEarly = true;
            NotifyCooldownOver = false;
            DayVote = true;
            NightVote = true;
            RainVote = true;
            AirdropVote = true;
            AirdropAllVote = true;
            HealAllVote = false;
            VehicleAllVote = false;
            KickVote = true;
            SpyVote = true;
            CustomVote = true;
            VoteTimer = 50;
            VoteCooldown = 50;
            RequiredPercent = 60;
        }
    }
}
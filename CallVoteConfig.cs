using Rocket.API;

namespace Arechi.CallVote
{
    public class CallVoteConfig : IRocketPluginConfiguration
    {
        public int VoteTimer;
        public int VoteCooldown;
        public int RequiredPercent;

        public void LoadDefaults()
        {
            VoteTimer = 50;
            VoteCooldown = 50;
            RequiredPercent = 60;
        }
    }
}
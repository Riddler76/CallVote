using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Arechi.CallVote
{
    public class Plugin : RocketPlugin<Config>
    {
        public static Plugin Instance;
        public Color MessageColor;
        public Dictionary<CSteamID, DateTime> MutedPlayers;
        public List<CSteamID> Voters;
        public CSteamID PlayerToKickOrMute, PlayerToSpy;
        public bool VoteInProgress, VoteInCooldown, VoteFinished;
        public string CurrentVote;
        public ushort ItemToGive;
        
        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.Color, Color.yellow);
            VoteInProgress = false;
            VoteInCooldown = false;
            VoteFinished = false;
            Voters = new List<CSteamID>();
            Utility.LogInfo();
        }

        protected override void Unload()
        {
            Voters.Clear();
            MutedPlayers.Clear();
            StopCoroutine(Voting.VotingProcess());
            if (Configuration.Instance.Votes[10].Enabled) { MutedPlayers.Clear(); Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted -= OnChatted; }
            Logger.LogWarning("CallVote has been unloaded!");
        }

        public delegate void VoteStart(IRocketPlayer player, string vote);
        public event VoteStart OnVoteStart;

        public delegate void PlayerVote(UnturnedPlayer player, string vote, int percent);
        public event PlayerVote OnPlayerVote;

        public delegate void VoteResult(string vote, int percent, string result);
        public event VoteResult OnVoteResult;

        internal void VoteStarted(IRocketPlayer player, string vote) { if (OnVoteStart != null) OnVoteStart(player, vote); }
        internal void PlayerVoted(UnturnedPlayer player, string vote, int percent) { if (OnPlayerVote != null) OnPlayerVote(player, vote, percent); }
        internal void VoteConcluded(string vote, int percent, string result) { if (OnVoteResult != null) OnVoteResult(vote, percent, result); }

        public void OnChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            if (MutedPlayers.ContainsKey(player.CSteamID) && !message.StartsWith("/") && chatMode == EChatMode.GLOBAL)
            {
                DateTime mute;
                int time = Configuration.Instance.MuteTime;
                MutedPlayers.TryGetValue(player.CSteamID, out mute);
                if ((DateTime.Now - mute).TotalMinutes >= time)
                {
                    MutedPlayers.Remove(player.CSteamID);
                }
                else
                {
                    cancel = true;
                    UnturnedChat.Say(player, Translate("mute_reason", time), Color.green);
                }
            }
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    { "", "==============================[Help]==============================" },
                    { "vote_help", "The votes you can call are: {0}. You can start one with /cvote <vote name|alias>" },
                    { "", "=============================[Events]=============================" },
                    { "vote_started", "{0} has called a {1} second vote to {2}." },
                    { "vote_ongoing", "[{2} Vote]: {0}%. Required: {1}%. Type /cv to vote." },
                    { "vote_success", "The vote was successful." },
                    { "vote_failed", "The vote was unsuccessful." },
                    { "", "===========================[Rejections]===========================" },
                    { "already_voted", "You have already voted!" },
                    { "no_ongoing_votes", "There are no votes currently active." },
                    { "not_enough_players", "At least {0} players are required to start a vote!" },
                    { "vote_cooldown", "A vote may only be called every {0} seconds." },
                    { "vote_disabled", "This type of vote is disabled on the server." },
                    { "vote_no_permission", "This type of vote is not permitted for you." },
                    { "vote_error", "Only one vote may be called at a time." },
                    { "", "==============================[Misc]==============================" },
                    { "kick_reason", "The majority decided so." },
                    { "mute_reason", "The majority decided to mute you. Wait {0} minutes." },
                    { "check_ready", "The spy screenshot is ready for {0}. Press ESC to check it out." },
                    { "airdropall_message", "An airdrop is coming right to your spot, {0}!" },
                    { "vehicleall_message", "You received a {0}!" },
                    { "itemall_message", "You received a {0}!" },
                    { "mute_message", "You have been muted for {0} minutes." },
                    { "cooldown_over", "The vote cooldown is over. You can start another vote if desired." },
                    { "", "===========================[Vote types]===========================" },
                    { "Day", "make it Day" },
                    { "Night", "make it Night" },
                    { "Rain", "start/stop Rain" },
                    { "Airdrop", "summon an Airdrop" },
                    { "AirdropAll", "summon an Airdrop for everyone" },
                    { "HealAll", "Heal everyone" },
                    { "VehicleAll", "give everyone a random Vehicle" },
                    { "ItemAll", "give everyone a {0}" },
                    { "MaxSkills", "Max everyone's skills" },
                    { "Unlock", "Unlock every Vehicle" },
                    { "Kick", "Kick {0}" },
                    { "Mute", "Mute {0} for {1} minutes" },
                    { "Spy", "Spy someone together" },
                    { "Custom", "{0}" },
                };
            }
        }
    }
}

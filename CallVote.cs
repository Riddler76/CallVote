using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Arechi.CallVote
{
    public class CallVote : RocketPlugin<CallVoteConfig>
    {
        public int onlinePlayers = 0;
        public int totalFor = 0;
        public bool VoteInProgress = false;
        public bool VoteInCooldown = false;
        public Dictionary<Steamworks.CSteamID, string> voteTracker = new Dictionary<Steamworks.CSteamID, string>();
        public Dictionary<Steamworks.CSteamID, string> playerTracker = new Dictionary<Steamworks.CSteamID, string>();
        public static CallVote Instance;

        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            Rocket.Core.Logging.Logger.Log("CallVote has been loaded! DayNight: " + Instance.Configuration.Instance.DayNightVote + "| Rain: " + Instance.Configuration.Instance.RainVote + "| Airdrop: " + Instance.Configuration.Instance.AirdropVote);
            Rocket.Core.Logging.Logger.Log("Vote Timer: " + Instance.Configuration.Instance.VoteTimer + " seconds");
            Rocket.Core.Logging.Logger.Log("Vote Cooldown: " + Instance.Configuration.Instance.VoteCooldown + " seconds");
            Rocket.Core.Logging.Logger.Log("Required Percent: " + Instance.Configuration.Instance.RequiredPercent + "%");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            Rocket.Core.Logging.Logger.Log("CallVote has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            playerTracker.Add(player.CSteamID, player.CharacterName);
            ++onlinePlayers;
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            --onlinePlayers;
            playerTracker.Remove(player.CSteamID);
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"vote_started", "{0} has called a vote to make it {1}. You have {2} seconds to type /cvote yes to vote." },
                    {"vote_started_storm", "{0} has called a vote to start/stop rain. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_airdrop", "{0} has called a vote to summon an airdrop. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_ongoing", "{0}% Yes. Required: {1}%." },
                    {"already_voted", "You have already voted!" },
                    {"vote_error", "Only one vote may be called at a time." },
                    {"vote_disabled", "This type of vote is disabled on the server." },
                    {"vote_cooldown", "A vote may only be called every {0} seconds." },
                    {"no_ongoing_votes", "There are no votes currently active." },
                    {"day_success", "The vote to make it Day was succesful." },
                    {"day_failed", "The vote to make it Day was unsuccesful."},
                    {"night_success", "The vote to make it Night was succesful." },
                    {"night_failed", "The vote to make it Night was unsuccesful."},
                    {"storm_success", "The vote to start or stop Rain was succesful." },
                    {"storm_failed", "The vote to start or stop Rain was unsuccesful."},
                    {"airdrop_success", "The vote to summon an Airdrop was succesful." },
                    {"airdrop_failed", "The vote to summon an Airdrop was unsuccesful."},
                };
            }
        }

        public static void initiateVote(string kind)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteTimer * 1000);
                double percentFor = ((Instance.totalFor / Instance.onlinePlayers) * 100);
                percentFor = Math.Round(percentFor, 2);
                if (percentFor >= Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say(Instance.Translate(kind + "_success"), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.yellow));
                    CommandWindow.input.onInputText(kind);
                }
                else if (percentFor < Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say(Instance.Translate(kind + "_failed"), Color.red);
                }
                Instance.voteTracker.Clear();
                Instance.VoteInCooldown = true;
                Instance.VoteInProgress = false;
                Instance.totalFor = 0;

                initiateVoteCooldown();

            }).Start();
        }

        public static void initiateVoteCooldown()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteCooldown * 1000);

                Instance.VoteInCooldown = false;

            }).Start();
        }
    }
}

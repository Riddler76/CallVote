using Rocket.Core.Logging;
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
        public Dictionary<Steamworks.CSteamID, String> voteTracker = new Dictionary<Steamworks.CSteamID, String>();
        public static CallVote Instance;

        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
            Rocket.Core.Logging.Logger.Log("CallVote has been loaded!");
            Rocket.Core.Logging.Logger.Log("VoteTimer: " + Configuration.Instance.VoteTimer.ToString() + " seconds");
            Rocket.Core.Logging.Logger.Log("VoteCooldown: " + Configuration.Instance.VoteCooldown.ToString() + " seconds");
            Rocket.Core.Logging.Logger.Log("Required Percent: " + Configuration.Instance.RequiredPercent.ToString() + "%");
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
            Rocket.Core.Logging.Logger.Log("CallVote has been unloaded!");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer player)
        {
            onlinePlayers += 1;
        }

        private void Events_OnPlayerDisconnected(UnturnedPlayer player)
        {
            onlinePlayers -= 1;
        }

        public static void initiateDayVote()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteTimer * 1000);
                double percentFor = (double)((Instance.totalFor / Instance.onlinePlayers) * 100);
                percentFor = Math.Round(percentFor, 2);
                if (percentFor >= Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say("The vote to make it daytime was successful.", Color.yellow);
                    CommandWindow.input.onInputText("day");
                }
                else if (percentFor < Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say("The vote to make it daytime was unsuccessful.", Color.red);
                }
                Instance.voteTracker.Clear();
                Instance.VoteInCooldown = true;
                Instance.VoteInProgress = false;
                Instance.totalFor = 0;

                initiateDayVoteCooldown();

            }).Start();
        }

        public static void initiateDayVoteCooldown()
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

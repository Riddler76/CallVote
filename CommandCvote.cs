using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace Arechi.CallVote
{
    public class CommandCvote : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Player;
            }
        }

        public string Help
        {
            get { return "Start a vote to make it day, night, start/stop rain or summon an airdrop"; }
        }

        public string Name
        {
            get { return "cvote"; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "cvote" };
            }
        }

        public string Syntax
        {
            get { return "day|night|storm|airdrop|yes"; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" && CallVote.Instance.Configuration.Instance.DayNightVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, command[0], CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote(command[0]);
                return;
            }

            if (command[0].ToString().ToLower() ==  "rain" && CallVote.Instance.Configuration.Instance.RainVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_storm", player.DisplayName, CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote("storm");
                return;
            }

            if (command[0].ToString().ToLower() == "airdrop" && CallVote.Instance.Configuration.Instance.AirdropVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_airdrop", player.DisplayName, CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote(command[0]);
                return;
            }

            if (command[0].ToString().ToLower() == "yes" && CallVote.Instance.VoteInProgress == true)
            {
                if (!CallVote.Instance.voteTracker.ContainsKey(player.CSteamID))
                {
                    CallVote.Instance.totalFor += 1;
                    double percentFor = (double)(CallVote.Instance.totalFor / CallVote.Instance.onlinePlayers) * 100;

                    UnturnedChat.Say(CallVote.Instance.Translate("vote_ongoing", percentFor, CallVote.Instance.Configuration.Instance.RequiredPercent), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                    CallVote.Instance.voteTracker.Add(player.CSteamID, player.CharacterName);
                }
                else if (CallVote.Instance.voteTracker.ContainsKey(player.CSteamID))
                {
                    UnturnedChat.Say(player, CallVote.Instance.Translate("already_voted"), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                }
                return;
            }

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" || command[0].ToString().ToLower() == "storm" || command[0].ToString().ToLower() == "airdrop" && CallVote.Instance.VoteInProgress == true)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_error"), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                return;
            }

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" || command[0].ToString().ToLower() == "storm" || command[0].ToString().ToLower() == "airdrop" && CallVote.Instance.VoteInCooldown == true)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_cooldown", CallVote.Instance.Configuration.Instance.VoteCooldown), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                return;
            }

            if (command[0].ToString().ToLower() == "yes" && CallVote.Instance.VoteInProgress == false)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("no_ongoing_votes"), Color.red);
                return;
            }
        }
    }
}

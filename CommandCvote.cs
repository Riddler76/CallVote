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
            get { return "Start a vote to make it day, night, start/stop rain or summon an airdrop or simply vote for an ongoing vote"; }
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
            get { return "day|night|rain|airdrop|yes or d|n|r|a|y"; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command.Length == 0)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_help"), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
            }

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" || command[0].ToString().ToLower() == "d" || command[0].ToString().ToLower() == "n" && CallVote.Instance.Configuration.Instance.DayNightVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, command[0], CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote(command[0]);
                return;
            }

            if (command[0].ToString().ToLower() ==  "rain" || command[0].ToString().ToLower() == "r" && CallVote.Instance.Configuration.Instance.RainVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_storm", player.DisplayName, CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote("storm");
                return;
            }

            if (command[0].ToString().ToLower() == "airdrop" || command[0].ToString().ToLower() == "a" && CallVote.Instance.Configuration.Instance.AirdropVote && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_airdrop", player.DisplayName, CallVote.Instance.Configuration.Instance.VoteTimer), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateVote(command[0]);
                return;
            }

            if (command[0].ToString().ToLower() == "yes" || command[0].ToString().ToLower() == "y" && CallVote.Instance.VoteInProgress == true)
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

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" || command[0].ToString().ToLower() == "rain" || command[0].ToString().ToLower() == "airdrop" ||
                command[0].ToString().ToLower() == "d" || command[0].ToString().ToLower() == "n" || command[0].ToString().ToLower() == "r" || command[0].ToString().ToLower() == "a" && CallVote.Instance.VoteInProgress == true)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_error"), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                return;
            }

            if (command[0].ToString().ToLower() == "day" || command[0].ToString().ToLower() == "night" || command[0].ToString().ToLower() == "rain" || command[0].ToString().ToLower() == "airdrop" ||
                command[0].ToString().ToLower() == "d" || command[0].ToString().ToLower() == "n" || command[0].ToString().ToLower() == "r" || command[0].ToString().ToLower() == "a" && CallVote.Instance.VoteInCooldown == true)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_cooldown", CallVote.Instance.Configuration.Instance.VoteCooldown), UnturnedChat.GetColorFromName(CallVote.Instance.Configuration.Instance.Color, Color.yellow));
                return;
            }

            if (command[0].ToString().ToLower() == "yes" || command[0].ToString().ToLower() == "y" && CallVote.Instance.VoteInProgress == false)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("no_ongoing_votes"), Color.red);
                return;
            }
        }
    }
}

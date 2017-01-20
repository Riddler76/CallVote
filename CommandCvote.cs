using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
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
            get { return AllowedCaller.Player; }
        }

        public string Help
        {
            get { return "Start a vote to make something happen or simply vote for an ongoing vote"; }
        }

        public string Name
        {
            get { return "cvote"; }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "cvote" }; }
        }

        public string Syntax
        {
            get { return "day|night|rain|airdrop|airdropall|healall|vehicleall|kick|spy|custom|yes or d|n|r|a|y|h|v|k|s|c"; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            bool VoteInProgress = CallVote.Instance.VoteInProgress;
            bool VoteInCooldown = CallVote.Instance.VoteInCooldown;
            bool DayAllowed = CallVote.Instance.Configuration.Instance.DayVote;
            bool NightAllowed = CallVote.Instance.Configuration.Instance.NightVote;
            bool RainAllowed = CallVote.Instance.Configuration.Instance.RainVote;
            bool AirdropAllowed = CallVote.Instance.Configuration.Instance.AirdropVote;
            bool AirdropAllAllowed = CallVote.Instance.Configuration.Instance.AirdropAllVote;
            bool HealAllAllowed = CallVote.Instance.Configuration.Instance.HealAllVote;
            bool VehicleAllAllowed = CallVote.Instance.Configuration.Instance.VehicleAllVote;
            bool KickAllowed = CallVote.Instance.Configuration.Instance.KickVote;
            bool SpyAllowed = CallVote.Instance.Configuration.Instance.SpyVote;
            bool CustomAllowed = CallVote.Instance.Configuration.Instance.CustomVote;
            int VoteTimer = CallVote.Instance.Configuration.Instance.VoteTimer;
            int VoteCooldown = CallVote.Instance.Configuration.Instance.VoteCooldown;

            //Command usage
            if (command.Length == 0)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_help"), CallVote.Instance.MessageColor);
                return;
            }

            #region Initiating Votes
            //Initiate Day vote
            if ((String.Compare(command[0], "Day", true) == 0 || String.Compare(command[0], "d", true) == 0) && DayAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.day")) return;
                CallVote.StartVote("Day");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_day", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Night vote
            if ((String.Compare(command[0], "Night", true) == 0 || String.Compare(command[0], "n", true) == 0) && NightAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.night")) return;
                CallVote.StartVote("Night");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_night", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Rain vote
            if ((String.Compare(command[0], "Rain", true) == 0 || String.Compare(command[0], "r", true) == 0) && RainAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.rain")) return;
                CallVote.StartVote("Storm");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_storm", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Airdrop vote
            if ((String.Compare(command[0], "Airdrop", true) == 0 || String.Compare(command[0], "a", true) == 0) && AirdropAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.airdrop")) return;
                CallVote.StartVote("Airdrop");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_airdrop", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Airdrop All vote
            if (String.Compare(command[0], "Airdropall", true) == 0 && AirdropAllAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.airdropall")) return;
                CallVote.StartVote("Airdropall");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_airdropall", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Heal All vote
            if ((String.Compare(command[0], "Healall", true) == 0 || String.Compare(command[0], "h", true) == 0) && HealAllAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.healall")) return;
                CallVote.StartVote("Healall");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_healall", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Vehicle All vote
            if ((String.Compare(command[0], "Vehicleall", true) == 0 || String.Compare(command[0], "v", true) == 0) && VehicleAllAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.vehicleall")) return;
                CallVote.StartVote("Vehicleall");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_vehicleall", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Kick vote
            if ((String.Compare(command[0], "Kick", true) == 0 || String.Compare(command[0], "k", true) == 0) && UnturnedPlayer.FromName(command[1]) != null && command.Length == 2 && KickAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.kick")) return;
                CallVote.Instance.PlayerToKick = UnturnedPlayer.FromName(command[1]).CSteamID;
                CallVote.StartVote("Kick");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_kick", player.DisplayName, VoteTimer, UnturnedPlayer.FromName(command[1]).DisplayName), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Spy vote
            if ((String.Compare(command[0], "Spy", true) == 0 || String.Compare(command[0], "s", true) == 0) && UnturnedPlayer.FromName(command[1]) != null && command.Length == 2 && SpyAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.spy")) return;
                CallVote.Instance.PlayerToSpy = UnturnedPlayer.FromName(command[1]).CSteamID;
                CallVote.StartVote("Spy");
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_spy", player.DisplayName, VoteTimer), CallVote.Instance.MessageColor);
                return;
            }

            //Initiate Custom vote
            if ((String.Compare(command[0], "Custom", true) == 0 || String.Compare(command[0], "c", true) == 0) && command.Length > 1 && CustomAllowed == true && VoteInProgress == false && VoteInCooldown == false)
            {
                if (!player.HasPermission("cvote.custom")) return;
                CallVote.StartVote("Custom");
                string Message = "";
                for (int i = 0; i < command.Length; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    string Word = command[i];
                    Message += Word + " ";
                }
                UnturnedChat.Say(CallVote.Instance.Translate("vote_started_custom", player.DisplayName, VoteTimer, Message), CallVote.Instance.MessageColor);
                return;
            }
            #endregion

            //Voting
            if ((String.Compare(command[0], "Yes", true) == 0 || String.Compare(command[0], "y", true) == 0) && VoteInProgress == true)
            {
                if (!CallVote.Instance.Voters.Contains(player.CSteamID))
                {
                    CallVote.Instance.Voters.Add(player.CSteamID);
                    double VotesFor = Math.Round((double)(CallVote.Instance.Voters.Count / Provider.clients.Count) * 100, 2);
                    UnturnedChat.Say(CallVote.Instance.Translate("vote_ongoing", VotesFor, CallVote.Instance.Configuration.Instance.RequiredPercent), CallVote.Instance.MessageColor);
                    if (VotesFor >= CallVote.Instance.Configuration.Instance.RequiredPercent && CallVote.Instance.Configuration.Instance.FinishVoteEarly == true)
                    {
                        CallVote.Instance.FinishVoteNow();
                    }
                }
                else if (CallVote.Instance.Voters.Contains(player.CSteamID))
                {
                    UnturnedChat.Say(player, CallVote.Instance.Translate("already_voted"), CallVote.Instance.MessageColor);
                }
                return;
            }

            //Initiating another vote in the middle of one
            if (command.Length == 1 && VoteInProgress == true && (command[0].ToLower() != "yes" || command[0].ToLower() != "y"))
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_error"), CallVote.Instance.MessageColor);
                return;
            }

            //Nothing to vote for
            if ((String.Compare(command[0], "Yes", true) == 0 || String.Compare(command[0], "y", true) == 0) && VoteInProgress == false)
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("no_ongoing_votes"), Color.red);
                return;
            }

            //Vote in cooldown
            if (command.Length == 1 && VoteInCooldown == true && (command[0].ToLower() != "yes" || command[0].ToLower() != "y"))
            {
                UnturnedChat.Say(player, CallVote.Instance.Translate("vote_cooldown", VoteCooldown), CallVote.Instance.MessageColor);
                return;
            }
        }
    }
}

using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arechi.CallVote
{
    public class CommandCvote : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string>() { "cv" }; }
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
            get { return "<vote>|<vote alias>"; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            bool VoteInProgress = CallVote.Instance.VoteInProgress;
            bool VoteInCooldown = CallVote.Instance.VoteInCooldown;
            int VoteTimer = CallVote.Instance.Configuration.Instance.VoteTimer;
            int VoteCooldown = CallVote.Instance.Configuration.Instance.VoteCooldown;

            if (command.Length == 0)
            {
                if (!VoteInProgress)
                {
                    UnturnedChat.Say(player, CallVote.Instance.Translate("no_ongoing_votes"), Color.red);
                    CallVote.Instance.Help(player);
                }
                else
                {
                    CallVote.Instance.Vote(player);
                }
                
                return;
            }

            if (command.Length == 1)
            {
                if (VoteInProgress)
                {
                    UnturnedChat.Say(player, CallVote.Instance.Translate("vote_error"), CallVote.Instance.MessageColor);
                    return;
                }

                if (VoteInCooldown)
                {
                    UnturnedChat.Say(player, CallVote.Instance.Translate("vote_cooldown", VoteCooldown), CallVote.Instance.MessageColor);
                    return;
                }
            }

            if (!VoteInProgress && !VoteInCooldown)
            {
                for (int i = 0; i < CallVote.Instance.Configuration.Instance.Votes.Length; i++)
                {
                    if (String.Compare(command[0], CallVote.Instance.Configuration.Instance.Votes[i].Name, true) == 0 || String.Compare(command[0], CallVote.Instance.Configuration.Instance.Votes[i].Alias, true) == 0)
                    {
                        if (!CallVote.Instance.Configuration.Instance.Votes[i].Enabled) { CallVote.Instance.Notify(player, 1); return; }
                        if (!player.HasPermission("cvote." + CallVote.Instance.Configuration.Instance.Votes[i].Name.ToLower())) { CallVote.Instance.Notify(player, 2); return; }

                        if (command.Length == 1)
                        {
                            UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate(CallVote.Instance.Configuration.Instance.Votes[i].Name)), CallVote.Instance.MessageColor);
                        }

                        else if (command.Length == 2)
                        {
                            if (i == 7)
                            {
                                ushort item;
                                ushort.TryParse(command[1], out item);
                                CallVote.Instance.ItemToGive = item;
                                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate("ItemAll", UnturnedItems.GetItemAssetById(CallVote.Instance.ItemToGive).itemName)), CallVote.Instance.MessageColor);
                            }
                            else if (i == 9)
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { CallVote.Instance.PlayerToKickOrMute = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate(CallVote.Instance.Configuration.Instance.Votes[i].Name, UnturnedPlayer.FromName(command[1]).DisplayName)), CallVote.Instance.MessageColor);
                            }
                            else if (i == 10)
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { CallVote.Instance.PlayerToKickOrMute = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate(CallVote.Instance.Configuration.Instance.Votes[i].Name, UnturnedPlayer.FromName(command[1]).DisplayName, CallVote.Instance.Configuration.Instance.MuteTime)), CallVote.Instance.MessageColor);
                            }
                            else if (i == 11)
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { CallVote.Instance.PlayerToSpy = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate("Spy")), CallVote.Instance.MessageColor);
                            }
                        }

                        else
                        {
                            string Message = "";
                            for (int x = 0; x < command.Length; x++)
                            {
                                if (x == 0)
                                {
                                    continue;
                                }

                                string Word = command[x];
                                Message += Word + " ";
                            }
                            UnturnedChat.Say(CallVote.Instance.Translate("vote_started", player.DisplayName, VoteTimer, CallVote.Instance.Translate("Custom", Message)), CallVote.Instance.MessageColor);
                        }

                        CallVote.StartVote(CallVote.Instance.Configuration.Instance.Votes[i].Name);
                        if (CallVote.Instance.Configuration.Instance.AutoVoteCaller) { CallVote.Instance.Vote(player); }
                    }
                }
            }
        }
    }
}

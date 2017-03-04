using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arechi.CallVote
{
    public class Command : IRocketCommand
    {
        public List<string> Aliases { get { return new List<string>() { "cv" }; } }
        public AllowedCaller AllowedCaller { get { return AllowedCaller.Both; } }
        public string Help { get { return "Start a vote to make something happen or simply vote for an ongoing vote"; } }
        public string Name { get { return "cvote"; } }
        public List<string> Permissions { get { return new List<string>() { "cvote" }; } }
        public string Syntax { get { return "<vote name|alias>"; } }
        public void Execute(IRocketPlayer caller, string[] command)
        {
            bool VoteInProgress = Plugin.Instance.VoteInProgress;
            bool VoteInCooldown = Plugin.Instance.VoteInCooldown;
            int VoteTimer = Plugin.Instance.Configuration.Instance.VoteTimer;
            int VoteCooldown = Plugin.Instance.Configuration.Instance.VoteCooldown;

            if (command.Length == 0)
            {
                if (!VoteInProgress)
                {
                    UnturnedChat.Say(caller, Plugin.Instance.Translate("no_ongoing_votes"), Color.red);
                    Utility.Help((UnturnedPlayer)caller);
                }
                else { Voting.Vote(caller); }
                return;
            }

            if (command.Length == 1)
            {
                if (VoteInProgress)
                {
                    UnturnedChat.Say(caller, Plugin.Instance.Translate("vote_error"), Plugin.Instance.MessageColor);
                    return;
                }

                if (VoteInCooldown)
                {
                    UnturnedChat.Say(caller, Plugin.Instance.Translate("vote_cooldown", VoteCooldown), Plugin.Instance.MessageColor);
                    return;
                }
            }

            if (!VoteInProgress && !VoteInCooldown)
            {
                if (Utility.PlayerRequirement() == false) { UnturnedChat.Say(caller, Plugin.Instance.Translate("not_enough_players", Plugin.Instance.Configuration.Instance.MinimumPlayers), Color.red); return; }
                foreach (Vote vote in Plugin.Instance.Configuration.Instance.Votes)
                {
                    if (String.Compare(command[0], vote.Name, true) == 0 || String.Compare(command[0], vote.Alias, true) == 0)
                    {
                        if (!vote.Enabled) { Utility.Notify((UnturnedPlayer)caller, 1); return; }
                        if (!caller.HasPermission("cvote." + vote.Name.ToLower())) { Utility.Notify((UnturnedPlayer)caller, 2); return; }

                        if (command.Length == 1)
                        {
                            if (vote.Name == "ItemAll" || vote.Name == "Kick" || vote.Name == "Mute" || vote.Name == "Spy") return;
                            UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name)), Plugin.Instance.MessageColor);   
                        }

                        else if (command.Length == 2)
                        {
                            if (vote.Name == "ItemAll")
                            {
                                ushort item;
                                ushort.TryParse(command[1], out item);
                                Plugin.Instance.ItemToGive = item;
                                UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name, UnturnedItems.GetItemAssetById(Plugin.Instance.ItemToGive).itemName)), Plugin.Instance.MessageColor);
                            }
                            else if (vote.Name == "Kick")
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { Plugin.Instance.PlayerToKickOrMute = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name, UnturnedPlayer.FromName(command[1]).DisplayName)), Plugin.Instance.MessageColor);
                            }
                            else if (vote.Name == "Mute")
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { Plugin.Instance.PlayerToKickOrMute = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name, UnturnedPlayer.FromName(command[1]).DisplayName, Plugin.Instance.Configuration.Instance.MuteTime)), Plugin.Instance.MessageColor);
                            }
                            else if (vote.Name == "Spy")
                            {
                                if (UnturnedPlayer.FromName(command[1]) != null) { Plugin.Instance.PlayerToSpy = UnturnedPlayer.FromName(command[1]).CSteamID; }
                                UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name)), Plugin.Instance.MessageColor);
                            }
                        }

                        else
                        {
                            string Message = "";
                            for (int x = 0; x < command.Length; x++)
                            {
                                if (x == 0) { continue; }
                                string Word = command[x];
                                Message += Word + " ";
                            }
                            UnturnedChat.Say(Plugin.Instance.Translate("vote_started", caller.DisplayName, VoteTimer, Plugin.Instance.Translate(vote.Name, Message)), Plugin.Instance.MessageColor);
                        }

                        Plugin.Instance.CurrentVote = vote.Name;
                        Plugin.Instance.StartCoroutine(Voting.VotingProcess());
                        Plugin.Instance.VoteStarted(caller, Plugin.Instance.CurrentVote);
                        if (Plugin.Instance.Configuration.Instance.AutoVoteCaller) { Voting.Vote(caller); }
                    }
                }
            }
        }
    }
}

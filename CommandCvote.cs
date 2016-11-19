using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            get { return "Start a vote to make it day"; }
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
            get { return "day|yes"; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;

            if (command[0].ToString().ToLower() == "day" && CallVote.Instance.VoteInProgress == false && CallVote.Instance.VoteInCooldown == false)
            {
                UnturnedChat.Say(player.DisplayName + " has called a vote to make it Day. You have " + CallVote.Instance.Configuration.Instance.VoteTimer + " seconds to type /cvote yes to vote.", Color.yellow);

                CallVote.Instance.VoteInProgress = true;
                CallVote.initiateDayVote();
            }
            if (command[0].ToString().ToLower() == "yes" && CallVote.Instance.VoteInProgress == true)
            {
                if (!CallVote.Instance.voteTracker.ContainsKey(player.CSteamID))
                {
                    CallVote.Instance.totalFor = CallVote.Instance.totalFor + 1;
                    float percentFor = (CallVote.Instance.totalFor / CallVote.Instance.onlinePlayers) * 100;

                    UnturnedChat.Say(percentFor + "% Yes, Required: " + CallVote.Instance.Configuration.Instance.RequiredPercent + "%.", Color.yellow);
                    CallVote.Instance.voteTracker.Add(player.CSteamID, player.CharacterName);
                }
                else if (CallVote.Instance.voteTracker.ContainsKey(player.CSteamID))
                {
                    UnturnedChat.Say(player, "You have already voted!", Color.yellow);
                }
            }
            if (command[0].ToString().ToLower() == "day" && CallVote.Instance.VoteInProgress == true)
            {
                UnturnedChat.Say(player, "Only one vote may be called at a time.", Color.yellow);
            }
            if (command[0].ToString().ToLower() == "day" && CallVote.Instance.VoteInCooldown == true)
            {
                UnturnedChat.Say(player, "A day vote may only be called every " + CallVote.Instance.Configuration.Instance.VoteCooldown + " seconds.", Color.yellow);
            }
            if (command[0].ToString().ToLower() == "yes" && CallVote.Instance.VoteInProgress == false)
            {
                UnturnedChat.Say(player, "There are no votes currently active.", Color.red);
            }
        }
    }
}

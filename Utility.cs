using Rocket.API;
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
    public static class Utility
    {
        public static ConsoleColor Check(this bool value)
        {
            return value ? ConsoleColor.Green : ConsoleColor.Red;
        }

        public static bool PlayerRequirement()
        {
            if (Provider.clients.Count < Plugin.Instance.Configuration.Instance.MinimumPlayers) { return false; }
            else return true;
        }

        public static void Help(UnturnedPlayer player)
        {
            List<string> Votes = new List<string>();
            for (int i = 0; i < Plugin.Instance.Configuration.Instance.Votes.Length; i++)
            {
                if (Plugin.Instance.Configuration.Instance.Votes[i].Enabled && player.HasPermission("cvote." + Plugin.Instance.Configuration.Instance.Votes[i].Name.ToLower()))
                {
                    Votes.Add(Plugin.Instance.Configuration.Instance.Votes[i].Name + "[" + Plugin.Instance.Configuration.Instance.Votes[i].Alias + "]");
                }
            }
            UnturnedChat.Say(player, Plugin.Instance.Translate("vote_help", String.Join(", ", Votes.ToArray())), Color.green);
        }

        public static void Notify(UnturnedPlayer player, int type)
        {
            if (type == 1) { UnturnedChat.Say(player, Plugin.Instance.Translate("vote_disabled"), Color.red); }
            if (type == 2) { UnturnedChat.Say(player, Plugin.Instance.Translate("vote_no_permission"), Color.red); }
        }

        public static void LogInfo()
        {
            if (Plugin.Instance.Configuration.Instance.Votes[10].Enabled)
            {
                Plugin.Instance.MutedPlayers = new Dictionary<CSteamID, DateTime>();
                Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted += Plugin.Instance.OnChatted;
            }

            Logger.LogWarning("================[CallVote]================");
            Logger.Log("Timer: " + Plugin.Instance.Configuration.Instance.VoteTimer + " seconds");
            Logger.Log("Cooldown: " + Plugin.Instance.Configuration.Instance.VoteCooldown + " seconds");
            Logger.Log("Required Players: " + Plugin.Instance.Configuration.Instance.MinimumPlayers);
            Logger.Log("Required Percent: " + Plugin.Instance.Configuration.Instance.RequiredPercent + "%");

            if (Plugin.Instance.Configuration.Instance.Votes[10].Enabled)
            {
                Logger.Log("Mute Time: " + Plugin.Instance.Configuration.Instance.MuteTime + " minutes");
            }

            Logger.LogWarning("================[Features]================");
            Logger.Log("Auto Vote For Caller", Plugin.Instance.Configuration.Instance.AutoVoteCaller.Check());
            Logger.Log("Finish Vote Before Timer", Plugin.Instance.Configuration.Instance.FinishVoteEarly.Check());
            Logger.Log("Notify When Cooldown Over", Plugin.Instance.Configuration.Instance.NotifyCooldownOver.Check());
            Logger.LogWarning("=================[Votes?]=================");

            for (int i = 0; i < Plugin.Instance.Configuration.Instance.Votes.Length; i++)
            {
                Logger.Log(Plugin.Instance.Configuration.Instance.Votes[i].Name + " [" + Plugin.Instance.Configuration.Instance.Votes[i].Alias + "]", Plugin.Instance.Configuration.Instance.Votes[i].Enabled.Check());
            }

            Logger.LogWarning("==========================================");
        }
    }
}

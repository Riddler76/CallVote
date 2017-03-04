using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Arechi.CallVote
{
    public static class Voting
    {
        public static void Vote(IRocketPlayer player)
        {
            var config = Plugin.Instance.Configuration.Instance;

            if (player is ConsolePlayer)
            {
                UnturnedChat.Say(Plugin.Instance.Translate("vote_ongoing", 0, config.RequiredPercent, Plugin.Instance.CurrentVote), Plugin.Instance.MessageColor);
                return;
            }

            if (!Plugin.Instance.Voters.Contains(((UnturnedPlayer)player).CSteamID))
            {
                Plugin.Instance.Voters.Add(((UnturnedPlayer)player).CSteamID);
                int VotesFor = (int)Math.Round((decimal)Plugin.Instance.Voters.Count / Provider.clients.Count * 100);

                UnturnedChat.Say(Plugin.Instance.Translate("vote_ongoing", VotesFor, config.RequiredPercent, Plugin.Instance.CurrentVote), Plugin.Instance.MessageColor);
                Plugin.Instance.PlayerVoted((UnturnedPlayer)player, Plugin.Instance.CurrentVote, VotesFor);

                if (VotesFor >= config.RequiredPercent && config.FinishVoteEarly == true)
                {
                    FinishVote();
                    Plugin.Instance.VoteConcluded(Plugin.Instance.CurrentVote, VotesFor, "Succeeded");
                }
            }
            else if (Plugin.Instance.Voters.Contains(((UnturnedPlayer)player).CSteamID))
            {
                UnturnedChat.Say(player, Plugin.Instance.Translate("already_voted"), Plugin.Instance.MessageColor);
            }
        }

        public static IEnumerator VotingProcess()
        {
            var config = Plugin.Instance.Configuration.Instance;

            Plugin.Instance.VoteInProgress = true;
            yield return new WaitForSeconds(config.VoteTimer);

            int VotesFor = (int)Math.Round((decimal)Plugin.Instance.Voters.Count / Provider.clients.Count * 100);
            if (VotesFor >= config.RequiredPercent && !Plugin.Instance.VoteFinished)
            {
                FinishVote();
                Plugin.Instance.VoteConcluded(Plugin.Instance.CurrentVote, VotesFor, "Succeeded");
            }
            else if (VotesFor < config.RequiredPercent)
            {
                UnturnedChat.Say(Plugin.Instance.Translate("vote_failed"), Color.red);
                Plugin.Instance.VoteConcluded(Plugin.Instance.CurrentVote, VotesFor, "Failed");
            }

            Plugin.Instance.VoteInCooldown = true;
            Plugin.Instance.VoteInProgress = false;
            Plugin.Instance.VoteFinished = true;
            Plugin.Instance.Voters.Clear();
            Plugin.Instance.CurrentVote = String.Empty;
            yield return new WaitForSeconds(config.VoteCooldown);

            Plugin.Instance.VoteInCooldown = false;
            if (config.NotifyCooldownOver == true) { UnturnedChat.Say(Plugin.Instance.Translate("cooldown_over"), Plugin.Instance.MessageColor); }
            Plugin.Instance.StopCoroutine(VotingProcess());
        }

        public static void FinishVote()
        {
            UnturnedChat.Say(Plugin.Instance.Translate("vote_success"), Plugin.Instance.MessageColor);

            try
            {
                switch (Plugin.Instance.CurrentVote)
                {
                    case "AirdropAll": Votes.AirdropAll(); break;
                    case "HealAll": Votes.HealAll(); break;
                    case "VehicleAll": Votes.VehicleAll(); break;
                    case "ItemAll": Votes.ItemAll(); break;
                    case "MaxSkills": Votes.MaxSkills(); break;
                    case "Unlock": Votes.Unlock(); break;
                    case "Kick": Votes.Kick(); break;
                    case "Mute": Votes.Mute(); break;
                    case "Spy": Votes.Spy(); break;
                    case "Custom": /*Nothing*/ break;
                    case "Rain": CommandWindow.input.onInputText("Storm"); break;
                    default: CommandWindow.input.onInputText(Plugin.Instance.CurrentVote); break;
                }
                Plugin.Instance.VoteFinished = true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}

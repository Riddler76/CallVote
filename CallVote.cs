using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using Rocket.Unturned.Skills;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace Arechi.CallVote
{
    public class CallVote : RocketPlugin<CallVoteConfig>
    {
        public bool VoteInProgress, VoteInCooldown, VoteFinished;
        public string CurrentVote;
        public ushort ItemToGive;
        public Color MessageColor;
        public List<CSteamID> Voters;
        public CSteamID PlayerToKickOrMute, PlayerToSpy;
        public static Dictionary<CSteamID, DateTime> MutedPlayers;
        public static CallVote Instance;

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.yellow);
            VoteInProgress = false;
            VoteInCooldown = false;
            VoteFinished = false;
            Voters = new List<CSteamID>();
            if (Instance.Configuration.Instance.Votes[10].Enabled) { MutedPlayers = new Dictionary<CSteamID, DateTime>(); ; Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted += OnChatted; }
            Logger.LogWarning("================[CallVote]================");
            Logger.Log("Timer: " + Instance.Configuration.Instance.VoteTimer + " seconds");
            Logger.Log("Cooldown: " + Instance.Configuration.Instance.VoteCooldown + " seconds");
            Logger.Log("Required Players: " + Instance.Configuration.Instance.MinimumPlayers);
            Logger.Log("Required Percent: " + Instance.Configuration.Instance.RequiredPercent + "%");
            if (Instance.Configuration.Instance.Votes[10].Enabled) { Logger.Log("Mute Time: " + Instance.Configuration.Instance.MuteTime + " minutes"); }
            Logger.LogWarning("================[Features]================");
            Logger.Log("Auto Vote For Caller", Instance.Configuration.Instance.AutoVoteCaller.Check());
            Logger.Log("Finish Vote Before Timer", Instance.Configuration.Instance.FinishVoteEarly.Check());
            Logger.Log("Notify When Cooldown Over", Instance.Configuration.Instance.NotifyCooldownOver.Check());  
            Logger.LogWarning("=================[Votes?]=================");
            for (int i = 0; i < Configuration.Instance.Votes.Length; i++)
            {
                Logger.Log(Configuration.Instance.Votes[i].Name + " [" + Configuration.Instance.Votes[i].Alias + "]", Configuration.Instance.Votes[i].Enabled.Check());
            }
            Logger.LogWarning("==========================================");
        }

        protected override void Unload()
        {
            Voters.Clear();
            MutedPlayers.Clear();
            if (Instance.Configuration.Instance.Votes[10].Enabled) { MutedPlayers.Clear(); Rocket.Unturned.Events.UnturnedPlayerEvents.OnPlayerChatted -= OnChatted; }
            Logger.LogWarning("CallVote has been unloaded!");
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    //Help
                    { "", "==============================[Help]==============================" },
                    { "vote_help", "The votes you can call are: {0}. You can start one with /cvote <vote name|alias>" },

                    //Events
                    { "", "=============================[Events]=============================" },
                    { "vote_started", "{0} has called a {1} second vote to {2}." },
                    { "vote_ongoing", "{0}% Yes. Required: {1}%. Type /cv to vote." },
                    { "vote_success", "The vote was successful." },
                    { "vote_failed", "The vote was unsuccessful." },

                    //Rejections
                    { "", "===========================[Rejections]===========================" },
                    { "already_voted", "You have already voted!" },
                    { "no_ongoing_votes", "There are no votes currently active." },
                    { "not_enough_players", "At least {0} players are required to start a vote!" },
                    { "vote_cooldown", "A vote may only be called every {0} seconds." },
                    { "vote_disabled", "This type of vote is disabled on the server." },
                    { "vote_no_permission", "This type of vote is not permitted for you." },
                    { "vote_error", "Only one vote may be called at a time." },

                    //Misc
                    { "", "==============================[Misc]==============================" },
                    { "kick_reason", "The majority decided so." },
                    { "mute_reason", "The majority decided to mute you. Wait {0} minutes." },
                    { "check_ready", "The spy screenshot is ready for {0}. Press ESC to check it out." },
                    { "airdropall_message", "An airdrop is coming right to your spot, {0}!" },
                    { "vehicleall_message", "You received a {0}!" },
                    { "itemall_message", "You received a {0}!" },
                    { "mute_message", "You have been muted for {0} minutes." },
                    { "cooldown_over", "The vote cooldown is over. You can start another vote if desired." },

                    //Vote types
                    { "", "===========================[Vote types]===========================" },
                    { "Day", "make it Day" },
                    { "Night", "make it Night" },
                    { "Rain", "start/stop Rain" },
                    { "Airdrop", "summon an Airdrop" },
                    { "AirdropAll", "summon an Airdrop for everyone" },
                    { "HealAll", "Heal everyone" },
                    { "VehicleAll", "give everyone a random Vehicle" },
                    { "ItemAll", "give everyone a {0}" },
                    { "MaxSkills", "Max everyone's skills" },
                    { "Unlock", "Unlock every Vehicle" },
                    { "Kick", "Kick {0}" },
                    { "Mute", "Mute {0} for {1} minutes" },
                    { "Spy", "Spy someone together" },
                    { "Custom", "{0}" },
                };
            }
        }

        public static bool PlayerRequirement()
        {
            if (Provider.clients.Count < Instance.Configuration.Instance.MinimumPlayers) { return false; }
            else return true;
        }

        private void OnChatted(UnturnedPlayer player, ref Color color, string message, EChatMode chatMode, ref bool cancel)
        {
            if (MutedPlayers.ContainsKey(player.CSteamID) && !message.StartsWith("/") && chatMode == EChatMode.GLOBAL)
            {
                DateTime mute;
                int time = Instance.Configuration.Instance.MuteTime;
                MutedPlayers.TryGetValue(player.CSteamID, out mute);
                if ((DateTime.Now - mute).TotalMinutes >= time)
                {
                    MutedPlayers.Remove(player.CSteamID);
                }
                else
                {
                    cancel = true;
                    UnturnedChat.Say(player, Instance.Translate("mute_reason", time), Color.green);
                }  
            }
        }

        public void Help(UnturnedPlayer player)
        {
            List<string> Votes = new List<string>();
            for (int i = 0; i < Configuration.Instance.Votes.Length; i++)
            {
                if (Configuration.Instance.Votes[i].Enabled && player.HasPermission("cvote." + Configuration.Instance.Votes[i].Name.ToLower()))
                {
                    Votes.Add(Configuration.Instance.Votes[i].Name + "[" + Configuration.Instance.Votes[i].Alias + "]");
                }
            }
            UnturnedChat.Say(player, Instance.Translate("vote_help", String.Join(", ", Votes.ToArray())), Color.green);
        }

        public void Notify(UnturnedPlayer player, int type)
        {
            if (type == 1) { UnturnedChat.Say(player, Instance.Translate("vote_disabled"), Color.red); }
            if (type == 2) { UnturnedChat.Say(player, Instance.Translate("vote_no_permission"), Color.red); }
        }

        public void Vote(IRocketPlayer player)
        {
            if (player is ConsolePlayer)
            {
                UnturnedChat.Say(Instance.Translate("vote_ongoing", 0, Instance.Configuration.Instance.RequiredPercent), Instance.MessageColor);
                return;
            }
            if (!Instance.Voters.Contains(((UnturnedPlayer)player).CSteamID))
            {
                Instance.Voters.Add(((UnturnedPlayer)player).CSteamID);
                int VotesFor = (int)Math.Round((decimal)Instance.Voters.Count / Provider.clients.Count * 100);
                UnturnedChat.Say(Instance.Translate("vote_ongoing", VotesFor, Instance.Configuration.Instance.RequiredPercent), Instance.MessageColor);
                if (VotesFor >= Instance.Configuration.Instance.RequiredPercent && Instance.Configuration.Instance.FinishVoteEarly == true)
                {
                    Instance.FinishVote();
                }
            }
            else if (Instance.Voters.Contains(((UnturnedPlayer)player).CSteamID))
            {
                UnturnedChat.Say(player, Instance.Translate("already_voted"), Instance.MessageColor);
            }
        }

        public void Mute()
        {
            MutedPlayers.Add(PlayerToKickOrMute, DateTime.Now);
            UnturnedChat.Say(PlayerToKickOrMute, Instance.Translate("mute_message", Configuration.Instance.MuteTime), Color.green);
        }

        public void AirdropAll()
        {
            System.Random rand = new System.Random();
            List<ushort> Airdropids = new List<ushort>();

            foreach (Node n in LevelNodes.nodes)
            {
                if (n.type == ENodeType.AIRDROP)
                {
                    AirdropNode node = (AirdropNode)n;
                    Airdropids.Add(node.id);
                }
            }

            foreach (var p in Provider.clients)
            {
                int id = rand.Next(Airdropids.Count);
                LevelManager.airdrop(p.player.transform.position, Airdropids[id]);
                UnturnedChat.Say(p.playerID.steamID, Instance.Translate("airdropall_message", UnturnedPlayer.FromCSteamID(p.playerID.steamID).DisplayName), Color.green);
            }
        }

        public void HealAll()
        {
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);
                player.Heal(100, true, true);
                player.Hunger = 0;
                player.Thirst = 0;
                player.Infection = 0;
            }
        }

        public void VehicleAll()
        {
            System.Random rand = new System.Random();
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                ushort vehicle = (ushort)rand.Next(1, 138);
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);
                player.GiveVehicle(vehicle);
                Asset a = Assets.find(EAssetType.VEHICLE, vehicle);
                UnturnedChat.Say(p.playerID.steamID, Instance.Translate("vehicleall_message", ((VehicleAsset)a).vehicleName), MessageColor);
            }
        }

        public void ItemAll()
        {
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);
                player.GiveItem(ItemToGive, 1);
                UnturnedChat.Say(p.playerID.steamID, Instance.Translate("itemall_message", UnturnedItems.GetItemAssetById(ItemToGive).itemName), MessageColor);
            }
        }

        public void MaxSkills()
        {
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);

                player.SetSkillLevel(UnturnedSkill.Agriculture, 255);
                player.SetSkillLevel(UnturnedSkill.Cooking, 255);
                player.SetSkillLevel(UnturnedSkill.Crafting, 255);
                player.SetSkillLevel(UnturnedSkill.Dexerity, 255);
                player.SetSkillLevel(UnturnedSkill.Diving, 255);
                player.SetSkillLevel(UnturnedSkill.Fishing, 255);
                player.SetSkillLevel(UnturnedSkill.Healing, 255);
                player.SetSkillLevel(UnturnedSkill.Immunity, 255);
                player.SetSkillLevel(UnturnedSkill.Mechanic, 255);
                player.SetSkillLevel(UnturnedSkill.Outdoors, 255);
                player.SetSkillLevel(UnturnedSkill.Overkill, 255);
                player.SetSkillLevel(UnturnedSkill.Parkour, 255);
                player.SetSkillLevel(UnturnedSkill.Sharpshooter, 255);
                player.SetSkillLevel(UnturnedSkill.Sneakybeaky, 255);
                player.SetSkillLevel(UnturnedSkill.Strength, 255);
                player.SetSkillLevel(UnturnedSkill.Survival, 255);
                player.SetSkillLevel(UnturnedSkill.Toughness, 255);
                player.SetSkillLevel(UnturnedSkill.Vitality, 255);
                player.SetSkillLevel(UnturnedSkill.Warmblooded, 255);
                player.SetSkillLevel(UnturnedSkill.Engineer, 255);
                player.SetSkillLevel(UnturnedSkill.Exercise, 255);
                player.SetSkillLevel(UnturnedSkill.Cardio, 255);
            }
        }

        public void Unlock()
        {
            using (List<InteractableVehicle>.Enumerator enumerator = VehicleManager.vehicles.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    InteractableVehicle vehicle = enumerator.Current;

                    if (vehicle != null) { VehicleManager.unlockVehicle(vehicle); }
                }
            }
        }

        public void Kick()
        {
            Provider.kick(PlayerToKickOrMute, Instance.Translate("kick_reason"));
        }

        public void Spy()
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(PlayerToSpy);
            foreach (var p in Provider.clients)
            {
                if (p.playerID.steamID != player.CSteamID)
                {
                    player.Player.sendScreenshot(p.playerID.steamID);
                    UnturnedChat.Say(UnturnedPlayer.FromCSteamID(p.playerID.steamID), Instance.Translate("check_ready", UnturnedPlayer.FromCSteamID(PlayerToSpy).DisplayName), MessageColor);
                }
            }
        }

        public void FinishVote()
        {
            UnturnedChat.Say(Instance.Translate("vote_success"), MessageColor);

            try
            {
                switch (CurrentVote)
                {
                    case "AirdropAll": AirdropAll(); break;
                    case "HealAll": HealAll(); break;
                    case "VehicleAll": VehicleAll(); break;
                    case "ItemAll": ItemAll(); break;
                    case "MaxSkills": MaxSkills(); break;
                    case "Unlock": Unlock(); break;
                    case "Kick": Kick(); break;
                    case "Mute": Mute(); break;
                    case "Spy": Spy(); break;
                    case "Custom": /*Nothing*/ break;
                    case "Rain": CommandWindow.input.onInputText("Storm"); break;
                    default: CommandWindow.input.onInputText(CurrentVote); break;
                }
                Cooldown();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void Cooldown()
        {
            Instance.VoteInCooldown = true;
            Instance.VoteInProgress = false;
            Instance.VoteFinished = true;

            InitiateVoteCooldown();
        }

        public static void StartVote(string kind)
        {
            new Thread(() =>
            {
                Instance.CurrentVote = kind;
                Instance.VoteInProgress = true;
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteTimer * 1000);

                if (Instance.VoteFinished == true) return;

                int VotesFor = (int)Math.Round((decimal)Instance.Voters.Count / Provider.clients.Count * 100);
                   
                if (VotesFor >= Instance.Configuration.Instance.RequiredPercent) { Instance.FinishVote(); }
                else if (VotesFor < Instance.Configuration.Instance.RequiredPercent) { UnturnedChat.Say(Instance.Translate("vote_failed"), Color.red); Instance.Cooldown(); }

            }).Start();
        }

        public static void InitiateVoteCooldown()
        {
            new Thread(() =>
            {
                Instance.Voters.Clear();
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteCooldown * 1000);

                if (Instance.Configuration.Instance.NotifyCooldownOver == true) { UnturnedChat.Say(Instance.Translate("cooldown_over"), Instance.MessageColor); }

                Instance.VoteInCooldown = false;

            }).Start();
        }
    }

    public static class Conversion
    {
        public static ConsoleColor Check(this bool value)
        {
            return value ? ConsoleColor.Green : ConsoleColor.Red;
        }
    }
}

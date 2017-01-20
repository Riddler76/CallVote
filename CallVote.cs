using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
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
        public bool VoteInProgress = false;
        public bool VoteInCooldown = false;
        public string CurrentVote;
        public Color MessageColor;
        public List<CSteamID> Voters;
        public CSteamID PlayerToKick, PlayerToSpy;
        public static CallVote Instance;

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.yellow);
            Voters = new List<CSteamID>();
            Logger.LogWarning("CallVote has been loaded!");
            Logger.Log("Vote Timer: " + Instance.Configuration.Instance.VoteTimer + " seconds");
            Logger.Log("Vote Cooldown: " + Instance.Configuration.Instance.VoteCooldown + " seconds");
            Logger.Log("Required Percent: " + Instance.Configuration.Instance.RequiredPercent + "%");
        }

        protected override void Unload()
        {
            Voters.Clear();
            Logger.LogWarning("CallVote has been unloaded!");
        }

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    {"vote_started_day", "{0} has called a vote to make it Day. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_night", "{0} has called a vote to make it Night. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_storm", "{0} has called a vote to start/stop Rain. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_airdrop", "{0} has called a vote to summon an Airdrop. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_airdropall", "{0} has called a vote to summon an Airdrop for everyone. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_healall", "{0} has called a vote to heal everyone. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_vehicleall", "{0} has called a vote to give everyone a random vehicle. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_unlock", "{0} has called a vote to unlock every vehicle. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_kick", "{0} has called a vote to kick {2}. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_spy", "{0} has called a vote to spy someone together. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_started_custom", "{0} has called a vote to {2}. You have {1} seconds to type /cvote yes to vote." },
                    {"vote_ongoing", "{0}% Yes. Required: {1}%." },
                    {"already_voted", "You have already voted!" },
                    {"vote_error", "Only one vote may be called at a time." },
                    {"vote_help", "The kind of votes are: Day, Night, Rain, Airdrop(all), HealAll, VehicleAll, Unlock, Kick, Spy and Custom. You can vote with /cvote yes|y or start one with /cvote d|n|r|a|h|v|u|k|s|c" },
                    {"vote_disabled", "This type of vote is disabled on the server." },
                    {"vote_cooldown", "A vote may only be called every {0} seconds." },
                    {"cooldown_over", "The vote cooldown is over. You can start another vote if desired." },
                    {"no_ongoing_votes", "There are no votes currently active." },
                    {"day_success", "The vote to make it Day was successful." },
                    {"day_failed", "The vote to make it Day was unsuccessful."},
                    {"night_success", "The vote to make it Night was successful." },
                    {"night_failed", "The vote to make it Night was unsuccessful."},
                    {"storm_success", "The vote to start or stop Rain was successful." },
                    {"storm_failed", "The vote to start or stop Rain was unsuccessful."},
                    {"airdrop_success", "The vote to summon an Airdrop was successful." },
                    {"airdrop_failed", "The vote to summon an Airdrop was unsuccessful."},
                    {"airdropall_success", "The vote to summon an Airdrop for everyone was successful." },
                    {"airdropall_failed", "The vote to summon an Airdrop for everyone was unsuccessful."},
                    {"healall_success", "The vote to heal everyone was successful." },
                    {"healall_failed", "The vote to heal everyone was unsuccessful."},
                    {"vehicleall_success", "The vote to give everyone a random vehicle was successful." },
                    {"vehicleall_failed", "The vote to give everyone a random vehicle was unsuccessful."},
                    {"unlock_success", "The vote to unlock every vehicle was successful." },
                    {"unlock_failed", "The vote to unlock every vehicle was unsuccessful."},
                    {"custom_success", "The custom vote was successful." },
                    {"custom_failed", "The custom vote was unsuccessful."},
                    {"kick_success", "The kick vote was successful." },
                    {"kick_failed", "The kick vote was unsuccessful."},
                    {"spy_success", "The spy vote was successful." },
                    {"spy_failed", "The spy vote was unsuccessful."},
                    {"kick_reason", "The majority decided so."},
                    {"check_ready", "The spy screenshot is ready for {0}. Press ESC to check it out."},
                };
            }
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

            int id = rand.Next(Airdropids.Count);
            
            foreach (var p in Provider.clients)
            {
                LevelManager.airdrop(p.player.transform.position, Airdropids[id]);
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
            }
        }

        public void Unlock()
        {
            using (List<InteractableVehicle>.Enumerator enumerator = VehicleManager.vehicles.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    InteractableVehicle vehicle = enumerator.Current;

                    if (vehicle != null)
                    {
                        VehicleManager.unlockVehicle(vehicle);
                    }
                }
            }
        }

        public void Kick()
        {
            Provider.kick(PlayerToKick, Instance.Translate("kick_reason"));
        }

        public void Spy()
        {
            foreach (var p in Provider.clients)
            {
                if (p.playerID.steamID == PlayerToSpy) return;
                p.player.sendScreenshot(PlayerToSpy);
                UnturnedChat.Say(UnturnedPlayer.FromCSteamID(p.playerID.steamID), Instance.Translate("check_ready", UnturnedPlayer.FromCSteamID(PlayerToSpy).DisplayName), MessageColor);
            }
        }

        public void FinishVoteNow()
        {
            UnturnedChat.Say(Instance.Translate(CurrentVote + "_success"), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.Color, Color.yellow));
            if (Instance.CurrentVote == "airdropall")
            {
                AirdropAll();
            }
            else if (Instance.CurrentVote == "healall")
            {
                HealAll();
            }
            else if (Instance.CurrentVote == "vehicleall")
            {
                VehicleAll();
            }
            else if (Instance.CurrentVote == "unlock")
            {
                Unlock();
            }
            else if (Instance.CurrentVote == "kick")
            {
                Kick();
            }
            else if (Instance.CurrentVote == "spy")
            {
                Spy();
            }
            else if (Instance.CurrentVote == "custom")
            {
                //Well, nothing
            }
            else
            {
                CommandWindow.input.onInputText(CurrentVote);
            }
            
            Instance.VoteInCooldown = true;
            Instance.VoteInProgress = false;

            InitiateVoteCooldown();
        }

        public static void StartVote(string kind)
        {
            new Thread(() =>
            {
                Instance.CurrentVote = kind.ToLower();
                Instance.VoteInProgress = true;
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteTimer * 1000);

                if (Instance.VoteInCooldown == true) return;

                double VotesFor;
                if (Provider.clients.Count == 1)
                {
                    VotesFor = 100;
                }
                else
                {
                    VotesFor = Math.Round((double)Instance.Voters.Count / Provider.clients.Count, 2);
                }
                    
                if (VotesFor >= Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say(Instance.Translate(kind.ToLower() + "_success"), Instance.MessageColor);
                    if (Instance.CurrentVote == "airdropall")
                    {
                        Instance.AirdropAll();
                    }
                    else if (Instance.CurrentVote == "healall")
                    {
                        Instance.HealAll();
                    }
                    else if (Instance.CurrentVote == "vehicleall")
                    {
                        Instance.VehicleAll();
                    }
                    else if (Instance.CurrentVote == "unlock")
                    {
                        Instance.Unlock();
                    }
                    else if (Instance.CurrentVote == "kick")
                    {
                        Instance.Kick();
                    }
                    else if (Instance.CurrentVote == "spy")
                    {
                        Instance.Spy();
                    }
                    else if (Instance.CurrentVote == "custom")
                    {
                        //Well, nothing
                    }
                    else
                    {
                        CommandWindow.input.onInputText(kind);
                    }   
                }
                else if (VotesFor < Instance.Configuration.Instance.RequiredPercent)
                {
                    UnturnedChat.Say(Instance.Translate(kind.ToLower() + "_failed"), Color.red);
                }

                Instance.VoteInCooldown = true;
                Instance.VoteInProgress = false;

                InitiateVoteCooldown();

            }).Start();
        }

        public static void InitiateVoteCooldown()
        {
            new Thread(() =>
            {
                Instance.Voters.Clear();
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(Instance.Configuration.Instance.VoteCooldown * 1000);

                if (Instance.Configuration.Instance.NotifyCooldownOver == true)
                {
                    UnturnedChat.Say(Instance.Translate("cooldown_over"), Instance.MessageColor);
                }

                Instance.VoteInCooldown = false;

            }).Start();
        }
    }
}

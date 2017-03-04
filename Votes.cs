using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Arechi.CallVote
{
    public static class Votes
    {
        public static void AirdropAll()
        {
            Random rand = new Random();
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
                UnturnedChat.Say(p.playerID.steamID, Plugin.Instance.Translate("airdropall_message", UnturnedPlayer.FromCSteamID(p.playerID.steamID).DisplayName), Color.green);
            }
        }

        public static void HealAll()
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

        public static void VehicleAll()
        {
            Random rand = new Random();
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                ushort vehicle = (ushort)rand.Next(1, 139);
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);
                player.GiveVehicle(vehicle);
                Asset a = Assets.find(EAssetType.VEHICLE, vehicle);
                UnturnedChat.Say(p.playerID.steamID, Plugin.Instance.Translate("vehicleall_message", ((VehicleAsset)a).vehicleName), Plugin.Instance.MessageColor);
            }
        }

        public static void ItemAll()
        {
            UnturnedPlayer player;

            foreach (var p in Provider.clients)
            {
                player = UnturnedPlayer.FromCSteamID(p.playerID.steamID);
                player.GiveItem(Plugin.Instance.ItemToGive, 1);
                UnturnedChat.Say(p.playerID.steamID, Plugin.Instance.Translate("itemall_message", UnturnedItems.GetItemAssetById(Plugin.Instance.ItemToGive).itemName), Plugin.Instance.MessageColor);
            }
        }

        public static void Unlock()
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

        public static void Kick()
        {
            Provider.kick(Plugin.Instance.PlayerToKickOrMute, Plugin.Instance.Translate("kick_reason"));
        }

        public static void Mute()
        {
            Plugin.Instance.MutedPlayers.Add(Plugin.Instance.PlayerToKickOrMute, DateTime.Now);
            UnturnedChat.Say(Plugin.Instance.PlayerToKickOrMute, Plugin.Instance.Translate("mute_message", Plugin.Instance.Configuration.Instance.MuteTime), Color.green);
        }

        public static void Spy()
        {
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(Plugin.Instance.PlayerToSpy);
            foreach (var p in Provider.clients)
            {
                if (p.playerID.steamID != player.CSteamID)
                {
                    player.Player.sendScreenshot(p.playerID.steamID);
                    UnturnedChat.Say(UnturnedPlayer.FromCSteamID(p.playerID.steamID), Plugin.Instance.Translate("check_ready", UnturnedPlayer.FromCSteamID(Plugin.Instance.PlayerToSpy).DisplayName), Plugin.Instance.MessageColor);
                }
            }
        }

        public static void MaxSkills()
        {
            UnturnedPlayer player;

            foreach (var p in Provider.clients) { player = UnturnedPlayer.FromCSteamID(p.playerID.steamID); player.MaxSkills(); }
        }
    }
}

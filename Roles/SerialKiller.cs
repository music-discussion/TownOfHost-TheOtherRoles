using System.Collections.Generic;
using Hazel;
using UnityEngine;
using static TownOfHost.Translator;
using AmongUs.GameOptions;
using TownOfHost.PrivateExtensions;

namespace TownOfHost
{
    public static class SerialKiller
    {
        private static readonly int Id = 1100;
        public static List<byte> playerIdList = new();

        private static CustomOption KillCooldown;
        private static CustomOption TimeLimit;

        private static Dictionary<byte, float> SuicideTimer = new();

        public static void SetupCustomOption()
        {
            Options.SetupRoleOptions(Id, CustomRoles.SerialKiller, AmongUsExtensions.OptionType.Impostor);
            KillCooldown = CustomOption.Create(Id + 10, Color.white, "SerialKillerCooldown", AmongUsExtensions.OptionType.Impostor, 20f, 2.5f, 180f, 2.5f, Options.CustomRoleSpawnChances[CustomRoles.SerialKiller]);
            TimeLimit = CustomOption.Create(Id + 11, Color.white, "SerialKillerLimit", AmongUsExtensions.OptionType.Impostor, 60f, 5f, 900f, 5f, Options.CustomRoleSpawnChances[CustomRoles.SerialKiller]);
        }
        public static void Init()
        {
            playerIdList = new();
            SuicideTimer = new();
        }
        public static void Add(byte serial)
        {
            playerIdList.Add(serial);
        }
        public static bool IsEnable() => playerIdList.Count > 0;
        public static void ApplyKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
        public static void ApplyGameOptions(NormalGameOptionsV07 opt) => opt.GetShapeshifterOptions().ShapeshifterCooldown = TimeLimit.GetFloat();

        public static void OnCheckMurder(PlayerControl killer, bool isKilledSchrodingerCat = false)
        {
            if (killer.Is(CustomRoles.SerialKiller))
            {
                if (isKilledSchrodingerCat)
                {
                    killer.RpcResetAbilityCooldown();
                    SuicideTimer[killer.PlayerId] = 0f;
                    return;
                }
                else
                {
                    killer.RpcResetAbilityCooldown();
                    SuicideTimer[killer.PlayerId] = 0f;
                    Main.AllPlayerKillCooldown[killer.PlayerId] = KillCooldown.GetFloat();
                    killer.CustomSyncSettings();
                }
            }
        }
        public static void OnReportDeadBody()
        {
            SuicideTimer.Clear();
        }
        public static void FixedUpdate(PlayerControl player)
        {
            if (!player.Is(CustomRoles.SerialKiller)) return; //??????????????????????????????????????????

            if (GameStates.IsInTask && SuicideTimer.ContainsKey(player.PlayerId))
            {
                if (!player.IsAlive() | player.Data.IsDead)
                    SuicideTimer.Remove(player.PlayerId);
                else if (SuicideTimer[player.PlayerId] >= TimeLimit.GetFloat() && !player.Data.IsDead)
                {
                    PlayerState.SetDeathReason(player.PlayerId, PlayerState.DeathReason.Suicide);//???????????????
                    player.RpcMurderPlayerV2(player);//???????????????
                }
                else
                    SuicideTimer[player.PlayerId] += Time.fixedDeltaTime;//?????????????????????
            }
        }
        public static void GetAbilityButtonText(HudManager __instance) => __instance.AbilityButton.OverrideText($"{GetString("SerialKillerSuicideButtonText")}");
        public static void AfterMeetingTasks()
        {
            foreach (var id in playerIdList)
            {
                if (!PlayerState.isDead[id])
                {
                    Utils.GetPlayerById(id)?.RpcResetAbilityCooldown();
                    SuicideTimer[id] = 0f;
                }
            }
        }
    }
}
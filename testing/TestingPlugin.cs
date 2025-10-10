using HarmonyLib;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Law;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.ObjectScripts.WateringCan;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerTasks.Tasks;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.UI;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(TestingPlugin), "Testing", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "For testing only";
    public static string EXTRA_DETAILS = "This mod does not make any permanent changes to the game files.  It simply modifies the strings in memory for the duration of the game.  Removing the mod and restarting the game will revert everything to its default state.";
    public static string VERSION;
    public static string AUTHOR;
    public static string GAME_TITLE = "Schedule I";
    public static string GAME = "schedule-one";
    public static string GUID;
    public static string REPO = GAME_TITLE + "-mods";

    static PluginInfo() {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        MelonInfoAttribute info = assembly.GetCustomAttribute<MelonInfoAttribute>();
        TITLE = info.Name;
        NAME = TITLE.ToLower().Replace(" ", "-");
        VERSION = info.Version;
        AUTHOR = info.Author;
        GUID =  AUTHOR + "." + GAME + "." + NAME;
    }

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

public class TestingPlugin : DDPlugin {
	
    public override void OnInitializeMelon() {
		try {
			logger = LoggerInstance;
			this.m_plugin_info = PluginInfo.to_dict();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

    public override void OnUpdate() {
		if (Player.Local != null && (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Q))) {
			try {
				SaveManager.Instance.Save();
			} catch (Exception e) {
				_error_log("** QuickSave ERROR - " + e);
			}
        }
    }

    #region Bottomless watering can
    [HarmonyPatch(typeof(Equippable_WateringCan), "CanPour")]
    class HarmonyPatch_Equippable_WateringCan_CanPour {
        private static bool Prefix(Equippable_WateringCan __instance) {
            try {
                ReflectionUtils.invoke_method((WateringCanInstance) ReflectionUtils.invoke_method(__instance, "get_WCInstance"), "set_CurrentFillAmount", new object[] { 15f });
                return true;
            } catch (Exception e) {
                _error_log("** HarmonyPatch_Equippable_WateringCan_CanPour.Prefix ERROR - " + e);
            }
            return true;
        }
    }

	[HarmonyPatch(typeof(Pot), "RandomizeTarget")]
	class HarmonyPatch_Pot_RandomizeTarget {
		private static void Postfix(Pot __instance) {
			try {
				__instance.Target.localPosition = new Vector3(0, __instance.Target.localPosition.y, 0);
			} catch (Exception e) {
				_error_log("** HarmonyPatch_Pot_RandomizeTarget.Prefix ERROR - " + e);
			}
		}
	}
	
	[HarmonyPatch(typeof(PourOntoTargetTask), "Update")]
	class HarmonyPatch_PourOntoTargetTask_Update {
		private static bool Prefix(PourOntoTargetTask __instance) {
			try {
				__instance.pourable.NormalizedPourRate *= 2f;
				return true;
			} catch (Exception e) {
				_error_log("** HarmonyPatch_PourOntoTargetTask_Update.Prefix ERROR - " + e);
			}
			return true;
		}
	}
	
	#endregion

	#region Remove ATM weekly deposit limit
	[HarmonyPatch(typeof(ATM), "Enter")]
	class HarmonyPatch_ATM_Enter {
		private static bool Prefix() {
			ATM.WeeklyDepositSum = 0;
			return true;
		}
	}

	[HarmonyPatch(typeof(ATM), "Exit")]
	class HarmonyPatch_ATM_Exit {
		private static bool Prefix() {
			ATM.WeeklyDepositSum = 0;
			return true;
		}
	}
	#endregion

	[HarmonyPatch(typeof(Il2CppScheduleOne.Console), "Log")]
	class HarmonyPatch_Console_Log {
		private static string m_prev_msg = null;
		private static void Postfix(Il2CppSystem.Object message) {
			string msg = message.ToString();
			if (msg == m_prev_msg) {
				return;
			}
			_warn_log($"[** Console.Log **] " + msg);
			m_prev_msg = msg;
		}
	}
	
	[HarmonyPatch(typeof(Bed), "CanSleep")]
	class HarmonyPatch_Bed_CanSleep {
		private static bool Prefix(ref bool __result) {
			DailySummary.Instance.xpGained = 2000;
			DailySummary.Instance.itemsSoldByPlayer["ogkush"] = 123;
			_info_log(DailySummary.Instance.xpGained);
			__result = true;
			return false;
		}
	}
	
	/*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

				return false;
			} catch (Exception e) {
				_error_log("** XXXXX.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {
				
			} catch (Exception e) {
				_error_log("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}
	*/
}
using HarmonyLib;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Trash;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(AutocleanPlugin), "Auto Clean Trash", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "Automatically pick up nearby trash with the trashgrabber.";
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

public class AutocleanPlugin : DDPlugin {
	private const float MAX_DISTANCE_TO_RECYCLER = 2f;

	private float m_check_frequency = 1.0f;
	private float m_check_elapsed = 0f;
	private float m_check_radius = 10f;
	private static List<Recycler> m_recyclers = new List<Recycler>();

	public override void OnInitializeMelon() {
		try {
			logger = LoggerInstance;
			this.m_plugin_info = PluginInfo.to_dict();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}
	
	[HarmonyPatch(typeof(Recycler), "Start")]
	class HarmonyPatch_Recycler_Start {
		private static void Postfix(Recycler __instance) {
			m_recyclers.Add(__instance);
		}
	}

	[HarmonyPatch(typeof(Recycler), "OnDestroy")]
	class HarmonyPatch_Recycler_OnDestroy {
		private static void Postfix(Recycler __instance) {
			m_recyclers.Remove(__instance);
		}
	}

	public override void OnUpdate() {
		if ((m_check_elapsed += Time.deltaTime) < m_check_frequency || Player.Local == null) {
			return;
		}
		m_check_elapsed = 0f;
		this.pick_up_trash(Player.Local);
	}

	private void pick_up_trash(Player player) {
		foreach (Recycler recycler in m_recyclers) {
			if (Vector3.Distance(player.transform.position, recycler.transform.position) < MAX_DISTANCE_TO_RECYCLER) {
				return;
			}
		}
		List<TrashItem> nearby_items = new List<TrashItem>();
		foreach (TrashItem item in TrashManager.Instance.trashItems) {
			if (Vector3.Distance(player.transform.position, item.transform.position) <= m_check_radius) {
				nearby_items.Add(item);
			}
		}
		nearby_items.ForEach(item => item.Interacted());
	}
}
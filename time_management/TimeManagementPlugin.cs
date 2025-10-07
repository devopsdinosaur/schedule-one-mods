using HarmonyLib;
using Il2CppScheduleOne.GameTime;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(typeof(TestingPlugin), "Time Management", "0.0.1", "devopsdinosaur")]

public static class PluginInfo {

    public static string TITLE;
    public static string NAME;
    public static string SHORT_DESCRIPTION = "Slow down, speed up, and pause time using in-game hotkeys.";
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
	private static float m_time_delta = 0.1f;
	private static float m_time_scale = 0.5f;
	private static bool m_time_is_paused = true;

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
		if (Input.GetKeyDown(KeyCode.RightBracket)) {
			_info_log($"Time Scale INCREASED to {(m_time_scale += m_time_delta):0.00}.");
		} else if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			_info_log($"Time Scale DECREASED to {(m_time_scale = (m_time_scale - m_time_delta < 0f ? 0f : m_time_scale - m_time_delta))::0.00}.");
		} else if (Input.GetKeyDown(KeyCode.Backslash)) {
			_info_log($"Time progression is {((m_time_is_paused = !m_time_is_paused) ? "PAUSED" : "ACTIVE")}.");
		}
	}

    [HarmonyPatch(typeof(TimeManager), "Update")]
	class HarmonyPatch_TimeManager_Update {
		
		private static bool Prefix(TimeManager __instance) {
			try {
				ReflectionUtils.invoke_method(__instance, "set_TimeProgressionMultiplier", new object[] { (m_time_is_paused ? 0f : m_time_scale) });
				return true;
			} catch (Exception e) {
				_error_log("** HarmonyPatch_TimeManager_Update.Prefix ERROR - " + e);
			}
			return true;
		}
	}
}
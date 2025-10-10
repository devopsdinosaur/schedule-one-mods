using HarmonyLib;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.UI;
using MelonLoader;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(TimeManagementPlugin), "Time Management", "0.0.1", "devopsdinosaur")]

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

public class TimeManagementPlugin : DDPlugin {
	private static HarmonyLib.Harmony m_harmony = null;
	private static float m_time_delta = 0.1f;
	private static float m_time_scale = 0.5f;
	private static bool m_time_is_paused = true;
	private static bool m_checked_for_time_text = false;
	private static bool m_found_UpdateMinimapTime = false;
	private static Transform m_minimap_time_transform = null;

	public override void OnInitializeMelon() {
		try {
			logger = LoggerInstance;
			this.m_plugin_info = PluginInfo.to_dict();
			(m_harmony = new HarmonyLib.Harmony(PluginInfo.GUID)).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	private static void update_minimap_time_postfix(Text ___minimapTimeText) {
		try {
			___minimapTimeText.text += (m_time_is_paused ? " [Paused]" : $" [{m_time_scale:0.00}]");
		} catch {}
	}

	public override void OnUpdate() {
		if (!m_checked_for_time_text) {
			m_checked_for_time_text = true;
			MethodInfo postfix = this.GetType().GetMethod("update_minimap_time_postfix", ReflectionUtils.BINDING_FLAGS_ALL);
			foreach (MelonAssembly assembly in MelonAssembly.LoadedAssemblies) {
				if (assembly.Assembly.GetName().Name != "BetterMiniMap") {
					continue;
				}
				foreach (Type type in assembly.Assembly.GetTypes()) {
					if (type.FullName != "BetterMiniMap.MiniMapHandler") {
						continue;
					}
					m_found_UpdateMinimapTime = (m_harmony.Patch(type.GetMethod("UpdateMinimapTime", ReflectionUtils.BINDING_FLAGS_ALL), null, new HarmonyLib.HarmonyMethod(postfix)) != null);
					break;
				}
				break;
			}
			if (m_found_UpdateMinimapTime) {
				_info_log("Found BetterMiniMap's time text; will add time scale/pause status.");
			} else {
				_warn_log("Unable to locate BetterMiniMap mod or its time text; time scale/pause status will only be printed to Melon console.  Install BetterMiniMap from NexusMods to get UI updates from this mod.");
			}
		}
		string text = null;
		if (Input.GetKeyDown(KeyCode.RightBracket)) {
			text = $"Time Scale INCREASED to {(m_time_scale += m_time_delta):0.00}.";
		} else if (Input.GetKeyDown(KeyCode.LeftBracket)) {
			text = $"Time Scale DECREASED to {(m_time_scale = (m_time_scale - m_time_delta < 0f ? 0f : m_time_scale - m_time_delta)):0.00}.";
		} else if (Input.GetKeyDown(KeyCode.Backslash)) {
			text = $"Time progression is {((m_time_is_paused = !m_time_is_paused) ? "PAUSED" : "ACTIVE")}.";
		}
		if (text != null) {
			HintDisplay.Instance.ShowHint_10s(text);
			_info_log(text);
		}
	}

	[HarmonyPatch(typeof(HintDisplay), "ShowHint", new Type[] { typeof(string), typeof(float) })]
	class HarmonyPatch_HintDisplay_ShowHint {
		private static void Postfix(HintDisplay __instance) {
			try {
				__instance.Container.transform.position += Vector3.down * 100f;
				//if (m_minimap_time_transform == null) {
				//	foreach (Text text in Resources.FindObjectsOfTypeAll<Text>()) {
				//		if (text.gameObject.name == "MinimapTime") {
				//			m_minimap_time_transform = text.transform;
				//			break;
				//		}
				//	}
				//}
				//if (m_minimap_time_transform == null) {
				//	return;
				//}
				//Rect rect = m_minimap_time_transform.GetComponent<RectTransform>().rect;
				//__instance.Container.SetParent(m_minimap_time_transform.parent, false);
				//__instance.Container.position = //new Vector2(Screen.width + __instance.Label.renderedWidth - __instance.Padding.x, m_minimap_time_transform.position.y - rect.height - 5f);
			} catch {
				m_minimap_time_transform = null;
			}
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
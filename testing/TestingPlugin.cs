using HarmonyLib;
using Il2CppScheduleOne.Combat;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.GameTime;
using Il2CppScheduleOne.Law;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.ObjectScripts.WateringCan;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.PlayerTasks.Tasks;
using Il2CppScheduleOne.Trash;
using Il2CppScheduleOne.UI;
using MelonLoader;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
			m_log_level = LogLevel.Debug;
			this.m_plugin_info = PluginInfo.to_dict();
			new HarmonyLib.Harmony(PluginInfo.GUID).PatchAll();
		} catch (Exception e) {
			_error_log("** OnInitializeMelon FATAL - " + e);
		}
	}

	private static void dump_all_objects() {
		string directory = "C:/tmp/dump_" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
		Directory.CreateDirectory(directory);
		foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly)) {
			File.Delete(Path.Combine(directory, file));
		}
		foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects()) {
			UnityUtils.json_dump(obj.transform, Path.Combine(directory, obj.name + ".json"));
		}
	}

	class Thug {
		private static string PREFAB_NAME = "Igor";
		private static float RADIUS_PREFERRED_DISTANCE_TO_PLAYER = 2f;
		private static float RADIUS_WARP_TO_PLAYER = 30f;
		private static float RADIUS_RUN_TO_PLAYER = 10f;

		private GameObject m_game_object = null;
		private Transform m_transform = null;
		private NPCMovement m_movement = null;
		private NPC m_npc = null;
		private enum State {
			Unitialized,
			FollowingPlayer
		}
		private State m_state = State.Unitialized;
		private float m_sleep_time = 0f;

		public Thug(string name) {
			GameObject prefab = null;
			foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects()) {
				if (obj.name == PREFAB_NAME) {
					prefab = obj;
					break;
				}
			}
			if (prefab == null) {
				throw new Exception($"Unable to locate prefab GameObject with name: '{PREFAB_NAME}'");
			}
			this.m_game_object = GameObject.Instantiate(prefab, prefab.transform.parent);
			this.m_game_object.name = name;
			MelonCoroutines.Start(this.run_routine());
		}

		private void initialize() {
			try {
				_info_log($"Initializing new Thug (name: '{this.m_game_object.name}').  Warping to player at {Player.Local.transform.position}.");
				this.m_transform = this.m_game_object.transform;
				this.m_npc = this.m_game_object.GetComponent<NPC>();
				this.m_npc.IgnoreImpacts = true;
				this.m_movement = this.m_game_object.GetComponent<NPCMovement>();
				this.m_movement.Warp(Player.Local.transform);
				this.m_state = State.FollowingPlayer;
				this.m_sleep_time = 0f;
			} catch (Exception e) {
				_error_log("** Thug.initialize ERROR - " + e);
			}
		}

		private Vector3 near_player_destination() {
			const float ANGLE_INC = 45f;
			const int NUM_RAYS = (int) (360f / ANGLE_INC);
			Vector3[] check_rays = new Vector3[NUM_RAYS];
			float angle = 0;
			for (int index = 0; index < NUM_RAYS; index++) {
				
				//check_rays[index] = 
			}
		}

		private void follow_player() {
			try {
				this.m_game_object.SetActive(true);
				float distance_to_player = Vector3.Distance(this.m_transform.position, Player.Local.transform.position);
				if (distance_to_player >= RADIUS_WARP_TO_PLAYER) {
					_info_log($"Thug '{this.m_game_object.name}' walking to player at {Player.Local.transform.position} [pos: {this.m_game_object.transform.position}, distance: {distance_to_player}].");
					this.m_movement.Warp(Player.Local.transform);
				}
				
				
				this.m_movement.SetDestination(Player.Local.transform.position);
				this.m_sleep_time = 1f;
			} catch (Exception e) {
				_error_log("** Thug.follow_player ERROR - " + e);
			}
		}

		[HarmonyPatch(typeof(Impact), "IsPlayerImpact")]
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

		private IEnumerator run_routine() {
			for (;;) {
				if (this.m_game_object == null) {
					yield break;
				}
				switch (this.m_state) {
					case State.Unitialized:
						this.initialize();
						break;
					case State.FollowingPlayer:
						this.follow_player();
						break;
				}
				yield return new WaitForSeconds(this.m_sleep_time);
			}
		}
	}

	public override void OnUpdate() {
		if (Player.Local != null && Input.GetKeyDown(KeyCode.Q)) {
			try {
				SaveManager.Instance.Save();
			} catch (Exception e) {
				_error_log("** QuickSave ERROR - " + e);
			}
        }
		if (Input.GetKeyDown(KeyCode.F5)) {
			//dump_all_objects();
			//Application.Quit();
			new Thug("Thug");
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

    #region Logging
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
    #endregion

    #region Sleep anytime
    [HarmonyPatch(typeof(Bed), "CanSleep")]
	class HarmonyPatch_Bed_CanSleep {
		private static bool Prefix(ref bool __result) {
			//DailySummary.Instance.xpGained = 2000;
			//DailySummary.Instance.itemsSoldByPlayer["ogkush"] = 123;
			//_info_log(DailySummary.Instance.xpGained);
			__result = true;
			return false;
		}
	}
	#endregion

	#region ** Testing **

	[HarmonyPatch(typeof(DialogueController_Employee), "GetChoices")]
	class HarmonyPatch_DialogueController_Employee_GetChoices {
		private static void Postfix() {
			try {
				_info_log("!!!");
			} catch (Exception e) {
				_error_log("** HarmonyPatch_DialogueController_Employee_GetChoices.Postfix ERROR - " + e);
			}
		}
	}

    #endregion

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
using Assets.Scripts.Missions;
using MissionExtenderAssembly;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace MissionExtenderAssembly {
	public class MissionExtender : MonoBehaviour {
		private KMGameInfo _gameInfo = null;
		private KMGameCommands _gameCommands = null;
		public static ExtendedMissionDetails CurrentMissionDetails = null;
		public Dictionary<string, ExtendedMissionDetails> MissionDetails = new Dictionary<string, ExtendedMissionDetails>();

		internal static ExtendedMissionSettingsProperties PublicProperties = new ExtendedMissionSettingsProperties();

		private bool _started = false;

		private void Awake() {
			Debug.Log("[Extended Mission Settings] Awakening");
			DestroyImmediate(GetComponent<KMService>()); //Hide from Mod Selector
			_gameInfo = GetComponent<KMGameInfo>();
			_gameCommands = GetComponent<KMGameCommands>();

			GameObject infoObject = new GameObject("ExtendedMissionSettingsProperties");
			infoObject.transform.parent = gameObject.transform;
			PublicProperties = infoObject.AddComponent<ExtendedMissionSettingsProperties>();
			PublicProperties.MissionExtender = this;
			LoadMod();
			_started = true;
			Debug.Log("[Extended Mission Settings] Awoken");

		}

		private bool _enabled;
		private void OnDestroy() {
			Debug.Log("[Extended Mission Settings] Shutting down.");
			UnloadMod();
			_started = false;
		}

		private void OnEnable() {
			if (!_started || _enabled) return;
			Debug.Log("[Extended Mission Settings] Enabled.");
			LoadMod();
		}

		private void OnDisable() {
			if (!_enabled) return;
			Debug.Log("[Extended Mission Settings] Disabled.");
			UnloadMod();
		}

		private void LoadMod() {
			_gameInfo.OnStateChange += OnGameStateChanged;
			_enabled = true;
		}

		private void UnloadMod() {
			_gameInfo.OnStateChange -= OnGameStateChanged;
			_enabled = false;

			StopAllCoroutines();
		}

		private void OnGameStateChanged(KMGameInfo.State state) {
			if (state == KMGameInfo.State.Gameplay) {
				StartCoroutine(SetupGameplay());
			}
			else {
				Debug.LogFormat("[Extended Mission Settings] Going into {0} state.", state);
				StopAllCoroutines();
				if (state == KMGameInfo.State.Setup) {
					StartCoroutine(SetupSetupRoom());
				}
			}
		}

		private IEnumerator SetupGameplay() {
			CurrentMissionDetails = null;
			Mission mission = null;
			//ComponentPool componentPool = null;
			if (GameplayState.MissionToLoad != ModMission.CUSTOM_MISSION_ID && GameplayState.MissionToLoad != FreeplayMissionGenerator.FREEPLAY_MISSION_ID) {
				// mission has a name
				mission = MissionManager.Instance.GetMission(GameplayState.MissionToLoad);
				CurrentMissionDetails = MissionDetails[mission.ID];
				Debug.LogFormat("[Extended Mission Settings] Mission {0} has {1} extended settings provided.", mission.ID, CurrentMissionDetails.ExtendedSettings.Count);
				//ExtendedMissionDetails.ReadMission(mission, true, out componentPool);
			}
			else {
				yield break;
			}
			//Debug.Log("[Extended Mission Settings] Done.");
			//yield return null;

			//if (GameplayState.MissionToLoad != ModMission.CUSTOM_MISSION_ID && GameplayState.MissionToLoad != FreeplayMissionGenerator.FREEPLAY_MISSION_ID) {
			//	mission.GeneratorSetting.ComponentPools.Add(componentPool);
			//}
		}

		private IEnumerator SetupSetupRoom() {
			yield return null;
			//SetupRoom setupRoom = FindObjectOfType<SetupRoom>();
			//ExtendedMissionSettingsMonitor missionPageMonitor = setupRoom.BombBinder.MissionDetailPage.gameObject.AddComponent<ExtendedMissionSettingsMonitor>();
			//Debug.Log("[Extended Mission Settings] Started monitoring Bomb Binder");
			FindMissionSettings();
		}


		void FindMissionSettings() {
			DateTime startTime = DateTime.Now;

			// Fetch a list of all scriptable objects with type 'ModMission'. TODO: This can probably be done more efficiently without fetching ALL scriptobs first.
			ModMission[] missions = GameObject.FindObjectsOfType<ModMission>();
			Debug.LogFormat("[Extended Mission Settings] Searching {0} missions for extended settings.", missions.Length);

			foreach (ModMission m in missions) {
				//Debug.Log(m.ID);
				ExtendedMissionDetails missionDetails = new ExtendedMissionDetails();

				//foreach (var pool in m.GeneratorSetting.ComponentPools) {
				for (int p = m.GeneratorSetting.ComponentPools.Count - 1; p >= 0; p--) {
					var pool = m.GeneratorSetting.ComponentPools[p];
					if (pool.ModTypes == null || pool.ModTypes.Count == 0) {
						// do a check for this upfront so that we dont fix unrelated mistakes made by a mission creator.
						continue;
					}
					//foreach (var modType in pool.ModTypes) {
					for (int mt = pool.ModTypes.Count - 1; mt >= 0; mt--) {
						var modType = pool.ModTypes[mt];
						if (!modType.StartsWith("Extended Settings")) {
							// not an EMS pool;
							continue;
						}

						// find the json in this modtype
						int bracketIndex = modType.IndexOf('{');
						if (bracketIndex == -1) {
							Debug.LogFormat("[Extended Mission Settings] Encountered missing Json in mission {0}", m.ID);
							continue;
						}
						string settings = modType.Substring(bracketIndex).Trim();

						JObject o1;
						try {
							o1 = JObject.Parse(settings);
						}
						catch (Exception e) {
							Debug.LogFormat("[Extended Mission Settings] Encountered invalid Json in mission {0}: {1}", m.ID, settings);
							Debug.Log(e.Message);
							continue;
						}

						// add setting to EMS.
						foreach (var o in o1.Properties()) {
							if (o.Value.Type == JTokenType.String || o.Value.Type == JTokenType.Integer) {
								Debug.LogFormat("[Extended Mission Settings] Encountered setting in mission {0}: {1} = {2}", m.ID, o.Name.ToString(), o.Value.ToString());
								DictionaryAdd(missionDetails.ExtendedSettings, o.Name.ToString(), o.Value.ToString());
							}
							else if (o.Value.Type == JTokenType.Array) {
								foreach (var oo in o.Value) {
									Debug.LogFormat("[Extended Mission Settings] Encountered setting in mission {0}: {1} = {2}", m.ID, o.Name.ToString(), oo.ToString());
									DictionaryAdd(missionDetails.ExtendedSettings, o.Name.ToString(), oo.ToString());
								}
							}
						}
						// remove setting from componentpool
						pool.ModTypes.Remove(modType);
					}
					if (pool.ModTypes.Count == 0) {
						m.GeneratorSetting.ComponentPools.Remove(pool);
					}
				}

				if (missionDetails.ExtendedSettings.Count > 0) {
					DictionaryAdd(MissionDetails, m.ID, missionDetails);
				}
			}
			Debug.LogFormat("[Extended Mission Settings] Completed in {0} ms", (DateTime.Now - startTime).TotalMilliseconds );
		}

		public static void DictionaryAdd(Dictionary<string, List<string>> dict, string key, string value) {
			if (dict.ContainsKey(key)) {
				dict[key].Add(value);
			}
			else {
				dict[key] = new List<string> { value };
			}
		}

		public static void DictionaryAdd(Dictionary<string, ExtendedMissionDetails> dict, string key, ExtendedMissionDetails value) {
			if (dict.ContainsKey(key)) {
				dict[key] = value;
			}
			else {
				dict[key] = value;
			}
		}
	}
}
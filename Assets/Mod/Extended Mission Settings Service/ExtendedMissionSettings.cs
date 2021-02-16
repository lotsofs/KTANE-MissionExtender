using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

public class ExtendedMissionSettings : MonoBehaviour {

	public static Dictionary<string, string> ExtendedMissions = new Dictionary<string, string>();
	
	[SerializeField] KMGameInfo _gameInfo;

	[HideInInspector] public bool _finished = false;
	[HideInInspector] public string CurrentMissionSettings = null;

	bool _disabled = false;

	void Start() {
		_gameInfo.OnStateChange += OnGameStateChange;
		Debug.Log("[Extended Mission Settings] Hello! :)");
	}

	void OnDisable() {
		_disabled = true;
	}

	void OnEnable() {
		if (_disabled) {
			Debug.Log("[Extended Mission Settings] Hello again! :)");
			_disabled = false;
			FindMissionSettings();
			_finished = true;
		}
	}

	void OnGameStateChange(KMGameInfo.State state) {
		switch (state) {
			case KMGameInfo.State.Setup:
				CurrentMissionSettings = null;
				if (!_finished) {
					FindMissionSettings();
					_finished = true;
				}
				break;
			case KMGameInfo.State.Gameplay:
				GetMissionSettings(GetMissionName());
				break;
		}
	}

	string GetMissionName() {
		try {
			Component gameplayState = GameObject.Find("GameplayState(Clone)").GetComponent("GameplayState");
			Type type = gameplayState.GetType();
			FieldInfo fieldMission = type.GetField("MissionToLoad", BindingFlags.Public | BindingFlags.Static);
			string currentMission = fieldMission.GetValue(gameplayState).ToString();
			Debug.LogFormat("[Extended Mission Settings] Detected mission: {0}.", currentMission);
			return currentMission;
		}
		catch (Exception e) {
			Debug.LogFormat("[Extended Mission Settings] Ran into an issue when fetching the current mission name: {0}", e.Message);
			return "unknown";
		}
	}

	void GetMissionSettings(string missionName) {
		if (ExtendedMissions.ContainsKey(missionName)) {
			Debug.LogFormat("[Extended Mission Settings] Found settings for mission {0}: {1}", missionName, ExtendedMissions[missionName]);
			CurrentMissionSettings = ExtendedMissions[missionName];
		}
		else {
			Debug.LogFormat("[Extended Mission Settings] No settings found for mission {0}", missionName);
			CurrentMissionSettings = "";
		}
	}

	/// <summary>
	/// Scans through ALL the modded missions to find any that contain our extended mission settings.
	/// TODO: Should probably only scan missions that are opened in the binder. This looks like a long process when many missions are present.
	/// </summary>
	/// <returns></returns>
	void FindMissionSettings() {
		DateTime start = DateTime.Now;
		Debug.Log("[Extended Mission Settings] Searching missions for extended settings.");

		// Fetch a list of all scriptable objects with type 'ModMission'. TODO: This can probably be done more efficiently without fetching ALL scriptobs first.
		var missions = new List<ScriptableObject>();
		foreach (var obj in GameObject.FindObjectsOfType<ScriptableObject>()) {
			if (obj.GetType().ToString() == "ModMission") {
				missions.Add(obj);
			}
		}
		Debug.LogFormat("[Extended Mission Settings] Checking {0} missions...", missions.Count);

		Type typeGS = missions[0].GetType();    // Todo: "ModMission", I already know that, but I don't know how to tell the script this upfront.
		FieldInfo fieldGeneratorSetting = typeGS.GetField("GeneratorSetting", BindingFlags.Public | BindingFlags.Instance);

		// get the type for generator settings here so we dont do it every time
		Type typeCP = fieldGeneratorSetting.GetValue(missions[0]).GetType();

		foreach (ScriptableObject mission in missions) {
			// get generator settings for this mission
			var generatorSettings = fieldGeneratorSetting.GetValue(mission);

			// get a list of component pools for this mission
			FieldInfo fieldComponentPool = typeCP.GetField("ComponentPools", BindingFlags.Public | BindingFlags.Instance);
			IList componentPools = fieldComponentPool.GetValue(generatorSettings) as IList;

			// component pools contain mod types that determine which mods are in that component pool
			Type typeMT = componentPools[0].GetType();
			FieldInfo fieldModTypes = typeMT.GetField("ModTypes", BindingFlags.Public | BindingFlags.Instance);

			List<object> componentPoolsToClear = new List<object>();
			foreach (var cp in componentPools) {
				bool foundEMS = false; // We dont want to clean up the mess made by someone who erroneously put empty component pools in their mission.
				// TODO: This for some reason wasn't commited. Did I not finish it? Test whether it works.
				IList<string> modTypes = fieldModTypes.GetValue(cp) as IList<string>;
				Debug.Log("ADASDASDASDASDASD");
				Debug.Log(modTypes.Count);
				for (int i = modTypes.Count - 1; i >= 0; i--) {
					Debug.Log(i);
					if (modTypes[i].StartsWith("Extended Settings")) {
						if (AddExtendedSettings(mission.name, modTypes[i])) {
							modTypes.RemoveAt(i);
							Debug.Log("Removed something. Now " + modTypes.Count);
							foundEMS = true;
						}
					}
				}
				Debug.Log(modTypes.Count);
				if (modTypes.Count == 0 && foundEMS) {
					componentPoolsToClear.Add(cp);
				}
			}
			// when we found our settings, remove their containing component pool so that the mission binder won't complain about 'missing mods'
			foreach (var cp in componentPoolsToClear) {
				componentPools.Remove(cp);
			}
		}
		TimeSpan duration = DateTime.Now - start;
		Debug.LogFormat("[Extended Mission Settings] Finished processing missions. Took {0} seconds.", duration.TotalSeconds);
	}

	/// <summary>
	/// Adds the settings to the mission.
	/// </summary>
	/// <param name="mission"></param>
	/// <param name="modType"></param>
	/// <returns>Whether it was succesful</returns>
	bool AddExtendedSettings(string mission, string modType) {
		string settings;
		try {
			settings = modType.Substring(modType.IndexOf('{')).Trim();
		}
		catch (Exception e) {
			Debug.LogFormat("[Extended Mission Settings] Encountered missing Json in mission {0}: {1}", mission, modType);
			return false;
		}
		
		JObject o1;
		try {
			o1 = JObject.Parse(settings);
		}
		catch (Exception e) {
			Debug.LogFormat("[Extended Mission Settings] Encountered invalid Json in mission {0}: {1}", mission, settings);
			return false;
		}

		if (ExtendedMissions.ContainsKey(mission)) {
			Debug.LogFormat("[Extended Mission Settings] Adding additional settings to settings pool for mission {0}: {1}", mission, settings);
			JObject o2 = JObject.Parse(ExtendedMissions[mission]);
			o2.Merge(o1, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
			ExtendedMissions[mission] = o2.ToString();
		}
		else {
			Debug.LogFormat("[Extended Mission Settings] Found settings for mission {0}: {1}", mission, settings);
			ExtendedMissions.Add(mission, settings);
		}
		return true;
	}

	bool AddExtendedSettings2(string mission, string modType) {
		GameObject settingsObject = new GameObject("ExtendedMissionSettingsObject");
		settingsObject.transform.parent = gameObject.transform;


		return false;
	}
}

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMExtendedMissionSettings : MonoBehaviour {

	public Dictionary<string, List<string>> Settings = new Dictionary<string, List<string>>();
	bool _ready = false;
	public string EditorSettings;

	/// <summary>
	/// Whether any settings are provided for the current mission
	/// </summary>
	public bool SettingsProvided {
		get { ReadSettings(); return Settings.Count > 0; }
		set { }
	}

	// Use this for initialization
	void Awake () {
		ReadSettings();
	}

	/// <summary>
	/// Returns the string stored in setting settingName.
	/// </summary>
	/// <param name="settingName">The name of the setting to return</param>
	/// <returns>The string, or null if not found.</returns>
	public string GetStringSetting(string settingName) {
		ReadSettings();
		if (Settings.ContainsKey(settingName)) {
			return Settings[settingName][0];
		}
		else {
			return null;
		}
	}

	/// <summary>
	/// Returns the int stored in setting settingName
	/// </summary>
	/// <param name="settingName">The name of the setting to return</param>
	/// <param name="noneFound">The int to return if none is found.</param>
	/// <returns>The int, or noneFound (default 0) if not found.</returns>
	public int GetIntSetting(string settingName, int noneFound = 0) {
		ReadSettings();
		if (Settings.ContainsKey(settingName)) {
			int result;
			if (int.TryParse(Settings[settingName][0], out result)) {
				return result;
			}
			else {
				return noneFound;
			}
		}
		else {
			return noneFound;
		}
	}

	/// <summary>
	/// Returns the list of strings stored in setting settingName
	/// </summary>
	/// <param name="settingName">The name of the setting to return</param>
	/// <returns>The string list, or null if not found.</returns>
	public List<string> GetStringListSetting(string settingName) {
		ReadSettings();
		if (Settings.ContainsKey(settingName)) {
			return Settings[settingName];
		}
		else {
			return null;
		}
	}

	/// <summary>
	/// Returns the list of ints stored in setting settingName
	/// </summary>
	/// <param name="settingName">The name of the setting to return</param>
	/// <returns>The int list, or null if not found.</returns>
	public List<int> GetIntListSetting(string settingName) {
		ReadSettings();
		List<int> results = new List<int>();
		if (Settings.ContainsKey(settingName)) {
			foreach (string item in Settings[settingName]) {
				int result;
				if (int.TryParse(Settings[settingName][0], out result)) {
					results.Add(result);
				}
				else {
					return null;
				}
			}			
			return results;
		}
		else {
			return null;
		}
	}

	private void ReadSettings() {
		if (_ready) {
			return;
		}
		if (Application.isEditor) {
			Settings = EditorJson(EditorSettings);
		}
		else {
			GameObject EMSGameObject = GameObject.Find("ExtendedMissionSettingsProperties");
			if (EMSGameObject == null) // Not installed
				return;

			IDictionary<string, object> ExtendedMissionSettingsAPI = EMSGameObject.GetComponent<IDictionary<string, object>>();
			if (ExtendedMissionSettingsAPI.ContainsKey("GetMissionSettings")) {
				Settings = (ExtendedMissionSettingsAPI["GetMissionSettings"] as Dictionary<string, List<string>>) ?? Settings;
			}
		}
		_ready = true;
	}

	#region editor

	private static Dictionary<string, List<string>> EditorJson(string modType) {
		Dictionary<string, List<string>> settingsDict = new Dictionary<string, List<string>>();
		int bracketIndex = modType.IndexOf('{');
		if (bracketIndex == -1) {
			Debug.LogWarningFormat("Invalid JSON provided in editor.");
			return settingsDict;
		}
		string settings;
		settings = modType.Substring(bracketIndex).Trim();
		JObject o1;
		try {
			o1 = JObject.Parse(settings);
		}
		catch (Exception e) {
			Debug.LogWarningFormat("Invalid JSON provided in editor.");
			Debug.LogWarning(e.Message);
			return settingsDict;
		}
		foreach (var o in o1.Properties()) {
			if (o.Value.Type == JTokenType.String || o.Value.Type == JTokenType.Integer) {
				DictionaryAdd(settingsDict, o.Name.ToString(), o.Value.ToString());
			}
			else if (o.Value.Type == JTokenType.Array) {
				foreach (var p in o.Value) {
					DictionaryAdd(settingsDict, o.Name.ToString(), p.ToString());
				}
			}
		}
		return settingsDict;
	}

	private static void DictionaryAdd(Dictionary<string, List<string>> dict, string key, string value) {
		if (dict.ContainsKey(key)) {
			dict[key].Add(value);
		}
		else {
			dict[key] = new List<string> { value };
		}
	}

	#endregion
}

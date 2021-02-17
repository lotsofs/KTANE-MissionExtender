using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KMExtendedMissionSettings : MonoBehaviour {


	public	Dictionary<string, List<string>> Settings = new Dictionary<string, List<string>>();

	// Use this for initialization
	void Awake () {
		GameObject EMSGameObject = GameObject.Find("ExtendedMissionSettingsProperties");
		if (EMSGameObject == null) // Not installed
			return;

		IDictionary<string, object> ExtendedMissionSettingsAPI = EMSGameObject.GetComponent<IDictionary<string, object>>();
		if (ExtendedMissionSettingsAPI.ContainsKey("GetMissionSettings")) {
			Settings = (ExtendedMissionSettingsAPI["GetMissionSettings"] as Dictionary<string, List<string>>) ?? settings;
		}
	}

	/// <summary>
	/// Returns whether settings are provided for the current mission
	/// </summary>
	/// <returns>True if the mission has extended settings, false if not</returns>
	public bool SettingsProvided() {
		return Settings.Count > 0;
	}
}

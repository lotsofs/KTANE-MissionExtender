using System;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;

public enum EMSRResults {
	Success,
	Empty,
	NotInstalled,
	ReceivedNull,
	Error,
}

public class ExtendedMissionSettingsReader<T> : MonoBehaviour {

	public static EMSRResults ReadMissionSettings(out T settings) {
		GameObject emsGO = GameObject.Find("ExtendedMissionSettings(Clone)");
		if (emsGO == null) {
			settings = default(T);
			return EMSRResults.NotInstalled;
		}
		Type type;
		Component extendedMissionSettings;
		try {
			extendedMissionSettings = emsGO.GetComponent("ExtendedMissionSettings");
			type = extendedMissionSettings.GetType();
			FieldInfo fieldSettings;
			fieldSettings = type.GetField("CurrentMissionSettings");
			string settingsValue = (string)fieldSettings.GetValue(extendedMissionSettings);
			settings = JsonConvert.DeserializeObject<T>(settingsValue);

			if (settingsValue == null) {
				Debug.LogWarning("[Extended Mission Settings] The EMS service did not keep up with the gamestate in time! Please hand in a bug report with an unfiltered log file");
				return EMSRResults.ReceivedNull;    
			}
			else if (settingsValue.Length == 0) return EMSRResults.Empty;
			else return EMSRResults.Success;
		}
		catch (Exception e) {
			settings = default(T);
			Debug.Log("[Extended Mission Settings] " + e.Message);
			return EMSRResults.Error;
		}
	}
}

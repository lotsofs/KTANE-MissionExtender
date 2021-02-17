using System;
using UnityEngine;

namespace MissionExtenderAssembly
{
    public class ExtendedMissionSettingsProperties : PropertiesBehaviour
    {
		public ExtendedMissionSettingsProperties()
        {
			AddProperty("GetMissionSettings", new Property(GetSettings, null));		
			//AddProperty("HasMissionSettings", new Property(SettingsAvailable, null));
			//AddProperty("Testest", new Property(Testest, null));
        }

		private static object GetSettings() {
			Debug.Log("DEBUG: " + ExtendedMissionDetails.ExtendedSettings.Count);
			return ExtendedMissionDetails.ExtendedSettings;
		}

		//private static object SettingsAvailable() {
		//	return ExtendedMissionDetails.ExtendedSettings.Count > 0;
		//}

		//private static object Testest() {
		//	return "AAAAAAAAAAAAAAAAAAAAABBBB";
		//}

		internal MissionExtender MissionExtender { get; set; }
    }
}
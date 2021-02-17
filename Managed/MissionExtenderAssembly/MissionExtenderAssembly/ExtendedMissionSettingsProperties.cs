using System;
using UnityEngine;

namespace MissionExtenderAssembly
{
    public class ExtendedMissionSettingsProperties : PropertiesBehaviour
    {
		public ExtendedMissionSettingsProperties()
        {
			AddProperty("GetMissionSettings", new Property(GetSettings, null));		
        }

		private static object GetSettings() {
			return MissionExtender.CurrentMissionDetails.ExtendedSettings;
		}

		internal MissionExtender MissionExtender { get; set; }
    }
}
﻿using Assets.Scripts.Missions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MissionExtenderAssembly {
	public class ExtendedMissionDetails {
		public Dictionary<string, List<string>> ExtendedSettings { get; set; } = new Dictionary<string, List<string>>();
		public bool InvalidJson = false;
		public GeneratorSetting GeneratorSetting;

		public static ExtendedMissionDetails ReadMission(Mission mission) {
			//List<ComponentPool> componentPools;
			return ReadMission(mission, false);
		}

		public static ExtendedMissionDetails ReadMission(Mission mission, bool removeComponentPools) {
			// todo: NESTING HELL. Fix
			ExtendedMissionDetails missionDetails = new ExtendedMissionDetails();
			//componentPools = new List<ComponentPool>();
			if (mission.GeneratorSetting != null) {
				GeneratorSetting generatorSetting = UnityEngine.Object.Instantiate(mission).GeneratorSetting;
				if (generatorSetting.ComponentPools != null) {
					for (int i = generatorSetting.ComponentPools.Count - 1; i >= 0; i--) {
						ComponentPool pool = generatorSetting.ComponentPools[i];
						if (pool.ModTypes != null && pool.ModTypes.Count > 0) {
							for (int j = pool.ModTypes.Count - 1; j >= 0; j--) {
								string modType = pool.ModTypes[j];
								if (modType.StartsWith("Extended Settings")) {
									int bracketIndex = modType.IndexOf('{');
									if (bracketIndex == -1) {
										Debug.LogFormat("[Extended Mission Settings] Encountered missing Json in mission {0}", mission);
										missionDetails.InvalidJson = true;
										continue;
									}
									string settings;
									settings = modType.Substring(bracketIndex).Trim();
									JObject o1;
									try {
										o1 = JObject.Parse(settings);
									}
									catch (Exception e) {
										Debug.LogFormat("[Extended Mission Settings] Encountered invalid Json in mission {0}: {1}", mission, settings);
										Debug.Log(e.Message);
										missionDetails.InvalidJson = true;
										continue;
									}
									foreach (var o in o1.Properties()) {
										if (o.Value.Type == JTokenType.String || o.Value.Type == JTokenType.Integer) {
											DictionaryAdd(missionDetails.ExtendedSettings, o.Name.ToString(), o.Value.ToString());
										}
										else if (o.Value.Type == JTokenType.Array) {
											foreach (var p in o.Value) {
												DictionaryAdd(missionDetails.ExtendedSettings, o.Name.ToString(), p.ToString());
											}
										}
									}
									generatorSetting.ComponentPools[i].ModTypes.RemoveAt(j);
									if (removeComponentPools) {
										mission.GeneratorSetting.ComponentPools[i].ModTypes.RemoveAt(j);
									}
								}
							}
							if (pool.ModTypes.Count == 0) {
								generatorSetting.ComponentPools.RemoveAt(i);
								if (removeComponentPools) {
									mission.GeneratorSetting.ComponentPools.RemoveAt(i);
								}
							}
						}
					}
				}
				missionDetails.GeneratorSetting = generatorSetting;
			}
			return missionDetails;
		}

		public static void DictionaryAdd(Dictionary<string,List<string>> dict, string key, string value) {
			if (dict.ContainsKey(key)) {
				dict[key].Add(value);
			}
			else {
				dict[key] = new List<string> { value };
			}
		}
	}
}

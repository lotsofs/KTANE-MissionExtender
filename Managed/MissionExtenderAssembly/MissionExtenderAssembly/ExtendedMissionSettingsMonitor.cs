using Assets.Scripts.Missions;
using I2.Loc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;

namespace MissionExtenderAssembly {

	// Original script by Lupo511 from Multiple Bombs

	class ExtendedMissionSettingsMonitor : MonoBehaviour {
		private MissionDetailPage page;

		// gets called the first time the player opens a mission detail page in the binder.
		private void Awake() {
			page = GetComponent<MissionDetailPage>();
		}
	
		// gets called every time the player opens a new mission detail page in the binder.
		private void OnEnable() {
			ExtendedMissionDetails.ExtendedSettings.Clear();
			StartCoroutine(SetupPage());
		}

		private void OnDisable() {
			StopAllCoroutines();
		}

		private void OnDestroy() {

		}

		private IEnumerator SetupPage() {
			yield return null;
			yield return null;
			Mission currentMission = (Mission)page.GetType().BaseType.GetField("currentMission", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);

			ExtendedMissionDetails extendedMissionDetails = ExtendedMissionDetails.ReadMission(currentMission);
			if (ExtendedMissionDetails.ExtendedSettings.Count == 0) {
				yield break;
			}
			
			foreach (string k in ExtendedMissionDetails.ExtendedSettings.Keys) {
				Debug.LogFormat("[Extended Mission Settings] Found setting for {0}: {1} of size {2}", currentMission.DisplayNameTerm, k, ExtendedMissionDetails.ExtendedSettings[k].Count);
			}
			bool canStart = UpdateMissionDetailInformation(currentMission, currentMission.DescriptionTerm, page);
			FieldInfo canStartField = typeof(MissionDetailPage).GetField("canStartMission", BindingFlags.Instance | BindingFlags.NonPublic);
			canStartField.SetValue(page, canStart);
		}

		public static bool FindMultipleBombs() {
			// https://github.com/ashbash1987/ktanemod-factory/blob/master/FactoryAssembly/Source/MultipleBombsInterface.cs
			Type multipleBombsType = null;
			object multipleBombsObject = null;
			multipleBombsType = ReflectionHelper.FindType("MultipleBombsAssembly.MultipleBombs");
			if (multipleBombsType == null) {
				Debug.Log("[Extended Mission Settings] Cannot find the MultipleBombs type - locking mission.");
				return false;
			}
			multipleBombsObject = GameObject.FindObjectOfType(multipleBombsType);
			if (multipleBombsObject == null) {
				Debug.Log("[Extended Mission Settings] Cannot find the MultipleBombs object - locking mission.");
				return false;
			}
			return true;
		}

		public static bool UpdateMissionDetailInformation(Mission mission, string descriptionTerm, MissionDetailPage page) {
			// Cooperation with the other binder readers (multiple bombs, factory mode) is probably necessary.
			// There'll be race conditions. If they check first and then this, this should work, but 
			// if this checks first and then they, they'll likely throw EM in the missing mod types pool again. 
			// TODO: Submit a PR there to account for the new boy in town. Although I'm not entirely sure what takes care of
			// 'Factory Mode' mod type. I couldn't find anything about it in Factory Mode's code. A better system may be possible.
			List<string> missingModTypes = new List<string>();
			int maxModuleCount = Math.Max(11, ModManager.Instance.GetMaximumModules());
			int maxFrontFaceModuleCount = Math.Max(5, ModManager.Instance.GetMaximumModulesFrontFace());

			int notModulesCount = 0;
			int selfCount = 0;
			
			bool canStart = false;

			TextMeshPro description = page.TextDescription;
			string moduleCountText = page.TextModuleCount.text;
			moduleCountText = moduleCountText.Split(' ')[0];
			
			foreach (ComponentPool pool in mission.GeneratorSetting.ComponentPools) {
				foreach (string modType in pool.ModTypes) {
					if (modType == "Factory Mode") {
						if (!FindMultipleBombs()) {
							missingModTypes.Add(modType);
						}
						else {
							notModulesCount++;
						}
						continue;
					}
					if (modType.StartsWith("Multiple Bombs")) {
						if (!FindMultipleBombs()) {
							missingModTypes.Add(modType);
						}
						else {
							notModulesCount++;
						}
						continue;
					}
					if (modType.StartsWith("Extended Settings")) {
						notModulesCount++;
						selfCount++;
						continue;
					}
					if (!ModManager.Instance.HasBombComponent(modType)) {
						missingModTypes.Add(modType);
					}
				}
			}

			int totalComponentPools = mission.GeneratorSetting.ComponentPools.Count;

			if (description.text.StartsWith("A room that can support more bombs is required.")) {
				// multiple bombs already looked at this and determined we need a different room, so the mission can't start regardless. Don't bother.
				canStart = false;
			}
			else if (missingModTypes.Count > 0) {
				canStart = false;
				Localization.SetTerm("BombBinder/error_missingModules", description.gameObject);
				Localization.SetParameter("MISSING_MODULES_LIST", string.Join("\n", missingModTypes.ToArray()), description.gameObject);
			}
			else if (totalComponentPools - notModulesCount > maxModuleCount) {
				canStart = false;
				Localization.SetTerm("BombBinder/error_needABiggerBomb", description.gameObject);
				Localization.SetParameter("MAX_MODULE_COUNT", maxModuleCount.ToString(), description.gameObject);
			}
			else if (mission.GeneratorSetting.FrontFaceOnly && totalComponentPools - notModulesCount > maxFrontFaceModuleCount) {
				canStart = false;
				Localization.SetTerm("BombBinder/error_needABiggerBomb", description.gameObject);
				Localization.SetParameter("MAX_MODULE_COUNT", maxModuleCount.ToString(), description.gameObject);
			}
			else {
				canStart = true;
				Localization.SetTerm(descriptionTerm, description.gameObject);
			}

			int writtenModuleCount = 0;
			if (!int.TryParse(moduleCountText, out writtenModuleCount)) {
				writtenModuleCount = mission.GeneratorSetting.ComponentPools.Count;
			}
			if (writtenModuleCount > totalComponentPools) {
				// multiple bombs already processed this. Don't touch it. 
			}
			else {
				Localization.SetTerm("BombBinder/txtModuleCount", page.TextModuleCount.gameObject);
				Localization.SetParameter("MODULE_COUNT", (totalComponentPools - notModulesCount).ToString(), page.TextModuleCount.gameObject);
			}
			return canStart;
		}

	}
}

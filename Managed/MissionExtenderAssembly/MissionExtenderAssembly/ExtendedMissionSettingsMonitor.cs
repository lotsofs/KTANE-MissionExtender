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

	// Original scripts by Lupo511 from Multiple Bombs

	class ExtendedMissionSettingsMonitor : MonoBehaviour {
		private MissionDetailPage page;

		// gets called the first time the player opens a mission detail page in the binder.
		private void Awake() {
			page = GetComponent<MissionDetailPage>();
		}
	
		// gets called every time the player opens a new mission detail page in the binder.
		private void OnEnable() {
			StartCoroutine(SetupPage());
		}

		private void OnDisable() {
			StopAllCoroutines();
		}

		private void OnDestroy() {

		}

		private IEnumerator SetupPage() {
			yield return null;
			yield return null;	// TODO: Double yield return null to ensure this goes after Multiple Bombs? Big oof. Collaborate with the MB team.
			// TODO: Maybe at this point just permanently erase it from the mission binder and store it in the property thing instead?
			Mission currentMission = (Mission)page.GetType().BaseType.GetField("currentMission", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);

			ExtendedMissionDetails extendedMissionDetails = ExtendedMissionDetails.ReadMission(currentMission);
			if (extendedMissionDetails.ExtendedSettings.Count == 0) {
				Debug.LogFormat("[Extended Mission Settings] Found no settings for {0}", currentMission.DisplayNameTerm);
				yield break;
			}
			
			foreach (string k in extendedMissionDetails.ExtendedSettings.Keys) {
				Debug.LogFormat("[Extended Mission Settings] Found setting for {0}: {1} of size {2}", currentMission.DisplayNameTerm, k, extendedMissionDetails.ExtendedSettings[k].Count);
			}
			bool canStart = UpdateMissionDetailInformation(currentMission, currentMission.DescriptionTerm, page, extendedMissionDetails);
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

		public static bool UpdateMissionDetailInformation(Mission mission, string descriptionTerm, MissionDetailPage page, ExtendedMissionDetails details) {
			// TODO: Cooperation with the other binder readers (multiple bombs, factory mode) is probably necessary.
			// There'll be race conditions. If they check first and then this, this should work, but 
			// if this checks first and then they, they'll likely throw EM in the missing mod types pool again. 
			// TODO: Submit a PR there to account for the new boy in town. Although I'm not entirely sure what takes care of
			// 'Factory Mode' mod type. I couldn't find anything about it in Factory Mode's code. A better system may be possible.
			List<string> missingModTypes = new List<string>();
			int maxModuleCount = Math.Max(11, ModManager.Instance.GetMaximumModules());
			int maxFrontFaceModuleCount = Math.Max(5, ModManager.Instance.GetMaximumModulesFrontFace());

			int notModulesCount = 0;
			//int partialPools = 0;
			
			bool canStart = false;
			bool notSupported = false;

			TextMeshPro description = page.TextDescription;
			string moduleCountText = page.TextModuleCount.text;
			moduleCountText = moduleCountText.Split(' ')[0];
			
			foreach (ComponentPool pool in details.GeneratorSetting.ComponentPools) {
				int counter = 0;
				foreach (string modType in pool.ModTypes) {
					if (modType == "Factory Mode") {
						notSupported = true;	// TODO: Fix this together with the Multiple Bombs team.
						if (!FindMultipleBombs() || counter > 0) {
							missingModTypes.Add(modType);
						}
						else {
							notModulesCount++;
						}
						break;	
					}
					if (modType.StartsWith("Multiple Bombs")) {
						notSupported = true;    // TODO: Fix this together with the Multiple Bombs team, as right now this overrides multiple bombs doing a missing component error if one of the modules is missing.
						// Multiple Bombs tosses the entire component pool and only checks index 0. We will do the same.
						if (!FindMultipleBombs() || counter > 0) {
						missingModTypes.Add(modType);
						}
						else {
							notModulesCount++;
						}
						break;
					}
					if (!ModManager.Instance.HasBombComponent(modType)) {
						missingModTypes.Add(modType);
					}
					counter++;
				}
			}


			int totalComponentPools = details.GeneratorSetting.ComponentPools.Count;

			if (notSupported) {
				// todo: Support multiple bombs.
				page.TextDescription.text = "Using both Extended Mission Settings & Multiple Bombs/Factory Mode is not supported at this time.";
				canStart = false;
			}
			else if (description.text.StartsWith("A room that can support more bombs is required.")) {
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
			else if (details.InvalidJson) {
				canStart = false;
				page.TextDescription.text = "An error occured reading the extended mission settings for this mission.";
			}
			else {
				canStart = true;
				Localization.SetTerm(descriptionTerm, description.gameObject);
			}

			int totalTotalComponentPools = mission.GeneratorSetting.ComponentPools.Count;
			int writtenModuleCount = -1;
			int selfCount = totalTotalComponentPools - totalComponentPools;

			if (!int.TryParse(moduleCountText, out writtenModuleCount)) {
				writtenModuleCount = totalTotalComponentPools;
			}
			if (writtenModuleCount > totalTotalComponentPools) {
				// Multiple Bombs established we have more modules due to multiple bombs.
				Localization.SetTerm("BombBinder/txtModuleCount", page.TextModuleCount.gameObject);
				Localization.SetParameter("MODULE_COUNT", (writtenModuleCount - selfCount).ToString(), page.TextModuleCount.gameObject);
			}
			else {
				Localization.SetTerm("BombBinder/txtModuleCount", page.TextModuleCount.gameObject);
				Localization.SetParameter("MODULE_COUNT", (totalComponentPools - notModulesCount).ToString(), page.TextModuleCount.gameObject);
			}
			return canStart;
		}

	}
}

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

	class MissionDetailPageMonitor : MonoBehaviour {
		private MissionDetailPage page;

		// gets called the first time the player opens a mission detail page in the binder.
		private void Awake() {
			page = GetComponent<MissionDetailPage>();
		}
	
		// gets called every time the player opens a new mission detail page in the binder.
		private void OnEnable() {
			Debug.Log("HAHAHAHA");
			StartCoroutine(SetupPage());
		}

		private void OnDisable() {
			StopAllCoroutines();
		}

		private void OnDestroy() {

		}

		private IEnumerator SetupPage() {
			yield return null;
			Mission currentMission = (Mission)page.GetType().BaseType.GetField("currentMission", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(page);
			Debug.Log(currentMission.DisplayNameTerm);
			Debug.Log("Ok");

			ExtendedMissionDetails extendedMissionDetails = ExtendedMissionDetails.ReadMission(currentMission);
			Debug.Log("Ok2");
			Debug.Log(extendedMissionDetails.ExtendedSettings.Count);
			foreach (string k in extendedMissionDetails.ExtendedSettings.Keys) {
				Debug.Log("HAHAHA: " + k + " " + extendedMissionDetails.ExtendedSettings[k][0] + " size " + extendedMissionDetails.ExtendedSettings[k].Count);
			}
		}
	
		//public static bool UpdateMissionDetailInformation() {

		//}

	}
}

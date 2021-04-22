using Assets.Scripts.Missions;
using MissionExtenderAssembly;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace MissionExtenderAssembly {
	public class MissionExtender : MonoBehaviour {
		private KMGameInfo _gameInfo = null;
		private KMGameCommands _gameCommands = null;
		public static ExtendedMissionDetails CurrentMissionDetails = null;

		private bool _currentRandomSeed;

		internal static ExtendedMissionSettingsProperties PublicProperties = new ExtendedMissionSettingsProperties();

		private bool _started = false;

		private void Awake() {
			Debug.Log("[Extended Mission Settings] Awakening");
			DestroyImmediate(GetComponent<KMService>()); //Hide from Mod Selector
			_gameInfo = GetComponent<KMGameInfo>();
			_gameCommands = GetComponent<KMGameCommands>();

			GameObject infoObject = new GameObject("ExtendedMissionSettingsProperties");
			infoObject.transform.parent = gameObject.transform;
			PublicProperties = infoObject.AddComponent<ExtendedMissionSettingsProperties>();
			PublicProperties.MissionExtender = this;
			LoadMod();
			_started = true;
			Debug.Log("[Extended Mission Settings] Awoken");

		}

		private bool _enabled;
		private void OnDestroy() {
			Debug.Log("[Extended Mission Settings] Shutting down.");
			UnloadMod();
			_started = false;
		}

		private void OnEnable() {
			if (!_started || _enabled) return;
			Debug.Log("[Extended Mission Settings] Enabled.");
			LoadMod();
		}

		private void OnDisable() {
			if (!_enabled) return;
			Debug.Log("[Extended Mission Settings] Disabled.");
			UnloadMod();
		}

		private void LoadMod() {
			_gameInfo.OnStateChange += OnGameStateChanged;
			_enabled = true;
			_currentState = KMGameInfo.State.Setup;
			_prevState = KMGameInfo.State.Setup;
		}

		private void UnloadMod() {
			_gameInfo.OnStateChange -= OnGameStateChanged;
			_enabled = false;

			StopAllCoroutines();
		}

		private KMGameInfo.State _currentState = KMGameInfo.State.Unlock;
		private KMGameInfo.State _prevState = KMGameInfo.State.Unlock;

		private void OnGameStateChanged(KMGameInfo.State state) {
			if (state == KMGameInfo.State.Gameplay) {
				StartCoroutine(SetupGameplay());
			}
			else {
				Debug.Log("[Extended Mission Settings] Cleaning up.");
				StopAllCoroutines();
				if (state == KMGameInfo.State.Setup) {
					StartCoroutine(SetupSetupRoom());
				}
			}
		}

		private IEnumerator SetupGameplay() {
			CurrentMissionDetails = null;
			Mission mission;
			if (GameplayState.MissionToLoad != ModMission.CUSTOM_MISSION_ID && GameplayState.MissionToLoad != FreeplayMissionGenerator.FREEPLAY_MISSION_ID) {
				mission = MissionManager.Instance.GetMission(GameplayState.MissionToLoad);
				CurrentMissionDetails = ExtendedMissionDetails.ReadMission(mission, true);
			}
			else {
				yield break;
			}
			yield return null;	
		}

		private IEnumerator SetupSetupRoom() {
			yield return null;
			SetupRoom setupRoom = FindObjectOfType<SetupRoom>();
			ExtendedMissionSettingsMonitor missionPageMonitor = setupRoom.BombBinder.MissionDetailPage.gameObject.AddComponent<ExtendedMissionSettingsMonitor>();
			Debug.Log("[Extended Mission Settings] Started monitoring Bomb Binder");
		}
	}
}
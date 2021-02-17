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

		///// <summary>
		///// Start Mission Extender
		///// </summary>
		//private void Start() {
		//	//GameObject infoObject2 = new GameObject("RuleSeedModifierProperties");
		//	//infoObject2.transform.parent = gameObject.transform;
		//	//PublicProperties[1] = infoObject2.AddComponent<RuleSeedModifierProperties>();
		//	//PublicProperties[1].VanillaRuleModifer = this;

		//}

		//public void SetRuleSeed(int seed, bool writeSettings) {
		//	if (seed == int.MinValue) seed = 0;
		//	_modSettings.Settings.RuleSeed = Mathf.Abs(seed);
		//	if (writeSettings) _modSettings.WriteSettings();
		//}

		//public void SetRandomRuleSeed(bool setting, bool writeSettings) {
		//	_modSettings.Settings.RandomRuleSeed = setting;
		//	if (writeSettings) _modSettings.WriteSettings();
		//}

		//public string GenerateManual() {
		//	if (CurrentState == KMGameInfo.State.Setup || CurrentState == KMGameInfo.State.PostGame)
		//		GenerateRules(_modSettings.Settings.RuleSeed);
		//	ManualGenerator.Instance.WriteManual(_modSettings.Settings.RuleSeed);
		//	return Path.Combine(Application.persistentDataPath, Path.Combine("ModifiedManuals", _modSettings.Settings.RuleSeed.ToString()));
		//}

		private bool _enabled;
		// ReSharper disable once UnusedMember.Local
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
			CurrentState = KMGameInfo.State.Setup;
			_prevState = KMGameInfo.State.Setup;
		}

		private void UnloadMod() {
			//UnloadRuleManager();
			_gameInfo.OnStateChange -= OnGameStateChanged;
			_enabled = false;

			StopAllCoroutines();
		}

		public KMGameInfo.State CurrentState = KMGameInfo.State.Unlock;
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
			//ExtendedMissionDetails details;
			//ExtendedMissionDetails.ExtendedSettings.Clear();
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


		// TODO: Look at Multiple Bombs, and do its thing with the binder to only process one mission at a time.

		//private void OnStateChange(KMGameInfo.State state) {
		//	if (AddWidget != null) {
		//		StopCoroutine(AddWidget);
		//		AddWidget = null;
		//	}

		//	if (FixMorseCode != null) {
		//		StopCoroutine(FixMorseCode);
		//		FixMorseCode = null;
		//	}

		//	//DebugLog("Transitioning from {1} to {0}", state, CurrentState);
		//	//if((_prevState == KMGameInfo.State.Setup || _prevState == KMGameInfo.State.PostGame) && CurrentState == KMGameInfo.State.Transitioning && state == KMGameInfo.State.Transitioning)
		//	if (CurrentState == KMGameInfo.State.Setup && state == KMGameInfo.State.Transitioning) {
		//		_modSettings.ReadSettings();
		//		var seed = _modSettings.Settings.RuleSeed;

		//		if (_modSettings.Settings.RandomRuleSeed)
		//			seed = new System.Random().Next(_modSettings.Settings.MaxRandomSeed < 0 ? int.MaxValue : _modSettings.Settings.MaxRandomSeed);

		//		_currentSeed = seed;
		//		_currentRandomSeed = _modSettings.Settings.RandomRuleSeed;

		//		DebugLog("Generating Rules based on Seed {0}", seed);
		//		GenerateRules(seed);
		//		ManualGenerator.Instance.WriteManual(seed);
		//	}
		//	else if ((_prevState == KMGameInfo.State.Setup || _prevState == KMGameInfo.State.PostGame) && CurrentState == KMGameInfo.State.Transitioning && state == KMGameInfo.State.Transitioning) {
		//		AddWidget = StartCoroutine(AddWidgetToBomb(RuleSeedWidget));
		//	}
		//	else if (state == KMGameInfo.State.Gameplay) {
		//		FixMorseCode = StartCoroutine(FixMorseCodeModule());
		//	}

		//	_prevState = CurrentState;
		//	CurrentState = state;
		//}
	}
}
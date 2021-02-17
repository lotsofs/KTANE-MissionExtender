using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleModule : MonoBehaviour
{
    public KMSelectable[] buttons;

    public ExtendedMissionSettingsReader<ExampleModuleMissionSettings> emsr;    // Relevant settings for this module are in ExampleModuleMissionSettings.cs
    public Renderer moduleBackdrop;

    int correctIndex;
    bool isActivated = false;

    void Start()
    {

        GameObject EMSGameObject = GameObject.Find("ExtendedMissionSettingsProperties");
        if (EMSGameObject == null) // Not installed
            return;

        IDictionary<string, object> ExtendedMissionSettingsAPI = EMSGameObject.GetComponent<IDictionary<string, object>>();
        Dictionary<string, List<string>> settings2 = new Dictionary<string, List<string>>();
        if (ExtendedMissionSettingsAPI.ContainsKey("GetMissionSettings")) {
            settings2 = (ExtendedMissionSettingsAPI["GetMissionSettings"] as Dictionary<string, List<string>>) ?? settings2;
		}
        Debug.Log(settings2.Count + " Apples");

        return;

        // We fetch the settings
        ExampleModuleMissionSettings settings;
        EMSRResults result = ExtendedMissionSettingsReader<ExampleModuleMissionSettings>.ReadMissionSettings(out settings);
        switch (result) {
            case EMSRResults.NotInstalled:
                // Returned when the service mod is not installed. If it was needed for this mission, the mission wouldn't have been able to start anyway.
            case EMSRResults.Empty:
                // The service mod is installed, but this mission has no settings.
                Init();
                break;
            case EMSRResults.Error:
                // An error occured when trying to read the settings from the service. Could be anything. Log worthy.
                Debug.Log("[Example Module #x] An exception occured when trying to read the mission settings.");
                Init();
                break;
            case EMSRResults.ReceivedNull:
                // If no settings are available for this mission, the service would provide an empty string. 
                // However, if the string is null, this means that the service is not detecting the mission correctly, or
                // something else is going wrong. Very log worthy.
                Debug.Log("[Example Module #x] There was an issue with the EMS service. Please file a bug report with an unfiltered log file.");
                Init();
                break;
            case EMSRResults.Success:
                // The service has some settings for this mission. This does not mean, however, that these settings also apply to our module. They could be for other mods instead.
                
                // Check the setting for this module dealing with changing its color.
                Color color;
                if (string.IsNullOrEmpty(settings.ExampleModule_Color)) {
                    Debug.LogFormat("[Example Module #x] Extended Mission Settings are present, but none dictating this module's color.");
                }
                else if (ColorUtility.TryParseHtmlString(settings.ExampleModule_Color, out color)) {
                    Debug.LogFormat("[Example Module #x] EMS dictates the changing of the module's color to {0}", settings.ExampleModule_Color);
                    moduleBackdrop.material.color = color;
                }
                else {
                    Debug.LogFormat("[Example Module #x] an EMS setting for changing this module's color was provided, but its value does not make sense: {0}", settings.ExampleModule_Color);
                }

                // Check the setting for this module dealing with setting its solution.
                if (!string.IsNullOrEmpty(settings.ExampleModule_CorrectButton)) {
                    switch (settings.ExampleModule_CorrectButton.ToLowerInvariant().Trim()) {
                        case "topleft":
                            Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", settings.ExampleModule_CorrectButton);
                            Init(0);       // TL
                            break;
                        case "bottomleft":
                            Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", settings.ExampleModule_CorrectButton);
                            Init(2);    // BL
                            break;
                        case "topright":
                            Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", settings.ExampleModule_CorrectButton);
                            Init(1); // TR
                            break;
                        case "bottomright":
                            Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", settings.ExampleModule_CorrectButton);
                            Init(3); // BR
                            break;
                        default:
                            Debug.LogFormat("[Example Module #x] The provided EMS value for ExampleModule_CorrectButton was invalid. Received: {0}", settings.ExampleModule_CorrectButton);
                            Init(); // random
                            break;
                    }
                }
                else {
                    Debug.LogFormat("[Example Module #x] Extended Mission Settings are present, but none dictating this module's solution.");
                    Init(); // random
                }
                break;                
        }
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    /// <summary>
    /// Calculates the solution.
    /// </summary>
    /// <param name="index">Set it to somethign specific (reading order)</param>
    void Init(int index = -1)
    {
        if (index < 0 || index > 3) {
            correctIndex = Random.Range(0, 4);
        }
        else {
            Debug.LogFormat("[Example Module #x] ----------------", correctIndex + 1);
            correctIndex = index;
        }
        Debug.LogFormat("[Example Module #x] Solution is button number {0} in standard reading order.", correctIndex + 1);

        for (int i = 0; i < buttons.Length; i++)
        {
            string label = i == correctIndex ? "O" : "X";

            TextMesh buttonText = buttons[i].GetComponentInChildren<TextMesh>();
            buttonText.text = label;
            int j = i;
            buttons[i].OnInteract += delegate () { OnPress(j == correctIndex); return false; };
        }
    }

    void ActivateModule()
    {
        isActivated = true;
    }

    void OnPress(bool correctButton)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.Log("[Example Module #x] Pressed button before module has been activated!");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            Debug.Log("[Example Module #x] Pressed " + correctButton + " button");
            if (correctButton)
            {
                GetComponent<KMBombModule>().HandlePass();
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }
}

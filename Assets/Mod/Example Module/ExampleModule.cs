using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleModule : MonoBehaviour
{
    public KMSelectable[] buttons;

    private KMExtendedMissionSettings _kmSettings;
    
    public Renderer moduleBackdrop;
    int correctIndex = -1;
    bool isActivated = false;


    void Start()
    {

        _kmSettings = GetComponent<KMExtendedMissionSettings>();

        ProcessExtendedMissionSettings();
        Init();

        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    /// <summary>
    /// Reads and processes all the settings from EMS possible for this module.
    /// </summary>
    void ProcessExtendedMissionSettings() {
        // dont do anything if no settings are provided at all
        if (!_kmSettings.SettingsProvided) {
            return;
        }

        // Look if a color has been provided in the settings
        string colorStr = _kmSettings.GetStringSetting("ExampleModule_Color");
        // If so, process it.
        if (colorStr != null) {
            Color color;
            if (ColorUtility.TryParseHtmlString(colorStr, out color)) {
                Debug.LogFormat("[Example Module #x] EMS dictates the changing of the module's color to {0}", colorStr);
                moduleBackdrop.material.color = color;
            }
            else {
                Debug.LogFormat("[Example Module #x] an EMS setting for changing this module's color was provided, but its value does not make sense: \"{0}\"", colorStr);
            }
        }

        // Now do the same, but for the solution setting.
        string solutionStr = _kmSettings.GetStringSetting("ExampleModule_CorrectButton");
        if (solutionStr != null) {
            switch (solutionStr.ToLowerInvariant().Trim()) {
                case "topleft":
                    Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", solutionStr);
                    correctIndex = 0;       // TL
                    break;
                case "bottomleft":
                    Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", solutionStr);
                    correctIndex = 2;    // BL
                    break;
                case "topright":
                    Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", solutionStr);
                    correctIndex = 1; // TR
                    break;
                case "bottomright":
                    Debug.LogFormat("[Example Module #x] EMS determined the solution should be {0}", solutionStr);
                    correctIndex = 3; // BR
                    break;
                default:
                    Debug.LogFormat("[Example Module #x] The provided EMS value for ExampleModule_CorrectButton was invalid. Received: {0}", solutionStr);
                    break;
            }
        } 
        // Put a separator bar in the logging.
        if (solutionStr != null || colorStr != null) {
            Debug.Log("[Example Module #x] ----------------");
        }
    }

    /// <summary>
    /// Calculates the solution.
    /// </summary>
    /// <param name="index">Set it to somethign specific (reading order)</param>
    void Init()
    {
        if (correctIndex == -1) {
            correctIndex = Random.Range(0, 4);
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

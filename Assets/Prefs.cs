using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Prefs : MonoBehaviour
{
    

    public static float mouse_sensitivity {
        get{ return m_sen; }
        set { m_sen = value;
            On_mouseSensitivityChanged();
        }
    }
    private static float m_sen = 1.0f;


    public static float volume_effects;
    public static float volumeMusic;
    public static float volumeMaster;


    public static void On_mouseSensitivityChanged() {
        NetworkPlayerMovement.OnMouseSensitivityChanged();
        player_camera_handler.mouse_sensitivity_multiplier = Prefs.mouse_sensitivity;
    }
}

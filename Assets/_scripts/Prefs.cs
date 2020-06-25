using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs : MonoBehaviour
{
    

    public static float mouse_sensitivity {
        get{ return m_sen; }
        set { m_sen = value;

        }
    }
    private static float m_sen = 1.0f;


    public static float volume_effects;
    public static float volumeMusic;
    public static float volumeMaster;
    public static bool showDirectionalArrow;

}

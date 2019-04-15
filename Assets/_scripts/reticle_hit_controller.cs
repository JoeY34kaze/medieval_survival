using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reticle_hit_controller : MonoBehaviour
{
    private static reticle_hit reticle_hit;
    private static reticle_hit reticle_block;
    private static GameObject canvas;

    public static void Initialize()
    {
        canvas = GameObject.Find("Canvas");
        
        reticle_hit = Resources.Load<reticle_hit>("reticle_hit/reticle_hit");
        reticle_block = Resources.Load<reticle_hit>("reticle_hit/reticle_block");
    }

    public static void CreateReticleHit(string tag)
    {

        reticle_hit instance = null;

        if (tag.Equals("block_player")) instance = GameObject.Instantiate(reticle_block);
        else instance = GameObject.Instantiate(reticle_hit);


        Vector2 screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
    }
}

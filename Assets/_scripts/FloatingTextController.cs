using UnityEngine;
using System.Collections;

public class FloatingTextController : MonoBehaviour
{
    private static FloatingText popupText_head;
    private static FloatingText popupText_torso;
    private static FloatingText popupText_limb;
    private static GameObject canvas;

    public static void Initialize()
    {
        canvas = GameObject.Find("Canvas");
        if (!popupText_head)
            popupText_head = Resources.Load<FloatingText>("k/popup_head_damage");
            popupText_torso = Resources.Load<FloatingText>("k/popup_torso_damage");
            popupText_limb = Resources.Load<FloatingText>("k/popup_limb_damage");
    }

    public static void CreateFloatingText(string text, Vector3 location, string tag)
    {
        //Debug.Log("Creating floating text!" + location);
        FloatingText instance;
        if (tag.Equals("coll_0")) instance = Instantiate(popupText_head);
        else if(tag.Equals("coll_1")) instance = Instantiate(popupText_torso);
        else instance = Instantiate(popupText_limb);

        // Vector2 screenPosition =location;
        Vector2 screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
    }
}
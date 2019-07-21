using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class chatInputHandler : MonoBehaviour
{
    private InputField input;
    private ChatManager manager;
    private void Start()
    {
        input = GetComponent<InputField>();
        manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();//nevem ce dela rekurzivno whatewer
        if (manager == null) manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();
        if (manager == null) manager = GetComponentInParent<ChatManager>();
    }
    void OnGUI()
    {
        if (input.isFocused && input.text != "" && Input.GetKey(KeyCode.Return))
        {
            manager.SendMessage();
        }
    }
}

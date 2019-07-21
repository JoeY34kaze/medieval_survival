using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// v originalu je blo od BeardedManStudios ampak sm kr prepisov z svojo kodo - repurposed
/// </summary>
public class ChatManager : ChatManagerBehavior
{
    public Transform text_panel;
	public GameObject message_prefab;

	public InputField messageInput;

	private List<Text> messages = new List<Text>();


	public override void SendMessage(RpcArgs args)
	{
		string username = args.GetNext<string>();
		string message = args.GetNext<string>();

        GameObject t = GameObject.Instantiate(message_prefab, text_panel);
        t.GetComponent<Text>().text = username + " : " + message;
        //t.transform.SetAsFirstSibling();
        if (text_panel.childCount > 8) {
            //odstran otroke k so vecji od 9

            Destroy(text_panel.GetChild(0).gameObject);
        }

	}

	public void SendMessage()
	{
		string message = messageInput.text.Trim();
		if (string.IsNullOrEmpty(message))
			return;

        string name = transform.root.GetComponent<NetworkPlayerStats>().playerName;//networkObject.Networker.Me.Name;

		if (string.IsNullOrEmpty(name))
			name = NetWorker.InstanceGuid.ToString().Substring(0, 5);

		networkObject.SendRpc(RPC_SEND_MESSAGE, Receivers.All, name, message);
		messageInput.text = "";
        messageInput.DeactivateInputField();
        onInputFielddeselected();

    }

    //prtisnli smo enter
    internal void onInputFieldSelected()
    {
        text_panel.parent.GetComponent<Image>().color = new Color(255, 255, 255, 0.3f);
        messageInput.GetComponent<Image>().color = new Color(255, 255, 255, 1f);
    }

    internal void onInputFielddeselected()
    {
        text_panel.parent.GetComponent<Image>().color = new Color(255, 255, 255, 0.05f);
        messageInput.GetComponent<Image>().color = new Color(255, 255, 255, 0.05f);
        GetComponentInParent<NetworkPlayerMovement>().lockMovement = false;
        GetComponentInParent<player_camera_handler>().lockCamera = false;
        GetComponentInParent<UILogic>().DisableMouse();
    }

}
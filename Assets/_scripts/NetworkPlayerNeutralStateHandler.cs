using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;

public class NetworkPlayerNeutralStateHandler : NetworkPlayerNeutralStateHandlerBehavior
{
    private NetworkPlayerCombatHandler combat_handler;
    private NetworkPlayerAnimationLogic anim_logic;
    private panel_bar_handler bar_handler;
    private NetworkPlayerInventory npi;

    public Transform toolContainerOnHand;
    

    private void Start()
    {
        this.combat_handler = GetComponent<NetworkPlayerCombatHandler>();
        this.anim_logic = GetComponent<NetworkPlayerAnimationLogic>();
        this.bar_handler = GetComponentInChildren<panel_bar_handler>();
        this.npi = GetComponent<NetworkPlayerInventory>();
    }

    private void Update()
    {
        if (networkObject.IsOwner) {
            if (combat_handler.Combat_mode == 0) {//smo v neutralnemu stanju in logiko prevzame naceloma ta skript
                if (bar_handler.gameObject.activeSelf) {
                    checkKeyInputBar();
                    if (Input.GetMouseButtonDown(0)) {
                        if (hasToolSelected()) {
                            ///poslat request da nrdimo swing z tem tool-om
                            networkObject.SendRpc(RPC_TOOL_USAGE_REQUEST, Receivers.Server);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// vrne true ce je izbran tool u roki. neglede kdo klice
    /// </summary>
    /// <returns></returns>
    private bool hasToolSelected()
    {
        foreach (Transform t in this.toolContainerOnHand) {
            if (t.gameObject.activeSelf && t.GetComponent<gathering_tool_collider_handler>() != null) {
                //ce ima to skripto nalimano pomen da je tool
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 1,2,3,4,5,6,7,8,9,0 - tko kot so na tipkovnci!
    /// </summary>
    private void checkKeyInputBar() {
        if (Input.GetButtonDown("Bar1")) localBarSlotSelectionRequest(0);
        if (Input.GetButtonDown("Bar2")) localBarSlotSelectionRequest(1);
        if (Input.GetButtonDown("Bar3")) localBarSlotSelectionRequest(2);
        if (Input.GetButtonDown("Bar4")) localBarSlotSelectionRequest(3);
        if (Input.GetButtonDown("Bar5")) localBarSlotSelectionRequest(4);
        if (Input.GetButtonDown("Bar6")) localBarSlotSelectionRequest(5);
        if (Input.GetButtonDown("Bar7")) localBarSlotSelectionRequest(6);
        if (Input.GetButtonDown("Bar8")) localBarSlotSelectionRequest(7);
        if (Input.GetButtonDown("Bar9")) localBarSlotSelectionRequest(8);
        if (Input.GetButtonDown("Bar0")) localBarSlotSelectionRequest(9);
    }


    internal void NeutralStateSetup()
    {
        Debug.LogError("not implemented yet");
        if (!networkObject.IsOwner) Debug.Log("NeutralStateSetup se klice tudi na clientu! juhu!");
    }

    /// <summary>
    /// disables everything related to this script. fires when player goes into combat state. this means it disables any tool currently in hand, deselects anything currently selected on bar, clears any animations and such
    /// </summary>
    internal void CombatStateSetup()
    {

        bar_handler.setSelectedSlot(-1);
    }

    /// <summary>
    /// index je int tko k je v arrayu in ne na tipkovnci! . poslje request na server da nj mu equipa itam, ki je trenutno na njegovem baru na tem indexu.
    /// </summary>
    /// <param name="index"></param>
    internal void localBarSlotSelectionRequest(int index) {
        if(bar_handler.getSelectedIndex()!=index)
            networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_REQUEST, Receivers.Server, index);
        else
            networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_REQUEST, Receivers.Server, -1);
    }

    public override void BarSlotSelectionRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            if (isRequestValid()) {
                int index = args.GetNext<int>();
                Item i = npi.GetBarItem(index);
                if (i != null)
                {
                    //vrnt mora response. ownerju vrne drugacn response kot drugim so..
                    lock (NetworkManager.Instance.Networker.Players)
                    {
                        NetworkManager.Instance.Networker.IteratePlayers((player) =>
                        {
                            if (player.NetworkId == networkObject.Owner.NetworkId) //passive target
                            {
                                networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.Owner, i.id, index);
                            }
                            else {
                                networkObject.SendRpc(player,RPC_BAR_SLOT_SELECTION_RESPONSE, i.id, -1);
                            }
                        });
                    }
                }
                else {//bar item je null. vrnemo rpc k pove da nj scleara kar ma u handu in scleara na baru. to se nrdi ce prtisne isti bar k ma ze izbran recimo
                    networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE,Receivers.All, -1, -1);
                }
            }
        }
    }

    /// <summary>
    /// security
    /// </summary>
    /// <returns></returns>
    private bool isRequestValid()
    {
        return true;
    }

    public override void BarSlotSelectionResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            int item_id = args.GetNext<int>();
            int index = args.GetNext<int>();
            if (networkObject.IsOwner)
            {
                setSelectedItem(Mapper.instance.getItemById(item_id));
                bar_handler.setSelectedSlot(index);
            }
            else {
                setSelectedItem(Mapper.instance.getItemById(item_id));
            }
        }
    }

    /// <summary>
    /// na podlagi tega itema i, ga nastela u roke playerju
    /// </summary>
    /// <param name="i"></param>
    private void setSelectedItem(Item i) {//item je lahko null
        if(i!=null)Debug.Log("Trying to place " + i.Display_name + " in the hands");
        else Debug.Log("Trying to clear everything currently in the hands");
        gathering_tool_collider_handler temp;
        foreach (Transform child in this.toolContainerOnHand) {
            temp = child.GetComponent<gathering_tool_collider_handler>();
            if (temp.item != null)
            {
                if (i != null)
                {
                    if (temp.item.id == i.id)
                    {
                        child.gameObject.SetActive(true);
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                else {//i == null ->clear all
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// enabla colider na isto foro k za weapon.
    /// </summary>
    private void enableColliderOnTool() {

    }

    /// <summary>
    /// owner poslje request da zamahne z toolom. server mora prevert ce je vse OK  -(legitimnost ukaza) in poslje ukaz vsem da nj izvedejo swing.
    /// </summary>
    /// <param name="args"></param>
    public override void ToolUsageRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            Item i = getActiveTool();
            if (i != null) {
                if (toolCanPerformAction()) {
                    if (isToolUseValid(i)) {
                        networkObject.SendRpc(RPC_TOOL_USAGE_RESPONSE, Receivers.All, i.id);
                    }
                }
            }
        }
    }

    /// <summary>
    /// security
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private bool isToolUseValid(Item i)
    {
        return true;
    }

    private Item getActiveTool()
    {
        foreach (Transform t in this.toolContainerOnHand)
        {
            if (t.gameObject.activeSelf && t.GetComponent<gathering_tool_collider_handler>() != null)
            {
                return t.GetComponent<gathering_tool_collider_handler>().item;
            }
        }
        return null;
    }

    private bool toolCanPerformAction()
    {
        return true;
    }

    public override void ToolUsageResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            int item_id = args.GetNext<int>();
            if (item_id != -1) {
                Item tool_to_use = Mapper.instance.getItemById(item_id);

                GameObject obj = getGameObjectInHandFromId(item_id);
                if (obj == null) Debug.LogError("this should not have happened..");
                ///nastimat v animatorju da zamahne

                //collider
                obj.GetComponent<Collider>().enabled = true;
                if (networkObject.IsServer) { }
            }
        }
    }

    /// <summary>
    /// klice se z animation eventa
    /// </summary>
    public void disableToolCollider() {
        Item i = getActiveTool();
        if (i = null)
        {
            GameObject obj = getGameObjectInHandFromId(i.id);
            if (obj == null) Debug.LogError("this should not have happened..");
            obj.GetComponent<Collider>().enabled = true;
        }
    }


    private GameObject getGameObjectInHandFromId(int item_id)
    {
        foreach (Transform t in this.toolContainerOnHand)
        {
            if (t.gameObject.activeSelf && t.GetComponent<gathering_tool_collider_handler>() != null)
            {
                if (t.GetComponent<gathering_tool_collider_handler>().item.id == item_id) return t.gameObject;
            }
        }
        return null;
    }
}

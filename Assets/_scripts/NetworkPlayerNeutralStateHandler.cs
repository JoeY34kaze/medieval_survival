using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UnityEngine;
using System.Collections.Generic;

public class NetworkPlayerNeutralStateHandler : NetworkPlayerNeutralStateHandlerBehavior
{
    private NetworkPlayerCombatHandler combat_handler;
    private NetworkPlayerAnimationLogic anim_logic;
    private panel_bar_handler bar_handler;
    private NetworkPlayerInventory npi;

    public Transform toolContainerOnHand;

    internal int selected_index = -1;
    internal int selected_index_shield = -1;


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
           // if (combat_handler.Combat_mode == 0) {//smo v neutralnemu stanju in logiko prevzame naceloma ta skript
                if (bar_handler.gameObject.activeSelf) {
                    checkInputBar();
                    if (Input.GetButtonDown("Fire1")) {
                        if (hasToolSelected())//za weapone se checkira v combat handlerju
                        {
                            ///poslat request da nrdimo swing z tem tool-om
                            networkObject.SendRpc(RPC_TOOL_USAGE_REQUEST, Receivers.Server);
                        }
                    }
                }
           // }
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
    private void checkInputBar() {
        if (Input.GetButtonDown("Bar1")) localBarSlotSelectionRequest(0);
        else if (Input.GetButtonDown("Bar2")) localBarSlotSelectionRequest(1);
        else if (Input.GetButtonDown("Bar3")) localBarSlotSelectionRequest(2);
        else if (Input.GetButtonDown("Bar4")) localBarSlotSelectionRequest(3);
        else if (Input.GetButtonDown("Bar5")) localBarSlotSelectionRequest(4);
        else if (Input.GetButtonDown("Bar6")) localBarSlotSelectionRequest(5);
        else if (Input.GetButtonDown("Bar7")) localBarSlotSelectionRequest(6);
        else if (Input.GetButtonDown("Bar8")) localBarSlotSelectionRequest(7);
        else if (Input.GetButtonDown("Bar9")) localBarSlotSelectionRequest(8);
        else if (Input.GetButtonDown("Bar0")) localBarSlotSelectionRequest(9);
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            //TODO: spisat kodo da manja weapon in ignorira slot ce je gor shield. nocmo da nam med fajtom zamenja shield
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            //TODO
            //spisat kodo da manja weapon in ignorira slot ce je gor shield. nocmo da nam med fajtom zamenja shield
        }
    }


    internal void NeutralStateSetup()
    {
        Debug.Log("not implemented yet");
        if (!networkObject.IsOwner) Debug.Log("NeutralStateSetup se klice tudi na clientu! juhu!");
    }

    /// <summary>
    /// handles any changes needed for going into combat state
    /// </summary>
    internal void CombatStateSetup()
    {

    }

    /// <summary>
    /// index je int tko k je v arrayu in ne na tipkovnci! . poslje request na server da nj mu equipa itam, ki je trenutno na njegovem baru na tem indexu.
    /// </summary>
    /// <param name="index"></param>
    internal void localBarSlotSelectionRequest(int index) {
        networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_REQUEST, Receivers.Server, index);

    }

    /// <summary>
    /// wrapper ki preprecu null exception pri item.id
    /// </summary>
    /// <returns></returns>
    private int getBarItemIdFromIndex(int id) {
        Predmet k = npi.getBarPredmet(id);
        if (k == null) return -1;
        else return k.item.id;
    }

    private Item.Type getBarItemTypeFromIndex(int id)
    {
        Predmet k = npi.getBarPredmet(id);
        if (k == null) return Item.Type.chest;
        else return k.item.type;
    }

    public override void BarSlotSelectionRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            if (isRequestValid()) {
                int index = args.GetNext<int>();
                
                Predmet i = npi.getBarPredmet(index);
                if (i != null)
                {

                    //ce smo dobil id shielda in nimamo equipanga shielda samo equipamo shield
                    if (getBarItemTypeFromIndex(index) == Item.Type.shield && this.selected_index_shield == -1)
                    {
                        this.selected_index_shield = index;
                    }

                    //ce mamo izbran shield in smo dobil index shielda mormo disablat samo shield
                    else if (this.selected_index_shield == index)
                    {
                        this.selected_index_shield = -1;
                    }
                    //ce mamo izbran shield in smo dobil index druzga shielda mormo zamenjat shield
                    else if (this.selected_index_shield != -1 && i.item.type == Item.Type.shield)
                    {
                        this.selected_index_shield = index;
                    }//dobil smo ukaz da nj damo weapon stran
                    else if (this.selected_index_shield != -1 && this.selected_index == index)
                    {
                        this.selected_index = -1;
                    }
                    //ce mamo izbran weapon in shield in smo dobil nov weapon mormo zamenjat weapon
                    else if (this.selected_index_shield != -1 && getBarItemTypeFromIndex(this.selected_index) == Item.Type.weapon && i.item.type == Item.Type.weapon)
                    {
                        this.selected_index = index;
                    }
                    //ce smo dobil karkoli druzga mormo disablat shield in karkoli smo mel in poslat samo tisto
                    else if (this.selected_index == index) {
                        this.selected_index = -1;
                    }
                    else
                    {

                        this.selected_index = index;
                    }

                }
                else {
                    this.selected_index = -1;
                    //this.selected_index_shield = -1;
                }

                //tle posljemo zdej rpc
                //TODO: ownerju poslat druugacn rpc kot drugim, drugi nebi smel vidt indexa ker je to slaba stvar - ESP
                networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.All, (this.selected_index==-1)?"-1" : npi.hotbar_objects[this.selected_index].toNetworkString(), this.selected_index, (this.selected_index_shield == -1) ? "-1" : npi.hotbar_objects[this.selected_index_shield].toNetworkString(), selected_index_shield);
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
            string predmet1 = args.GetNext<string>();
            int index = args.GetNext<int>();

            string predmet2 = args.GetNext<string>();//za shield rabmo met 2 indexa in tko
            int index2 = args.GetNext<int>();

            Debug.Log("bar update - " + index);
            if (networkObject.IsOwner)
            {
                setSelectedItems(Predmet.createNewPredmet(predmet1), Predmet.createNewPredmet(predmet2));
                bar_handler.setSelectedSlots(index,index2);
            }
            else {
                setSelectedItems(Predmet.createNewPredmet(predmet1), Predmet.createNewPredmet(predmet2));
            }

            //ZA COMBAT MODE - precej neefektivno ker pri menjavi itema na baru se klice dvakrat rpc...........
            if(combat_handler.currently_equipped_weapon!=null)
                combat_handler.ChangeCombatMode(combat_handler.currently_equipped_weapon.item);
            
        }
    }

    /// <summary>
    /// na podlagi tega itema i, ga nastela u roke playerju
    /// </summary>
    /// <param name="i"></param>
    private void setSelectedItems(Predmet i, Predmet shield) {//item je lahko null
        if(i!=null)Debug.Log("Trying to place " + i.item.Display_name + " in the hands");
        else Debug.Log("Trying to clear everything currently in the hands");
        if (i != null)
        {
            if (i.item.type == Item.Type.tool) SetToolSelected(i);
            else if (i.item.type == Item.Type.weapon || i.item.type == Item.Type.ranged)
            {
                combat_handler.currently_equipped_weapon = i;
            }
            else Debug.Log("item youre trying to equip cannot be equipped : " + i.item.Display_name);
        }
        else {//clearat vse razen shielda ce je slucajn equipan - bom vrgu u combat handler pa nj se tam jebe
            SetToolSelected(i);
            combat_handler.currently_equipped_weapon = null;
        }

        if (shield != null)
            combat_handler.currently_equipped_shield = shield;
        else 
            combat_handler.currently_equipped_shield = null;
        combat_handler.update_equipped_weapons();//weapon in shield
    }

    private void SetToolSelected(Predmet i) {
        gathering_tool_collider_handler temp;
        foreach (Transform child in this.toolContainerOnHand)
        {
            temp = child.GetComponent<gathering_tool_collider_handler>();
            if (temp.item != null)
            {
                if (i != null)
                {
                    if (temp.item.id == i.item.id)
                    {
                        child.gameObject.SetActive(true);
                    }
                    else
                    {
                        child.gameObject.SetActive(false);
                    }
                }
                else
                {//i == null ->clear all
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

                //nared animacijo
                anim_logic.startToolAction(tool_to_use);
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

    internal bool isNotSelected(int a, int b)
    {

        
        if (((a == this.selected_index_shield || a == this.selected_index) && a!=-1) || ((b == this.selected_index_shield || b == this.selected_index) && b!=-1)) return false;
        return true;
    }

    internal void ClearActiveWeapons()
    {
        if (networkObject.IsServer) {
            this.selected_index = -1;
            this.selected_index_shield = -1;
            networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.All, "-1", -1, "-1", -1);
        }
    }
    private GameObject getCurrentTool() {
        foreach (Transform t in this.toolContainerOnHand)
        {
            if (t.gameObject.activeSelf && t.GetComponent<gathering_tool_collider_handler>() != null)
            {
                 return t.gameObject;
            }
        }
        return null;


    }

    /// <summary>
    /// klice animation event v layer movement na animaciji za uporabo toolov kot so kramp, sekira, in podobno kar rabi collider
    /// </summary>
    public void OnToolSwingStart() {
        getCurrentTool().GetComponent<Collider>().enabled = true;
    }
    /// <summary>
    /// klice animation event v layer movement na animaciji za uporabo toolov kot so kramp, sekira, in podobno kar rabi collider
    /// </summary>
    public void OnToolSwingEnd() {
        getCurrentTool().GetComponent<Collider>().enabled = false;
    }
}

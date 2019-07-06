using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBackpack : NetworkBackpackBehavior
{
    // Start is called before the first frame update
    private NetworkContainer_items nci;
    private int owner_id = -1;
    private GameObject owning_player = null;
    private NetworkPlayerInventory npi = null;
    private Rigidbody r;
    private backpack_local_panel_handler panel_handler;
    void Start()
    {
        nci = GetComponent<NetworkContainer_items>();
        if (networkObject.IsServer) {
            nci.init(Mapper.instance.getItemById(GetComponent<identifier_helper>().id).size);
        }
            
        r = GetComponent<Rigidbody>();
    }

    public bool hasSpace() {
        if (this.nci.getEmptySpace() > 0)
            return true;
        else return false;
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    public void sendItemsUpdate() {
        if (networkObject.IsServer) {
            networkObject.SendRpc(networkObject.Owner, RPC_BACKPACK_ITEMS_OWNER_RESPONSE, nci.getItemsNetwork());
        }
    }

    //------------------------------LOKALNI KLICI ZA INTERAKCIJO------------------
    internal void local_player_equip_request()
    {
        networkObject.SendRpc(RPC_BACKPACK_INTERACTION_REQUEST, Receivers.Server, (byte)0);
    }

    internal void local_player_look_request()
    {
        networkObject.SendRpc(RPC_BACKPACK_INTERACTION_REQUEST, Receivers.Server, (byte)1);
    }

    public void local_player_backpack_unequip_request() {
        networkObject.SendRpc(RPC_BACKPACK_UNEQUIP_REQUEST,Receivers.Server);
    }
    internal void localPlayerdropItemFromBackpackRequest(int v)
    {
        Camera c = Camera.main;
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_BACKPACK_DROP_ITEM_REQUEST, Receivers.Server, v, c.transform.position + (c.transform.forward * 3), c.transform.forward);
    }

    //----------------------------RPC's----------------------------------------

    public override void BackpackInteractionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
       

        byte tip = args.GetNext<byte>();
        uint player_id = args.Info.SendingPlayer.NetworkId;
        if (tip == 0 && this.owner_id == -1)//equip request
        {


            //poglej ce ze ima equipan backpack
            GameObject player = FindByid(player_id);
            if (player.GetComponentInChildren<NetworkBackpack>() == null)
            {
                //lahko pobere
                //networkObject.AssignOwnership(args.Info.SendingPlayer);
                NetworkPlayerInventory n = player.GetComponent<NetworkPlayerInventory>();
                n.SetLoadoutItem(Mapper.instance.getItemById(GetComponent<identifier_helper>().id), 0);//to nrdi samo server..
                n.sendNetworkUpdate(false, true);
                sendOwnershipResponse(args.Info.SendingPlayer);
            }
        }
        else if (tip == 1 && (networkObject.IsOwner) &&(this.owner_id!=0))
        { //look request. ce je od serverja
            //lahko pogleda i guess.
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_BACKPACK_ITEMS_OTHER_RESPONSE, nci.getItemsNetwork());
        }
        else if (tip == 1 && (networkObject.Owner.NetworkId == args.Info.SendingPlayer.NetworkId)) {//look request od ownerja. pomen da je odpru inventorij al ravnokar dubu ownership response in rab pohendlat panelo
            networkObject.SendRpc(networkObject.Owner, RPC_BACKPACK_ITEMS_OWNER_RESPONSE, nci.getItemsNetwork());
        }
    }



    private void sendItemsResponse(NetworkingPlayer sendingPlayer)
    {
        if(networkObject.IsServer)
            networkObject.SendRpc(sendingPlayer, RPC_BACKPACK_ITEMS_OTHER_RESPONSE, this.nci.getItemsNetwork());
    }

    private void sendOwnershipResponse(NetworkingPlayer sendingPlayer)
    {
        if (networkObject.IsServer)
        {
            networkObject.AssignOwnership(sendingPlayer);
            networkObject.SendRpc(RPC_OWNERSHIP_CHANGE_RESPONSE, Receivers.All, sendingPlayer.NetworkId);
        }
    }
    /// <summary>
    /// poslje client, any client lahko ma na hrbtu ali pa gleda ko je na tleh, dobi server, server vrne novo stanje itemov
    /// </summary>
    /// <param name="args"></param>
    public override void BackpackDropItemRequest(RpcArgs args)
    {
        //poslat kamero!
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            Item i = this.nci.popItem(args.GetNext<int>());
            this.npi.instantiateDroppedItem(i, 1, args.GetNext<Vector3>(), args.GetNext<Vector3>());
            sendItemsUpdate();
        }

    }
    public override void BackpackItemsOwnerResponse(RpcArgs args)//tole dobi owner, ponavad ko odpre inventorij al pa kj tazga
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        Item[] serverjevi_itemi = nci.parseItemsNetworkFormat(args.GetNext<string>());
        nci.setAll(serverjevi_itemi);
        //izrisat iteme k jih mamo tle u arrayu na panele.

        this.panel_handler.updateUI();//tole je za tvoj inventorij ko imas equippan
    }

    public override void BackpackItemsOtherResponse(RpcArgs args)//odpre backpack da ga lahko gleda kot nek chest in pobira iteme vn
    {
         if (args.Info.SendingPlayer.NetworkId != 0) return;

        //odpret panel ce nima se odprtga

        Item[] serverjevi_itemi = nci.parseItemsNetworkFormat(args.GetNext<string>());
        nci.setAll(serverjevi_itemi);
        //izrisat iteme k jih mamo tle u arrayu na panele.
        //this.panel_handler.updateUI();//tole je za tvoj inventorij ko imas equippan

    }

    /// <summary>
    /// zamenja pozicije itemov znotraj inventorija
    /// </summary>
    /// <param name="args"></param>
    public override void BackpackSwapItemsRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            this.nci.swap(args.GetNext<int>(),args.GetNext<int>());
            sendItemsUpdate();
        }
    }
    /// <summary>
    /// inv->back & back -> inv
    /// </summary>
    /// <param name="args"></param>
    public override void InventoryToBackpackSwapRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            int back_index = args.GetNext<int>();
            int inv_index = args.GetNext<int>();

            Item b = this.nci.popItem(back_index);
            Item i = this.npi.popPersonalItem(inv_index);

            this.npi.setPersonalItem(b, inv_index);
            this.nci.setItem(back_index, i);

            sendItemsUpdate();
            this.npi.sendNetworkUpdate(true, false);
        }
    }

    public override void LoadoutToBackpackRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)
        {
            bool changed = false;
            string type = args.GetNext<string>();
            Item.Type t = this.npi.getItemTypefromString(type);
            int loadout_index = args.GetNext<int>();
            int back_index = args.GetNext<int>();

            //ce je backpack null ga samo not dej, ce je occupied probej nrdit swap, sicer nared nč
            if (this.nci.getItem(back_index) != null)
            {//swap
                if (this.npi.GetItemLoadout(this.npi.getItemTypefromString(type), loadout_index).type == this.nci.getItem(back_index).type)
                {//ce se itema ujemata, sicer nima smisla
                    Item l = this.npi.PopItemLoadout(t, loadout_index);
                    Item b = this.nci.popItem(back_index);
                    this.nci.setItem(back_index, l);
                    this.npi.SetLoadoutItem(b, loadout_index);
                    changed = true;
                }
            }
            else
            {
                this.nci.setItem(back_index, this.npi.PopItemLoadout(this.npi.getItemTypefromString(type), loadout_index));//backpack slot je null tko da je vse kul.
                changed = true;
            }

            if (changed) {//ker smo stvari spremenil rabmo sinhronizirat loadout in backpack.

                if (this.npi.current_equipped_weapon_was_removed(t, loadout_index))
                {
                    //set current weapon to first non empty.
                    this.npi.combatHandler.setCurrentWeaponToFirstNotEmpty();
                }

                this.sendItemsUpdate();
                this.npi.sendNetworkUpdate(false, true);
            }
        }
    }

    public override void OwnershipChangeResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        uint player_id = args.GetNext<uint>();
        this.owner_id = (int)player_id;
        this.owning_player = FindByid(player_id);
        //poisc playerja in se mu prlimej na hrbet
        this.npi = this.owning_player.GetComponent<NetworkPlayerInventory>();
        this.npi.backpack_inventory = this;
        Item item =Mapper.instance.getItemById(GetComponent<identifier_helper>().id);
        
        Transform transformForBackpack = this.npi.backpackSpot;
        //this.panel_handler = this.npi.backpackPanel;
        if(r==null) r = GetComponent<Rigidbody>();
        
        if (r.detectCollisions) r.detectCollisions = false;
        if (!r.isKinematic) r.isKinematic = true;

        this.transform.SetParent(transformForBackpack);
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        npi.requestUiUpdate();//najbrz overkill k itak posle redraw zmer k odpres inventorij ampak za zacetk je ok - da izrise backpack na svoj slot u loadoutu
        
        //nastimat je treba tud panel za backpack slote.

        

        if (networkObject.IsOwner)
        {
            this.panel_handler = this.npi.backpackPanel;
            this.panel_handler.init(item.size, this.nci);//nastav samo slote
            networkObject.SendRpc(RPC_BACKPACK_INTERACTION_REQUEST, Receivers.Server, (byte)1, networkObject.Owner.NetworkId);//posle request da mu updejta iteme
        }
    }

    public override void BackpackUnequipRequest(RpcArgs args)
    {
        if (networkObject.IsServer && (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId )) {//ce je server in ce je poslov owner
            networkObject.TakeOwnership();//server vzame ownership
            networkObject.SendRpc(RPC_BACKPACK_UNEQUIP_RESPONSE, Receivers.All);

        }
    }
    //klice server
    public void local_server_BackpackUnequip() {
        if (networkObject.IsServer) {
            networkObject.TakeOwnership();//server vzame ownership
            networkObject.SendRpc(RPC_BACKPACK_UNEQUIP_RESPONSE, Receivers.All);
        }
    }

    public override void BackpackUnequipResponse(RpcArgs args)
    {
        //ce dobimo tole zmer pomen da mora owner unequipat
        if (args.Info.SendingPlayer.NetworkId == 0) {
            transform.SetParent(null);
            this.owner_id = -1;
            this.owning_player = null;
            this.npi.RemoveItemLoadout(Item.Type.backpack, 0);
            //nastimat nek force da ga nekam vrze? mybe. kodo ze mamo u instantiationu itemov
            if (r.isKinematic) r.isKinematic = false;
            if (!r.detectCollisions) r.detectCollisions = true;
            if(networkObject.IsOwner)
                this.panel_handler.clear();
            npi.requestUiUpdate();//najbrz overkill k itak posle redraw zmer k odpres inventorij ampak za zacetk je ok
            this.npi.backpack_inventory = null;
            this.npi = null;
            this.panel_handler = null;
        }
    }



    internal void putFirst(Item resp, int quantity)
    {
        this.nci.putFirst(resp,quantity);

    }

    public override void BackpackToLoadoutRequest(RpcArgs args)
    {
        if (networkObject.IsServer && (args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId)) {//ce je server in ce je poslov owner
            //dob backapack item
            int backpack_index = args.GetNext<int>();
            Item i = nci.getItem(backpack_index);
            //dob laodout item
            Item load;
            int weaponIndex = 0;
            if (i.type == Item.Type.weapon)
            {
                if (npi.GetWeapon0() != 0 && npi.GetWeapon1() == 0)//ce ima weapon u main handu pa nima u off handu ga damo v offhand
                    weaponIndex = 1;
            }
            
                load = this.npi.PopItemLoadout(i.type,weaponIndex);


            //nared swap

            if (this.npi.try_to_upgrade_loadout(i) != null) Debug.LogError("pri backapck swapu ni null, to ni mogoce. fix it");

            this.nci.setItem(backpack_index, load);



            //poslat loadout. loadout u celotiu pohandla networkPlayerInventory, medtem ko backapack pohendlamo tle.
            this.npi.sendNetworkUpdate(false, true);
            networkObject.SendRpc(networkObject.Owner, RPC_BACKPACK_ITEMS_OWNER_RESPONSE, nci.getItemsNetwork());
        }
    }

    internal void localPlayerRequestBackpackToLoadout(int index_backpack)//za obratno vbom naredu nov rpc ker bom poslov string k j eitem type
    {
        if (networkObject.IsOwner) {
            networkObject.SendRpc(RPC_BACKPACK_TO_LOADOUT_REQUEST, Receivers.Server, index_backpack);
        }
    }

    internal void localPlayerLoadoutToBackpackRequest(string loadout_type, int weapon_index, int backpack_index)
    {
        if (networkObject.IsOwner) {
            networkObject.SendRpc(RPC_LOADOUT_TO_BACKPACK_REQUEST, Receivers.Server, loadout_type,weapon_index,backpack_index);
        }
    }

    internal void localPlayerInventorySwapRequest(int backpack_index, int inv_index)
    {
        if (networkObject.IsOwner) {
            networkObject.SendRpc(RPC_INVENTORY_TO_BACKPACK_SWAP_REQUEST, Receivers.Server, backpack_index, inv_index);
        }
    }

    internal void localPlayerBackpackToBackpackRequest(int index1, int index2)
    {
        if (networkObject.IsOwner) {
            networkObject.SendRpc(RPC_BACKPACK_SWAP_ITEMS_REQUEST, Receivers.Server, index1, index2);
        }
    }
}

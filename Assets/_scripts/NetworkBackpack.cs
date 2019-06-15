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
        if (networkObject.IsServer)
            nci.init(20);
        r = GetComponent<Rigidbody>();
    }


    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
            if (p.GetComponent<NetworkPlayerStats>().server_id == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    //------------------------------LOKALNI KLICI ZA INTERAKCIJO------------------
    internal void local_player_equip_request(uint server_id)
    {
        networkObject.SendRpc(RPC_BACKPACK_INTERACTION_REQUEST, Receivers.Server, (byte)0, server_id);
    }

    internal void local_player_look_request(uint server_id)
    {
        networkObject.SendRpc(RPC_BACKPACK_INTERACTION_REQUEST, Receivers.Server, (byte)1,server_id);
    }

    public void local_player_unequip_request() {
        networkObject.SendRpc(RPC_BACKPACK_UNEQUIP_REQUEST,Receivers.Server);
    }

    //----------------------------RPC's----------------------------------------

    public override void BackpackInteractionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
       

        byte tip = args.GetNext<byte>();
        uint player_id = args.GetNext<uint>();
        if (tip == 0 && this.owner_id == -1)//equip request
        {


            //poglej ce ze ima equipan backpack
            GameObject player = FindByid(player_id);
            if (player.GetComponentInChildren<NetworkBackpack>() == null) {
                //lahko pobere
                sendOwnershipResponse(args.Info.SendingPlayer);
            }
        }
        else if(tip == 1 && (networkObject.IsOwner || networkObject.Owner.NetworkId==args.Info.SendingPlayer.NetworkId)){ //look request. ce je ali od serverja item ali od tega playerja k je poslov rpc
            //lahko pogleda i guess.
            sendItemsResponse(args.Info.SendingPlayer);
        }
    }

    private void sendItemsResponse(NetworkingPlayer sendingPlayer)
    {
        networkObject.SendRpc(sendingPlayer, RPC_BACKPACK_ITEMS_RESPONSE, this.nci.getItemsNetwork());
    }

    private void sendOwnershipResponse(NetworkingPlayer sendingPlayer)
    {
        networkObject.AssignOwnership(sendingPlayer);
        networkObject.SendRpc(RPC_OWNERSHIP_CHANGE_RESPONSE, Receivers.All, sendingPlayer.NetworkId);
    }
    /// <summary>
    /// poslje client, any client lahko ma na hrbtu ali pa gleda ko je na tleh, dobi server, server vrne novo stanje itemov
    /// </summary>
    /// <param name="args"></param>
    public override void BackpackDropItemRequest(RpcArgs args)
    {

    }

    public override void BackpackItemsResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        //odpret panel ce nima se odprtga

        Item[] serverjevi_itemi = nci.parseItemsNetworkFormat(args.GetNext<string>());
        nci.setAll(serverjevi_itemi);
        //izrisat iteme k jih mamo tle u arrayu na panele.
        this.panel_handler.updateUI();

    }

    public override void BackpackSwapItemsRequest(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    public override void InventoryToBackpackSwapRequest(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    public override void LoadoutToBackpackSwapRequest(RpcArgs args)
    {
        throw new System.NotImplementedException();
    }

    public override void OwnershipChangeResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return;

        uint player_id = args.GetNext<uint>();
        this.owner_id = (int)player_id;
        this.owning_player = FindByid(player_id);
        //poisc playerja in se mu prlimej na hrbet
        this.npi = this.owning_player.GetComponent<NetworkPlayerInventory>();
        this.npi.SetLoadoutItem(Mapper.instance.getItemById(GetComponent<identifier_helper>().id),0);
        Transform transformForBackpack = this.npi.backpackSpot;
        this.panel_handler = this.owning_player.GetComponentInChildren<backpack_local_panel_handler>();
        if (!r.isKinematic) r.isKinematic = true;
        if (r.detectCollisions) r.detectCollisions = false;

        this.transform.SetParent(transformForBackpack);
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        npi.requestUiUpdate();//najbrz overkill k itak posle redraw zmer k odpres inventorij ampak za zacetk je ok
    }

    public override void BackpackUnequipRequest(RpcArgs args)
    {
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {//ce je server in ce je poslov owner
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
            npi.requestUiUpdate();//najbrz overkill k itak posle redraw zmer k odpres inventorij ampak za zacetk je ok
            this.npi = null;
            this.panel_handler = null;
        }
    }
}

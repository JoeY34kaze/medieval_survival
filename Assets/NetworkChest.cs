using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vse kar ma veze z chestom in ne pase u superclass placeabla
/// </summary>
/// 

    //---------------------------------------------------------networkObject.owner bi mogu bit server!----------
public class NetworkChest : NetworkContainer
{
    private Predmet p;

    private NetworkContainer_items nci;

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {

            //ce je client nj zahteva od serverja
        }
    }


        public void interact() {
        Debug.Log("Interacting. this is it");
    }

    //nekak se mora klicat da se nastimajo parametri ob postavitvi
    public void init(Predmet p)
    {
        this.nci = GetComponent<NetworkContainer_items>();
        this.p = p;

        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
            this.nci = GetComponent<NetworkContainer_items>();
            if (this.p != null)
                this.nci.init(p.item.backpack_capacity);
            else this.nci.init(30);
        }
    }

    /// <summary>
    /// player (se da nekak zvodat id z networkinga ampak nerabmo. server vid takoj id) proba pobrat ta chest
    /// </summary>
    internal void local_chest_pickup_request()
    {

        networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
    }

    internal void local_chest_open_request()
    {
        if (networkObject.IsOwner)
            networkObject.SendRpc(RPC_OPEN_REQUEST, Receivers.Server);
    }

    public override void pickupRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            NetworkingPlayer requester_networkPlayer = args.Info.SendingPlayer;
            GameObject requester_gameObject= FindByid(requester_networkPlayer.NetworkId);

            //----------checks for security and valididty
            if (!this.nci.isEmpty()) return;

            //prevert se ce je owner, ali ma privilegij za pobiranje pa take fore odvisn od guilda. sam zaenkrat guildi se nimajo influenca
            //------------------------
            requester_gameObject.GetComponent<NetworkPlayerInventory>().handleItemPickup(p);
            Destroy(this.gameObject);

        }
    }

    public override void openRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            //nekej securityja pa autorizacije rabmo ko bomo mel guilde pa tak

            if (isPlayerAuthorizedToOpen(args.Info.SendingPlayer.NetworkId))
            {
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());//ta metoda se klice tudi v vsakmu tipu requesta za manipulacijo z itemi
            }
            else {//fail, send fail response. pr rust bi ga kljucavnca shokirala recimo
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 0, "-1");
            }
        }
    }

    private bool isPlayerAuthorizedToOpen(uint networkId)
    {
        Debug.LogWarning("no security");
        return true;
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
         //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }

    /// <summary>
    /// od serverja dobi podatke o itemih k so u chestu.
    /// </summary>
    /// <param name="args"></param>
    public override void openResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0) {
            if (args.GetNext<int>() == 1)
            {
                Predmet[] predmeti = this.nci.parseItemsNetworkFormat(args.GetNext<string>());
                FindByid(networkObject.Networker.Me.NetworkId).GetComponent<NetworkPlayerInventory>().onChestOpen(this,predmeti);
            }
            else {//fail - nismo authorized al pa kej tazga
                FindByid(networkObject.Networker.Me.NetworkId).GetComponentInChildren<UILogic>().clear();//da se miska zbrise
            }
        }
    }


    #region PREMIKANJE

    //lokalni klici 
    internal override void localRequestPersonalToContainer(int indexFrom, int indexTo) {
        networkObject.SendRpc(RPC_PERSONAL_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToPersonal(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_PERSONAL, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestBackpackToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_BACKPACK_TO_CONTAINER,Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToBackpack(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_BACKPACK, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestBarToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_BAR_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToBar(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_BAR, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestLoadoutToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_LOADOUT_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToLoadout(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_LOADOUT_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestDropItemContainer(int v)
    {
        networkObject.SendRpc(RPC_DROP_ITEM, Receivers.Server, v);
    }

    //  RPCJI NA SERVERJU


    /// <summary>
    /// nekka je napisan tud za desni klik
    /// </summary>
    /// <param name="args"></param>
    public override void PersonalToContainer(RpcArgs args)
    {
        if (networkObject.IsServer) {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            //nrdit mormo swap
            int personalIndex = args.GetNext<int>();
            int containerIndex = args.GetNext<int>();

            if (this.nci.predmeti.Length > (containerIndex + 1) && containerIndex > -1)
            {//to bi mogl zmer bit true btw
                Predmet c = this.nci.predmeti[containerIndex];
                this.nci.predmeti[containerIndex] = requester_npi.predmeti_personal[personalIndex];
                requester_npi.predmeti_personal[personalIndex] = c;
            }
            else {//desni klik - index == -1
                if (this.nci.hasSpace())
                {
                    this.nci.putFirst(requester_npi.predmeti_personal[personalIndex]);
                    requester_npi.predmeti_personal[personalIndex] = null;
                }
                else {
                    // ni placa
                }
            }
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.sendNetworkUpdate(true, false);
        }
    }

    public override void ContainerToPersonal(RpcArgs args)//desni klik ne pride v postev ker se nemore sprozit. also desni klik na container pomen da proba dat in na loadout, hotbar, in povsod. ubistvu HandleItemPickup, z checkom prej ce ima plac za pobrat.
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            //nrdit mormo swap
            int containerIndex = args.GetNext<int>();
            int personalIndex = args.GetNext<int>();
            

           
            //to bi mogl zmer bit true btw
                Predmet c = this.nci.predmeti[containerIndex];
                this.nci.predmeti[containerIndex] = requester_npi.predmeti_personal[personalIndex];
                requester_npi.predmeti_personal[personalIndex] = c;
            

            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.sendNetworkUpdate(true, false);
        }
    }

    public override void BarToContainer(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            //nrdit mormo swap
            int bar_index = args.GetNext<int>();
            int containerIndex = args.GetNext<int>();


            if (containerIndex > -1)
            {
                Predmet c = this.nci.predmeti[containerIndex];
                this.nci.predmeti[containerIndex] = requester_npi.popBarPredmet(bar_index);
                requester_npi.setBarPredmet(c, bar_index);
            }
            else {//desni klik
                if (this.nci.hasSpace())
                {
                    this.nci.putFirst(requester_npi.popBarPredmet(bar_index));
                    requester_npi.setBarPredmet(null, bar_index);
                }
            }
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.sendNetworkUpdate(true, false);
        }
    }

    public override void ContainerToBar(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            //nrdit mormo swap
            int containerIndex = args.GetNext<int>();
            int bar_index = args.GetNext<int>();
            



                Predmet c = this.nci.predmeti[containerIndex];
                this.nci.predmeti[containerIndex] = requester_npi.popBarPredmet(bar_index);
                requester_npi.setBarPredmet(c, bar_index);
            
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.sendNetworkUpdate(true, false);
        }
    }

    public override void BackpackToContainer(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            //nrdit mormo swap
            if (requester_npi.backpack == null) return;
            int backpack_index = args.GetNext<int>();
            int containerIndex = args.GetNext<int>();

            if (this.nci.predmeti.Length > (containerIndex + 1) && containerIndex > -1)
            {//to bi mogl zmer bit true btw
                Predmet c = this.nci.predmeti[containerIndex];
                this.nci.predmeti[containerIndex] = requester_npi.backpack_inventory.nci.predmeti[backpack_index];
                requester_npi.backpack_inventory.nci.predmeti[backpack_index] = c;
            }
            else
            {//desni klik - index == -1
                if (this.nci.hasSpace())
                {
                    this.nci.putFirst(requester_npi.backpack_inventory.nci.predmeti[backpack_index]);
                    requester_npi.backpack_inventory.nci.predmeti[backpack_index] = null;
                }
                else
                {
                    // ni placa
                }
            }
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.backpack_inventory.sendBackpackItemsUpdate();
        }
    }

    public override void ContainerToBackpack(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requester_npi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            if (requester_npi.backpack == null) return;
            //nrdit mormo swap
            int containerIndex = args.GetNext<int>();
            int backpack_index = args.GetNext<int>();



         
                Predmet c = this.nci.predmeti[containerIndex];
            this.nci.predmeti[containerIndex] = requester_npi.backpack_inventory.nci.predmeti[backpack_index];
                requester_npi.backpack_inventory.nci.predmeti[backpack_index] = c;
            

            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
            //poslat update za personal inventory
            requester_npi.backpack_inventory.sendBackpackItemsUpdate();
        }
    }

    public override void LoadoutToContainer(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToLoadout(RpcArgs args)
    {
        throw new NotImplementedException();
    }

    public override void ContainerToContainer(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            this.nci.swap(args.GetNext<int>(), args.GetNext<int>());
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
        }
    }

    /// <summary>
    /// dropa item direkt. ce je container tak da se kej crafta not ( pecica) je treba nekak pohendlat interruption, magar tko kot je zrihtan player chafting v npi
    /// </summary>
    /// <param name="args"></param>
    public override void dropItem(RpcArgs args)
    {
        if (networkObject.IsServer) {
            Predmet p = this.nci.popPredmet(args.GetNext<int>());
            FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerInventory>().instantiateDroppedPredmet(p);
        }
    }

    #endregion
}

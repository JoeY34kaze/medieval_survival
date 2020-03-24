using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// moj class k extenda base NetworkContainerBehaviour class on forge networkinga. extendan je zato ker nesmemo spreminjat kode od networkinga ker se avtomatsko generira in bi nam posral / zbrisal kodo
/// </summary>
public class NetworkContainer : NetworkContainerBehavior
{
    protected NetworkContainer_items nci;
    protected Predmet p;


    internal bool Remove(Item i, int amount)
    {
        if (this.nci.containsAmount(i, amount))
        {
            return this.nci.Remove(i, amount);

        }
        return false;
    }


    #region RPC

    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            //ce je client nj zahteva od serverja
        }
    }

    internal Predmet[] get_container_inventory() {
        if (networkObject.IsServer)
            return this.nci.predmeti;
        return null;
    }

    internal bool try_to_add_predmet(Predmet p) {
        if (this.nci.hasSpace(p))
            return this.nci.Add(p);
        return false;
        
    }


    public void interact()
    {
        Debug.Log("Interacting. this is it");
    }

    //nekak se mora klicat da se nastimajo parametri ob postavitvi
    public void init(Predmet p)
    {
        this.nci = GetComponent<NetworkContainer_items>();
        if (this.nci == null) throw new Exception("cannot find nci");
        this.p = p;

        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
            this.nci = GetComponent<NetworkContainer_items>();
            if (this.p != null)
                this.nci.init(p.item.capacity);
            else throw new Exception("Predmet not found!");
        }
    }

    /// <summary>
    /// player (se da nekak zvodat id z networkinga ampak nerabmo. server vid takoj id) proba pobrat ta container
    /// </summary>
    internal void local_container_pickup_request()
    {
        networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
    }

    internal  void local_open_container_request()
    {
        UILogic.Instance.allows_UI_opening = true;
        UILogic.Instance.currently_openened_container = this;
        networkObject.SendRpc(RPC_OPEN_REQUEST, Receivers.Server);
    }

    public override void pickupRequest(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            NetworkingPlayer requester_networkPlayer = args.Info.SendingPlayer;
            GameObject requester_gameObject = FindByid(requester_networkPlayer.NetworkId);

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
        if (networkObject.IsServer)
        {
            //nekej securityja pa autorizacije rabmo ko bomo mel guilde pa tak

            if (isPlayerAuthorizedToOpen(args.Info.SendingPlayer.NetworkId))
            {
                if (this.is_with_upkeep()) {
                    this.send_rpc_response_with_upkeep_cost(args);
                }else
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());//ta metoda se klice tudi v vsakmu tipu requesta za manipulacijo z itemi
            }
            else
            {//fail, send fail response. pr rust bi ga kljucavnca shokirala recimo
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 0, "-1");
            }
        }
    }

    private void send_rpc_response_with_upkeep_cost(RpcArgs args)
    {
        if (gameObject.GetComponent<NetworkGuildFlag>() != null)
        {
            int[] a = gameObject.GetComponent<NetworkGuildFlag>().get_upkeep_for_24h();
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE_WITH_UPKEEP, 1, this.nci.getItemsNetwork(), a[0], a[1], a[2], a[3]);
        }
        
    }

    private bool is_with_upkeep() {
        return gameObject.GetComponent<NetworkGuildFlag>() != null;
    }

    internal void send_container_update_crafting_station() {
        if (networkObject.IsServer) {
            //iterate over players
            //if allowed to see contents (everyone can see contents of crafting stations for now)
            //if distance is close enoug
            //send update
            //or just send it to others.close or something
            networkObject.SendRpc(RPC_OPEN_RESPONSE,Receivers.AllProximity, 1, this.nci.getItemsNetwork());
        }
    }

    internal bool isEmpty()
    {
        return this.nci.isEmpty();
    }

    private bool isPlayerAuthorizedToOpen(uint id)
    {

        //what kind of container is this?
        NetworkGuildFlag f = gameObject.GetComponent<NetworkGuildFlag>();
        if (f != null) {
            return f.is_player_authorized(id);
        }


        Debug.LogWarning("no security unless specifically locked i suppose");
        return true;
    }

    /// <summary>
    /// od serverja dobi podatke o itemih k so u containerju.
    /// </summary>
    /// <param name="args"></param>
    public override void openResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            if (args.GetNext<int>() == 1)
            {
                Predmet[] predmeti = this.nci.parseItemsNetworkFormat(args.GetNext<string>());
                FindByid(networkObject.Networker.Me.NetworkId).GetComponent<NetworkPlayerInventory>().onContainerOpen(this, predmeti);
            }
            else
            {//fail - nismo authorized al pa kej tazga
                UILogic.Instance.ClearAll();//da se miska zbrise
            }
        }
    }

    //za flag trenutno
    public override void openResponseWithUpkeep(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            if (args.GetNext<int>() == 1)
            {
                Predmet[] predmeti = this.nci.parseItemsNetworkFormat(args.GetNext<string>());
                int a = args.GetNext<int>();
                int b = args.GetNext<int>();
                int c = args.GetNext<int>();
                int d = args.GetNext<int>();
                FindByid(networkObject.Networker.Me.NetworkId).GetComponent<NetworkPlayerInventory>().onContainerOpenWithUpkeep(this, predmeti, a,b,c,d);
            }
            else
            {//fail - nismo authorized al pa kej tazga
                UILogic.Instance.ClearAll();//da se miska zbrise
            }
        }
    }

    #endregion

    #region PREMIKANJE

    //lokalni klici 
    internal  void localRequestPersonalToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_PERSONAL_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestContainerToPersonal(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_PERSONAL, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestBackpackToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_BACKPACK_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestContainerToBackpack(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_BACKPACK, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestBarToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_BAR_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestContainerToBar(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_BAR, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestLoadoutToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_LOADOUT_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestContainerToLoadout(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_LOADOUT_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestContainerToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal  void localRequestDropItemContainer(int v)
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
        if (networkObject.IsServer)
        {
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
            else
            {//desni klik - index == -1
                if (this.nci.hasSpace())
                {
                    this.nci.putFirst(requester_npi.predmeti_personal[personalIndex]);
                    requester_npi.predmeti_personal[personalIndex] = null;
                }
                else
                {
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

            Predmet c = this.nci.predmeti[containerIndex];
            if (personalIndex == -1)
            {
                //desni click na container. -> handleItemPickup
                if (requester_npi.canPickupPredmetFullStack(c))
                {
                    this.nci.predmeti[containerIndex] = null;
                    requester_npi.handleItemPickup(c);
                }
                else if (c.item.stackSize > 1)
                {//probamo pobrat delni stack. kot response dobimo del stacka k ga nismo mogli pobrat in ga pac damo nazaj v container na isto mesto.
                    c = requester_npi.tryToAddPredmetToExistingStack(c);
                    this.nci.predmeti[containerIndex] = c;
                }
            }
            else
            {



                //to bi mogl zmer bit true btw

                this.nci.predmeti[containerIndex] = requester_npi.predmeti_personal[personalIndex];
                requester_npi.predmeti_personal[personalIndex] = c;

            }

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
            else
            {//desni klik
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
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requesterNpi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            int item_type_casted_enum = args.GetNext<int>();
            Item.Type t = (Item.Type)item_type_casted_enum;

            int to = args.GetNext<int>();
            if (this.nci.hasSpace())
            {
                if (to > -1)
                {
                    this.nci.predmeti[to] = requesterNpi.popPredmetLoadout(t);
                }
                else
                {
                    this.nci.putFirst(requesterNpi.popPredmetLoadout(t));
                }

                //poslat update za container
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());

                requesterNpi.sendNetworkUpdate(false, true);
            }
        }
    }

    public override void ContainerToLoadout(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            uint requester = args.Info.SendingPlayer.NetworkId;
            NetworkPlayerInventory requesterNpi = FindByid(requester).GetComponent<NetworkPlayerInventory>();
            int cont_index = args.GetNext<int>();

            if (this.nci.predmeti[cont_index] != null)
            {
                Predmet c = this.nci.predmeti[cont_index];
                if (c.item.type == Item.Type.head && requesterNpi.getHeadItem() == null ||
                c.item.type == Item.Type.chest && requesterNpi.getChestItem() == null ||
                c.item.type == Item.Type.hands && requesterNpi.getHandsItem() == null ||
                c.item.type == Item.Type.legs && requesterNpi.getLegsItem() == null ||
                c.item.type == Item.Type.feet && requesterNpi.getFeetItem() == null)
                {//je valid

                    this.nci.predmeti[cont_index] = requesterNpi.popPredmetLoadout(c.item.type);
                    requesterNpi.SetPredmetLoadout(c);

                    //poslat update za container
                    networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());

                    requesterNpi.sendNetworkUpdate(false, true);
                }
            }


        }
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



    #endregion

    /// <summary>
    /// dropa item direkt. ce je container tak da se kej crafta not ( pecica) je treba nekak pohendlat interruption, magar tko kot je zrihtan player chafting v npi
    /// </summary>
    /// <param name="args"></param>
    public override void dropItem(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            Predmet p = this.nci.popPredmet(args.GetNext<int>());
            FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerInventory>().instantiateDroppedPredmet(p);
            //poslat update za container
            networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());
        }
    }


    protected GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        //Debug.Log("interactable.findplayerById");
        //Debug.Log(targetNetworkId);
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {//very fucking inefficient ampak uno k je spodej nedela. nevem kaj je fora une kode ker networker,NetworkObjects niso playerji, so networkani objekti k drzijo playerje in njihova posizija znotraj lista se spreminja. kojikurac
         //    Debug.Log(p.GetComponent<NetworkPlayerStats>().server_id);
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        //Debug.Log("TARGET PLAYER NOT FOUND!");
        // NetworkBehavior networkBehavior = (NetworkBehavior)NetworkManager.Instance.Networker.NetworkObjects[(uint)targetNetworkId].AttachedBehavior;
        // GameObject obj = networkBehavior.gameObject;


        return null;
    }
}

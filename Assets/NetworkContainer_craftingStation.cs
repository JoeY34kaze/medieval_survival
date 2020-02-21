﻿using BeardedManStudios.Forge.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkContainer_craftingStation : NetworkContainer
{
    protected NetworkContainer_items nci;

    internal void init(int capacity)
    {
        this.nci = GetComponent<NetworkContainer_items>();
        this.nci.init(capacity);
    }

    internal string getItemsNetwork()
    {
        return this.nci.getItemsNetwork();
    }

    internal Predmet[] parseItemsNetworkFormat(string v)
    {
        return this.nci.parseItemsNetworkFormat(v);
    }


    public override void openRequest(RpcArgs args) {
        if (networkObject.IsServer)
        {
            //nekej securityja pa autorizacije rabmo ko bomo mel guilde pa tak

            if (isPlayerAuthorizedToOpen(args.Info.SendingPlayer.NetworkId))
            {
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 1, this.nci.getItemsNetwork());//ta metoda se klice tudi v vsakmu tipu requesta za manipulacijo z itemi
            }
            else
            {//fail, send fail response. pr rust bi ga kljucavnca shokirala recimo
                networkObject.SendRpc(args.Info.SendingPlayer, RPC_OPEN_RESPONSE, 0, "-1");
            }
        }
    }

    public override void openResponse(RpcArgs args) {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            if (args.GetNext<int>() == 1)
            {
                Predmet[] predmeti = this.nci.parseItemsNetworkFormat(args.GetNext<string>());
                FindByid(networkObject.Networker.Me.NetworkId).GetComponent<NetworkPlayerInventory>().onContainerOpen(this, predmeti);
            }
            else
            {//fail - nismo authorized al pa kej tazga
                FindByid(networkObject.Networker.Me.NetworkId).GetComponentInChildren<UILogic>().clear();//da se miska zbrise
            }
        }
    }


    #region PREMIKANJE

    //lokalni klici 
    

    internal override void localRequestPersonalToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_PERSONAL_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestContainerToPersonal(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_CONTAINER_TO_PERSONAL, Receivers.Server, indexFrom, indexTo);
    }

    internal override void localRequestBackpackToContainer(int indexFrom, int indexTo)
    {
        networkObject.SendRpc(RPC_BACKPACK_TO_CONTAINER, Receivers.Server, indexFrom, indexTo);
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
                {//probamo pobrat delni stack. kot response dobimo del stacka k ga nismo mogli pobrat in ga pac damo nazaj v chest na isto mesto.
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
        }
    }

    #endregion

    private bool isPlayerAuthorizedToOpen(uint networkId)
    {
        Debug.LogWarning("no security");
        return true;
    }
}

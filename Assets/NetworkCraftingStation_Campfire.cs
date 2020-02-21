using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCraftingStation_Campfire : NetworkCraftingStation
{

    //nekak se mora klicat da se nastimajo parametri ob postavitvi


    public override void Withdraw(RpcArgs args)
    {
        base.Withdraw(args);
    }

    public override void Deposit(RpcArgs args)
    {
        base.Deposit(args);
    }

    public override void InventoryRequest(RpcArgs args)
    {
        base.InventoryRequest(args);
    }

    public override void InventoryResponse(RpcArgs args)
    {
        if (args.Info.SendingPlayer.NetworkId == 0)
        {
            if (args.GetNext<int>() == 1)
            {
                Predmet[] predmeti = this.container.parseItemsNetworkFormat(args.GetNext<string>());
                FindByid(networkObject.Networker.Me.NetworkId).GetComponent<NetworkPlayerInventory>().onCampfireOpen(this.container, predmeti);
            }
            else
            {//fail - nismo authorized al pa kej tazga
                FindByid(networkObject.Networker.Me.NetworkId).GetComponentInChildren<UILogic>().clear();//da se miska zbrise
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using UMA.CharacterSystem;

public class NetworkArmorStand : NetworkArmorStandBehavior
{


    private DynamicCharacterAvatar avatar;


    public Predmet head;
    public Predmet chest ;
    public Predmet hands ;
    public Predmet legs ;
    public Predmet feet ;

    public Predmet weapon;
    public Predmet shield;
    public Predmet ranged; //empty ker nimamo ranged weaponov

    public Transform w0;
    public Transform w1;
    public Transform sh;
    public Transform ra;

    public List<GameObject> instantiatable_weapons_for_armor_stand; //tle samo nameces iteme k so v /resources/weapons
    //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        this.avatar = GetComponent<DynamicCharacterAvatar>();
    }

    protected override void NetworkStart()
    {
        base.NetworkStart();
        networkObject.SendRpc(RPC_NETWORK_REFRESH_REQUEST, Receivers.Server);

    }

    internal void local_interaction_request(int collider_index, uint server_id)
    {
        //check if both equipped item and armor stand collider items are empty, else send request
        if (Is_sending_request_valid(collider_index, server_id))
        {
            networkObject.SendRpc(RPC_ARMOR_STAND_INTERACTION_REQUEST, Receivers.Server, server_id, collider_index);
        }
    }




    private bool Is_sending_request_valid(int collider_index, uint server_id)
    {
        NetworkPlayerInventory npi = FindByid(server_id).GetComponent<NetworkPlayerInventory>();
        switch (collider_index)
        {
            case 0://head
                if (this.head == null && npi.getHeadItem() == null) return false;
                break;
            case 1://chest
                if (this.chest == null && npi.getChestItem() == null) return false;
                break;
            case 2://hands
                if (this.hands == null && npi.getHandsItem() == null) return false;
                break;
            case 3://legs
                if (this.legs == null && npi.getLegsItem() == null) return false;
                break;
            case 4://feet
                if (this.feet == null && npi.getFeetItem() == null) return false;
                break;
            case 5://wep0
                if (this.weapon == null && npi.GetWeaponItemInHand() == null) return false;
                break;
            case 7://shield
                if (this.shield == null && npi.GetShieldItemInHand() == null) return false;
                break;
            case 8://ranged
                if (this.ranged == null && npi.GetRangedItemInHand() == null) return false;
                break;
            default:
                Debug.LogError("request for armor stand interaction is locally found to be valid");
                return true;
        }
        return true;
    }

    internal void local_interaction_swap_request(uint server_id)
    {
        networkObject.SendRpc(RPC_ARMOR_STAND_BULK_INTERACTION_REQUEST, Receivers.Server, server_id, (byte)0);
    }

    /// <summary>
    /// tle spisat metodo k poslje rpcje za iteme, ktere player se nima equipane.
    /// </summary>
    /// <param name="server_id"></param>
    internal void local_interaction_get_all_request(uint server_id)
    {
        networkObject.SendRpc(RPC_ARMOR_STAND_BULK_INTERACTION_REQUEST, Receivers.Server, server_id, (byte)2);
    }

    /// <summary>
    /// tle spisat metodo da posle rpcje za slote k jih mannequin se nima equipanih
    /// </summary>
    /// <param name="server_id"></param>
    internal void local_interaction_give_all_request(uint server_id)
    {
        networkObject.SendRpc(RPC_ARMOR_STAND_BULK_INTERACTION_REQUEST, Receivers.Server, server_id, (byte)1);
    }

    internal Item.Type getItemTypeFromColliderIndex(int i)
    {
        switch (i)
        {
            case 0:
                return Item.Type.head;
            case 1:
                return Item.Type.chest;
            case 2:
                return Item.Type.hands;
            case 3:
                return Item.Type.legs;
            case 4:
                return Item.Type.feet;
            case 5:
                return Item.Type.weapon;
            case 6:
                return Item.Type.weapon;
            case 7:
                return Item.Type.shield;
            case 8:
                return Item.Type.ranged;
            default:
                Debug.LogError("collider_index doesnt match anything!");
                return Item.Type.resource;
        }
    }


    public GameObject FindByid(uint targetNetworkId) //koda kopširana v network_body.cs in Interactable.cs
    {
        Debug.Log("interactable.findplayerById");
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
    //------------------------------------------------------------------------------------------------------RPCS-----------------------------------------------------------------------
    public override void ArmorStandInteractionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
        uint server_id = args.GetNext<uint>();
        NetworkPlayerInventory npi = FindByid(server_id).GetComponent<NetworkPlayerInventory>();//bols bi blo zvohat objekt prek args.info.sendingplayer ampak to mi ne ratuje sploh nekej..
        int collider_index = args.GetNext<int>();

        if (!Is_sending_request_valid(collider_index, server_id)) return;
        //tle je najbol pomembna stvar
        switch (collider_index)
        {
            case 0:
                if (this.head != null)
                {
                    if (npi.getHeadItem() != null)
                    {
                        //perform swap
                        Predmet loadout_p = npi.popPredmetLoadout(Item.Type.head);
                        Predmet onStand = this.head;
                        this.head = loadout_p;
                        npi.SetPredmetLoadout(onStand);
                    }
                    else
                    {
                        //equip item from stand
                        npi.SetPredmetLoadout(this.head);
                        this.head = null;
                    }
                }
                else
                {
                    if (npi.getHeadItem() != null)
                    {
                        //place equipped item on stand
                        this.head = npi.popPredmetLoadout(Item.Type.head);
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 1:

                if (this.chest != null)
                {
                    if (npi.getChestItem() != null)
                    {
                        //perform swap
                        Predmet loadout_item = npi.popPredmetLoadout(Item.Type.chest);
                        Predmet onStand = this.chest;
                        this.chest = loadout_item;
                        npi.SetPredmetLoadout(onStand);
                    }
                    else
                    {
                        //equip item from stand
                        npi.SetPredmetLoadout(this.chest);
                        this.chest = null;
                    }
                }
                else
                {
                    if (npi.getChestItem() != null)
                    {
                        this.chest = npi.popPredmetLoadout(Item.Type.chest);
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 2:
                if (this.hands != null)
                {
                    if (npi.getHandsItem() != null)
                    {
                        //perform swap
                        Predmet loadout_item = npi.popPredmetLoadout(Item.Type.hands);
                        Predmet onStand =this.hands;
                        this.hands = loadout_item;
                        npi.SetPredmetLoadout(onStand);
                    }
                    else
                    {
                        //equip item from stand
                        npi.SetPredmetLoadout(this.hands);
                        this.hands = null;
                    }
                }
                else
                {
                    if (npi.getHandsItem() != null)
                    {
                        //place equipped item on stand
                        this.hands = npi.popPredmetLoadout(Item.Type.hands);
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 3:
                if (this.legs != null)
                {
                    if (npi.getLegsItem() != null)
                    {
                        //perform swap
                        Predmet loadout_item = npi.popPredmetLoadout(Item.Type.legs);
                        Predmet onStand = this.legs;
                        this.legs = loadout_item;
                        npi.SetPredmetLoadout(onStand);
                    }
                    else
                    {
                        //equip item from stand 
                        npi.SetPredmetLoadout(this.legs);
                        this.legs = null;
                    }
                }
                else
                {
                    if (npi.getLegsItem() != null)
                    {
                        //place equipped item on stand
                        this.legs = npi.popPredmetLoadout(Item.Type.legs);
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 4:
                if (this.feet != null)
                {
                    if (npi.getFeetItem() != null)
                    {
                        //perform swap
                        Predmet loadout_item = npi.popPredmetLoadout(Item.Type.feet);
                        Predmet onStand = this.feet;
                        this.feet = loadout_item;
                        npi.SetPredmetLoadout(onStand);
                    }
                    else
                    {
                        //equip item from stand
                        npi.SetPredmetLoadout(this.feet);
                        this.feet = null;
                    }
                }
                else
                {
                    if (npi.getFeetItem() != null)
                    {
                        //place equipped item on stand
                        this.feet = npi.popPredmetLoadout(Item.Type.feet);
                    }
                    else
                    {
                        //return because nothing can happen

                    }
                }
                break;
            case 5:
                if (this.weapon != null)
                {
                    if (npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace() )
                    {
                        //take the weapon into inventory

                        if(npi.tryToAddItem(this.weapon))
                            this.weapon = null;
                    } 
                }
                else
                {
                    if (npi.GetWeaponItemInHand() != null)
                    {
                        //place equipped item on stand
                        this.weapon = npi.PopWeaponPredmetInHand();
                    }
                }
                break;
            case 7:
                if (this.shield !=null)
                {
                    if (npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace())
                    {
                        //perform swap
                        if (npi.tryToAddItem(this.shield))
                            this.shield = null;
                    }
                }
                else
                {
                    if (npi.GetShieldItemInHand() != null)
                    {
                        //place equipped item on stand
                        this.shield = npi.PopShieldPredmetInHand();
                    }

                }
                break;
            case 8:
                if (this.ranged != null)
                {
                    if(npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace())
                    {


                        if (npi.tryToAddItem(this.ranged))
                            this.ranged = null;
                    }
                }
                else
                {
                    if (npi.GetRangedItemInHand() != null)
                    {
                        //place equipped item on stand
                        this.ranged = npi.PopRangedItemInHand();
                    }

                }
                break;
            default:
                Debug.LogError("collider_index doesnt match anything!");
                break;
        }
        npi.sendNetworkUpdate(true, true);
        networkObject.SendRpc(RPC_ARMOR_STAND_REFRESH,Receivers.All, this.head == null ? "-1" : this.head.toNetworkString(), this.chest == null ? "-1" : this.chest.toNetworkString(), this.hands == null ? "-1" : this.hands.toNetworkString(), this.legs == null ? "-1" : this.legs.toNetworkString(), this.feet == null ? "-1" : this.feet.toNetworkString(), this.weapon == null ? "-1" : this.weapon.toNetworkString(), this.shield == null ? "-1" : this.shield.toNetworkString(), this.ranged == null ? "-1" : this.ranged.toNetworkString());
    }



    /// <summary>
    /// poslje client serverju ko se connecta gor. kot response pricakuje rpc armorStandRefresh
    /// </summary>
    /// <param name="args"></param>
    public override void NetworkRefreshRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;

        networkObject.SendRpc(args.Info.SendingPlayer, RPC_ARMOR_STAND_REFRESH, this.head==null ? "-1":this.head.toNetworkString(), this.chest == null ? "-1" : this.chest.toNetworkString(), this.hands == null ? "-1" : this.hands.toNetworkString(), this.legs == null ? "-1" : this.legs.toNetworkString(), this.feet == null ? "-1" : this.feet.toNetworkString(), this.weapon == null ? "-1" : this.weapon.toNetworkString(),this.shield == null ? "-1" : this.shield.toNetworkString(), this.ranged == null ? "-1" : this.ranged.toNetworkString());
    }

    /// <summary>
    /// dobi server. parameter je int. 0-swap, 1-give all, 2-take all missing
    /// </summary>
    /// <param name="args"></param>
    public override void ArmorStandBulkInteractionRequest(RpcArgs args)
    {
        if (!networkObject.IsServer) return;
        uint server_id = args.GetNext<uint>();
        byte tip = args.GetNext<byte>();
        NetworkPlayerInventory npi = FindByid(server_id).GetComponent<NetworkPlayerInventory>();
        switch (tip)
        {
            case 0://swap
                swap_in_full(server_id, npi);
                break;
            case 1://give
                give_all_missing(server_id, npi);
                break;
            case 2://take
                take_all_missing(server_id, npi);
                break;
            default:
                //throw new NotImplementedException; ?
                break;
        }

        npi.sendNetworkUpdate(true, true);
        networkObject.SendRpc(RPC_ARMOR_STAND_REFRESH,Receivers.All, this.head == null ? "-1" : this.head.toNetworkString(), this.chest == null ? "-1" : this.chest.toNetworkString(), this.hands == null ? "-1" : this.hands.toNetworkString(), this.legs == null ? "-1" : this.legs.toNetworkString(), this.feet == null ? "-1" : this.feet.toNetworkString(), this.weapon == null ? "-1" : this.weapon.toNetworkString(), this.shield == null ? "-1" : this.shield.toNetworkString(), this.ranged == null ? "-1" : this.ranged.toNetworkString());
    }

    private void take_all_missing(uint server_id, NetworkPlayerInventory npi)
    {

        if (npi.getHeadItem() == null)
        {
            if (this.head != null)
            {
                npi.SetPredmetLoadout(this.head);
                this.head = null;
            }
        }


        if (npi.getChestItem() == null)
        {
            if (this.chest != null)
            {
                npi.SetPredmetLoadout(this.chest);
                this.chest = null;
            }
        }


        if (npi.getHandsItem() == null)
        {
            if (this.hands != null)
            {
                npi.SetPredmetLoadout(this.hands);
                this.hands = null;
            }
        }


        if (npi.getLegsItem() == null)
        {
            if (this.legs != null)
            {
                npi.SetPredmetLoadout(this.legs);
                this.legs = null;
            }
        }

        if (npi.getFeetItem() == null)
        {
            if (this.feet != null)
            {
                npi.SetPredmetLoadout(this.feet);
                this.feet = null;
            }
        }

        if (this.weapon != null)
        {
            if (npi.tryToAddItem(this.weapon))
                this.weapon = null;
        }



        if (this.shield != null)
        {
            if (npi.tryToAddItem(this.shield))
                this.shield = null;
        }

        if (this.ranged != null)
        {
            if (npi.tryToAddItem(this.ranged))
                this.ranged = null;
        }
    }

    private void give_all_missing(uint server_id, NetworkPlayerInventory npi)
    {
        Predmet loadout_item;
        //head

        if (this.head == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.popPredmetLoadout(Item.Type.head);//lahko vrne null
            if (loadout_item != null) this.head = loadout_item;
        }

        //chest
        if (this.chest == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.popPredmetLoadout(Item.Type.chest);//lahko vrne null
            if (loadout_item != null) this.chest = loadout_item;
        }
        //hands
        if (this.hands == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.popPredmetLoadout(Item.Type.hands);//lahko vrne null
            if (loadout_item != null) this.hands = loadout_item;
        }
        //legs
        if (this.legs == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.popPredmetLoadout(Item.Type.legs);//lahko vrne null
            if (loadout_item != null) this.legs = loadout_item;
        }
        //feet
        if (this.feet == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.popPredmetLoadout(Item.Type.feet);//lahko vrne null
            if (loadout_item != null) this.feet = loadout_item;
        }
        //wep0
        if (this.weapon == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.PopWeaponPredmetInHand();//lahko vrne null
            if (loadout_item != null) this.weapon = loadout_item;
        }
        //shield
        if (this.shield == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.PopShieldPredmetInHand();//lahko vrne null
            if (loadout_item != null) this.shield = loadout_item;
        }
        //ranged
        if (this.ranged == null)
        {//samo pogleda, nc ne spremeni
            loadout_item = npi.PopRangedItemInHand();//lahko vrne null
            if (loadout_item != null) this.ranged = loadout_item;
        }
    }

    private void swap_in_full(uint server_id, NetworkPlayerInventory npi)
    {

        //head
        Predmet loadout_item = npi.popPredmetLoadout(Item.Type.head);//lahko vrne null
        Predmet onStand = this.head; //lahko vrne null
        if (loadout_item != null) this.head = loadout_item;
        else this.head = null;
        if (onStand != null) npi.SetPredmetLoadout(onStand);
        else npi.RemoveItemLoadout(Item.Type.head);

        //chest
        loadout_item = npi.popPredmetLoadout(Item.Type.chest);//lahko vrne null
        onStand = this.chest; //lahko vrne null
        if (loadout_item != null) this.chest = loadout_item;
        else this.chest = null;
        if (onStand != null) npi.SetPredmetLoadout(onStand);
        else npi.RemoveItemLoadout(Item.Type.chest);
        //hands
        loadout_item = npi.popPredmetLoadout(Item.Type.hands);//lahko vrne null
        onStand = this.hands; //lahko vrne null
        if (loadout_item != null) this.hands = loadout_item;
        else this.hands = null;
        if (onStand != null) npi.SetPredmetLoadout(onStand);
        else npi.RemoveItemLoadout(Item.Type.hands);
        //legs
        loadout_item = npi.popPredmetLoadout(Item.Type.legs);//lahko vrne null
        onStand = this.legs; //lahko vrne null
        if (loadout_item != null) this.legs = loadout_item;
        else this.legs = null;
        if (onStand != null) npi.SetPredmetLoadout(onStand);
        else npi.RemoveItemLoadout(Item.Type.legs);
        //feet
        loadout_item = npi.popPredmetLoadout(Item.Type.feet);//lahko vrne null
        onStand = this.feet; //lahko vrne null
        if (loadout_item != null) this.feet = loadout_item;
        else this.feet = null;
        if (onStand != null) npi.SetPredmetLoadout(onStand);
        else npi.RemoveItemLoadout(Item.Type.feet);
        //wep0
        loadout_item = npi.PopWeaponPredmetInHand();//lahko vrne null
        onStand = this.weapon; //lahko vrne null
        if (loadout_item != null) this.weapon = loadout_item;
        else this.weapon = null;
        if (onStand != null && (npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace())) npi.tryToAddItem(onStand);
        //shield
        loadout_item = npi.PopShieldPredmetInHand();//lahko vrne null
        onStand = this.shield; //lahko vrne null
        if (loadout_item != null) this.shield = loadout_item;
        else this.shield = null;
        if (onStand != null && (npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace())) npi.tryToAddItem(onStand);
        //ranged
        loadout_item = npi.PopRangedItemInHand();//lahko vrne null
        onStand =this.ranged; //lahko vrne null
        if (loadout_item != null)this.ranged = loadout_item;
        else this.ranged = null;
        if (onStand != null && (npi.hasBarSpace() || npi.hasInventoryEmptySlot() || npi.hasBackpackSpace())) npi.tryToAddItem(onStand);


    }

    public override void ArmorStandRefresh(RpcArgs args)//pri dodajanju dobi tud server
    {
        if (args.Info.SendingPlayer.NetworkId != 0) return; //ni poslov player ampak nas edn hacka

        if (!networkObject.IsServer)//server ze ima podatke, jih nerab povozt z potencialno napacnimi..
        {

            this.head   = Predmet.createNewPredmet(args.GetNext<string>());
            this.chest  = Predmet.createNewPredmet(args.GetNext<string>());
            this.hands  = Predmet.createNewPredmet(args.GetNext<string>());
            this.legs   = Predmet.createNewPredmet(args.GetNext<string>());
            this.feet   = Predmet.createNewPredmet(args.GetNext<string>());
            this.weapon = Predmet.createNewPredmet(args.GetNext<string>());
            this.shield = Predmet.createNewPredmet(args.GetNext<string>());
            this.ranged = Predmet.createNewPredmet(args.GetNext<string>());
        }

        redraw_armor_stand();

    }

    private void redraw_armor_stand()//izrise stvari k so na uma
    {
        //Debug.Log("Redrawing");

        //--------------------------------------CLOTHING-----------------------------------------

        avatar.ClearSlots();

        if (this.head != null)
        {
            avatar.SetSlot("Helmet", this.head.item.uma_item_recipe_name);
        }

        if (this.chest != null)
        {
            avatar.SetSlot("Chest", this.chest.item.uma_item_recipe_name);
        }

        if (this.hands != null)
        {
            avatar.SetSlot("Hands", this.hands.item.uma_item_recipe_name);
        }

        if (this.legs != null)
        {
            avatar.SetSlot("Legs", this.legs.item.uma_item_recipe_name);
        }

        if (this.feet != null)
        {
            avatar.SetSlot("Feet", this.feet.item.uma_item_recipe_name);
        }

        avatar.BuildCharacter();

        //--------------------------------------WEAPONS AND SHIELD-----------------------------------------


        show_weapon(this.weapon, w0);

        show_weapon(this.shield, sh);

        show_weapon(this.ranged, ra);



    }
    //private void Update()
    //{
    //    redraw_armor_stand();
    //}
    /// <summary>
    /// ineficiend. optimize it later
    /// </summary>
    /// <param name="i"></param>
    /// <param name="retard"></param>
    private void show_weapon(Predmet p, Transform retard)//prikaze weapon k pripada id-ju na pozicijo kot child transforma.
    {
        //zbris vse
        for (int k = 0; k < retard.childCount; k++)//ce dam tle samo destroy ucas kr faila. nimam pojma zakaj. ce dam destroy u update ga ubije, sicer g apa ne. no fucking clue. to sm skor prepičan da je unity bug
        {
            Destroy(retard.GetChild(k).gameObject);// crkni cigan en
        }

        if (p == null) { }
        else if (p.item == null)
        {
            //ce je prazn nared nc ker si itak ze vse zbrisal
        } else if (p.item.id ==-1) { }
        else//ce ni prazn izris to kar mora bit gor
        {

            GameObject w = null;
            foreach (GameObject g in this.instantiatable_weapons_for_armor_stand)
            {
                if (g.GetComponent<identifier_helper>().id == p.item.id)
                {
                    w = GameObject.Instantiate(g);
                    break;
                }
            }
            if (w == null)
                throw new Exception("shits fucked yo");

            w.transform.SetParent(retard);
            w.transform.localPosition = Vector3.zero;
            w.transform.localRotation = Quaternion.identity;
        }

    }
    /// <summary>
    /// klice NetowrkStartupSynchronizer da poslje povatke o tem objektu playerju, ki se je ravnokar sconnectal na server
    /// </summary>
    /// <param name="p"></param>
    internal void ServerSendAllToPlayer(NetworkingPlayer p)//duplikat funkcionalnosti networkRefreshRequest ??
    {
        if(networkObject.IsServer)
         networkObject.SendRpc(p,RPC_ARMOR_STAND_REFRESH, 
             (this.head==null)? "-1":this.head.toNetworkString(),
             (this.chest == null) ? "-1" : this.chest.toNetworkString(),
             (this.hands == null) ? "-1" : this.hands.toNetworkString(),
             (this.legs == null) ? "-1" : this.legs.toNetworkString(),
             (this.feet == null) ? "-1" : this.feet.toNetworkString(),
             (this.weapon == null) ? "-1" : this.weapon.toNetworkString(),
             (this.shield == null) ? "-1" : this.shield.toNetworkString(),
             (this.ranged == null) ? "-1" : this.ranged.toNetworkString());
    }
}

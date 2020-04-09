using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using System;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking;

/// <summary>
/// stvari k so skupne vsem stvarem k jih loh postavs. health, lastnistvo, take fore. boljs specificne stvari nrdit svojo skripto i guess
/// </summary>
public class NetworkPlaceable : NetworkPlaceableBehavior
{
    [SerializeField]
    private uint player_who_placed_this;

    [SerializeField]
    internal static float distanceForSnappingHandling = 0.05f;

    public Predmet p;
    [SerializeField]
    public Item.SnappableType snappableType;

    [SerializeField]
    private AttachmentPoint[] attachmentPoints;

    [SerializeField]
    internal NetworkGuildFlag upkeep_flag;

    private NetWorker myNetWorker;

    private gibs_handler gibs;

    public static float repair_rate=0.1f;
    public static readonly float max_distance_for_durability_check = 5f;

    private void Start()
    {
        this.gibs = GetComponentInChildren<gibs_handler>(true);
         
    }


    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();//server prevzame ownership
        }
    }

    public void init(Predmet p, uint player_who_placed_this)
    {
        this.attachmentPoints = GetComponentsInChildren<AttachmentPoint>();

        this.p = p;
        this.player_who_placed_this = player_who_placed_this;
        //server mu nastavi vse stvari k jih rab nastavt ob instanciaciji objekta.
        if (GetComponent<NetworkGuildFlag>() != null) GetComponent<NetworkGuildFlag>().init();
        if (GetComponent<NetworkContainer>() != null) GetComponent<NetworkContainer>().init(this.p);
        if (GetComponent<NetworkCraftingStation>() != null) GetComponent<NetworkCraftingStation>().init(this.p);

        if (networkObject.IsServer)
        {
            myNetWorker = GameObject.Find("NetworkManager(Clone)").GetComponent<NetworkManager>().Networker;
            attach_land_claim_object();
        }
    }

    private void attach_land_claim_object() {
        this.upkeep_flag = NetworkGuildFlag.get_dominant_guild_flag_in_range(transform.position);
        if (this.upkeep_flag != null) this.upkeep_flag.add_placeable_for_upkeep(this);
    }

    

    internal void sendAttachmentUpdate(int sibling, bool boo)
    {
        if (networkObject.IsServer) networkObject.SendRpc(RPC_NETWORK_ATTACHMENT_UPDATE, Receivers.Others, sibling, boo);
    }

    internal void local_placement_of_placeable_request(Quaternion rotation, int v)
    {
        //tukej klicemo rpc za postavlanje objekta k je snappan na nekej.!
        networkObject.SendRpc(RPC_NETWORK_PLACEABLE_ATTACHMENT_REQUEST, Receivers.Server, rotation, v);
    }

    internal uint get_creator()
    {
        //vrne playerja, ki je postavil ta objekt. naceloma se klice samo na serverju imo
        return this.player_who_placed_this;
        
    }

    internal void on_upkeep_pay() {
        //Debug.Log("Paying upkeep for " + this.p.item.Display_name);
        if (this.upkeep_flag == null || !this.upkeep_flag.pay_upkeep_for(this))//ce ni flaga ali pa placevanje upkeepa faila
            take_damage((int)this.p.item.Max_durability / 24);
    }

    internal void take_weapon_damage(Predmet p) {
        if (p == null)
        {
            Debug.LogWarning("weapon predmet is null! taking default damage of 250");
            take_damage(250);
        }
        else
            take_damage(p.item.damage);
    }

    private void take_damage(int d) {
        //Debug.Log("object " + p.ToString() + " from " + this.p.current_durabilty + " takes " + d + " dmg and is now at " +(this.p.current_durabilty-d));
        if (networkObject.IsServer)
        {
            this.p.current_durabilty -= d;

            send_predmet_update_to_all_valid_nearby_players();
            if (this.p.current_durabilty <= 0)
                handle_object_destruction();
        }
    }



    public override void NetworkPlaceableAttachmentRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
                GameObject player = FindByid(args.Info.SendingPlayer.NetworkId);
            NetworkPlayerNeutralStateHandler neutral_state_handler = FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerNeutralStateHandler>();


            if (networkObject.IsServer)
            {
                Debug.Log("server - placing " + neutral_state_handler.current_placeable_item.Display_name);

                Quaternion rot = args.GetNext<Quaternion>();

                //get current placeable predmet!
                Predmet p = neutral_state_handler.activePlaceable;
                Transform ch = transform.GetChild(args.GetNext<int>());
                if (ch.GetComponent<AttachmentPoint>().attached_placeable != null) {
                    Debug.LogError("Attachment slot occupied");
                    return;
                }

                if (!NetworkPlayerNeutralStateHandler.has_building_privilege(args.Info.SendingPlayer, ch.position)) return;

                if (!neutral_state_handler.currentTransformOfPlaceableIsValid(ch.transform.position)) return;
                
                NetworkPlaceable created = neutral_state_handler.NetworkPlaceableInstantiationServer(p, ch.position, rot);



                /*tole je blo pre

                ch.GetComponent<AttachmentPoint>().attachTryReverse(created.gameObject);//proba nrdit obojestransko referenco ce se vse izide, sicer samo v eno smer
                //samo dve povezavi nista dovolj, ko se sklene krog je treba to tud ugotovit, recimo 4 foundationi
                created.refreshAttachmentPointOccupancy();

                */

                created.refreshAttachmentPointsInDomain();





                player.GetComponent<NetworkPlayerInventory>().reduceCurrentActivePlaceable(neutral_state_handler.selected_index);//sicer vrne bool da nam pove ce smo pobral celotn stack, ampak nima veze ker rabmo poslat update za kvantiteto v vsakem primeru.
                                                                                                                //nastavi selected index na -1 ce smo pobral vse - da gre lepo v rpc             
                neutral_state_handler.sendBarUpdate();
            }



        }
    }


    /// <summary>
    /// to pride v postev, ko se recimo podre foundation in rabimo pogledat ali se stena drzi druge stene mogoce in ne lebdi v zraku
    /// </summary>
    /// <param name="attachmentPoint"></param>
    /// <returns></returns>
    internal bool isAttachedToAlternativeAttachmentPoint(AttachmentPoint at)
    {
        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return false;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return false;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, this.p.item.blocks_placements);

        foreach (AttachmentPoint p in all_AttachmentPoints)
        {
            if (p.isFree() && !p.Equals(at))//tole neb smel bit ampak ce slucajno je se mora pohandlat
                if (p.acceptsAttachmentOfType(this.snappableType))
                {
                    if (Vector3.Distance(p.gameObject.transform.position, this.transform.position) < NetworkPlaceable.distanceForSnappingHandling)
                    {
                        p.attachTryReverse(this.transform.gameObject);
                        Debug.LogError("tole se neb smel izvest. attachment pointi ne smejo bit prazni ampak bi mogl zmer ze prej kazat na ta objekt! glej attachment point refresh ocupancy pri postavitvi objekta");
                        return true;
                    }
                }
                else if (p.attached_placeable.Equals(gameObject)) return true;
        }
        return false;
    }


    /// <summary>
    /// metoda se klice ob networkInstantiation placeable objekta. metoda mora prever kaj se dogaja z attachment pointi tega objekta (če so znotraj druzga objekta) in povezat na soseda, ter v primeru da se dotika tud drugih pointov (recimo če smo postavli 2x2 povezat tud une.
    /// </summary>
    /// <param name="networkPlaceable"></param>
    private void refreshAttachmentPointOccupancy()
    {
        //poisc vse attachment pointe v dolocenem rangeu (1f recimo)

        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, this.p.item.blocks_placements);

        foreach (AttachmentPoint p in all_AttachmentPoints) {


            if (p.isFree())
                if (p.acceptsAttachmentOfType(this.snappableType))
                {

                    if (this.p.item.PlacementType == Item.SnappableType.door) { //ce je vrata, mora poblikirat vsa druga vrata na parent objektu.
                        if (p.is_sibling_attachment_point_occupied_by(this.gameObject)) { //to mora poblokirat sicer bi na en frame vrat nalimal lahko dvoje vrat, vsaka pod svojim kotom
                            p.attach(this.gameObject);
                        }
                    }

                    if (Vector3.Distance(p.gameObject.transform.position, this.transform.position) < NetworkPlaceable.distanceForSnappingHandling)
                    {
                        p.attachTryReverse(this.transform.gameObject);
                    }
                }
        }
    }

    /// <summary>
    /// klice se, ko se ustvari nov networkplaceable z snappable nacinom. pohandlat mora VSE reference, ki se ga ticejo.
    /// </summary>
    private void refreshAttachmentPointsInDomain()
    {
        //naredit mora 3 locene operacije.
        //1 - poiskat vse attachment pointe, katere sam poblokira in nastavit referenco na njih, nase
        //2 - za vse svoje child attachment pointe pogledat, ali so znotraj že postavljenih networkplaceable objektov in ustrezno nastavit reference
        //3 - ce je objekt posebnega tipa ( door , narrow/wide stairs) treba pohandlat blokiranje tudi izven rangea

        List<AttachmentPoint> attachedTo = new List<AttachmentPoint>();
        //1 -----------------------------------------------------------------------------------------------------------------------------------------
        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, this.p.item.blocks_placements);

        foreach (AttachmentPoint p in all_AttachmentPoints)
        {
            if (Vector3.Distance(p.gameObject.transform.position, this.transform.position) < NetworkPlaceable.distanceForSnappingHandling)
                if (p.acceptsAttachmentOfType(this.snappableType))
                {
                    if (p.isFree())
                    {
                        p.attach(gameObject);
                        attachedTo.Add(p);
                    }
                }
        }


        //2-----------------------------------------------------------------------------------------------------------------------------------------
        NetworkPlaceable[] arr = new NetworkPlaceable[0];
        foreach (AttachmentPoint p in GetComponentsInChildren<AttachmentPoint>()) {
            arr = getAllNetworkplaceablesInRange(p.transform.position, NetworkPlaceable.distanceForSnappingHandling);
            foreach (NetworkPlaceable np in arr)
                p.attach(np.gameObject);//ustvari enosmerno referenco. posledicno je dvosmerna zaradi tocke 1. v tej metodi. mora bit dvosmerna!
        }

        //3-----------------------------------------------------------------------------------------------------------------------------------------

        if (this.p.item.PlacementType == Item.SnappableType.door || this.p.item.PlacementType == Item.SnappableType.stairs_narrow || this.p.item.PlacementType == Item.SnappableType.stairs_wide)
        { //ce je vrata, mora poblikirat vsa druga vrata na parent objektu.
            foreach (AttachmentPoint att in attachedTo) { //za vsak networkplaceable na kterga smo attachan ( naceloma samo en ker je samo vrata in stenge)
                NetworkPlaceable np = att.GetComponentInParent<NetworkPlaceable>();
                np.blockChildAttachmentPointsOfTypeWith(this.p.item.PlacementType, gameObject);
            }
        }


    }

    internal void blockChildAttachmentPointsOfTypeWith(Item.SnappableType placeableType, GameObject g) {
        foreach (AttachmentPoint a in GetComponentsInChildren<AttachmentPoint>()) {
            if (a.acceptsAttachmentOfType(placeableType) && a.isFree())
                a.attach(g);
        } 
    }


    public static NetworkPlaceable[] getAllNetworkplaceablesInRange(Vector3 pos, float r) { 
        List<NetworkPlaceable> rez = new List<NetworkPlaceable>();
        foreach (NetworkPlaceable p in GameObject.FindObjectsOfType<NetworkPlaceable>())
            if (Vector3.Distance(p.gameObject.transform.position, pos) < NetworkPlaceable.distanceForSnappingHandling)
                rez.Add(p);
        return rez.ToArray();
    }

    private AttachmentPoint[] GetAttachmentPointsInRangeForSnapping(AttachmentPoint[] all_valid)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_valid) {
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if ( dist< NetworkPlaceable.distanceForSnappingHandling)
                l.Add(p);
            Debug.Log(Vector3.Distance(p.transform.position, transform.position));

        }

        return l.ToArray();
    }

    public AttachmentPoint[] GetAllValidAttachmentPoints(Item.SnappableType current_snappable_type)//kopiran z neutralstatehandlerja
    {
        AttachmentPoint[] all_AttachmentPoints = (AttachmentPoint[])GameObject.FindObjectsOfType<AttachmentPoint>();
        if (all_AttachmentPoints == null) return null;
        all_AttachmentPoints = removeTakenAttachmentPoints(all_AttachmentPoints);
        if (all_AttachmentPoints == null) return null;
        all_AttachmentPoints = removeNotValidSnappableTypeAttachmentPoints(all_AttachmentPoints, current_snappable_type);
        return all_AttachmentPoints;
    }

    private AttachmentPoint[] removeNotValidSnappableTypeAttachmentPoints(AttachmentPoint[] all_AttachmentPoints, Item.SnappableType snappable_type)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints)
        {
                if (p.acceptsAttachmentOfType(snappable_type)) l.Add(p);
        }
        return l.ToArray();
    }

    private AttachmentPoint[] removeNotValidSnappableTypeAttachmentPoints(AttachmentPoint[] all_AttachmentPoints, Item.SnappableType[] snappable_types)
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints) {
            foreach(Item.SnappableType t in snappable_types)
            if (p.acceptsAttachmentOfType(t)) l.Add(p);
        }
        return l.ToArray();
    }

    private AttachmentPoint[] removeTakenAttachmentPoints(AttachmentPoint[] all_AttachmentPoints)//kopiran z neutralstatehandlerja
    {
        List<AttachmentPoint> l = new List<AttachmentPoint>();
        foreach (AttachmentPoint p in all_AttachmentPoints) if (p.isFree()) l.Add(p);

        return l.ToArray();
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
    }

    public List<AttachmentPoint> getAttachmentPointsForType(Item.SnappableType s) {
        List<AttachmentPoint> r = new List<AttachmentPoint>();
        foreach (AttachmentPoint t in this.attachmentPoints) {
            if (t.acceptsAttachmentOfType(s)) r.Add(t);
        }
        return r;
    }

    public override void NetworkAttachmentUpdate(RpcArgs args)
    {
        //sendingPlayer je zmer server so..
        transform.GetChild(args.GetNext<int>()).GetComponent<AttachmentPoint>().setAttachedClient(args.GetNext<bool>());
    }

    internal bool is_placement_possible_for(Item current_placeable_item)
    {
        //gremo cez vse attachment pointe in ce najdemo en point ki je ze zaseden med blockerjim vrzemo false sicer true.
        for (int i = 0; i < this.attachmentPoints.Length; i++) {
            if (attachment_point_can_be_blocked_by(this.attachmentPoints[i], current_placeable_item.blocks_placements) ) {
                if (this.attachmentPoints[i].attached_placeable != null) return false;
            }
        }
        return true;
    }

    private bool attachment_point_can_be_blocked_by(AttachmentPoint attachmentPoint, Item.SnappableType[] blocks_placements)
    {
        for (int i = 0; i < blocks_placements.Length; i++)
            for (int j = 0; j < attachmentPoint.allowed_attachment_types.Length; j++)
                if (attachmentPoint.allowed_attachment_types[j] == blocks_placements[i])
                    return true;
        return false;
    }

    internal void durability_repair_request_server()//durability se nebo prenašal na cliente. client ne rabi vedt za vsak block kolk ima lajfa skos. to se mu pošle na začetku pa ce pogleda na block z hammerjem v roki
    {
        if (networkObject.IsServer) {
            if (is_allowed_to_be_repaired()) {
                if (burn_resources_for_repair())
                    this.p.current_durabilty += this.p.item.Max_durability * NetworkPlaceable.repair_rate;
                if (this.p.current_durabilty > this.p.item.Max_durability) this.p.current_durabilty = this.p.item.Max_durability;
            }
        }
    }

    internal bool is_player_owner(uint myPlayerId)
    {
        return this.player_who_placed_this == myPlayerId;
    }

    private bool burn_resources_for_repair() {
        Debug.LogWarning("not implemented yet, - burning resources for repair");
        return true;
    }

    internal bool is_allowed_to_be_repaired() {
        Debug.LogWarning("not implemented yet, - checking is it is being raided");
        return true;
    }

    private void send_predmet_update_to_all_valid_nearby_players() {
        GameObject temp=null;
        lock (myNetWorker.Players)
        {
            myNetWorker.IteratePlayers((player) =>
            {
                temp = FindByid(player.NetworkId);
                if (Vector3.Distance(temp.transform.position, transform.position)<=NetworkPlaceable.max_distance_for_durability_check && temp.GetComponent<NetworkPlayerNeutralStateHandler>().is_repair_hammer_active()) //passive target
                {
                    networkObject.SendRpc(player, RPC_SERVER_UPDATE_PREDMET, this.p.toNetworkString());
                }
            });

        }
    }



    public override void ClientDurabilityRequest(RpcArgs args)
    {
        if (networkObject.IsServer) {
            //tole bi lahko naredil brez enega network calla. da se direkt chekira na serverju pa samo pošilja playerju mogoce
            GameObject p = FindByid(args.Info.SendingPlayer.NetworkId);
            if (p != null)
                if (Vector3.Distance(p.transform.position, transform.position) < NetworkPlaceable.max_distance_for_durability_check)
                {
                    networkObject.SendRpc(args.Info.SendingPlayer, RPC_SERVER_UPDATE_PREDMET, this.p.toNetworkString());
                }
        }
    }

    public override void ServerUpdatePredmet(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            string pred = args.GetNext<String>();
            this.p.setParametersFromNetworkString(pred);
            UILogic.Instance.try_drawing_durability_for_placeable(this);
        }
    }

    private void clear_potential_ui_durability_panel() {
        UILogic.Instance.clear_durability_panel_for_placeable(this);
          }

   

    internal void local_player_predmet_update_request()
    {
        networkObject.SendRpc(RPC_CLIENT_DURABILITY_REQUEST, Receivers.Server);
    }

    private void OnDestroy()
    {
        if (this.gibs != null)
        {
            if (!this.gibs.gameObject.activeSelf) this.gibs.gameObject.SetActive(true);
            this.gibs.enableGibs();
        }
        foreach (AttachmentPoint a in GetComponentsInChildren<AttachmentPoint>()) {
            if(a.attached_placeable!=null)
                a.OnPlaceableDestroyed(this);
        }
        clear_potential_ui_durability_panel();
    }


    /// <summary>
    /// klice lahko samo host. ubije objekt
    /// </summary>
    internal void handle_object_destruction()
    {
        if (networkObject.IsServer)
        {
            networkObject.Destroy();
        }
    }
}

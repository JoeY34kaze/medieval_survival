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
    private Material valid_material;
    private Material invalid_material;
    public Transform toolContainerOnHand;

    internal int selected_index = -1;
    internal int selected_index_shield = -1;
    public Predmet activeTool = null;
    public Predmet activePlaceable = null;
    private bool inPlaceableMode;
    private GameObject CurrentLocalPlaceable;
    private Vector3 previously_valid_position;
    private Quaternion previously_valid_rotation;
    private Item current_placeable_item;
    private BoxCollider currentPlaceableCollider;
    private Renderer currentPlaceableRenderer;

    private float mouseWheelRotation;
    [SerializeField]
    private LayerMask placement_layer_mask;

    [SerializeField]
    private float placeable_snapping_range = 8f;

    private void Start()
    {
        this.combat_handler = GetComponent<NetworkPlayerCombatHandler>();
        this.anim_logic = GetComponent<NetworkPlayerAnimationLogic>();
        this.bar_handler = GetComponentInChildren<panel_bar_handler>();
        this.npi = GetComponent<NetworkPlayerInventory>();
        this.valid_material = (Material)Resources.Load("Glow_green", typeof(Material));
        this.invalid_material = (Material)Resources.Load("Glow_red", typeof(Material));
    }

    private void Update()
    {
        if (networkObject.IsOwner) {
            if (bar_handler.gameObject.activeSelf) {
                checkInputBar();
                if (Input.GetButtonDown("Fire1"))
                {
                    if (this.activeTool!=null && combat_handler.is_allowed_to_attack_local())//za weapone se checkira v combat handlerju
                    {
                        ///poslat request da nrdimo swing z tem tool-om
                        networkObject.SendRpc(RPC_TOOL_USAGE_REQUEST, Receivers.Server);
                    }

                    else if (this.CurrentLocalPlaceable != null)
                    {
                        //ce ni valid, se itak poslje zadnji valid transform ker se je tko updejtal na koncu metode v update pri nastavlanju pozicije

                        Debug.Log("tukej bomo poslal rpc za postavlanje itema");
                        networkObject.SendRpc(RPC_PLACEMENTOF_ITEM_REQUEST, Receivers.Server, this.CurrentLocalPlaceable.transform.position, this.CurrentLocalPlaceable.transform.rotation);
                    }
                }
                else if (this.current_placeable_item != null) {
                    if(this.CurrentLocalPlaceable.GetComponent<LocalPlaceableHelper>()!=null) this.CurrentLocalPlaceable.GetComponent<LocalPlaceableHelper>().isSnapping = false;
                    handlePlaceableLocalPlacementSelection();
                }
            }
        }
    }

    /// <summary>
    /// lokalno izrise placeable karkoli ze pac hocmo postavt
    /// </summary>
    private void handlePlaceableLocalPlacementSelection()
    {
        RaycastHit h= local_MoveCurrentPlaceableObjectToMouseRay();
        rotatePlaceableWithMouseFree(GetAllowedAngleRotationFromCurrentPlaceable());

        if (currentTransformOfPlaceableIsValid(h))
        {
            this.previously_valid_position = this.CurrentLocalPlaceable.transform.position;
            this.previously_valid_rotation = this.CurrentLocalPlaceable.transform.rotation;
            this.currentPlaceableRenderer.material = this.valid_material;

        }
        else
        {
            if (this.previously_valid_position != Vector3.zero && this.previously_valid_rotation != Quaternion.identity)
            {
                this.CurrentLocalPlaceable.transform.position = this.previously_valid_position;
                this.CurrentLocalPlaceable.transform.rotation = this.previously_valid_rotation;
                
            }
            this.currentPlaceableRenderer.material = this.invalid_material;
        }
    }

    //vrne stopinje kolkr se loh obrne. ce je 0 ni limita, se loh obraca kolkr hoce
    private float GetAllowedAngleRotationFromCurrentPlaceable()
    {
        //karkoli se ne snappa trenutno lahko rotiramo in pade v prvi if. drugi so samo za izbiranje kote, potem ko je ze snappan gor
        if (current_placeable_item.snappableType == Item.SnappableType.none || current_placeable_item.snappableType == Item.SnappableType.free_in_range || current_placeable_item.snappableType == Item.SnappableType.wall_attachment_free || (!this.CurrentLocalPlaceable.GetComponent<LocalPlaceableHelper>().isSnapping))
        {
            return 0;
        }
        else if (current_placeable_item.snappableType == Item.SnappableType.foundation) return 90f;
        else return 180f;
    }


    /// <summary>
    /// klice se samo na lokalnemu ker dostopa do stvari k jih server ne vid, like raycast k smo g anrdil. server to posebej pohendla na podobn nacin
    /// </summary>
    /// <param name="h"></param>
    /// <returns></returns>
    private bool currentTransformOfPlaceableIsValid(RaycastHit h)
    {
        //nevem kk bom se zrihtov tole tbh
        if (current_placeable_item.snappableType == Item.SnappableType.none || current_placeable_item.snappableType == Item.SnappableType.free_in_range)
            if (Vector3.Distance(transform.position, this.CurrentLocalPlaceable.transform.position) < 5f && Vector3.Angle(Vector3.up, h.normal) < 50f)
                return true;
            else return false;
        else if (current_placeable_item.snappableType == Item.SnappableType.foundation) {
            if (Vector3.Distance(transform.position, this.CurrentLocalPlaceable.transform.position)<5f & currentPlaceableIsCollidingWithTerrain())
                return true;
            else return false;
        }

        Debug.LogError("we are trying to place a placeable for which we have not setup validity checks");
        return false;
    }

    private bool currentPlaceableIsCollidingWithTerrain()
    {
        return this.CurrentLocalPlaceable.GetComponent<LocalPlaceableHelper>().isCollidingWithTerrain();
    }

    /// <summary>
    /// i guess se klice na serverju, ker manjkajo podatki k jih ma lokaln player, server pa nima. posiljat pa tud nima smisla ker je tole samo za security da client ne pohacka
    /// </summary>
    /// <returns></returns>
    private bool currentTransformOfPlaceableIsValid()
    {
        //nevem kk bom se zrihtov tole tbh
        if (Vector3.Distance(transform.position, this.CurrentLocalPlaceable.transform.position) < 4f)
            return true;
        else return false;
    }

    private void rotatePlaceableWithMouseFree(float allowedAngleChunk)
    {
        if (allowedAngleChunk <1)
        {
            this.mouseWheelRotation = Input.mouseScrollDelta.y;
            this.CurrentLocalPlaceable.transform.Rotate(Vector3.up, this.mouseWheelRotation * 10);
        }
        else{
            this.mouseWheelRotation = Input.mouseScrollDelta.y;
            this.CurrentLocalPlaceable.transform.Rotate(CurrentLocalPlaceable.transform.up, (this.mouseWheelRotation/ this.mouseWheelRotation) * allowedAngleChunk);
        }
    }

    private RaycastHit local_MoveCurrentPlaceableObjectToMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f,0f));
        //ignorirat mora 9,10,13 ter tud vse kar spada pod UI, ker je canvas narisan cez ekran.
       /* int layermask1 = (1 << 9);
        int layermask4 = (1 << 5);
        int layermask2 = (1 << 10);
        int layermask3 =  (1 << 13);
        int finalLayermask = ~(layermask3 | layermask1 | layermask2 | layermask4 );*/
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, this.placement_layer_mask)) {
            //Debug.Log(hitInfo.collider.name);
            Vector3 offsetOfColliderHeight=Vector3.up* this.currentPlaceableCollider.size.y / 2;
            //pivot objekta je v sredini njegovga colliderja. tko da ga izrise not v zemljo. to rabmo compensatat
            
            SnappableObject s= GetClosestValidSnapPointInRange(hitInfo.point, this.current_placeable_item.snappableType);

            if (s == null)
            {
                if (this.current_placeable_item.snappableType == Item.SnappableType.none || this.current_placeable_item.snappableType == Item.SnappableType.free_in_range || this.current_placeable_item.snappableType == Item.SnappableType.foundation)
                {
                    if (!this.current_placeable_item.ignorePlacementNormal)
                    {
                        this.CurrentLocalPlaceable.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);//ce hocmo da je zmer alignan z terenom - chesti pa take stvari
                        offsetOfColliderHeight = Vector3.up * this.currentPlaceableCollider.size.y / 2;

                        offsetOfColliderHeight = hitInfo.normal * this.currentPlaceableCollider.size.y / 2;

                    }
                    this.CurrentLocalPlaceable.transform.position = hitInfo.point + offsetOfColliderHeight;
                }
            }
            else {//nalimej na tocko k smo jo dobil - snap sistem



                this.CurrentLocalPlaceable.transform.position = s.position;
                this.CurrentLocalPlaceable.transform.rotation = s.rotation;
                this.CurrentLocalPlaceable.GetComponent<LocalPlaceableHelper>().isSnapping = true;
            }
        }
        return hitInfo;
    }

    /// <summary>
    /// vrne najblizjo tocko kamor se lahko nas trenutno izban objekt snappa. na podlagi odgovora bomo postavli in rotiral objekt. Ce je valid al pa ne se ugotovi ksnej. to je zato da se foundation vid kam ga hoce postavt ampak se sezmer pobarva
    /// </summary>
    /// <param name="point"></param>
    /// <param name="snappableType"></param>
    /// <returns></returns>
    private SnappableObject GetClosestValidSnapPointInRange(Vector3 point, Item.SnappableType snappableType)
    {

        if (snappableType == Item.SnappableType.foundation)
        {//foundation se itak snappa na 4 strani, ista visina, rotacija ni vazna, vzamemo isto rotacijo zaenkrat.
            //ce je scale 1 potem je snappable point za naslednji foundation v vsako izmed 3 strani za |scale| dolzino, pod pogojem, da se collida z terenom
            float dolzinaStranice = currentPlaceableCollider.gameObject.transform.localScale.x;//je itak kocka
            

            NetworkPlaceable[] all_placeables = (NetworkPlaceable [])GameObject.FindObjectsOfType<NetworkPlaceable>();
            if (all_placeables == null) return null;
            NetworkPlaceable closest = FindClosestPlaceableFromArray(all_placeables, point, snappableType);
            if (closest == null) return null;
            //mamo samo 4 tocke. poiscmo najblizjo
            Vector3 center = closest.transform.position;
            Vector3 point1 = center + closest.transform.forward * dolzinaStranice;//naprej
            Vector3 point2 = center - closest.transform.forward * dolzinaStranice;//nazaj

            Vector3 point3 = center + closest.transform.right * dolzinaStranice;//desno
            Vector3 point4 = center - closest.transform.right * dolzinaStranice;//levo

            SnappableObject r = null;

            Vector3 rezz = point1;

            float min = Vector3.Distance(point, point1);
            if (Vector3.Distance(point, point2) < min) { min = Vector3.Distance(point, point2); rezz = point2; }
            if (Vector3.Distance(point, point3) < min) { min = Vector3.Distance(point, point3); rezz = point3; }
            if (Vector3.Distance(point, point4) < min) { min = Vector3.Distance(point, point4); rezz = point4; }

            r = new SnappableObject(rezz, closest.transform.rotation);//nevem ce je rotacija prav
            if(Vector3.Distance(point,r.position)<this.placeable_snapping_range)
                return r;
            

        }
        return null;
    }

    private NetworkPlaceable FindClosestPlaceableFromArray(NetworkPlaceable[] all_placeables, Vector3 point,  Item.SnappableType snappableType)
    {
        NetworkPlaceable kandidat=null;
        float current_min = float.MaxValue;
        float current_dist =0;
        foreach (NetworkPlaceable p in all_placeables) {
            if (p.snappableType == snappableType)
            {
                current_dist = (p.transform.position - point).sqrMagnitude;//Vector3.Distance je pocasen, ta je bols
                if (current_dist < current_min)
                {//current min je ze kvadrirana
                    kandidat = p;
                    current_min = current_dist;
                }
            }
        }
        return kandidat;
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
        //Debug.Log("not implemented yet");
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
                networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.All, (this.selected_index==-1)?"-1" : npi.predmeti_hotbar[this.selected_index].toNetworkString(), this.selected_index, (this.selected_index_shield == -1) ? "-1" : npi.predmeti_hotbar[this.selected_index_shield].toNetworkString(), selected_index_shield);
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
            Predmet p = Predmet.createNewPredmet(predmet1);


            //ce je enako stanje kot je zdj in je izbran indeks indeks placeable itema ne nrdimo nic ker to samo pomen da smo postavli en item z stacka k ga imamo. zdi se mal hacky ampak tak je. restructure code ksnej i guess - mogoce bo treba enako nrdit za puscice / javelins ksnej
            if(p!=null)
                if(p.item!=null)
                    if (p.item.type == Item.Type.placeable && this.current_placeable_item!=null)
                        if(p.item.id == this.current_placeable_item.id)
                            return;


            if (networkObject.IsOwner)
            {
                setSelectedItems(p, Predmet.createNewPredmet(predmet2));
                bar_handler.setSelectedSlots(index,index2);
            }
            else {
                setSelectedItems(p, Predmet.createNewPredmet(predmet2));
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


        clearAllPossiblySelected();//ne cleara shielda
        

        if (i != null)
        {
            if (i.item.type == Item.Type.tool)
            {
                SetToolSelected(i);
                //combat_handler.currently_equipped_weapon = null;

            }
            else if (i.item.type == Item.Type.weapon || i.item.type == Item.Type.ranged)
            {
                combat_handler.currently_equipped_weapon = i;

            }
            else if (i.item.type == Item.Type.placeable)
            {
                Debug.Log("lets try to place down " + i.item.Display_name);
                setPlaceableState(i);
            }
            else { Debug.Log("item youre trying to equip cannot be equipped : " + i.item.Display_name);

            }
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

    private void clearAllPossiblySelected()
    {
        clearPlaceableState();
        SetToolSelected(null);
        combat_handler.currently_equipped_weapon = null;
        combat_handler.currently_equipped_weapon = null;
    }

    private void clearPlaceableState()
    {
        this.inPlaceableMode = false;
        if (this.CurrentLocalPlaceable != null) Destroy(this.CurrentLocalPlaceable);
        this.CurrentLocalPlaceable = null;
        this.current_placeable_item = null;
        this.currentPlaceableCollider = null;
        this.currentPlaceableRenderer = null;
        this.activePlaceable = null;
    }

    private void setPlaceableState(Predmet i)
    {
        this.inPlaceableMode = true;
        this.CurrentLocalPlaceable = Instantiate(i.item.placeable_Local_object);
        this.current_placeable_item = i.item;
        this.activePlaceable = i;
        this.currentPlaceableCollider = this.CurrentLocalPlaceable.GetComponent<BoxCollider>();
        this.currentPlaceableRenderer = this.CurrentLocalPlaceable.GetComponent<MeshRenderer>();
        if (this.currentPlaceableRenderer == null) this.currentPlaceableRenderer= this.CurrentLocalPlaceable.GetComponentInChildren<MeshRenderer>();
        if (this.currentPlaceableRenderer == null) this.currentPlaceableRenderer = this.CurrentLocalPlaceable.GetComponent<Renderer>();
        if (this.currentPlaceableRenderer == null) this.currentPlaceableRenderer = this.CurrentLocalPlaceable.GetComponentInChildren<Renderer>();
        //SetToolSelected(null);
        //combat_handler.currently_equipped_weapon=null;
    }

    private void SetToolSelected(Predmet i) {
        this.activeTool = i;
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
    /// owner poslje request da zamahne z toolom. server mora prevert ce je vse OK  -(legitimnost ukaza) in poslje ukaz vsem da nj izvedejo swing oziroma da naj aktivirajo item k je u roki.
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
        if (i == null)
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

    #region BUILDING

    /// <summary>
    /// dobi od ownerja k ma uz rok nek placeable.
    /// </summary>
    /// <param name="args"></param>
    public override void PlacementofItemRequest(RpcArgs args)
    {
        

        /*ugotovmo kter item ma u roki
        spawnamo lokalni item na isti poiziciji, pogledamo ce je valid*
        -   ce ni valid je konc
        -   ce je valid networkInstantiatamo ta placeable in vrnemo playerju odgovor kšn zgleda zdj njegov nov hotbar.
    */
   
        if (networkObject.IsServer && args.Info.SendingPlayer.NetworkId == networkObject.Owner.NetworkId) {
            Debug.Log("server - placing "+this.current_placeable_item.Display_name);

            Vector3 pos = args.GetNext<Vector3>();
            Quaternion rot = args.GetNext <Quaternion>();

            //get current placeable predmet!
            Predmet p = this.activePlaceable;
            NetworkPlaceableInstantiationServer(p, pos, rot);
            this.npi.reduceCurrentActivePlaceable(this.selected_index);//sicer vrne bool da nam pove ce smo pobral celotn stack, ampak nima veze ker rabmo poslat update za kvantiteto v vsakem primeru.
                                                         //nastavi selected index na -1 ce smo pobral vse - da gre lepo v rpc             
            
            networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.All, (this.selected_index == -1) ? "-1" : npi.predmeti_hotbar[this.selected_index].toNetworkString(), this.selected_index, (this.selected_index_shield == -1) ? "-1" : npi.predmeti_hotbar[this.selected_index_shield].toNetworkString(), selected_index_shield);


        }



    }


    public void NetworkPlaceableInstantiationServer(Predmet p, Vector3 pos, Quaternion rot)
    {
        if (!networkObject.IsServer) { Debug.LogError("instanciacija na clientu ne na serverju!"); return; }
        int net_id = getPlaceableNetworkIdFromItem(p.item);
        if (net_id == -1) return;//item is not interactable object
        NetworkPlaceableBehavior b = NetworkManager.Instance.InstantiateNetworkPlaceable(net_id, pos, rot);

        //apply force on clients, sets predmet
        b.gameObject.GetComponent<NetworkPlaceable>().init(p);

    }

    private int getPlaceableNetworkIdFromItem(Item item)
    {
        GameObject[] prefabs = NetworkManager.Instance.NetworkPlaceableNetworkObject;

        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i].Equals(item.placeable_networked_object))
                return i;
        }

        Debug.LogWarning("Id of item not found.");
        return -1;

    }

    /// <summary>
    /// izvaja samo an serverju. klice se ko v npi porihtamo kej v zvezi z hotbarom ( pa se to ne povsod ker je bla ta metoda dodana ksnej).
    /// updejt itemov se poslje drugje. npi.sendInventoryUpdate...
    /// </summary>
    internal void sendBarUpdate()
    {
        if (this.selected_index != -1) {
            if (npi.predmeti_hotbar[this.selected_index] == null) this.selected_index = -1;
        }

        if (this.selected_index_shield != -1)
        {
            if (npi.predmeti_hotbar[this.selected_index_shield] == null) this.selected_index = -1;
            else if (npi.predmeti_hotbar[this.selected_index_shield].item.type != Item.Type.shield) this.selected_index_shield = -1;
        }

        if (networkObject.IsServer)
        {
            npi.sendNetworkUpdate(true, false);
            networkObject.SendRpc(RPC_BAR_SLOT_SELECTION_RESPONSE, Receivers.All, (this.selected_index == -1) ? "-1" : npi.predmeti_hotbar[this.selected_index].toNetworkString(), this.selected_index, (this.selected_index_shield == -1) ? "-1" : npi.predmeti_hotbar[this.selected_index_shield].toNetworkString(), selected_index_shield);
        }
    }

    #endregion
}

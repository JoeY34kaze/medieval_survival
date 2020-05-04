using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSiegeTrebuchet : NetworkedSiegeWeaponBehavior
{
    private int state = 0;
    private Animator anim;
    private float interactable_distance=15f;
    private bool in_animation = false;
    private NetworkContainer container;

    private GameObject shot_spawn_location;

    [SerializeField] private Item[] allowed_projectiles;

    [SerializeField]  private Item ready_shot = null;
    private float min_vertical_coeff = 0f;
    private float max_vertical_coeff = 2f;
    public  float current_vertical_coefficient = 1.0f;//tole nj bo med 0-2 ?


    public Transform platform;
    public direction_vector_helper direction;

    protected override void NetworkStart()
    {
        base.NetworkStart();
        // TODO:  Your initialization code that relies on network setup for this object goes here
        if (networkObject.IsServer)
        {
            networkObject.TakeOwnership();
            NetworkManager.Instance.Networker.playerConnected += (player, networker) =>
            {
                Debug.Log("Started sending trebuchet synch");
                MainThreadManager.Run(() => {
                    networkObject.SendRpc(RPC_INITIALIZATION, Receivers.Others, transform.position, transform.rotation, this.platform.rotation, state);
                });
            };
        }
    }

    private void Start()
    {
        this.anim = GetComponent<Animator>();
        this.container = GetComponent<NetworkContainer>();
        this.shot_spawn_location = GetComponentInChildren<shot_spawn_location>().gameObject;
    }

    public override void advance_state_request(RpcArgs args)
    {
        if (networkObject.IsServer)
        {
            if (is_player_allowed_to_interact_with_this(args.Info.SendingPlayer.NetworkId)) {
                try_to_advance_state();
            }
        }
        else return;
    }

    private void try_to_advance_state()
    {
        if (!in_animation) {
            if(this.state==0 && try_loading_next_shot() || ! (this.state==0))
                send_state_update((this.state + 1) % 3);

            //0 - base, ce poberemo vn ko je ze nalovdan gre nazaj tud
            //1 - reloading
            //2 - firing
        }
    }

    private bool try_loading_next_shot()
    {
        //sprozi se samo ko se lovda shot v state 0
        for (int i = 0; i < this.allowed_projectiles.Length; i++) {
            if (this.container.Remove(this.allowed_projectiles[i], 1))
            {
                this.ready_shot = this.allowed_projectiles[i];
                return true;
            }
        }
        this.ready_shot = null;
        return true; //ce hocemo da strelja v prazno. i think i want that


    }

    private bool is_player_allowed_to_interact_with_this(uint networkId)
    {
        return Vector3.Distance(transform.position, FindByid(networkId).transform.position) < this.interactable_distance;
    }

    /// <summary>
    /// klice animation event Trebuchet_Shot
    /// </summary>
    public void local_spawn_siege_projectile_server() {
        if (networkObject.IsServer) {
            try_firing_trebuchet();
        }
    }

    private void try_firing_trebuchet() {
        if (networkObject.IsServer) {
            if (this.ready_shot != null) {
                //instantiate the treb shot

                int net_id = get_id_for_instantiation_from_treb_shot(this.ready_shot.networked_physical_instantiated_object);
                if (net_id != -1)
                { //item is interactable object
                    NetworkedSiegeProjectileBehavior b = NetworkManager.Instance.InstantiateNetworkedSiegeProjectile(net_id, this.shot_spawn_location.transform.position);
                    //apply force on clients, sets predmet
                    Predmet predmet = new Predmet(this.ready_shot);
                    b.gameObject.GetComponent<Networked_siege_projectile>().init(predmet, get_spawn_location(), get_direction_vector(), get_force());
                }
            }
        }
    }

    private Vector3 get_spawn_location() {
        return this.shot_spawn_location.transform.position;
    }

    private Vector3 get_direction_vector() {
        
        return Vector3.Normalize(this.direction.getForward() + transform.up * Mathf.Clamp(this.current_vertical_coefficient,this.min_vertical_coeff,max_vertical_coeff));
    }

    private float get_force() {
        float weight = 100f;//neka vrednost da se prazen treb ne ubije
        foreach (Predmet p in this.container.getAllOfType(Item.Type.resource)) {
            weight += p.GetWeight();
        }
        return weight;
    }

    public static int get_id_for_instantiation_from_treb_shot(GameObject shot) {
        GameObject[] shots = NetworkManager.Instance.NetworkedSiegeProjectileNetworkObject;
        for (int i = 0; i < shots.Length; i++)
            if (shot.Equals(shots[i])) return i;

        Debug.LogError("NetworkId for siege projectile not ofund on instantiation!");
        return -1;
    }

    public GameObject FindByid(uint targetNetworkId) //koda kopširana povsod
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<NetworkPlayerStats>().Get_server_id() == targetNetworkId) return p;
        }
        return null;
    }

    /// <summary>
    /// updates the atributes specific to siege engine. placeable data is handled there
    /// </summary>
    /// <param name="args"></param>
    public override void atribute_update(RpcArgs args)
    {
       
        if (args.Info.SendingPlayer.IsHost) {

            this.state = args.GetNext<int>();
            this.anim.SetInteger("state", this.state);
            this.in_animation = true;
        }
    }

    internal void local_player_change_rotation_request()
    {
        networkObject.SendRpc(RPC_SIEGE_WEAPON_ROTATE_HORIZONTALLY, Receivers.Server, this.platform.rotation.eulerAngles.y);
    }

    internal Quaternion get_rotation_of_platform()
    {
        return this.platform.rotation;
    }

    /// <summary>
    /// sprozi se samo z animatorja po reloadu in po streljanju
    /// </summary>
    public void reset_animation_bool(int i) {
        this.in_animation = false;
        if (i != -1) this.state = 0;
    }

    private void send_state_update(int new_state) {
        if (networkObject.IsServer) {
            networkObject.SendRpc(RPC_ATRIBUTE_UPDATE, Receivers.All, new_state);
        }
    }

    internal void local_player_siege_weapon_advance_fire_state_request()
    {
        networkObject.SendRpc(RPC_ADVANCE_STATE_REQUEST, Receivers.Server);
    }

    internal void local_player_siege_weapon_change_trajectory_request()
    {
        networkObject.SendRpc(RPC_WEAPON_CHANGE_TRAJECTORY_REQUEST, Receivers.Server);
    }

    public override void weapon_change_trajectory_request(RpcArgs args)
    {
        if (networkObject.IsServer)
            if (is_player_allowed_to_interact_with_this(args.Info.SendingPlayer.NetworkId))
                this.current_vertical_coefficient = (this.current_vertical_coefficient + 1) % 3;
    }

    public override void siege_weapon_rotate_horizontally(RpcArgs args)
    {
        if (is_player_allowed_to_interact_with_this(args.Info.SendingPlayer.NetworkId))
        {
            networkObject.SendRpc(RPC_SIEGE_WEAPON_ROTATION_UPDATE, Receivers.All, Quaternion.Euler(this.platform.rotation.eulerAngles.x, args.GetNext<float>(), this.platform.rotation.eulerAngles.z));
        }
    }

    public override void siege_weapon_rotation_update(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost)
            this.platform.rotation = args.GetNext<Quaternion>();
    }

    internal void local_player_siege_weapon_open_container_request()
    {
        this.container.local_open_container_request();
    }

    internal void local_player_siege_weapon_pickup_request()
    {
        if (GetComponent<NetworkPlaceable>().get_player_who_placed_this() == networkObject.MyPlayerId) {
            networkObject.SendRpc(RPC_PICKUP_REQUEST, Receivers.Server);
        }
    }

    public override void pickup_request(RpcArgs args)
    {
        NetworkPlaceable pl = GetComponent<NetworkPlaceable>();
        if (pl.get_player_who_placed_this() == args.Info.SendingPlayer.NetworkId)
        {
            if (this.container.isEmpty())
            {
                Debug.Log("Picking up trebuchet..");
                FindByid(args.Info.SendingPlayer.NetworkId).GetComponent<NetworkPlayerInventory>().handleItemPickup(pl.p);
                networkObject.Destroy();
            }
        }
    }

    public override void initialization(RpcArgs args)
    {
        if (args.Info.SendingPlayer.IsHost) {
            transform.position = args.GetNext<Vector3>();
            transform.rotation = args.GetNext<Quaternion>();
            this.platform.rotation= args.GetNext<Quaternion>();
            this.state = (int)args.GetNext<byte>();
            this.anim.SetInteger("state", this.state); ;
        }
    }
}

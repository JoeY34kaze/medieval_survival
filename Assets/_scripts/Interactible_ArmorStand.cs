using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactible_ArmorStand : Interactable
{
    private NetworkArmorStand nas;

    /*
       case 0://head
    case 1://chest
    case 2://hands
    case 3://legs
    case 4://feet
    case 5://wep0
    case 6://wep1
    case 7://shield
    case 8://ranged

             */

    private void Awake()
    {
        this.nas = GetComponent<NetworkArmorStand>();
    }

    internal void local_player_interaction_ranged_request(uint server_id)
    {
        nas.local_interaction_request(8, server_id);
    }

    internal void local_player_interaction_shield_request(uint server_id)
    {
        nas.local_interaction_request(7, server_id);
    }

    internal void local_player_interaction_weapon0_request(uint server_id)
    {
        nas.local_interaction_request(5, server_id);
    }

    internal void local_player_interaction_feet_request(uint server_id)
    {
        nas.local_interaction_request(4, server_id);
    }

    internal void local_player_interaction_legs_request(uint server_id)
    {
        nas.local_interaction_request(3, server_id);
    }

    internal void local_player_interaction_hands_request(uint server_id)
    {
        nas.local_interaction_request(2, server_id);
    }

    internal void local_player_interaction_chest_request(uint server_id)
    {
        nas.local_interaction_request(1, server_id);
    }

    internal void local_player_interaction_helmet_request(uint server_id)
    {
        nas.local_interaction_request(0, server_id);
    }

    internal void local_player_interaction_give_all_request(uint server_id, NetworkPlayerInventory player_npi)
    {

        nas.local_interaction_give_all_request(server_id);
    }

    internal void local_player_interaction_get_all_request(uint server_id, NetworkPlayerInventory player_npi)
    {
        nas.local_interaction_get_all_request(server_id);
    }

    internal void local_player_interaction_swap_request(uint server_id)
    {
        nas.local_interaction_swap_request(server_id);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[Serializable] public  class PlayerSynchronizationContainer
{
    internal float pos_x;
    internal float pos_y;
    internal float pos_z;
    internal float rot_x;
    internal float rot_y;
    internal float rot_z;
    internal float rot_w;
    internal bool crouched;
     internal string displayName;
     internal bool downed;
     internal bool dead;
     internal float health;
     internal Predmet head;
     internal Predmet chest;
     internal Predmet hands;
     internal Predmet legs;
     internal Predmet feet;
     internal Predmet tool;
     internal byte combat_State;
     internal bool blocking;
     internal Predmet equipped_shield;
     internal Predmet equipped_weapon;
     internal byte direction;
     internal bool is_readying;
     internal bool ready_atk;
     internal bool exec_atk;
     internal int an_combat;
     internal int an_dir;
     internal bool an_shield;
     internal bool an_blocking;
     internal bool an_crouched;
     internal bool an_grounded;
     internal int an_weapon_anim_cl;


    public PlayerSynchronizationContainer(
            Vector3 pos,
            Quaternion rot,
            bool crouched,
            string displayName,
            bool downed,
            bool dead,
            float health,
            Predmet head,
            Predmet chest,
            Predmet hands,
            Predmet legs,
            Predmet feet,
            Predmet tool,
            byte combat_State,
            bool blocking,
            Predmet equipped_shield,
            Predmet equipped_weapon,
            byte direction,
            bool is_readying,
            bool ready_atk,
            bool exec_atk,
            int an_combat,
            int an_dir,
            bool an_shield,
            bool an_blocking,
            bool an_crouched,
            bool an_grounded,
            int an_weapon_anim_cl
        ) {
        this.pos_x= pos.x;
        this.pos_y = pos.y;
        this.pos_z = pos.z;
        this.rot_x = rot.x;
        this.rot_y = rot.y;
        this.rot_z = rot.z;
        this.rot_w = rot.w;
        this.crouched=crouched;
        this.displayName=displayName;
        this.downed=downed;
        this.dead=dead;
        this.health =health;
        this.head=head;
        this.chest=chest;
        this.hands=hands;
        this.legs=legs;
        this.feet=feet;
        this.tool=tool;
        this.combat_State=combat_State;
        this.blocking=blocking;
        this.equipped_shield=equipped_shield;
        this.equipped_weapon=equipped_weapon;
        this.direction=direction;
        this.is_readying=is_readying;
        this.ready_atk=ready_atk;
        this.exec_atk=exec_atk;
        this.an_combat=an_combat;
        this.an_dir=an_dir;
        this.an_shield=an_shield;
        this.an_blocking=an_blocking;
        this.an_crouched=an_crouched;
        this.an_grounded=an_grounded;
        this.an_weapon_anim_cl = an_weapon_anim_cl;

    }

    
}

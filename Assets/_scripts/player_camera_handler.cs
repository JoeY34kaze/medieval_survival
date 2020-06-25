using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;
using System;
using UMA.CharacterSystem;
using System.Collections.Generic;

public class player_camera_handler : NetworkPlayerCameraHandlerBehavior
{
    public Vector3 camera_starting_offset = new Vector3(0.0f, 0.0f, -2);
    public Vector3 camera_rotation_offset = Vector3.zero;
    public bool inverse_vertical = true;
    public bool freeLook = false;
    public Transform _camera_framework;
    public Camera player_cam;
    private bool network_handled = false;
    public bool pov;
    private DynamicCharacterAvatar avatar;

    // CE BO DAT KAMERO POD KOTOM JE TREBA POHENDLAT DA JE ZMER VODORAVNO KER DRUGAC JE NEKEJ WONKY
    protected override void NetworkStart()
    {
        base.NetworkStart();
        if (!networkObject.IsOwner) return;
        player_cam = Camera.main;
        player_cam.transform.parent = this._camera_framework;
        player_cam.transform.localPosition = Vector3.zero;
        player_cam.transform.localRotation = Quaternion.Euler(camera_rotation_offset);
        network_handled = true;
        this.pov = false;
    }

    void LateUpdate()
    {
        if (networkObject == null)
        {
            Debug.LogWarning("networkObject is null.");
            return;
        }
        if (networkObject.IsOwner && !network_handled)
        {
            // Debug.LogWarning("lateupdate is called before NetworkStart(). waiting..");
            return;
        }
        if (!networkObject.IsOwner) return;
        if (UILogic.hasControlOfInput) return;
        if (UILogic.Instance.hasOpenWindow) return;
        if (player_cam == null) return;

        update_player_input();

        moveCamera();
        check_for_collisions_and_handle_them();//da nemore player gledat cez steno
    }

    private void check_for_collisions_and_handle_them()
    {
        Vector3 start = Vector3.zero;//ampak ne na kameri, na parentu ali playerju
        Vector3 target = player_cam.transform.localPosition;
        //obe v world
        Vector3 world_start = transform.position+new Vector3(0,1.5f,0);
        Vector3 world_target = player_cam.transform.position;
        //raycast

        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1;

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(world_start, (world_target-world_start), out hit, Vector3.Distance(world_start, world_target), layerMask))
        {
            Debug.DrawRay(world_start, (world_target - world_start) * hit.distance, Color.red);
            Debug.Log("Did Hit");
            player_cam.transform.position = hit.point - Vector3.Normalize(world_target - world_start)*0.15f;
        }
        else
        {
            Debug.DrawRay(world_start, (world_target - world_start)*5, Color.white);
        }



    }

    private void update_player_input()
    {
        if (Input.GetButtonDown("TogglePOV"))
        {
            this.pov = !this.pov;
            firstPersonSetup(this.pov);
        }
    }

    /// <summary>
    /// hides head, torso and other unneccessary meshes.
    /// </summary>
    private void firstPersonSetup(bool first_person)
    {
        UILogic.localPlayerGameObject.transform.Find("UMARenderer").gameObject.SetActive(!first_person);
    }

    private void moveCamera()
    {
        if (this.pov) handle_move_camera_firstPerson();
        else handle_move_camera_third_person();
    }

    private void handle_move_camera_firstPerson()
    {
        player_cam.transform.localPosition = Vector3.zero;
        player_cam.transform.localRotation = Quaternion.Euler(camera_rotation_offset);

        //float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float turnAngle;
        if (inverse_vertical)
            turnAngle = mouseY * Prefs.mouse_sensitivity;
        else
            turnAngle = -mouseY * Prefs.mouse_sensitivity;
        Vector3 rotation = new Vector3(turnAngle, 0, 0);
        //CLAMP THE DAMN CAMERA
        _camera_framework.Rotate(rotation);
        Vector3 xx = _camera_framework.localEulerAngles;
        _camera_framework.localEulerAngles = new Vector3(xx.x, 0, 0);
        if (xx.x < 280 && xx.x > 180) _camera_framework.localEulerAngles = new Vector3(280, 0, 0);
        if (xx.x <= 180 && xx.x > 90) _camera_framework.localEulerAngles = new Vector3(90, 0, 0);
    }

    private void handle_move_camera_third_person()
    {
        player_cam.transform.localPosition = camera_starting_offset;
        player_cam.transform.localRotation = Quaternion.Euler(camera_rotation_offset);

        if (Input.GetButton("FreeLook"))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float turnAngleY;
            if (inverse_vertical)
                turnAngleY = mouseY * Prefs.mouse_sensitivity;
            else
                turnAngleY = -mouseY * Prefs.mouse_sensitivity;

            float turnAngleX = mouseX * Prefs.mouse_sensitivity;
            Vector3 rotation = new Vector3(turnAngleY, turnAngleX, 0);
            //CLAMP THE DAMN CAMERA
            _camera_framework.Rotate(rotation);
            Vector3 xx = _camera_framework.localEulerAngles;
            _camera_framework.localEulerAngles = new Vector3(xx.x, xx.y, 0);
            if (xx.x < 280 && xx.x > 180) _camera_framework.localEulerAngles = new Vector3(280, _camera_framework.localEulerAngles.y, 0);
            if (xx.x <= 180 && xx.x > 90) _camera_framework.localEulerAngles = new Vector3(90, _camera_framework.localEulerAngles.y, 0);
        }
        else {//horizontalna rotacija je u NetworkPlayerMovement. tam ubistvu playerja rotiramo in se kamera temu ustrezno prilagaja in ne obratno.
            //float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float turnAngle;
            if (inverse_vertical)
                turnAngle = mouseY * Prefs.mouse_sensitivity;
            else
                turnAngle = -mouseY * Prefs.mouse_sensitivity;
            Vector3 rotation = new Vector3(turnAngle, 0, 0);
            //CLAMP THE DAMN CAMERA
            _camera_framework.Rotate(rotation);
            Vector3 xx = _camera_framework.localEulerAngles;
            _camera_framework.localEulerAngles = new Vector3(xx.x, 0, 0);
            if (xx.x < 280 && xx.x > 180) _camera_framework.localEulerAngles = new Vector3(280, 0, 0);
            if (xx.x <= 180 && xx.x > 90) _camera_framework.localEulerAngles = new Vector3(90, 0, 0);
        } 
    }
}
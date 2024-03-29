﻿using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using UnityEngine;

public class player_camera_handler : NetworkPlayerCameraHandlerBehavior
{
    public Vector3 camera_starting_offset = new Vector3(0.0f, 0.0f, -2);
    public Vector3 camera_rotation_offset = Vector3.zero;


    public bool inverse_vertical = true;

    public bool freeLook = false;

    public Transform _camera_framework;
    public Transform point_to_look_at_on_player;
    public float mouse_sensitivity_multiplier = 1.0f;



    // CE BO DAT KAMERO POD KOTOM JE TREBA POHENDLAT DA JE ZMER VODORAVNO KER DRUGAC JE NEKEJ WONKY
    protected override void NetworkStart()
    { 
        base.NetworkStart();

    
        if (!networkObject.IsOwner) return;
        Camera.main.transform.parent = this._camera_framework;
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.Euler(camera_rotation_offset);
    }

    void LateUpdate()
    {
        if (!networkObject.IsOwner) return;
        Camera.main.transform.localPosition = camera_starting_offset;
        Camera.main.transform.localRotation = Quaternion.Euler(camera_rotation_offset);

        //float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        //float rotX = mouseX * mouse_sensitivity_multiplier * Time.deltaTime;
        //float rotY = mouseY * mouse_sensitivity_multiplier * Time.deltaTime;

        float turnAngle;
        if (inverse_vertical)
            turnAngle = mouseY * mouse_sensitivity_multiplier;
        else
            turnAngle = -mouseY * mouse_sensitivity_multiplier;

        //Vector3 euler = _camera_framework.eulerAngles + turnAngle.eulerAngles;

        Vector3 rotation = new Vector3(turnAngle, 0, 0);
        //CLAMP THE DAMN CAMERA

        _camera_framework.Rotate(rotation);


        Vector3 xx = _camera_framework.localEulerAngles;

        _camera_framework.localEulerAngles = new Vector3(xx.x, 0, 0);
        if (xx.x < 280 && xx.x > 180) _camera_framework.localEulerAngles = new Vector3(280, 0, 0);
        if (xx.x <= 180 && xx.x > 90) _camera_framework.localEulerAngles = new Vector3(90, 0, 0);
    }
}

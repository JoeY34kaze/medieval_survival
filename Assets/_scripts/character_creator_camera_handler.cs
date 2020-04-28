using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character_creator_camera_handler : MonoBehaviour
{
    public Transform player_position;
    private float distance;



    void Update()
    {
        if (CharacterCreationHandler.mouse_is_over_canvas)
            return;

        if (Input.GetMouseButtonDown(0) )
        {
            DisableMouse();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            enableMouse();
        }
        else if (Input.GetMouseButton(0)) {
            rotateAround();
            moveVertically();
        }
        zoom(Input.GetAxis("Mouse ScrollWheel"));
    }

    private void moveVertically()
    {
        float Y = -Input.GetAxis("Mouse Y");
        
        if((transform.position + transform.up * Y*Time.deltaTime).y >40.5f && (transform.position + transform.up * Y * Time.deltaTime).y<42f)
            transform.position = transform.position + transform.up * Y * Time.deltaTime;
    }

    private void zoom(float v)
    {
       // Debug.Log(Distance(transform.position, player_position));

        if (v == 0) return;
        if (Distance(new Vector2(transform.position.x, transform.position.z), player_position) > 0.2f) {
            Vector3 new_position;
            if (v > 0)
            {
                new_position = transform.position + 0.2f * transform.forward;
            }
            else {
                new_position = transform.position - 0.2f * transform.forward;
            }
            if (Distance(new_position,player_position) > 0.2f && Distance(new_position, player_position)<2.5f)
                transform.position = new_position;

        }
    }

    private float Distance(Vector3 new_position, Transform player_position)
    {
        return Vector2.Distance(new Vector2(new_position.x, new_position.z), new Vector2(player_position.position.x, player_position.position.z));
    }

    private void rotateAround()
    {
        float mouseX = Input.GetAxis("Mouse X");
        transform.RotateAround(player_position.position, player_position.up, mouseX);
        
    }

    public void enableMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

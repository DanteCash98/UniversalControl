using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : Mirror.NetworkBehaviour
{
    [Range(0.1f,10f)]
    public float speed;
    void HandleMovement() 
    {
        if(isLocalPlayer)
        {
            float moveHoriz = Input.GetAxis("Horizontal");
            float moveVert = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(moveHoriz, 0, moveVert) * speed;
            transform.position = transform.position + movement;
        }
    }

    void Update() 
    {
        HandleMovement();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Control Script/FPS Input")]
public class FPSInput : MonoBehaviour
{
    public float speed = 0f;
    private float _gravity = -9.8f;

    private CharacterController _charController;

    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();    
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Get rid of this?
        /*float deltaX = speed * Input.GetAxis("Horizontal");
        float deltaZ = speed * Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);
        movement.y = _gravity;

        movement *= Time.deltaTime;
        movement = transform.TransformDirection(movement);
        _charController.Move(movement);*/

        //Debug.Log("speed: " + speed);
        //Debug.Log("X speed: " + deltaX);
        //Debug.Log("Z speed: " + deltaZ);
    }
}

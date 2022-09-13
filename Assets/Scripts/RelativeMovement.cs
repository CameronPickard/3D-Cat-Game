using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class controls movement relative to the current camera position
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RelativeMovement : MonoBehaviour
{
    [SerializeField] private Transform target; //this script needs a reference to the object to move relative to - this should be the CAMERA
    private ControllerColliderHit _contact; //Neded to store collision data between functions (p.156)
    private float rotationSpeed = 3.0f;
    private float moveSpeed = 6.0f;
    //private float maxHorizontalSpeed = 0.1f;

    //Jumping/faling constants
    private float jumpSpeed = 15.0f;
    private float gravity = -9.8f;
    private float terminalVelocity = -10.0f;
    private float minFall = -1.5f;

    private float _vertSpeed;

    private CharacterController _charController;

    private Animator _animator;
    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();
        _vertSpeed = minFall;
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero; //Start with vector (0, 0, 0) and add movement components progressively

        float horInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
		if (horInput != 0 || vertInput != 0)
		{
			#region Horizontal Movement
			//only handle movement while arrow keys are pressed
			//movement.x = (Math.Min(horInput, maxHorizontalSpeed)) * moveSpeed;
			movement.x = horInput * moveSpeed;
			movement.z = vertInput * moveSpeed;
			//Debug.Log("Max Horizontal Speed: " + maxHorizontalSpeed);
			Debug.Log("X speed: " + movement.x);
			Debug.Log("Z speed: " + movement.z);


			movement = Vector3.ClampMagnitude(movement, moveSpeed); //needed to ensure diagonal movement does not exceed the max speed

			Quaternion tmp = target.rotation; //Keep the inital rotation to restore after finishing with the target object
			target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
			movement = target.TransformDirection(movement); //Transform movement direction from Local to Global coordinates
			target.rotation = tmp;

			#region When not using Lerp
			//transform.rotation = Quaternion.LookRotation(movement); //LookRotation() calculates a quaternion facing in that direction
			#endregion

			#region Using Lerp
			Quaternion direction = Quaternion.LookRotation(movement);
			transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotationSpeed * Time.deltaTime);
			#endregion
			#endregion Horizontal Movement
		}
        _animator.SetFloat("Speed", movement.sqrMagnitude);
        if (movement.sqrMagnitude > 0)
        {
            Debug.Log("total speed: " + movement.sqrMagnitude);
        }
        #region Vertical Movement
        bool hitGround = false;
        RaycastHit hit;

        //If falling, and our center is touching ground, set "hitGround" = true, potentially
        if(_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit)) {
            float check = (_charController.height + _charController.radius) / 1.9f;
            hitGround = hit.distance <= check;
            //explanation on p.157
		}

        if(hitGround) 
        {
            if(Input.GetButtonDown("Jump")) {
                _vertSpeed = jumpSpeed;
			}
            else {
                _vertSpeed = minFall;
                _animator.SetBool("Jumping", false);
			}
		} 
        else 
        {
            _vertSpeed += gravity * 5 * Time.deltaTime;
            if(_vertSpeed < terminalVelocity) { 
                _vertSpeed = terminalVelocity;
			}
            if(_contact!=null) {
                _animator.SetBool("Jumping", true);
			}
            //If grounded means: if bottom-curvature of capsule is touching something
            if (_charController.isGrounded) {
                float contactDot = Vector3.Dot(movement, _contact.normal);
                //What is a normal? https://answers.unity.com/questions/588972/what-is-a-normal.html
                //What is a dot product of vector? https://byjus.com/maths/dot-product-of-two-vectors/#:~:text=Dot%20Product%20of%20Vectors%3A,the%20direction%20of%20the%20vectors.
                //What is the magnitude of a vector? https://www.cuemath.com/magnitude-of-a-vector-formula/

                if (contactDot < 0) {
                    movement = _contact.normal * moveSpeed;
                    
				}
                else {
                    movement += _contact.normal * moveSpeed;
				}
			}
		}
        movement.y = _vertSpeed;
        #endregion Vertical Movement

        //Apply Movement
        movement *= Time.deltaTime;
        _charController.Move(movement);
    }

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
        _contact = hit;
	}
}

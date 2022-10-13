using UnityEngine;

/// <summary>
/// This class controls movement relative to the current camera position
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RelativeMovement : MonoBehaviour
{
	#region Constants
    /// <summary> Force to apply to other objects</summary>
	private readonly float _pushForce = 3.0f;
    /// <summary> Visual rotation speed of the model as it's moving (does not affect the movement of the object)</summary>
    private readonly float _rotationSpeed = 3.0f;
    /// <summary> Movement speed </summary>
    private readonly float _moveSpeed = 6.0f;
    /// <summary> Steepest angle of a slope this character should be allowed to walk down. *Make this a positive number* </summary>
    private readonly int _steepestWalkableAngle = 45;

    #region Jumping/falling constants
    /// <summary> Jumping speed, correlates with how high the object can rise when it jumps</summary>
    private readonly float _jumpSpeed = 15.0f;
    /// <summary> Gravity constant </summary>
    private readonly float _gravity = -9.8f;
    /// <summary> Terminal falling velocity </summary>
    private readonly float _terminalFallVelocity = -10.0f;
    /// <summary> Initial falling velocity </summary>
    private readonly float _initialFallVelocity = -1.5f;
    #endregion Jumping/falling constants
    #endregion Constants

    /// <summary> Reference to the target (the camera) that this object is moving relative to </summary>
    [SerializeField] private Transform target;
    /// <summary> Needed to store collision data between functions (p.156) </summary>
    private ControllerColliderHit _contact;
    /// <summary> Current vertical speed (with regard to falling & jumping </summary>
    private float _vertSpeed;
    /// <summary> True if this object was in a state of "falling" last frame </summary>
    private bool _wasFallingLastFrame;
    /// <summary> CharacterController object, used for movement </summary>
    private CharacterController _charController;
    /// <summary> Animator object </summary>
    private Animator _animator;

    #region Debug Booleans
    [SerializeField] private bool _isDebugging = false;
	#endregion Debug Booleans

	// Start is called before the first frame update
	void Start()
    {
        _charController = GetComponent<CharacterController>();
        _vertSpeed = _initialFallVelocity;
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero; //Start with vector (0, 0, 0) and add movement components progressively
        bool isFallingThisFrame = _wasFallingLastFrame;
        float horInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
		if (horInput != 0 || vertInput != 0)
		{
			#region Ground Movement
			//only handle movement while arrow keys are pressed
			movement.x = horInput * _moveSpeed;
			movement.z = vertInput * _moveSpeed;
			movement = Vector3.ClampMagnitude(movement, _moveSpeed); //needed to ensure diagonal movement does not exceed the max speed

            if (_isDebugging) { Debug.Log("Camera rotation: " + target.rotation); }
			Quaternion tmp = target.rotation; //Keep the inital rotation to restore after finishing with the target object
			target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0); //We need to imagine the target is level with the player, for a couple lines
			movement = target.TransformDirection(movement); //Transform movement direction from Local to Global coordinates, and do it from the perspective of the target
			target.rotation = tmp; //restore target's

			#region When not using Lerp
			//transform.rotation = Quaternion.LookRotation(movement); //LookRotation() calculates a quaternion facing in that direction
			#endregion

			#region Using Lerp
			Quaternion direction = Quaternion.LookRotation(movement);
			transform.rotation = Quaternion.Lerp(transform.rotation, direction, _rotationSpeed * Time.deltaTime);
			#endregion
			#endregion Ground Movement
		}
        _animator.SetFloat("Speed", movement.sqrMagnitude);
        if (_isDebugging && movement.sqrMagnitude > 0)
        {
            Debug.Log("total speed: " + movement.sqrMagnitude);
            Debug.Log("isDebugging: " + _isDebugging);
        }
        #region Jumping/Falling Movement
        bool hitGround = false;
        RaycastHit hit;

        //If falling, and our center of our bottom is touching ground (or very close), set "hitGround" = true
        if(_vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit)) {
            float check = (_charController.height + _charController.radius) / 1.9f;
            if (_isDebugging) { Debug.Log("Char Height: " + _charController.height + ", Radius: " + _charController.radius + ", Check: " + check + ", hit.distance: " + hit.distance); }
            hitGround = hit.distance <= check;
            //explanation on p.157
		}

        if(hitGround) //if bottom-center of capsule is touching ground
        {
            if(Input.GetButtonDown("Jump")) {
                _vertSpeed = _jumpSpeed;
                isFallingThisFrame = true;
            }
            else {
                _vertSpeed = _initialFallVelocity;
                _animator.SetBool("Jumping", false);
                isFallingThisFrame = false;
			}
		} 
        else 
        {
            _vertSpeed += _gravity * 5 * Time.deltaTime;
            if(_vertSpeed < _terminalFallVelocity) { 
                _vertSpeed = _terminalFallVelocity;
			}

            if (!_wasFallingLastFrame)
            {
                //TODO: Iron out this downard-slope-walking math
                //float angleOfGroundNormal;
                //float normalGroundMagnitude
                //Vector3 normalXAndZ = new Vector3(movement.)
                //If character wasn't falling last frame... there's a chance they're just walking down a slope. Let's try shoving the character downward for a single frame
                Vector3 horizontalDisplacement = new Vector3(movement.x, 0, movement.z);
                float groundSpeed = horizontalDisplacement.sqrMagnitude;
                _vertSpeed = -3 * groundSpeed * Mathf.Tan(_steepestWalkableAngle); //TODO: assumes "worst-case" scenario (walking down the slope as steeply as possible) - which may not match what's actually happening
                isFallingThisFrame = true;
            }
            else if (_contact != null)
            {
                _animator.SetBool("Jumping", true);

                //If grounded means: if bottom-curvature of capsule is touching something
                if (_charController.isGrounded)
                {
                    float contactDot = Vector3.Dot(movement, _contact.normal);
                    //What is a normal? https://answers.unity.com/questions/588972/what-is-a-normal.html
                    //What is a dot product of vector? https://byjus.com/maths/dot-product-of-two-vectors/#:~:text=Dot%20Product%20of%20Vectors%3A,the%20direction%20of%20the%20vectors.
                    //What is the magnitude of a vector? https://www.cuemath.com/magnitude-of-a-vector-formula/

                    if (contactDot < 0)
                    {
                        //the surface normal and our object are facing away from each other
                        movement = _contact.normal * _moveSpeed; //going up a slope. Dont let player keep their movement
                    }
                    else
                    {
                        //the surface normal and our object are facing towards each other
                        movement += _contact.normal * _moveSpeed; //going down a steep slope. Let player keep their movement
                    }
                }
            }
		}
        movement.y = _vertSpeed;
        #endregion Jumping/Falling Movement

        //Apply Movement
        movement *= Time.deltaTime;
        _charController.Move(movement);
        _wasFallingLastFrame = isFallingThisFrame;
    }

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
        _contact = hit;

        Rigidbody body = hit.collider.attachedRigidbody;
        if(body != null && !body.isKinematic) {
            body.velocity = hit.moveDirection * _pushForce;
		}
	}
}

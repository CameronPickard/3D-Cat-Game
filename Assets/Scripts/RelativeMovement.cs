﻿using UnityEngine;
using Baracuda.Monitoring;
using UnityEngine.UI;

/// <summary>
/// This class controls movement relative to the current camera position
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class RelativeMovement : MonitoredBehaviour
{
	#region Constants
    /// <summary> Force to apply to other objects</summary>
	private readonly float _pushForce = 3.0f;
    /// <summary> Visual rotation speed of the model as it's moving (does not affect the movement of the object)</summary>
    private readonly float _visualRotationSpeed = 15.0f;
    /// <summary> Movement speed </summary>
    private readonly float _moveSpeed = 7.0f;
    /// <summary> Launch speed </summary>
    private readonly float _launchSpeed = 10.0f;
    /// <summary> Steepest angle of a slope this character should be allowed to walk down. *Make this a positive number* </summary>
    private readonly int _steepestWalkableAngle = 45;
    /// <summary> Max wiggle speed. 3.0 seems to be nice.</summary>
    private readonly float _maxWiggleSpeed = 3.0f;
    /// <summary> Set how wiggling is controlled </summary>
    private readonly WiggleMechanics _howToWiggle = WiggleMechanics.Hybrid;
    private readonly WiggleInput _wiggleInputButton = WiggleInput.LeftStick;


    #region Jumping/falling constants
    [MonitorField]
    public bool hitGround = false;
    /// <summary> Jumping speed, correlates with how high the object can rise when it jumps</summary>
    public float _jumpSpeed = 12.5f;
    /// <summary> Gravity constant </summary>
    private readonly float _gravity = -9.8f;
    /// <summary> Terminal falling velocity </summary>
    private readonly float _terminalFallVelocity = -10.0f;
    /// <summary> Initial falling velocity </summary>
    private readonly float _initialFallVelocity = -1.5f;
    /// <summary> Coyote time frames </summary>
    private readonly int _coyotoeTimeFrames = 5;
    #endregion Jumping/falling constants
    #endregion Constants

    /// <summary> Reference to the target (the camera) that this object is moving relative to </summary>
    [SerializeField] private Transform target;
    [SerializeField] private GameObject virtualCameraObject;
    private OrbitVirtualCamera _virtualCameraScript;
    [SerializeField] private GameObject basicCameraObject;
    private BasicCamera _basicCameraScript;
    [SerializeField] private GameObject jumpPowerBar;
    private Slider _jumpPowerSlider;
    /// <summary> Needed to store collision data between functions (p.156) </summary>
    private ControllerColliderHit _contact;
    /// <summary> Current vertical speed (with regard to falling & jumping </summary>
    [MonitorField]
    private float _vertSpeed;
    
    /// <summary> Number of frames in the air </summary>
    private int _framesInTheAir;
    /// <summary> True if in the air due to jumping </summary>
    private bool _inMiddleOfJumping = false;
    [MonitorField]
    private bool _inMiddleOfLaunching = false;
    private int _framesSinceLaunch = 6;
    private float _launchXSpeed = 0.0f;
    private float _launchZSpeed = 0.0f;
    /// <summary> True if this object was in a state of "falling" last frame </summary>
    [MonitorField]
    private bool _wasFallingLastFrame;
    private bool _isCrouching;
    [MonitorField]
    private float _wiggleSpeed = 0.0f;
    [MonitorField]
    private float _wiggleXInput = 0.0f;
    [MonitorField]
    private int _framesOfNoWiggle = 0;
    private int _nextExpectedWiggleDirection = 0;
    /// <summary> CharacterController object, used for movement </summary>
    private CharacterController _charController;
    /// <summary> Animator object </summary>
    private Animator _animator;

    private Vector3 _spawnPosition;

    [MonitorField]
    private float _crouchInput;
    #region Debug Booleans
    [SerializeField] private bool _isDebugging = false;
	#endregion Debug Booleans

	// Start is called before the first frame update
	void Start()
    {
        _charController = GetComponent<CharacterController>();
        _vertSpeed = _initialFallVelocity;
        _animator = GetComponent<Animator>();
        _virtualCameraScript = virtualCameraObject.GetComponent<OrbitVirtualCamera>();
        _basicCameraScript = basicCameraObject.GetComponent<BasicCamera>();
        _jumpPowerSlider = jumpPowerBar.GetComponent<Slider>();
        _spawnPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero; //Start with vector (0, 0, 0) and add movement components progressively
        if (_framesSinceLaunch < 6 || _inMiddleOfLaunching) {
            _inMiddleOfLaunching = true;
            movement.x = _launchXSpeed;
            movement.z = _launchZSpeed;
		}
        bool isFallingThisFrame = _wasFallingLastFrame;
        float horInput = Input.GetAxis("Horizontal");
        float vertInput = Input.GetAxis("Vertical");
        _crouchInput = Input.GetAxis("Crouch");
        _animator.SetBool("Crouching", false); //might get overwritten later
		if (!_inMiddleOfLaunching && _framesSinceLaunch==6 && (horInput != 0 || vertInput != 0))
		{
            #region Ground Movement
            //only handle movement while arrow keys are pressed
            _launchXSpeed = 0.0f;
            _launchZSpeed = 0.0f;
			movement.x = horInput * _moveSpeed;
			movement.z = vertInput * _moveSpeed;
			movement = Vector3.ClampMagnitude(movement, _moveSpeed); //needed to ensure diagonal movement does not exceed the max speed

            #region Rotate the camera
			if (_isDebugging) { Debug.Log("Camera rotation: " + target.rotation); }
			Quaternion tmp = target.rotation; //Keep the inital rotation to restore after finishing with the target object
			target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0); //We need to imagine the target is level with the player, for a couple lines
			movement = target.TransformDirection(movement); //Transform movement direction from Local to Global coordinates, and do it from the perspective of the target
			target.rotation = tmp; //restore target's
			#endregion Rotate the camera

			#region Rotate the player
			#region When not using Lerp
			//transform.rotation = Quaternion.LookRotation(movement); //LookRotation() calculates a quaternion facing in that direction
			#endregion

			#region Using Lerp
            if(!_isCrouching) 
            {
                Quaternion direction = Quaternion.LookRotation(movement);
                transform.rotation = Quaternion.Lerp(transform.rotation, direction, _visualRotationSpeed * Time.deltaTime);
            }
			#endregion
			#endregion Rotate the player
            #endregion Ground Movement
		}
		_animator.SetFloat("Speed", movement.sqrMagnitude);
        if (_isDebugging && movement.sqrMagnitude > 0)
        {
            Debug.Log("total speed: " + movement.sqrMagnitude);
            Debug.Log("isDebugging: " + _isDebugging);
        }
        #region Jumping/Falling Movement
        hitGround = false;
        RaycastHit hit;
        bool isJumpButtonDown = Input.GetButtonDown("Jump");
        //If not rising, and our center of our bottom is touching ground (or very close), set "hitGround" = true
        bool wasNotRisingLastFrame = _vertSpeed < 0;
        if(wasNotRisingLastFrame && _framesSinceLaunch==6 && Physics.Raycast(transform.position, Vector3.down, out hit)) 
        {
            float check = (_charController.height + _charController.radius) / 12.5f;
            if (_isDebugging) { Debug.Log("Char Height: " + _charController.height + ", Radius: " + _charController.radius + ", Check: " + check + ", hit.distance: " + hit.distance); }
            hitGround = hit.distance <= check;
            if (hitGround && _inMiddleOfLaunching)
            {
                _inMiddleOfLaunching = false;
            }
            //explanation on p.157
		}

        bool inCoyoteTime = !_inMiddleOfLaunching && !hitGround && !_inMiddleOfJumping && _framesInTheAir < _coyotoeTimeFrames;
        if ((hitGround || inCoyoteTime) && isJumpButtonDown && !_isCrouching && !_inMiddleOfLaunching && _framesSinceLaunch == 6) 
        {
            _vertSpeed = _jumpSpeed;
            isFallingThisFrame = true;
            _inMiddleOfJumping = true;
        }
        else if(hitGround) //if bottom-center of capsule is touching ground
        {
            _vertSpeed = _initialFallVelocity;
            _animator.SetBool("Jumping", false);
            isFallingThisFrame = false;
            _inMiddleOfJumping = false;
            _framesInTheAir = 0;
            if (_crouchInput == 1.00f)
            {
                if (_wiggleInputButton == WiggleInput.LeftStick) setCrouchingValues(true, horInput);
                else if (_wiggleInputButton == WiggleInput.RightTrigger) setCrouchingValues(true, Input.GetAxis("RightTrigger"));
                if (isJumpButtonDown) 
                {
                    //LAUNCH HIM

                    //ver.1 
                    /*Vector3 launchDirection = _virtualCameraCineObj.LookAt.transform.position - normalCameraObject.transform.position;
                    movement = Vector3.ClampMagnitude(launchDirection, _launchSpeed * _wiggleSpeed);
                    _vertSpeed = movement.y;
                    isFallingThisFrame = true;
                    _inMiddleOfJumping = true;*/

                    //ver.2
                    
                    movement = target.TransformDirection(new Vector3(0.0f, 0.0f, _launchSpeed * _wiggleSpeed));
                    movement.y = movement.y + 12;
                    _vertSpeed = movement.y;
                    isFallingThisFrame = true;
                    _inMiddleOfJumping = true;
                    _inMiddleOfLaunching = true;
                    _framesSinceLaunch = 0;
                    _launchXSpeed = movement.x;
                    _launchZSpeed = movement.z;
                    setCrouchingValues(false);

                }
                return;
            }
            else {
                setCrouchingValues(false);
            }
        } 
        else 
        {
            _framesInTheAir++;
            setCrouchingValues(false);
            _vertSpeed = Mathf.Max(_terminalFallVelocity, _vertSpeed + _gravity * 5 * Time.deltaTime);

            if(_inMiddleOfLaunching && _framesSinceLaunch==6) 
            {
                isFallingThisFrame = true;    //not important for anything specific. Just good practice to have this set right.
			}
            if (!_wasFallingLastFrame && _framesSinceLaunch==6 && !_inMiddleOfLaunching)
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
                if (_charController.isGrounded && !_inMiddleOfLaunching) //TODO: _launching check should be improved. Just helps prevent edge-jitters during launching for Demo purposes
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
        if(_framesSinceLaunch!=6) { _framesSinceLaunch++; }
        #endregion Jumping/Falling Movement

        //Apply Movement
        movement *= Time.deltaTime;
        _charController.Move(movement);
        _wasFallingLastFrame = isFallingThisFrame;

		#region Death Plane Code
        //Since people keep killing themselves in the Demo, Im gonna build in a min-height check that forces the character to respawn.
        if(transform.position.y < -10.0f) { transform.position = _spawnPosition; }
        #endregion
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
        _contact = hit;

        Rigidbody body = hit.collider.attachedRigidbody;
        if(body != null && !body.isKinematic) {
            body.velocity = hit.moveDirection * _pushForce;
		}
	}

    private void setCrouchingValues(bool isCrouchingNow, float horInput = 0.0f) 
    {
        _wiggleXInput = horInput;
        if(isCrouchingNow) 
        {
            _isCrouching = true;
            _animator.SetBool("Crouching", _crouchInput == 1.00f);
            //_animator.SetFloat("Wiggling", horInput);
            if(_howToWiggle==WiggleMechanics.Hold) 
            {
                //Ver. 1 - Holding left or right
                if (horInput > 0.01 || horInput < -0.01) { _wiggleSpeed = Mathf.Min(3.0f, Mathf.Max(0.5f, _wiggleSpeed) + 0.008f); }
                else { _wiggleSpeed = 0.0f; }
            }
            else if(_howToWiggle==WiggleMechanics.Mash) 
            {
                //Ver. 2 - Mashing left and right
                float horInputAbs = Mathf.Abs(horInput);
                if (horInputAbs < 0.01 || (_nextExpectedWiggleDirection!= 0 && horInput*_nextExpectedWiggleDirection < 0.01))
                {
                    _framesOfNoWiggle++;
                    if (_framesOfNoWiggle > 30) { 
                        _wiggleSpeed = 0.0f;
                        _nextExpectedWiggleDirection = 0;
                    }
                }
                else
                {
                    if (horInputAbs < 0.7)
                    {
                        _wiggleSpeed += 0.06f;
                    }
                    else
                    {
                        _wiggleSpeed += 0.15f;
                    }
                    Debug.Log("Flick strength: " + horInput);
                    if(horInput>0.0) { _nextExpectedWiggleDirection = -1; }
                    else { _nextExpectedWiggleDirection = 1; }
                    _framesOfNoWiggle = 0;
                    _wiggleSpeed = Mathf.Min(3.0f, Mathf.Max(0.5f, _wiggleSpeed));
                }
			}
            else if(_howToWiggle==WiggleMechanics.Hybrid) {
                float amountToAdd = 0.0f;
                float horInputAbs = Mathf.Abs(horInput);
                if (horInputAbs < 0.01) { _framesOfNoWiggle++; }
                else { _framesOfNoWiggle = 0; }
                if (_framesOfNoWiggle > 7)
                {
                    _wiggleSpeed = 0.0f;
                    _nextExpectedWiggleDirection = 0;
                }
                else 
                {
                    amountToAdd = 0.4f;
                    //Ver. 2 - Mashing left and right
                    if (_nextExpectedWiggleDirection != 0 && horInput * _nextExpectedWiggleDirection < 0.01)
                    {
                        //nada
                    }
                    else
                    {
                        if (horInputAbs < 0.7)
                        {
                            amountToAdd += 1.6f;
                        }
                        else
                        {
                            amountToAdd += 2.8f;
                        }
                        Debug.Log("Flick strength: " + horInput);
                        if (horInput > 0.0) { _nextExpectedWiggleDirection = -1; }
                        else { _nextExpectedWiggleDirection = 1; }
                    }
                    _wiggleSpeed = Mathf.Min(3.0f, Mathf.Max(0.65f, _wiggleSpeed + amountToAdd * Time.deltaTime));
                }
            }
            _animator.SetFloat("wigglingSpeed", _wiggleSpeed);
            _virtualCameraScript.SetCrouching(true);
            _basicCameraScript.SetCrouching(true);
            _jumpPowerSlider.value = _wiggleSpeed * 10;
        }
        else 
        {
            _isCrouching = false;
            _wiggleSpeed = 0.0f;
            _nextExpectedWiggleDirection = 0;
            _framesOfNoWiggle = 0;
            _virtualCameraScript.SetCrouching(false);
            _basicCameraScript.SetCrouching(false);
            _jumpPowerSlider.value = 0.0f;
        }
	}

    public void setCrouchingRotation(Vector3 rotation) 
    {
        if (_isCrouching) { transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, rotation.y, transform.rotation.eulerAngles.z); }
	}
}

enum WiggleMechanics {
    Hold,
    Mash,
    Hybrid
}

enum WiggleInput {
    LeftStick,
    RightTrigger
}
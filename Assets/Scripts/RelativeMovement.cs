using UnityEngine;
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
    //[MonitorField]
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
    [MonitorField]
    private PlayerState _playerState;
    private Vector3 _movement;
    private float _horInput;
    private float _vertInput;
    /// <summary> Current vertical speed (with regard to falling & jumping </summary>
    //[MonitorField]
    private float _vertSpeed;
    
    /// <summary> Number of frames in the air </summary>
    private int _framesInTheAir;
    /// <summary> True if in the air due to jumping </summary>
    private bool _inMiddleOfJumping = false;
    //[MonitorField]
    private bool _inMiddleOfLaunching = false;
    //[MonitorField]
    private int _framesSinceLaunch = 6;
    private float _launchXSpeed = 0.0f;
    private float _launchZSpeed = 0.0f;
    /// <summary> True if this object was in a state of "falling" last frame </summary>
    //[MonitorField]
    private bool _wasFallingLastFrame;
    private bool _isCrouching;
    [MonitorField]
    private float _wiggleSpeed = 0.0f;
    private float _wiggleXInput = 0.0f;
    private int _framesOfNoWiggle = 0;
    private int _nextExpectedWiggleDirection = 0;
    /// <summary> CharacterController object, used for movement </summary>
    private CharacterController _charController;
    /// <summary> Animator object </summary>
    private Animator _animator;

    private Vector3 _spawnPosition;

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
        _movement = Vector3.zero; //Start with vector (0, 0, 0) and add movement components progressively
        _horInput = Input.GetAxis("Horizontal");
        _vertInput = Input.GetAxis("Vertical");
        _crouchInput = Input.GetAxis("Crouch");
        _animator.SetBool("Crouching", false); //might get overwritten later
        bool isFallingThisFrame = false;
        if (_framesSinceLaunch < 6) {
            //State: JustLaunched = True / Response
            _playerState = PlayerState.JustLaunched;
            _inMiddleOfLaunching = true;
        }
        else {
            _inMiddleOfLaunching = false;
		}

        if (_playerState == PlayerState.JustLaunched)
        {
            //Check if we should grab a ledge
            RaycastHit possibleWallHit;
            RaycastHit possibleFloorHit;
            if (Physics.Raycast(transform.position + new Vector3(0.0f, 0.5f, 0.0f), transform.forward, out possibleWallHit))
            {
                //State: Grounded = True
                float checkPassDistance = 3.0f;
                bool hitWall = possibleWallHit.distance <= checkPassDistance;
                if (hitWall)
                {

                    Debug.Log("GRAB THE WALL");

                    Vector3 invisibleFloatingPointAboveSurface =
                        transform.position +
                        new Vector3(
                            (float)((possibleWallHit.distance + 0.1) * possibleWallHit.normal.x * -1),
                            (float)1.0,
                            (float)((possibleWallHit.distance + 0.1) * possibleWallHit.normal.z * -1)
                        );

                    Debug.Log("Current transform: " + transform.position);
                    Debug.Log("Invisible floor point: " + invisibleFloatingPointAboveSurface);
                    if (Physics.Raycast(invisibleFloatingPointAboveSurface, Vector3.down, out possibleFloorHit))
                    {
                        if (possibleFloorHit.point.y > possibleWallHit.point.y)
                        {
                            Debug.Log("Grab the ledge!!!!");
                            _playerState = PlayerState.GrabLedgeStart;
                            GrabLedgeStartResponse(possibleFloorHit.point);
                            return;
                        }
                    }
                }
            }
        }
        #region Jumping/Falling Movement
        hitGround = false;
        RaycastHit hit;
        bool isJumpButtonDown = Input.GetButtonDown("Jump");
        //If not rising, and our center of our bottom is touching ground (or very close), set "hitGround" = true
        bool wasNotRisingLastFrame = _vertSpeed < 0;
        if(wasNotRisingLastFrame && _framesSinceLaunch==6 && Physics.Raycast(transform.position, Vector3.down, out hit)) 
        {
            //State: Grounded = True
            float check = (_charController.height + _charController.radius) / 12.5f;
            hitGround = hit.distance <= check;
            //explanation on p.157
		}

        bool inCoyoteTime = !_inMiddleOfLaunching && !hitGround && !_inMiddleOfJumping && _framesInTheAir < _coyotoeTimeFrames;
        if(inCoyoteTime) 
        {
            _playerState = PlayerState.CoyoteTime;
        }
        if ((_playerState == PlayerState.OnGround || _playerState == PlayerState.CoyoteTime) && isJumpButtonDown && _playerState != PlayerState.Crouching && !_inMiddleOfLaunching && _framesSinceLaunch == 6) 
        {
            _playerState = PlayerState.JumpStart;
        }
        else if(hitGround) //if bottom-center of capsule is touching ground
        {
            //State: Grounded response
            _vertSpeed = _initialFallVelocity;
            _animator.SetBool("Jumping", false);
            _inMiddleOfJumping = false;
            _wasFallingLastFrame = false;
            _framesInTheAir = 0;
            if (_crouchInput == 1.00f)
            {
                _playerState = PlayerState.Crouching;
                if (isJumpButtonDown) 
                {
                    _playerState = PlayerState.LaunchStart;
                    //State: LaunchStart = True
                }
            }
            else {
                _playerState = PlayerState.OnGround; //unnecessary probably
            }
        } 
        else 
        {
            _framesInTheAir++;
            setCrouchingValues(false);
            _vertSpeed = Mathf.Max(_terminalFallVelocity, _vertSpeed + _gravity * 5 * Time.deltaTime);
            isFallingThisFrame = true;
            if (!_wasFallingLastFrame && _vertSpeed < 0 && _framesSinceLaunch==6 && _playerState != PlayerState.JustLaunched)
            {
                //State: SlopeFrame
                _playerState = PlayerState.SlopeFrame;
            }
            else if (_contact != null && (_playerState != PlayerState.JustLaunched && _framesSinceLaunch == 6))
            {
                //State: Falling
                _playerState = PlayerState.Falling;
            }
		}
        #endregion Jumping/Falling Movement

		#region Death Plane Code
        //Since people keep killing themselves in the Demo, Im gonna build in a min-height check that forces the character to respawn.
        if(transform.position.y < -10.0f) {
            _playerState = PlayerState.DeathPlaneHit; 
        }

        #endregion Death Plane Code

        //Reset _wasFallingLastFrame..
        if (isFallingThisFrame) _wasFallingLastFrame = true;
        else { _wasFallingLastFrame = false; }

        switch (_playerState){
            case PlayerState.CoyoteTime:
                CoyoteTimeResponse();
                break;
            case PlayerState.Crouching:
                CrouchingResponse();
                break;
            case PlayerState.DeathPlaneHit:
                DeathPlaneHitResponse();
                break;
            case PlayerState.Falling:
                FallingResponse();
                break;
            case PlayerState.JumpStart:
                JumpStartResponse();
                break;
            case PlayerState.JustLaunched:
                JustLaunchedResponse();
                break;
            case PlayerState.LaunchStart:
                LaunchStartResponse();
                break;
            case PlayerState.OnGround:
                OnGroundResponse();
                break;
            case PlayerState.SlopeFrame:
                SlopeFrameResponse();
                break;
            default:
                break;
		}

        //Apply Movement
        _movement.y = _vertSpeed;
        _movement *= Time.deltaTime;
        if (_framesSinceLaunch != 6) { _framesSinceLaunch++; }
        _charController.Move(_movement);
    }

	#region State Responses
    private void OnGroundResponse()
    {
        handleHorizontalMovement();
        handleCameraRotation();
        rotatePlayerBasedOnMovement();
        setCrouchingValues(false);
        _animator.SetFloat("Speed", _movement.sqrMagnitude);
    }
    private void CrouchingResponse() 
    {
        //State: Crouching = True
        _animator.SetBool("Crouching", true); //might get overwritten later
        if (_wiggleInputButton == WiggleInput.LeftStick) setCrouchingValues(true);
        //else if (_wiggleInputButton == WiggleInput.RightTrigger) setCrouchingValues(true, Input.GetAxis("RightTrigger"));
    }
    private void JumpStartResponse() 
    {
        handleHorizontalMovement();
        handleCameraRotation();
        rotatePlayerBasedOnMovement();
        setCrouchingValues(false);
        //Jump-specific code
        _vertSpeed = _jumpSpeed;
        _inMiddleOfJumping = true;

    }
    private void CoyoteTimeResponse() 
    {
        handleHorizontalMovement();
        handleCameraRotation();
        rotatePlayerBasedOnMovement();
        setCrouchingValues(false);
    }
    private void FallingResponse() 
    {
        handleHorizontalMovement();
        handleCameraRotation();
        rotatePlayerBasedOnMovement();
        setCrouchingValues(false);

        _animator.SetBool("Jumping", true);

        //If grounded means: if bottom-curvature of capsule is touching something
        if (_charController.isGrounded && !_inMiddleOfLaunching) //TODO: _launching check should be improved. Just helps prevent edge-jitters during launching for Demo purposes
        {
            float contactDot = Vector3.Dot(_movement, _contact.normal);
            //What is a normal? https://answers.unity.com/questions/588972/what-is-a-normal.html
            //What is a dot product of vector? https://byjus.com/maths/dot-product-of-two-vectors/#:~:text=Dot%20Product%20of%20Vectors%3A,the%20direction%20of%20the%20vectors.
            //What is the magnitude of a vector? https://www.cuemath.com/magnitude-of-a-vector-formula/

            if (contactDot < 0)
            {
                //the surface normal and our object are facing away from each other
                _movement = _contact.normal * _moveSpeed; //going up a slope. Dont let player keep their movement
            }
            else
            {
                //the surface normal and our object are facing towards each other
                _movement += _contact.normal * _moveSpeed; //going down a steep slope. Let player keep their movement
            }
        }
    }
    private void LaunchStartResponse() 
    {
        //LAUNCH HIM

        //ver.1 
        /*Vector3 launchDirection = _virtualCameraCineObj.LookAt.transform.position - normalCameraObject.transform.position;
        movement = Vector3.ClampMagnitude(launchDirection, _launchSpeed * _wiggleSpeed);
        _vertSpeed = movement.y;
        _inMiddleOfJumping = true;*/

        //ver.2

        _movement = target.TransformDirection(new Vector3(0.0f, 0.0f, _launchSpeed * _wiggleSpeed));
        _movement.y = _movement.y + 12;
        _vertSpeed = _movement.y;
        _inMiddleOfJumping = true;
        _inMiddleOfLaunching = true;
        _framesSinceLaunch = 0;
        _launchXSpeed = _movement.x;
        _launchZSpeed = _movement.z;
        _animator.SetBool("Jumping", true);
        setCrouchingValues(false);
    }
    private void JustLaunchedResponse() 
    { 
        _inMiddleOfLaunching = true;
        _movement.x = _launchXSpeed;
        _movement.z = _launchZSpeed;
    }
    private void SlopeFrameResponse() 
    {
        handleHorizontalMovement();
        handleCameraRotation();
        rotatePlayerBasedOnMovement();
        setCrouchingValues(false);
        //TODO: Iron out this downard-slope-walking math
        //float angleOfGroundNormal;
        //float normalGroundMagnitude
        //Vector3 normalXAndZ = new Vector3(movement.)
        //If character wasn't falling last frame... there's a chance they're just walking down a slope. Let's try shoving the character downward for a single frame
        Vector3 horizontalDisplacement = new Vector3(_movement.x, 0, _movement.z);
        float groundSpeed = horizontalDisplacement.sqrMagnitude;
        _vertSpeed = -3 * groundSpeed * Mathf.Tan(_steepestWalkableAngle); //TODO: assumes "worst-case" scenario (walking down the slope as steeply as possible) - which may not match what's actually happening

        Debug.Log("Dumb slope code has run");
    }

    private void GrabLedgeStartResponse(Vector3 newPosition)
    {
        transform.position = newPosition;
        _vertSpeed = _terminalFallVelocity;
        _framesSinceLaunch = 6;
    }
    private void DeathPlaneHitResponse() 
    {
        transform.position = _spawnPosition;
        setCrouchingValues(false);
    }
	#endregion State Responses

    private void handleHorizontalMovement() 
    {
        //only handle movement while arrow keys are pressed
        _launchXSpeed = 0.0f;
        _launchZSpeed = 0.0f;
        _movement.x = _horInput * _moveSpeed;
        _movement.z = _vertInput * _moveSpeed;
        _movement = Vector3.ClampMagnitude(_movement, _moveSpeed); //needed to ensure diagonal movement does not exceed the max speed
    }

    private void handleCameraRotation() {
        if (_isDebugging) { Debug.Log("Camera rotation: " + target.rotation); }
        Quaternion tmp = target.rotation; //Keep the inital rotation to restore after finishing with the target object
        target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0); //We need to imagine the target is level with the player, for a couple lines
        _movement = target.TransformDirection(_movement); //Transform movement direction from Local to Global coordinates, and do it from the perspective of the target
        target.rotation = tmp; //restore target's

        #region Rotate the player
        #region When not using Lerp
        //transform.rotation = Quaternion.LookRotation(movement); //LookRotation() calculates a quaternion facing in that direction
        #endregion
    }

    private void rotatePlayerBasedOnMovement() 
    {
        if(_movement.x != 0.0f || _movement.z != 0.0f) 
        {
            Quaternion direction = Quaternion.LookRotation(_movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, _visualRotationSpeed * Time.deltaTime);
        }
    }
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
        _contact = hit;

        Rigidbody body = hit.collider.attachedRigidbody;
        if(body != null && !body.isKinematic) {
            body.velocity = hit.moveDirection * _pushForce;
		}
	}

    private void setCrouchingValues(bool isCrouchingNow) 
    {
        _wiggleXInput = _horInput;
        if(isCrouchingNow) 
        {
            _isCrouching = true;
            _animator.SetBool("Crouching", _crouchInput == 1.00f);
            //_animator.SetFloat("Wiggling", horInput);
            if(_howToWiggle==WiggleMechanics.Hold) 
            {
                //Ver. 1 - Holding left or right
                if (_horInput > 0.01 || _horInput < -0.01) { _wiggleSpeed = Mathf.Min(3.0f, Mathf.Max(0.5f, _wiggleSpeed) + 0.008f); }
                else { _wiggleSpeed = 0.0f; }
            }
            else if(_howToWiggle==WiggleMechanics.Mash) 
            {
                //Ver. 2 - Mashing left and right
                float horInputAbs = Mathf.Abs(_horInput);
                if (horInputAbs < 0.01 || (_nextExpectedWiggleDirection!= 0 && _horInput * _nextExpectedWiggleDirection < 0.01))
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
                    if(_horInput > 0.0) { _nextExpectedWiggleDirection = -1; }
                    else { _nextExpectedWiggleDirection = 1; }
                    _framesOfNoWiggle = 0;
                    _wiggleSpeed = Mathf.Min(3.0f, Mathf.Max(0.5f, _wiggleSpeed));
                }
			}
            else if(_howToWiggle==WiggleMechanics.Hybrid) {
                float amountToAdd = 0.0f;
                float horInputAbs = Mathf.Abs(_horInput);
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
                    if (_nextExpectedWiggleDirection != 0 && _horInput * _nextExpectedWiggleDirection < 0.01)
                    {
                        //nada
                    }
                    else
                    {
                        if (horInputAbs < 0.7)
                        {
                            amountToAdd += 2.4f;
                        }
                        else
                        {
                            amountToAdd += 3.8f;
                        }
                        if (_horInput > 0.0) { _nextExpectedWiggleDirection = -1; }
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

enum PlayerState
{
    OnGround,
    Crouching,
    JumpStart,
    CoyoteTime,
    Falling,
    LaunchStart,
    JustLaunched,
    SlopeFrame,
    GrabLedgeStart,
    DeathPlaneHit,
    Unknown
}
#endregion
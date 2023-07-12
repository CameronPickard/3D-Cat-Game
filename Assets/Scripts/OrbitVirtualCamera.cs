using Baracuda.Monitoring;
using Cinemachine;
using System;
using UnityEngine;

/// <summary>
/// 3rd person camera that orbits around the player character.
/// Created in Chapter 7, pg 146.
/// </summary>
public class OrbitVirtualCamera : MonitoredBehaviour
{
	#region Consts
	private static float maxPosCameraRotationX = 75.0f;
    private static float maxNegCameraRotationX = -50.0f;
    private static float rotSpeed = 1.2f;
    private static bool InvertedY = false;
    #endregion Consts

    #region Serialized Fields 
    [SerializeField]
    private GameObject _player;
    #endregion Serialized Fields

    #region Scripts
    private RelativeMovement _relativeMovementScript;
    private CameraMarkBehavior _cameraMarkBehavior;
    #endregion Scripts

    private float _rotY;
    [MonitorField]
    private float _rotX;
    [MonitorField]
    private Vector3 cameraTransformPosition;
    private Vector3 _offset;

    private CinemachineVirtualCamera _camera;
    private Transform _followTransform;
    private bool _isCrouching;

    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        _followTransform = _camera.Follow;

        //Get the various scripts
        _cameraMarkBehavior = _camera.LookAt.GetComponent<CameraMarkBehavior>();
        _relativeMovementScript = _player.GetComponent<RelativeMovement>();
    }

    private void Update()
    {
        if (!_cameraMarkBehavior.needToFocus)
        {
            _rotY = Input.GetAxis("Right Joy X") * rotSpeed * 3; //mouse input
            _rotX = Input.GetAxis("Right Joy Y") * rotSpeed * 3 * (InvertedY ? -1 : 1); //mouse input
                                                                                        //_rotX = Math.Min(_rotX, maxPosCameraRotationX);
                                                                                        //_rotX = Math.Max(_rotX, maxNegCameraRotationX);


            _followTransform.rotation = Quaternion.Euler(_followTransform.rotation.eulerAngles.x + _rotX, _followTransform.rotation.eulerAngles.y + _rotY, 0);
            transform.rotation = Quaternion.Euler(0, _followTransform.transform.rotation.eulerAngles.y, 0);
        }
        //if(_cameraMarkBehavior.hitGround) { _camera.riority
    }

	private void LateUpdate()
	{
        if (_isCrouching && !_cameraMarkBehavior.needToFocus)
        {
            _relativeMovementScript.setCrouchingRotation(transform.rotation.eulerAngles);
        }
    }

	public void SetCrouching(bool isCrouchingNow) 
    {
        if (_isCrouching == isCrouchingNow) return;

        _isCrouching = isCrouchingNow;
        _cameraMarkBehavior.SetCrouching(_isCrouching);
	}
}

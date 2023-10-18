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
    private static float rotSpeed = 200.0f;
    private static bool InvertedY = false;
    private static bool InvertedX = true;
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

    [MonitorField]
    private Quaternion _followQuaternion;
    [MonitorField]
    private Vector3 _followEuler;

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
            float rotSpeedAdjusted = rotSpeed * Time.deltaTime;
            _rotY = Input.GetAxis("Right Joy X") * rotSpeedAdjusted * (InvertedX ? 1 : -1); //mouse input
            _rotX = Input.GetAxis("Right Joy Y") * rotSpeedAdjusted * (InvertedY ? -1 : 1); //mouse input
                                                                                        //_rotX = Math.Min(_rotX, maxPosCameraRotationX);
                                                                                        //_rotX = Math.Max(_rotX, maxNegCameraRotationX);

            float newXEuler = _followTransform.rotation.eulerAngles.x + _rotX;
            if(newXEuler > 89.0f && newXEuler < 180.0f) {
                newXEuler = 89.0f;
			}
            else if(newXEuler < 271.0f && _followTransform.rotation.eulerAngles.x >= 180.0f) {
                newXEuler = 271.0f;
            }
            _followTransform.rotation = Quaternion.Euler(newXEuler, _followTransform.rotation.eulerAngles.y + _rotY, 0);
            _followQuaternion = _followTransform.rotation;
            _followEuler = _followTransform.rotation.eulerAngles;
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

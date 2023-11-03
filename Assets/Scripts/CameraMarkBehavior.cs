using Baracuda.Monitoring;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMarkBehavior : MonitoredBehaviour
{
    [SerializeField]
    private Transform _followTransform;
    [SerializeField]
    private GameObject _followObject;
    private RelativeMovement _playerScript;
    public bool hitGround;
    private Vector3 _initialPositionDifference;
    private Vector3 _initialRotationEulerDifference;
    private float _yPos;
    private bool _fallingApparently = false;
    [SerializeField]
    private Camera _camera;
    //[MonitorField]
    private Vector3 _viewportPosition;
    private bool _isCrouching;
    private float _visualFocusSpeed = 18.0f;
    //[MonitorField]
    public bool needToFocus = false;
    // Start is called before the first frame update
    void Start()
    {
        _yPos = transform.position.y;
        _initialPositionDifference = _followTransform.position - transform.position;
        _initialRotationEulerDifference = _followTransform.rotation.eulerAngles - transform.rotation.eulerAngles;

        //Get the player's relative movemenet script. The player's camera mark depends on some current state stuff.
        _playerScript = _followObject.GetComponent<RelativeMovement>();
        //CinemachineVirtualCamera vc = 

    }

    // Update is called once per frame
    private void Update()
    {
        //Jump-Enhanced logic
        Vector3 newPos = _followTransform.position - _initialPositionDifference;
        _viewportPosition = _camera.WorldToViewportPoint(_followTransform.position);
        hitGround = _playerScript.hitGround;
        if (hitGround) {
            _fallingApparently = false;
            _yPos = _followTransform.position.y - _initialPositionDifference.y;
        }
        else if (_viewportPosition.y < 0.2 || _viewportPosition.y > 0.9 || _fallingApparently) 
        {
            _fallingApparently = true;
            _yPos = _followTransform.position.y - _initialPositionDifference.y; 
        }
        transform.position = new Vector3(newPos.x, _yPos, newPos.z);

        if(needToFocus)
        {
            float initialYDifference = _initialRotationEulerDifference.y;
            Quaternion fullBackRotation = Quaternion.Euler(transform.eulerAngles.x, _followTransform.rotation.eulerAngles.y - initialYDifference, transform.eulerAngles.z);
            if(Math.Round(transform.rotation.eulerAngles.y,2) != Math.Round(fullBackRotation.eulerAngles.y,2))
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, fullBackRotation, _visualFocusSpeed * Time.deltaTime);
            }
            else {
                needToFocus = false;
			}
        }
        //Basic logic
        //transform.position = _followTransform.position - _initialPositionDifference;
    }

    public void SetCrouching(bool isCrouchingNow)
    {
        if (_isCrouching == isCrouchingNow) return;

        _isCrouching = isCrouchingNow;
        if (_isCrouching)
        {
            //Position the camera behind the player. Maybe zoom in.
            needToFocus = true;
        }
        else
        {
            needToFocus = false;
            //Done crouching. Zoom out I guess?
        }
    }
}


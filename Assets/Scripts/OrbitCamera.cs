using Baracuda.Monitoring;
using System;
using UnityEngine;

/// <summary>
/// *****DEPRECATED. Look at OrbitVirtualCamera.cs******
/// 3rd person camera that orbits around the player character.
/// Created in Chapter 7, pg 146.
/// </summary>
public class OrbitCamera : MonitoredBehaviour
{
    private static float maxPosCameraRotationX = 75.0f;
    private static float maxNegCameraRotationX = -50.0f;
    [SerializeField] private Transform target;

    public float rotSpeed = 2.5f;

    private bool InvertedY = false;
    private float _rotY;
    [MonitorField]
    private float _rotX;
    [MonitorField]
    private Vector3 cameraTransformPosition;
    private Vector3 _offset;

    // Start is called before the first frame update
    void Start()
    {
        _rotY = transform.eulerAngles.y;
        _offset = target.position - transform.position;
    }

	private void LateUpdate()
	{
        _rotY += Input.GetAxis("Right Joy X") * rotSpeed * 3; //mouse input
        _rotX += Input.GetAxis("Right Joy Y") * rotSpeed * 3 * (InvertedY ? -1 : 1); //mouse input
        _rotX = Math.Min(_rotX, maxPosCameraRotationX);
        _rotX = Math.Max(_rotX, maxNegCameraRotationX);

        //_rotY += Input.GetAxis("Mouse X") * rotSpeed * 3; //mouse input
        //_rotX += Input.GetAxis("Mouse Y") * rotSpeed * 3 * (InvertedY ? -1 : 1); //mouse input

        //Debug.Log("Orbit Camera - rotX: " + _rotX);

        Quaternion rotation = Quaternion.Euler(_rotX, _rotY, 0);
        //Pg 147
        //"Multiplying a position vector by a quaternion results in a position that's shifted over according to that rotation"
        transform.position = target.position - (rotation * _offset);
   
        //cache the new position for debugging purposes :^}
        cameraTransformPosition = transform.position;

        //Make the camera look at the player... which will cause it's rotation to change
        transform.LookAt(target);
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3rd person camera that orbits around the player character.
/// Created in Chapter 7, pg 146.
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target;

    public float rotSpeed = 1.5f;

    private bool InvertedY = true;
    private float _rotY;
    private float _rotX;
    private Vector3 _offset;

    // Start is called before the first frame update
    void Start()
    {
        _rotY = transform.eulerAngles.y;
        _offset = target.position - transform.position;
    }

	private void LateUpdate()
	{
        _rotY += Input.GetAxis("Mouse X") * rotSpeed * 3; //mouse input
        _rotX += Input.GetAxis("Mouse Y") * rotSpeed * 3 * (InvertedY ? -1 : 1); //mouse input

        Debug.Log("Orbit Camera - rotX: " + _rotX);

        Quaternion rotation = Quaternion.Euler(_rotX, _rotY, 0);
        //Pg 147
        //"Multiplying a position vector by a quaternion results in a position that's shifted over according to that rotation"
        transform.position = target.position - (rotation * _offset);
        transform.LookAt(target);
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}

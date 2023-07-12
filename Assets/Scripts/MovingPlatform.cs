using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float resetTime;
    public float smooth;
    [SerializeField] private Transform movingPlatform;
    [SerializeField] private Transform position1;
    [SerializeField] private Transform position2;
    private Vector3 newPosition;
    private string currentState;
    


    // Start is called before the first frame update
    void Start()
    {
        ChangeTarget();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movingPlatform.position = Vector3.Lerp(movingPlatform.position, newPosition, smooth * Time.deltaTime);
    }

    void ChangeTarget() 
    {
        if (currentState == "Moving to Position 1") 
        {
            currentState = "Moving to Position 2";
            newPosition = position2.position;
        }
        else if (currentState == "Moving to Position 2") 
        {
            currentState = "Moving to Position 1";
            newPosition = position1.position;
        }
        else if (currentState == null) 
        {
            currentState = "Moving to Position 2";
            newPosition = position2.position;
        }
        Invoke("ChangeTarget", resetTime);
	}
}

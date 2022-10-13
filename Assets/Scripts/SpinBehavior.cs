using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for objects that just spin
/// </summary>
public class SpinBehavior : MonoBehaviour
{
    /// <summary> Rotation speed </summary>
    private int _rotationSpeed = 60;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0);
    }
}

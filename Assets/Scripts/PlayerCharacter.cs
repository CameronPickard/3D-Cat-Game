using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior controlling the player's living status
/// </summary>
public class PlayerCharacter : MonoBehaviour
{
    /// <summary>Current health</summary>
    private int _health;

	#region Scripts
	private DeviceOperator _deviceOperator;
    private RelativeMovement _relativeMovement;
	#endregion

	// Start is called before the first frame update
	void Start()
    {
        _health = 3;
        _deviceOperator = gameObject.GetComponent<DeviceOperator>();
        _relativeMovement = gameObject.GetComponent<RelativeMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Hurt the player by substracting from its health
    /// </summary>
    /// <param name="damage">Amount of damage to do</param>
    public void Hurt(int damage)
    {
        _health -= damage;
        Debug.Log("Health: " + _health);
    }
}

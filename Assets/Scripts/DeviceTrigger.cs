using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for a trigger-object. Will send messages out to designated "target" objects when the trigger is activated.
/// </summary>
public class DeviceTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] targets;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
        foreach (GameObject target in targets)
        {
            target.SendMessage("Activate");
        }
	}

	private void OnTriggerExit(Collider other)
	{
		foreach (GameObject target in targets) {
            target.SendMessage("Deactivate");
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for objects that act as "device operators." Can send "Operate" messages out to other objects
/// </summary>
public class DeviceOperator : MonoBehaviour
{
    public float radius = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire3")) {
            Collider[] hitColliders =
                Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hitCollider in hitColliders) {
                Vector3 direction = hitCollider.transform.position - transform.position;
                float dotProduct = Vector3.Dot(transform.forward, direction);
                Debug.Log("Person x " + hitCollider.name + " dot product: " + dotProduct);
                //if (Vector3.Dot(transform.forward, direction) > .5f) {
                if (dotProduct > .5f)
                {
                    hitCollider.SendMessage("Operate",
                    SendMessageOptions.DontRequireReceiver);
                }
			}
		}
    }
}

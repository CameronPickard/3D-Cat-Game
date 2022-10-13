using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for collectible items
/// </summary>
public class CollectibleItem : MonoBehaviour
{

    [SerializeField] private string itemName;
 
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
        Debug.Log("Item collected: " + itemName);
        Destroy(this.gameObject);
    }
}

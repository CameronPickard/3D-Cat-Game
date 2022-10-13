using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reacts to actions inflicted by other objects (such as getting hit by another object)
/// </summary>
public class ReactiveTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// React to a hit that inflicted damage on this object
    /// </summary>
    public void ReactToHit()
    {
        WanderingAI behavior = GetComponent<WanderingAI>();
        if (behavior != null)
        {
            behavior.SetAlive(false); //Marking the WanderingAI as dead so it stops moving around
        }
        StartCoroutine(Die());
    }

    /// <summary>
    /// Die (rotate and stay still for a few seconds, before being destroyed in the scene)
    /// </summary>
    private IEnumerator Die()
    {
        this.transform.Rotate(-75, 0, 0);
        yield return new WaitForSeconds(1.5f);
        Destroy(this.gameObject);
    }
}

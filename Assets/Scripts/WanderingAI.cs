using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for randomly-wandering objects.
/// </summary>
public class WanderingAI : MonoBehaviour
{
    #region AI Properties
    /// <summary> Speed the object should move at </summary>
	public float speed = 3.0f;
    /// <summary> Number representing the tolerated range of this object (the wandering AI) from other objects (walls, doors, etc.) </summary>
    public float obstacleRange = 5.0f;
    /// <summary> True if alive, false if dead </summary>
    private bool _alive;
	#endregion AI Properties

	#region Fireball Properties
    /// <summary> Fireball prefab to use when instantiating new fireballs </summary>
	[SerializeField] private GameObject fireballPrefab;
    /// <summary> The currently-instantiated fireball object </summary>
    private GameObject _fireball;
	#endregion

	// Start is called before the first frame update
	void Start()
    {
        _alive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_alive)
        {
            transform.Translate(0, 0, speed * Time.deltaTime);

            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;
            if (Physics.SphereCast(ray, 0.75f, out hit)) //returns true if there's any collider intersection in the radius
            {
                GameObject hitObject = hit.transform.gameObject;
                if(!hitObject.name.Contains("Column") && !hitObject.name.Contains("Wall")) Debug.Log("Minion hit... " + hitObject.name);
                if(hitObject.GetComponent<PlayerCharacter>()) //player character is in range
                {
                    if(_fireball == null)
                    {
                        _fireball = Instantiate(fireballPrefab) as GameObject;
                        _fireball.transform.position = transform.TransformPoint(Vector3.forward * 1.5f);
                        _fireball.transform.rotation = transform.rotation;
                    }
                }
                //Check if the wandering AI hit an object. If so, change direction (randomly)
                if (hit.distance < obstacleRange && !hitObject.CompareTag("Collectible"))
                {
                    float angle = Random.Range(-110, 110);
                    transform.Rotate(0, angle, 0);
                }
            }
        }
    }

    /// <summary>Set the living status of the Wandering AI </summary>
    /// <param name="alive">True if we should be alive, false if it should be dead </param>
    public void SetAlive(bool alive)
    {
        _alive = alive;
    }
}

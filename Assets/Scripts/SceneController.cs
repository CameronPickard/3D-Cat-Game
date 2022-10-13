using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Behavior for the overall scene
/// </summary>
public class SceneController : MonoBehaviour
{
    /// <summary> Prefab from which to create new enemy objects </summary>
    [SerializeField] private GameObject enemyPrefab;
    /// <summary> Currently instantiated enemy </summary>
    private GameObject _enemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Create an enemy if the one we're tracking died
        if(_enemy == null)
        {
            Debug.Log("Respawning!");
            _enemy = Instantiate(enemyPrefab) as GameObject;
            _enemy.transform.position = new Vector3(0, 1f, 0);
            float angle = Random.Range(0, 360);
            _enemy.transform.Rotate(0, angle, 0);
        }
    }
}

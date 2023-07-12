using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IGameManager
{
    public ManagerStatus status { get; private set; }

    public int health { get; private set; }
    public int maxHealth { get; private set; }

    public void Startup() 
    {
        Debug.Log("Player manager starting... on frame" + Time.frameCount);
        health = 50;
        maxHealth = 100;

        status = ManagerStatus.Started;
	}

    public void ChangeHealth(int healthToAdd) 
    {
        health += healthToAdd;
        if(health > maxHealth) 
        {
            health = maxHealth;
		}
        if(health < 0) { health = 0; }

        Debug.Log("Health: " + health + "/" + maxHealth);
        
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

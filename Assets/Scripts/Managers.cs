using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//pg 182
public class Managers : MonoBehaviour
{
	public static PlayerManager Player { get; private set; }
	public static InventoryManager Inventory { get; private set; }

	private List<IGameManager> _startSequence;

	private void Awake() //note to self: Awake is like Start, except that it runs even before Start
	{
		Player = GetComponent<PlayerManager>();
		Inventory = GetComponent<InventoryManager>();

		_startSequence = new List<IGameManager>();
		_startSequence.Add(Player);
		_startSequence.Add(Inventory);

		StartCoroutine("StartupManagers");	
	}

	private IEnumerator StartupManagers() 
	{
		foreach (IGameManager manager in _startSequence) {
			manager.Startup();
		}

		yield return null;

		int numModules = _startSequence.Count;
		int numReady = 0;

		while(numReady < numModules) {
			int lastReady = numReady;
			numReady = 0;

			foreach(IGameManager manager in _startSequence) {
				if (manager.status == ManagerStatus.Started)
				{
					numReady++;
				}
			}

			if (numReady > lastReady) {
				Debug.Log("Progress: " + numReady + " / " + numModules);
				yield return null;
			}
		}
		Debug.Log("All managers started up. On frame " + Time.frameCount); //this is not where the book says this line goes but I think it goes here
	}
}

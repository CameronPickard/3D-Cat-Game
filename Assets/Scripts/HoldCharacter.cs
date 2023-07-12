using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldCharacter : MonoBehaviour
{
	[SerializeField] private Transform unscaledParent;

	private void Start()
	{
		if (unscaledParent == null) unscaledParent = transform;
	}
	private void OnTriggerEnter(Collider other)
	{
		other.transform.parent = unscaledParent.transform;
		Debug.Log("On Moving Platform!");
	}

	private void OnTriggerExit(Collider other)
	{
		other.transform.parent = null;
		Debug.Log("OFF Moving Platform!");
	}
}

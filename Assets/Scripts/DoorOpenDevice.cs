using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Behavior for doors that open and close
/// </summary>
public class DoorOpenDevice : MonoBehaviour
{
    [SerializeField] private Vector3 dPos;

    private bool _open;
    private bool _isTransitioning;
    private Vector3 _startingPos;

    // Start is called before the first frame update
    void Start()
    {
        _startingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Operate() {
        OperateDoor();
	}

    public void Activate() {
        if (!_open)
        {
            OpenDoor();
        }
	}

    public void Deactivate() {
        if(_open) {
            CloseDoor();
		}
	}

    /// <summary>
    /// Opens the door if it's closed, and closes the door if it's opened.
    /// Does nothing if it's transitioning from one state to another
    /// </summary>
    public void OperateDoor() 
    {
        if (_isTransitioning) return;
        if (_open) {
            CloseDoor();
        }
        else {
            OpenDoor();
		}
	}

    /// <summary>
    /// Opens the door by lowering it
    /// </summary>
    private void OpenDoor() {
        Vector3 newPos = _startingPos + dPos;
        _isTransitioning = true;
        //Use iTween to make the position change more naturally
        _open = true;
        iTween.MoveTo(gameObject, iTween.Hash(
            "position", newPos,
            "speed", 2,
            "oncomplete", "DoorAnimationFinish"
        ));
    }

    /// <summary>
    /// Closes the door by raising it
    /// </summary>
    private void CloseDoor()
    {
        Vector3 newPos = _startingPos;
        _isTransitioning = true;
        //Use iTween to make the position change more naturally
        _open = false;
        iTween.MoveTo(gameObject, iTween.Hash(
            "position", newPos,
            "speed", 2,
            "oncomplete", "DoorAnimationFinish"
        ));
    }

    /// <summary>
    /// Marks the door has having finished its transition from open to closed, or vice-versa
    /// </summary>
    private void DoorAnimationFinish() {
        _isTransitioning = false;
    }
}

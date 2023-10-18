using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCamera : MonoBehaviour
{
    /// <summary>
    /// If a character controller is defined, it means we dont want our ray shooter to hit our character. Defined in 3rd person games, undefined in FPS games (typically)
    /// </summary>
   [SerializeField] private CharacterController character;
   [SerializeField] private bool _isDebugging = false;
   /// <summary>
   /// Camera obejct
   /// </summary>
    private Camera _camera;
    private bool _isCrouching = false;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        if(_isCrouching) 
        {
            var blackStyle = new GUIStyle();
            blackStyle.normal.textColor = Color.black;
            blackStyle.fontSize = 29;
            var whiteStyle = new GUIStyle();
            whiteStyle.normal.textColor = Color.white;
            whiteStyle.fontSize = 20;
            int blackSize = 29;
            int whiteSize = 20;
            float posX = _camera.pixelWidth / 2;
            float posY = _camera.pixelHeight / 2;
            GUI.Label(new Rect(posX - 1, posY - 1, blackSize, blackSize), "*", blackStyle);
            GUI.Label(new Rect(posX + 1, posY + 2, whiteSize, whiteSize), "*", whiteStyle);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            /*Vector3 point;
            point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);
            if(_isDebugging) 
            {
                Debug.Log("Point: " + point.ToString());
                Debug.Log("Ray: " + ray.ToString());
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.transform.gameObject;
                ReactiveTarget target = hitObject.GetComponent<ReactiveTarget>();
                if (target != null)
                {
                    target.ReactToHit();
                }
                else
                {
                    StartCoroutine(SphereIndicator(hit.point));
                }
            }*/
        }
    }

    public void SetCrouching(bool isCrouching) 
    {
        _isCrouching = isCrouching;
	}

    /// <summary>
    /// Create a sphere at a certain position
    /// </summary>
    /// <param name="pos">Position at which to create the sphere</param>
    /// <returns></returns>
    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        yield return new WaitForSeconds(1);

        Destroy(sphere);
    }
}

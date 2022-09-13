using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayShooter : MonoBehaviour
{
    /// <summary>
    /// If a character controller is defined, it means we dont want our ray shooter to hit our character. Defined in 3rd person games, undefined in FPS games (typically
    /// ** 
    /// </summary>
   [SerializeField] private CharacterController character;

    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnGUI()
    {
        int size = 12;
        float posX = _camera.pixelWidth / 2;
        float posY = _camera.pixelHeight / 2;
        GUI.Label(new Rect(posX, posY, size, size), "*");
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 point;
            //if (character != null) { point = character.transform.position; } //3rd person
            point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0); //1st person //I think this is the origin of the camera...
            Ray ray = _camera.ScreenPointToRay(point);
            Debug.Log("Point: " + point.ToString());
            Debug.Log("Ray: " + ray.ToString());
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
            }
        }
    }

    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        yield return new WaitForSeconds(1);

        Destroy(sphere);
    }
}

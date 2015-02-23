using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;

public class PlayerController : MonoBehaviour
{
    public Background background;
    [SerializeField]
    private KeyCode TheOneButton = KeyCode.Space;
    private Transform[] planets;

    private Transform grabble;
    private float distance;
    private bool clockwise;

    // Use this for initialization
    void Start()
    {
        if(background && background.planetCount > 0)
        {
            planets = new Transform[background.planetCount];
            for(int i = 0; i < background.planetCount; ++i)
            {
                planets[i] = background.planets[i].transform;
            }
        }
        else
        {
            planets = GameObject.FindObjectsOfType<Transform>().Where(a => a.name == "Planet").ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(TheOneButton))
        {
            var position = transform.position;
            var planet = planets.OrderBy(p => Vector3.Distance(p.position, position)).First();

            distance = Vector3.Distance(planet.position, transform.position);

            grabble = planet;

            var up = transform.up.normalized;

            var connection = (transform.position - grabble.position).normalized;


            var angle = Mathf.Atan2(-connection.y, -connection.x) - Mathf.Atan2(up.y, up.x);

            if(Mathf.Abs(angle) > Mathf.PI) {
                angle = -angle - Mathf.PI;
            }

            clockwise = angle < 0;
        }
        else if (Input.GetKeyUp(TheOneButton))
        {
            grabble = null;
        }
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.up * Time.fixedDeltaTime * 20, Space.Self);
        Debug.DrawRay(transform.position, transform.up * 2);

        if (grabble != null)
        {
            Debug.DrawLine(transform.position, grabble.position);

            var connection = (transform.position - grabble.position).normalized;
            var dir = new Vector2(connection.x, connection.y);
            var angle = Mathf.Atan2(dir.y, dir.x) / Mathf.PI * 180;


            transform.position = grabble.position + connection * distance;
            transform.rotation = Quaternion.Euler(0, 0, angle + (clockwise ? 180 : 0));
        }
    }
}

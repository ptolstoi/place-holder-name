using UnityEngine;
using System.Collections;
using System.Linq;

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

        if (Input.GetKey(TheOneButton) && grabble == null)
        {
            var position = transform.position;
            var planet = GetMatchingPlanet();

            if (planet != null)
            {
                grabble = planet;
            }
        }
        else if (Input.GetKeyUp(TheOneButton))
        {
            grabble = null;
        }

        if(Input.GetKeyUp(KeyCode.Escape)) {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
        }
    }

    Transform GetMatchingPlanet()
    {
        var possiblePlanets = (from p in planets
                                     let dist = Vector3.Distance(p.position, transform.position)
                                     where dist < 10
                                     orderby dist
                                     select p).ToArray();

        var up = transform.up.normalized;

        foreach (var planet in possiblePlanets)
        {
            distance = Vector3.Distance(planet.position, transform.position);

            var angle = GetAngle(planet);

            clockwise = angle < 0;

            angle = Mathf.Abs(angle);

            var connection = (transform.position - planet.position).normalized;
            var parallelAngle = Mathf.Atan2(connection.y, connection.x);
            var rotation = Mathf.Atan2(up.y, up.x);

            if (Mathf.Abs(angle - Mathf.PI / 2) < 0.2)
            {
                return planet;
            }
        }

        if(possiblePlanets.Length == 1) {
            return possiblePlanets[0];
        }

        return null;
    }

    float GetAngle(Transform planet) {
        var up = transform.up.normalized;

            var connection = (transform.position - planet.position).normalized;


            var angle = Mathf.Atan2(-connection.y, -connection.x) - Mathf.Atan2(up.y, up.x);

            if (Mathf.Abs(angle) > Mathf.PI)
            {
                return -angle - Mathf.PI;
            }

        return angle;
    }

    void FixedUpdate()
    {
        transform.Translate(Vector3.up * Time.fixedDeltaTime * 20, Space.Self);
        Debug.DrawRay(transform.position, transform.up * 2);

        if (grabble != null)
        {
            Debug.DrawLine(transform.position, grabble.position);

            var connection = (transform.position - grabble.position).normalized;
            var angle = Mathf.Atan2(connection.y, connection.x) / Mathf.PI * 180;


            transform.position = grabble.position + connection * distance;
            transform.rotation = Quaternion.Euler(0, 0, angle + (clockwise ? 180 : 0));
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + Vector3.back * 25, Time.deltaTime * 10);
    }
}

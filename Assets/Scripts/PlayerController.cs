using UnityEngine;
using System.Collections;
using System.Linq;
using System.IO;

public class PlayerController : MonoBehaviour
{
    const float maximal_grapple_distance = 20;

    public Background background;
    [SerializeField]
    private KeyCode TheOneButton = KeyCode.Space;
    private Transform[] planets;

    private Transform grappleTarget;
    private float distance;
    private bool clockwise;

    private Transform previous;
    float lastDistance;

    private bool isInOrbit;

    public Vector3 velocity;
    private LineRenderer lineRenderer;

    // Use this for initialization
    void Start()
    {
        if (background && background.planetCount > 0)
        {
            planets = new Transform[background.planetCount];
            for (int i = 0; i < background.planetCount; ++i)
            {
                planets[i] = background.planets[i].transform;
            }
        }
        else
        {
            planets = GameObject.FindObjectsOfType<Transform>().Where(a => a.name == "Planet").ToArray();
        }

        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(TheOneButton) && grappleTarget == null)
        {
            var planet = GetNearestPlanet();

            if (planet != null)
            {
                grappleTarget = planet;
                lastDistance = float.MaxValue;
            }
        }
        else if (Input.GetKeyUp(TheOneButton) && grappleTarget != null)
        {
            velocity = velocity.magnitude * transform.up;
            previous = grappleTarget;
            grappleTarget = null;
            isInOrbit = false;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Die();
        }
    }

    Transform GetNearestPlanet()
    {

        var possiblePlanets = (from p in planets
                                     let dist = Vector3.Distance(p.position, transform.position)
                                     where dist < maximal_grapple_distance
                                     orderby dist
                                     select p).ToArray();

        Transform result = null;

        var position = transform.position + velocity * 0.125f;
        var minimalDistance = float.MaxValue;
        var flag = false;

        for (int i = 0; i < possiblePlanets.Length; i++)
        {
            var planet = possiblePlanets[i];
            var distanceToPlanet = Vector3.Distance(planet.position, position);

            if (planet == previous)
            {
                if (distanceToPlanet > minimalDistance - 2.5f)
                {
                    continue;
                }
            }
            else
            {
                if (flag && distanceToPlanet > minimalDistance + 2.5f)
                {
                    continue;
                }
                if (distanceToPlanet > minimalDistance)
                {
                    continue;
                }
            }

            if (Vector3.Distance(position + velocity.normalized * distanceToPlanet, planet.position) > 1)
            {
                flag = previous == planet;
                result = planet;
                minimalDistance = distance;
            }

        }

        return result;
        
    }

    float GetAngle(Transform planet)
    {
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

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, Vector3.up * transform.position.y + Vector3.back * 25, Time.deltaTime * 10);

        if (grappleTarget == null || !isInOrbit)
        {
            rigidbody2D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
            lineRenderer.enabled = false;
        }
        if (grappleTarget != null)
        {
            if (isInOrbit)
            {
                Debug.DrawLine(transform.position, grappleTarget.position, Color.green);

                var connection = (transform.position - grappleTarget.position).normalized;
                var angle = Mathf.Atan2(connection.y, connection.x) / Mathf.PI * 180;

                rigidbody2D.MovePosition(grappleTarget.position + connection * distance + velocity.magnitude * transform.up * Time.fixedDeltaTime);
                rigidbody2D.MoveRotation(angle + (clockwise ? 180 : 0));
            }
            else
            {
                var distanceToPlanet = Vector3.Distance(transform.position, grappleTarget.position);

                if (distanceToPlanet > maximal_grapple_distance)
                {
                    grappleTarget = null;
                }
                else
                {
                    Debug.DrawLine(transform.position, grappleTarget.position, Color.red);

                    if (lastDistance < distanceToPlanet)
                    {
                        isInOrbit = true;
                        distance = Vector3.Distance(transform.position, grappleTarget.position);
                        clockwise = GetAngle(grappleTarget) < 0;
                        lastDistance = float.MaxValue;
                    }
                    lastDistance = distanceToPlanet;
                }
            }

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, grappleTarget.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Planet")
        {
            Die();
        }
    }

    void Die()
    {
        var tr = GetComponent<TrailRenderer>();
        var time = tr.time;
        tr.time = 0;

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        isInOrbit = false;
        grappleTarget = null;
        velocity = Vector3.up * velocity.magnitude;
        Camera.main.transform.position = Vector3.zero;

        StartCoroutine(ShowTrail(tr, time));
    }

    IEnumerator ShowTrail(TrailRenderer tr, float time) {
        yield return 0;
        tr.time = time;
    }

}

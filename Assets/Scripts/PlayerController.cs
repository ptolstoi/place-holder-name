﻿using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float maximal_grapple_distance = 20;

    public Background background;
    public KeyCode TheOneButton = KeyCode.Space;
    public Player Player = Player.Player1;

    private Planet[] planets;

    private Planet grappledPlanet;
    private Planet previous;

    private float distance;
    private bool clockwise;
    float lastDistance;

    private bool isInOrbit;

    public Vector3 velocity;
    private LineRenderer lineRenderer;

    // Use this for initialization
    void Start()
    {
        int planetCount = background.planets.Count;
        if (background && planetCount > 0)
        {
            planets = new Planet[planetCount];
            int count = 0;
            foreach(var p in background.planets)
            {
                planets[count] = p;
                count++;
            }
        }
        else
        {
            planets = FindObjectsOfType<Transform>().
                Where(a => a.gameObject.layer == LayerMask.NameToLayer("Planet")).
                Select(a => a.GetComponent<Planet>()).
                ToArray();
        }

        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(TheOneButton) && grappledPlanet == null)
        {
            var planet = GetNearestPlanet();

            if (planet != null)
            {
                grappledPlanet = planet;
                lastDistance = float.MaxValue;
            }
        }
        else if (Input.GetKeyUp(TheOneButton) && grappledPlanet != null)
        {
            ReleaseGrapple();
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Die();
        }
    }

    Planet GetNearestPlanet()
    {

        var possiblePlanets = (from p in planets
                                     let dist = Vector3.Distance(p.transform.position, transform.position)
                                     where dist < maximal_grapple_distance
                                     orderby dist
                                     select p).ToArray();

        Planet result = null;

        var position = transform.position + velocity * 0.125f;
        var minimalDistance = float.MaxValue;
        var flag = false;

        for (int i = 0; i < possiblePlanets.Length; i++)
        {
            var planet = possiblePlanets[i];
            var planetSize = 0;

            var distanceToPlanet = Vector3.Distance(planet.transform.position, position) - planetSize;

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

            if (Vector3.Distance(position + velocity.normalized * distanceToPlanet, planet.transform.position) > 1 + planetSize)
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

//        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, transform.position + Vector3.back * 15 + Vector3.down * 2, Time.deltaTime * 10);

        if (grappledPlanet == null || !isInOrbit)
        {
            rigidbody2D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
            lineRenderer.enabled = false;
        }
        if (grappledPlanet != null)
        {
            if (isInOrbit)
            {
                Debug.DrawLine(transform.position, grappledPlanet.transform.position, Color.green);

                var connection = (transform.position - grappledPlanet.transform.position).normalized;
                var angle = Mathf.Atan2(connection.y, connection.x) / Mathf.PI * 180;

                rigidbody2D.MovePosition(grappledPlanet.transform.position + connection * distance + velocity.magnitude * transform.up * Time.fixedDeltaTime);
                rigidbody2D.MoveRotation(angle + (clockwise ? 180 : 0));
            }
            else
            {
                var distanceToPlanet = Vector3.Distance(transform.position, grappledPlanet.transform.position);

                if (distanceToPlanet > maximal_grapple_distance)
                {
                    grappledPlanet = null;
                }
                else
                {
                    Debug.DrawLine(transform.position, grappledPlanet.transform.position, Color.red);

                    if (lastDistance < distanceToPlanet)
                    {
                        GrappleOnPlanet();
                    }
                    lastDistance = distanceToPlanet;
                }
            }

            lineRenderer.enabled = grappledPlanet != null;

            if (grappledPlanet != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, grappledPlanet.transform.position);
            }
        }
    }

    private void GrappleOnPlanet()
    {
        isInOrbit = true;
        distance = Vector3.Distance(transform.position, grappledPlanet.transform.position);
        clockwise = GetAngle(grappledPlanet.transform) < 0;
        lastDistance = float.MaxValue;
        background.ChangeOwner(grappledPlanet, Player);
        grappledPlanet.Grapple(this);
    }

    private void ReleaseGrapple()
    {
        var planet = grappledPlanet.GetComponent<Planet>();
        if (planet != null)
        {
            planet.ReleaseGrapple(this);
        }
        velocity = velocity.magnitude * transform.up;
        previous = grappledPlanet;
        grappledPlanet = null;
        isInOrbit = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Planet"))
        {
            Die();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Border") && grappledPlanet == null)
        {
            Die();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Border") && grappledPlanet == null)
        {
            Die();
        }
    }

    void Die()
    {
        transform.position = new Vector3(1,1);
        transform.rotation = Quaternion.identity;
        isInOrbit = false;
        grappledPlanet = null;
        velocity = Vector3.up * velocity.magnitude;
        Camera.main.transform.position = Vector3.zero;

        StartCoroutine(ShowTrail());
    }

    IEnumerator ShowTrail() {
        var tr = GetComponent<TrailRenderer>();
        var time = tr.time;
        tr.time = 0;
        yield return 0;
        tr.time = time;
    }

}

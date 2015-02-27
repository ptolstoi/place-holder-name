using System;
using System.Collections;
using System.Linq;
using InControl;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float maximal_grapple_distance = 20;

    public GameObject Explosion;

    public Background background;
    public KeyCode TheOneButton = KeyCode.Space;
    public Player Player = Player.Player1;
    private bool rotating = true;

    public SpriteRenderer PlayerSprite;
    public float DeathCoolDown = 5;
    public float OutSideDeathTimer = 1;

    private Planet[] planets;

    private Planet grappledPlanet;
    private Planet previous;

    private float distance;
    private bool clockwise;
    float lastDistance;

    private bool isInOrbit;

    public Vector3 velocity;
    private Trail trail;
    private LineRenderer lineRenderer;

    private Vector3 startVelocity;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool Paused;
    public float Speed;
    private float outSideTimer;
    public float deathTimer { get; protected set; }

    public event System.Action OnUnDie;

    public bool IsActive { get; protected set; }

    // Use this for initialization
    void Start()
    {
        IsActive = false;
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
        lineRenderer.material.color = Player.GetColor();
        
        var trailController = GetComponentInChildren<TrailMaterialControler>();
        trailController.Color = Player.GetColor();
        trail = GetComponentInChildren<Trail>();

        PlayerSprite.color = Player.GetColor();

        startVelocity = velocity;
        startPosition = transform.position;
        startRotation = transform.rotation;
        Paused = false;
        outSideTimer = 0;

        RotateAroundCenter();
    }

    InputDevice GetInputDevice()
    {
        var id = (int)Player;
        return InputManager.Devices.Count > id ? InputManager.Devices[id] : null;
    }

    bool InputIsGrappling()
    {
        if (Input.GetKey(TheOneButton))
        {
            return true;
        }

        if (GetInputDevice() != null)
        {
            return GetInputDevice().Action1.IsPressed;
        }
        
        return false;
    }

    bool InputStoppedGrappling()
    {
        if (Input.GetKeyUp(TheOneButton))
        {
            return true;
        }

        if (GetInputDevice() != null)
        {
            return GetInputDevice().Action1.WasReleased;
        }

        return false;
    }

    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_MorphParams" + (int)Player, 
            new Vector4(transform.position.x, transform.position.y, 1, 1));

        if (Paused)
        {
            deathTimer = Mathf.Max(-1, deathTimer - Time.deltaTime);
        }
        if (deathTimer < 0 && deathTimer + Time.deltaTime > 0)
        {
            if (OnUnDie != null)
            {
                OnUnDie();
            }
        }
        if (Paused || deathTimer > 0) return;
        if (InputIsGrappling() && grappledPlanet == null)
        {
            IsActive = true;
            var planet = GetNearestPlanet();

            if (planet != null)
            {
                grappledPlanet = planet;
                lastDistance = float.MaxValue;
            }

            rotating = false;
        }
        else if (InputStoppedGrappling() && grappledPlanet != null)
        {
            IsActive = true;
            ReleaseGrapple();
            rotating = false;
        }

        if (transform.position.magnitude > background.outerRadius && grappledPlanet == null)
        {
            outSideTimer += Time.deltaTime;
            if (outSideTimer > OutSideDeathTimer)
            {
                Die();
            }
        }

        if(Input.GetKey(TheOneButton) && !background.GameStarted)
        {
            background.GameStarted = true;
        }
    }

    public void UnDie()
    {
        UnPause();
        trail.Reset();
        RotateAroundCenter();
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = true;
        } 
        
        SoundSystem.Instance.PlayChord(audio, ChordType.Revive);

        if (GetInputDevice() != null)
        {
            GetInputDevice().Vibrate(this, 0.4f, 0.5f);
        }
    }

    public bool GetRotating()
    {
        return rotating;
    }

    private void RotateAroundCenter()
    {
        rotating = true;
        grappledPlanet = planets[0];
        GrappleOnPlanet(true);
        //IsActive = true;
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
        if (Paused)
        {
            return;
        }

        if (grappledPlanet == null || !isInOrbit)
        {
            velocity = velocity.normalized*Speed;

            rigidbody2D.MovePosition(transform.position + velocity * Time.fixedDeltaTime);
            lineRenderer.enabled = false;

            var nextPlanet = planets.Select(p => new
            {
                t = p.transform,
                p,
                dist = Vector3.Distance(p.transform.position, transform.position)
            }).OrderBy(p => p.dist).First();
            if (nextPlanet.dist < nextPlanet.t.localScale.x * 1.4f)
            {
                velocity += (transform.position - nextPlanet.t.position).normalized * 2 * (nextPlanet.p.Celestial ? -0.5f : 1);
            } 
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
                lineRenderer.SetPosition(0, transform.position + transform.up * 0.5f);
                lineRenderer.SetPosition(1, grappledPlanet.OuterPlanet.position);
            }
        }
    }

    private void GrappleOnPlanet(bool force = false)
    {
        isInOrbit = true;
        distance = Vector3.Distance(transform.position, grappledPlanet.transform.position);
        clockwise = !force ? GetAngle(grappledPlanet.transform) < 0 : false;
        lastDistance = float.MaxValue;
        background.ChangeOwner(grappledPlanet, this, !force);
        grappledPlanet.Grapple(this);

        if (GetInputDevice() != null)
        {
            var d = distance*(transform.position.x - grappledPlanet.transform.position.x) < 0 ? -1 : 1;
            
            GetInputDevice().Vibrate(this, 0.4f, d, -d);
        }
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
        else if (other.gameObject.tag == "Player" && IsActive && !rotating)
        {
            var pc = other.GetComponent<PlayerController>();
            if (!pc.IsActive && !pc.rotating && deathTimer < 0)
            {
                Die();
                pc.Die();
            }
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
        if (rotating || deathTimer > 0) return;
        var x = Instantiate(Explosion, transform.position, transform.rotation) as GameObject;
        var ps = x.GetComponent<Explosion>();
        ps.Player = this;

        if (GetInputDevice() != null)
        {
            GetInputDevice().Vibrate(this, 0.4f, 1);
        }

        SoundSystem.Instance.PlayChord(audio, ChordType.Die);

        transform.position = startPosition;
        transform.rotation = startRotation;
        velocity = startVelocity;

        isInOrbit = false;
        grappledPlanet = null;

        trail.Reset();
        Pause();
        outSideTimer = 0;

        deathTimer = DeathCoolDown;
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = false;
        }
    }

    public void Pause()
    {
        Paused = true;
        trail.Pause();
    }

    public void UnPause()
    {
        Paused = false;
        trail.UnPause();
    }
}

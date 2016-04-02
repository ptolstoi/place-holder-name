using System.Collections;
using System.Linq;
using UnityEngine;
using InControl;

public enum PlayerCorner {
    LeftTop,
    LeftBottom,
    RightTop,
    RightBottom
}

public class PlayerController : MonoBehaviour
{
    const float maximal_grapple_distance = 20;

    public GameObject Explosion;

    public Background background;
    public KeyCode TheOneButton = KeyCode.Space;
    public PlayerCorner corner = PlayerCorner.LeftTop;
    public Player Player = Player.Player1;
    private bool rotating = true;

    public SpriteRenderer PlayerSprite;
    public float DeathCoolDown = 5;
    public float OutSideDeathTimer = 1;

    private Planet[] planets;

    public Planet grappledPlanet { get; protected set;}
    private Planet previous;

    public float distance { get; protected set;}
    private bool clockwise;
    float lastDistance;

    public bool isInOrbit { get; protected set;}

//    public Vector3 velocity;
    private Trail trail;
    private TrailMaterialControler trailMaterialControler;
    private LineRenderer lineRenderer;

//    private Vector3 startVelocity;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool Paused;
    public float Speed;
    private float StartSpeed;
    private float outSideTimer;
    public float deathTimer { get; protected set; }

    public event System.Action OnUnDie;

    public bool IsActive { get; protected set; }
    public bool IsDead { get { return deathTimer < 0; } }

    void Awake() {
        trailMaterialControler = GetComponentInChildren<TrailMaterialControler>();
    }

    // Use this for initialization
    void Start()
    {
        IsActive = false;
        StartSpeed = Speed;
        int planetCount = background.planets.Count;
        if (background && planetCount > 0)
        {
            planets = new Planet[planetCount];
            int count = 0;
            foreach (var p in background.planets)
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

        trailMaterialControler.Color = Player.GetColor();
        trail = GetComponentInChildren<Trail>();

        PlayerSprite.color = Player.GetColor();

        startPosition = transform.position;
        startRotation = transform.rotation;
        Paused = false;
        outSideTimer = 0;

        RotateAroundCenter();


        if (GetInputDevice() != null)
        {
            GetInputDevice().Vibrate(0);
        }
    }

    public void ClearPlayerPlanets()
    {
        foreach(var planet in planets)
        {
            planet.ClearOwner();
        }
        IsActive = false;
//        Die();
    }

    InputDevice GetInputDevice()
    {
        var id = (int)Player;
        return InputManager.Devices.Count > id ? InputManager.Devices[id] : null;
    }

    bool InputIsGrappling()
    {
        foreach(var touch in Input.touches) {
          
            if(touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled) {
                var left = touch.position.x < Screen.width * 0.5;
                var right = touch.position.x > Screen.width * 0.5;
                var top = touch.position.y < Screen.height * 0.5;
                var bottom = touch.position.y > Screen.height * 0.5;
                
                if (left && (corner == PlayerCorner.LeftBottom || corner == PlayerCorner.LeftTop)) {
                    if (top && corner == PlayerCorner.LeftTop) {
                        return true;
                    } else if (bottom && corner == PlayerCorner.LeftBottom) {
                        return true;
                    }
                } else if (right && (corner == PlayerCorner.RightBottom || corner == PlayerCorner.RightTop)) {
                    if (top && corner == PlayerCorner.RightTop) {
                        return true;
                    } else if (bottom && corner == PlayerCorner.RightBottom) {
                        return true;
                    }
                }
            }
          
        }
        
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
        foreach(var touch in Input.touches) {
          
            if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {  
                var left = touch.position.x < Screen.width * 0.5;
                var right = touch.position.x > Screen.width * 0.5;
                var top = touch.position.y < Screen.height * 0.5;
                var bottom = touch.position.y > Screen.height * 0.5;
                
                if (left && (corner == PlayerCorner.LeftBottom || corner == PlayerCorner.LeftTop)) {
                    if (top && corner == PlayerCorner.LeftTop) {
                        return true;
                    } else if (bottom && corner == PlayerCorner.LeftBottom) {
                        return true;
                    }
                } else if (right && (corner == PlayerCorner.RightBottom || corner == PlayerCorner.RightTop)) {
                    if (top && corner == PlayerCorner.RightTop) {
                        return true;
                    } else if (bottom && corner == PlayerCorner.RightBottom) {
                        return true;
                    }
                }
            }
          
        }
        
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

        deathTimer = Mathf.Max(-1, deathTimer - Time.deltaTime);

        if (deathTimer < 0 && deathTimer + Time.deltaTime > 0)
        {
            if (OnUnDie != null)
            {
                OnUnDie();
            }
        }
        if (Paused || deathTimer > 0) return;

        Speed = Mathf.Lerp(Speed,
            StartSpeed + Mathf.FloorToInt(Mathf.Abs(background.GameTime - background.TimeLeft)/20) * 5,
            Time.deltaTime * 0.33f);
        Speed = Mathf.Min(40, Speed);

        if (InputIsGrappling() && grappledPlanet == null)
        {
            var planet = GetNearestPlanet();

            if (planet != null)
            {
                grappledPlanet = planet;
                lastDistance = float.MaxValue;
            }

            rotating = false;
            IsActive = true;
        }
        else if (InputStoppedGrappling() && grappledPlanet != null)
        {
            ReleaseGrapple();
            rotating = false;
            background.GameStarted = true;
            IsActive = true;
        }

        if (transform.position.magnitude > background.radius && grappledPlanet == null)
        {
            outSideTimer += Time.deltaTime;
            if (outSideTimer > OutSideDeathTimer)
            {
                Die();
            }
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

        SoundSystem.Instance.PlayChord(GetComponent<AudioSource>(), ChordType.Revive); 
        
        if (GetInputDevice() != null)
        {
            GetInputDevice().Vibrate(this, 0.4f, 0.4f);
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
//        IsActive = true;
    }

    Planet GetNearestPlanet()
    {

        var possiblePlanets = (from p in planets
                               let dist = Vector3.Distance(p.transform.position, transform.position)
                               where dist < maximal_grapple_distance
                               orderby dist
                               select p).ToArray();

        Planet result = null;

        var position = transform.position + transform.up * Speed * 0.125f;
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

            if (Vector3.Distance(position + transform.up * distanceToPlanet, planet.transform.position) > 1 + planetSize)
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

            GetComponent<Rigidbody2D>().MovePosition(transform.position + transform.up * Speed * Time.fixedDeltaTime);
            lineRenderer.enabled = false;

            var nextPlanet = planets.Select(p => new
            {
                t = p.transform,
                p,
                dist = Vector3.Distance(p.transform.position, transform.position)
            }).OrderBy(p => p.dist).First();
            if (nextPlanet.dist < nextPlanet.t.localScale.x * 1.4f)
            {
				var gravity = transform.up * Speed +
					(transform.position - nextPlanet.t.position).normalized * 1.5f * (nextPlanet.p.Celestial ? -0.5f : 1.2f);
					transform.up = gravity.normalized;
//                velocity += (transform.position - nextPlanet.t.position).normalized * 2 * (nextPlanet.p.Celestial ? -0.5f : 1);
            }
        }
        if (grappledPlanet != null)
        {
            if (isInOrbit)
            {
                Debug.DrawLine(transform.position, grappledPlanet.transform.position, Color.green);

                var connection = (transform.position - grappledPlanet.transform.position).normalized;
                var angle = Mathf.Atan2(connection.y, connection.x) / Mathf.PI * 180;

                GetComponent<Rigidbody2D>().MovePosition(grappledPlanet.transform.position + connection * distance + Speed * transform.up * Time.fixedDeltaTime);
                GetComponent<Rigidbody2D>().MoveRotation(angle + (clockwise ? 180 : 0));
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
                var playerPosition = transform.position + transform.up*0.5f;
                var planetPosition = grappledPlanet.OuterPlanet.position;

                lineRenderer.SetPosition(0, playerPosition);
                lineRenderer.SetPosition(1, planetPosition + (playerPosition - planetPosition).normalized 
                    *grappledPlanet.transform.localScale.x * 0.5f);
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
        trailMaterialControler.SetDirection(clockwise ? 1 : -1);
        
        if (GetInputDevice() != null)
        {
            var d = transform.position.x - grappledPlanet.transform.position.x;
            GetInputDevice().Vibrate(this, 0.4f, Mathf.Clamp01(d), Mathf.Clamp01(-d));
        }
    }

    private void ReleaseGrapple()
    {
        if(grappledPlanet == null) {
            return;
        }

        var planet = grappledPlanet.GetComponent<Planet>();
        if (planet != null)
        {
            planet.ReleaseGrapple(this);
        }

        //velocity = Speed * transform.up;
        previous = grappledPlanet;
        grappledPlanet = null;
        isInOrbit = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Planet"))
        {
            var planet = other.gameObject.GetComponent<Planet>();
            if (planet != null && !planet.Celestial)
            {
                background.ChangeOwner(planet, this, false);
            }
            Die();
        }
        else if (other.gameObject.tag == "Player")
        {
            var pc = other.GetComponent<PlayerController>();
            if (pc.IsActive && !pc.rotating && pc.deathTimer < 0 && IsActive && !rotating)
            {
                Die();
                pc.Die();
            }
        }
    }

    void Die()
    {
        if (rotating || deathTimer > 0) return;
        var x = Instantiate(Explosion, transform.position, transform.rotation) as GameObject;
        var ps = x.GetComponent<Explosion>();
        ps.Player = this;

        SoundSystem.Instance.PlayChord(GetComponent<AudioSource>(), ChordType.Die);

        if (GetInputDevice() != null)
        {
            GetInputDevice().Vibrate(this, 0.4f, 1);
        }

        transform.position = startPosition;
        transform.rotation = startRotation;

        isInOrbit = false;
        ReleaseGrapple();
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
        trailMaterialControler.Pause();
    }

    public void UnPause()
    {
        Paused = false;
        trail.UnPause();
        trailMaterialControler.UnPause();
    }
}

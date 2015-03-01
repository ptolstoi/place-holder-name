using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Background : MonoBehaviour {

    public float radius = 10.0f;
    public float innerRadius = 2.0f;
    public float outerRadius = 10.0f;

    public float minDistance = 2.0f;
    public int planetCount = 10;
    public GameObject planetPrefab;

    private List<Vector2> planetPositions = new List<Vector2>();
    public List<Planet> planets = new List<Planet>();
    private int maxTries = 10;
    public int createdPlanets = 0;

    public Dictionary<Player, int> ownership;

    public float GameTime = 2 * 60;
    private PlayerController[] players;

    public float TimeLeft { get; protected set; }
    public float CurrPauseTime { get; set; }
    public float pauseTime;
    public bool GameStarted { get; set; }
    public bool Restart { get; set; }

	// Use this for initialization
	void Start ()
    {
        GameStarted = false;
        Restart = false;
        players = GameObject.FindGameObjectsWithTag("Player").Select(p => p.GetComponent<PlayerController>()).ToArray();

	    TimeLeft = GameTime;
        ownership = new Dictionary<Player, int>();
        foreach(Player i in System.Enum.GetValues(typeof(Player)))
        {
            if (i == Player.PlayerNone) continue;
	        ownership[i] = 0;
	    }

        int addedPlanets = 0;

        for(int i = 0; i < planetCount; ++i)
        {
            int count = 0;
            Vector2 currPos = CalcPosition();
            //calculate new position if the distance to the other planets is too small
            while(count < maxTries)
            {
                if(!CheckDistance(currPos))
                {
                    currPos = CalcPosition();
                    count++;
                }
                else
                {
                    planetPositions.Add(currPos);
                    addedPlanets++;
                    break;
                }
            }
        }

        var centralPlanet = Instantiate(planetPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
        Planet central = centralPlanet.GetComponent<Planet>();
	    central.Celestial = true;
        central.SetCentralPlanet();
        planets.Add(central);
        foreach (Vector2 pos in planetPositions)
        {         
            //Instantiate the prefabs
            var go = Instantiate(planetPrefab, new Vector3(pos.x, pos.y , 0.0f), Quaternion.identity) as GameObject;
            planets.Add(go.GetComponent<Planet>());
            createdPlanets++;
        }

	}

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.LoadLevel(0);
        }
        if (Input.GetKeyUp(KeyCode.M)) {
            if(audio.isPlaying) {
                audio.Pause();
            } else {
                audio.Play();
            }
        }

        if (TimeLeft > 0)
        {
            if(GameStarted)
            { 
                TimeLeft -= Time.deltaTime;
                CurrPauseTime = pauseTime;
            }
                
            Restart = false;
        }
        else
        {
            TimeLeft = 0;
            
            foreach (var player in players)
            {
                player.Pause();
            }
            CurrPauseTime -= Time.deltaTime;

            if(CurrPauseTime <= 0.0f)
            {
                TimeLeft = GameTime;
                CurrPauseTime = 0.0f;
                foreach(var player in players)
                {
                    player.ClearPlayerPlanets();
                    player.UnPause();
                }
                Restart = true;
                GameStarted = false;
                foreach(var planet in planets)
                {
                    planet.ClearOwner();
                }
               
                foreach(var player in players)
                {
                    ownership[player.Player] = 0;
                    player.UnDie();
                }

                Application.LoadLevel(0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        Gizmos.DrawWireSphere(transform.position, outerRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private BoxCollider2D CreateBorder(string name, Rect rect)
    {
        var border = new GameObject(name, typeof (BoxCollider2D));
        border.transform.SetParent(transform);
        border.layer = LayerMask.NameToLayer("Border");
        
        Vector2 center = rect.center;
        center.x -= radius;
        center.y -= radius;
        border.transform.position = center;

        var collider = border.GetComponent<BoxCollider2D>();
        collider.size = new Vector2(1, 1);

        var sr = border.AddComponent<SpriteRenderer>();
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0,0, new Color(1,1,1,0.1f));
        texture.Apply();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);

        border.transform.localScale = rect.size;


        return collider;
    }

    public void ChangeOwner(Planet planet, PlayerController newOwner, bool playSound = true)
    {
        if (planet.Celestial)
        {
            if (playSound)
            {
                SoundSystem.Instance.PlayChord(newOwner.audio, ChordType.GrabOld);
            }
            return;
        }
        var previousOwner = planet.Owner;
        if (previousOwner != Player.PlayerNone)
        {
            ownership[previousOwner]--;
        }

        ownership[newOwner.Player]++;

        planet.ChangeOwner(newOwner);
    }

    private bool CheckDistance(Vector2 pos)
    {
        foreach(Vector2 pPos in planetPositions)
        {
            float distance = Vector2.Distance(pos, pPos);
            if (distance < minDistance)
                return false;
        }
        return true;
    }

    private bool CheckPosition(Vector2 pos)
    {
        float distance = pos.magnitude;
        if (distance < innerRadius || distance > outerRadius)
            return false;
        return true;
    }

    private Vector2 CalcPosition()
    {
        float radius = Random.Range(innerRadius, outerRadius);
        float angle = Random.Range(0.0f, 360.0f);

        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        
        Vector2 currPos = new Vector2(x, y);
        return currPos;
    }
}

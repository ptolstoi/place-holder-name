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

	// Use this for initialization
	void Start ()
    {
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

        foreach (Vector2 pos in planetPositions)
        {         
            //Instantiate the prefabs
            var go = Instantiate(planetPrefab, new Vector3(pos.x, pos.y , 0.0f), Quaternion.identity) as GameObject;
            planets.Add(go.GetComponent<Planet>());
            createdPlanets++;
        }

	    float borderWidth = 50;

        float radiusDouble = radius * 2.0f;
        CreateBorder("LeftBorder", new Rect(-borderWidth, -borderWidth, borderWidth, radiusDouble + borderWidth * 2));
        CreateBorder("RightBorder", new Rect(radiusDouble, -borderWidth, borderWidth, radiusDouble + borderWidth * 2));
        CreateBorder("BottomBorder", new Rect(-borderWidth, -borderWidth, radiusDouble + borderWidth * 2, borderWidth));
        CreateBorder("TopBorder", new Rect(-borderWidth, radiusDouble, radiusDouble + borderWidth * 2, borderWidth));
	}

    void Update()
    {
        if (TimeLeft > 0)
        {
            TimeLeft -= Time.deltaTime;
        }
        else
        {
            TimeLeft = 0;


            foreach (var player in players)
            {
                player.Pause();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        Gizmos.DrawWireSphere(transform.position, outerRadius);
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
        texture.SetPixel(0,0, Color.white);
        texture.Apply();
        sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);

        border.transform.localScale = rect.size;


        return collider;
    }

    public void ChangeOwner(Planet planet, Player newOwner)
    {
        var previousOwner = planet.Owner;
        if (previousOwner != Player.PlayerNone)
        {
            ownership[previousOwner]--;
        }

        ownership[newOwner]++;

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

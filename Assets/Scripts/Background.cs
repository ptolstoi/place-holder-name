using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour {

    public float xDimension = 10.0f;
    public float yDimension = 10.0f;
    public Vector2 safeZoneSize = new Vector2(1.0f, 1.0f);
    private Vector2 center, xSafeDimension, ySafeDimension;
    public float minDistance = 2.0f;
    public int planetCount = 10;
    public GameObject planetPrefab;

    private List<Vector2> planetPositions = new List<Vector2>();
    public List<Planet> planets = new List<Planet>();
    private int maxTries = 10;
    public int createdPlanets = 0;

	// Use this for initialization
	void Start () {
        center = new Vector2(xDimension * 0.5f, yDimension * 0.5f);
        xSafeDimension = new Vector2(center.x - safeZoneSize.x * 0.5f, center.x + safeZoneSize.x * 0.5f);
        ySafeDimension = new Vector2(center.y - safeZoneSize.y * 0.5f, center.y + safeZoneSize.y * 0.5f);
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
                else if(!CheckPosition(currPos))
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
            var go = Instantiate(planetPrefab, new Vector3(pos.x - xDimension * 0.5f, pos.y - yDimension*0.5f , 0.0f), Quaternion.identity) as GameObject;
            planets.Add(go.GetComponent<Planet>());
            createdPlanets++;
        }

	    float borderWidth = 50;
        
        CreateBorder("LeftBorder", new Rect(-borderWidth, -borderWidth, borderWidth, yDimension + borderWidth * 2));
        CreateBorder("RightBorder", new Rect(xDimension, -borderWidth, borderWidth, yDimension + borderWidth * 2));
        CreateBorder("BottomBorder", new Rect(-borderWidth, -borderWidth, xDimension + borderWidth * 2, borderWidth));
        CreateBorder("TopBorder", new Rect(-borderWidth, yDimension, xDimension + borderWidth * 2, borderWidth));
	}

    private BoxCollider2D CreateBorder(string name, Rect rect)
    {
        var border = new GameObject(name, typeof (BoxCollider2D));
        border.transform.SetParent(transform);
        border.layer = LayerMask.NameToLayer("Border");
        
        Vector2 center = rect.center;
        center.x -= xDimension * 0.5f;
        center.y -= yDimension * 0.5f;
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
        if (pos.x > xSafeDimension.x && pos.x < xSafeDimension.y && pos.y > ySafeDimension.x && pos.y < ySafeDimension.y)
            return false;
        return true;
    }

    private Vector2 CalcPosition()
    {
        float x = Random.Range(0.0f, xDimension);
        float y = Random.Range(0.0f, yDimension);
        
        Vector2 currPos = new Vector2(x, y);
        return currPos;
    }
}

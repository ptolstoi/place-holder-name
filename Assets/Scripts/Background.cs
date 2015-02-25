using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Background : MonoBehaviour {

    public float xDimension = 10.0f;
    public float yDimension = 10.0f;
    public float minDistance = 2.0f;
    public int planetCount = 10;
    public GameObject planetPrefab;

    private List<Vector2> planetPositions = new List<Vector2>();
    public List<GameObject> planets = new List<GameObject>();
    private int maxTries = 10;
    public int createdPlanets = 0;

	// Use this for initialization
	void Start () {

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
            planets.Add((GameObject)Instantiate(planetPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity));
            createdPlanets++;
        }

	    float borderWidth = 5;
        
        CreateBorder("LeftBorder", new Rect(-borderWidth * 1.5f, -borderWidth * 1.5f, borderWidth, yDimension + borderWidth * 3));
        CreateBorder("RightBorder", new Rect(xDimension + borderWidth / 2, -borderWidth * 1.5f, borderWidth, yDimension + borderWidth * 3));
        CreateBorder("BottomBorder", new Rect(-borderWidth * 1.5f, -borderWidth * 1.5f, yDimension + borderWidth * 3, borderWidth));
        CreateBorder("TopBorder", new Rect(-borderWidth * 1.5f, yDimension + borderWidth / 2, yDimension +borderWidth * 3, borderWidth));
	}

    private BoxCollider2D CreateBorder(string name, Rect rect)
    {
        var border = new GameObject(name, typeof (BoxCollider2D));
        border.transform.SetParent(transform);
        border.layer = LayerMask.NameToLayer("Border");
        border.transform.position = rect.center;
        var collider = border.GetComponent<BoxCollider2D>();
        collider.size = rect.size;
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

        //for(int i = 0; i < index; ++i)
        //{
        //    float distance = Vector2.Distance(pos, planetPositions[i]);
        //    if (distance < minDistance)
        //        return false;
        //}
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

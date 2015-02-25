using UnityEngine;
using System.Collections;

public class MultiplePlayerCamera : MonoBehaviour {

    private GameObject[] players;
    private Vector2 minW, maxW;
    private float startDistance = 0.0f;
    private float frustumWidth, frustumHeight;
    public float minDistance = -10.0f;
    public float offset = 2.0f;
    private bool cameraRotated = false;
    private float prevScale = 0.0f;

	// Use this for initialization
	void Start () {
        players = GameObject.FindGameObjectsWithTag("Player");
        startDistance = Camera.main.transform.position.z;
        cameraRotated = Camera.main.transform.rotation != Quaternion.identity;

        CalcFrustumDimnensionW();
	}
	
	// Update is called once per frame
	void Update () {
	    if(players.Length > 1)
        {
            CalculateBounds();

            if(cameraRotated)
                ProjectPoints();

            CalculateCameraPosition();
        }
        else if (players.Length == 1)
        {
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(players[0].transform.position.x, players[0].transform.position.y, minDistance), Time.deltaTime * 100);
        }
	}

    void CalculateBounds()
    {
        minW.x = minW.y = Mathf.Infinity;
        maxW.x = maxW.y = -Mathf.Infinity;

        foreach(GameObject player in players)
        {
            Vector2 pos = player.transform.position;
            
            //x bounds
            if (pos.x < minW.x)
                minW.x = pos.x - offset;
            if (pos.x > maxW.x)
                maxW.x = pos.x + offset;

            //y bounds
            if (pos.y < minW.y)
                minW.y = pos.y - offset;
            if (pos.y > maxW.y)
                maxW.y = pos.y + offset;
        }
    }

    private void ProjectPoints()
    {
        Transform cam = Camera.main.transform;
        Vector3 normal = cam.forward.normalized;
        float distance = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 point = normal * distance;
        Plane frustumPlane = new Plane(normal, point);

        //project min and max points to the plane
        Vector3 direction = new Vector3(0.0f, 0.0f, 1.0f);
        Vector3 min = new Vector3(minW.x, minW.y, 0.0f);
        Ray minRay = new Ray(min, direction);
        Vector3 max = new Vector3(maxW.x, maxW.y, 0.0f);
        Ray maxRay = new Ray(max, direction);

        float minDistance, maxDistance;
        if(frustumPlane.Raycast(minRay, out minDistance) && frustumPlane.Raycast(maxRay, out maxDistance))
        {
            Vector3 minPlane = min + direction * minDistance;
            Vector3 maxPlane = max + direction * maxDistance;
            //rotate according to the negative angle
            Quaternion rot = Quaternion.Inverse(cam.rotation);
            minW = rot * (minPlane - cam.position) + cam.position;
            maxW = rot * (maxPlane - cam.position) + cam.position;
        }
    }

    void CalculateCameraPosition()
    {
        Vector3 center = (minW + maxW) * 0.5f;
        Debug.Log(center);
        center.z = Camera.main.transform.position.z;

        float scale = CalcScaling();
        scale = Mathf.Lerp(prevScale, scale, Time.deltaTime * 10);

        Vector3 position = center;
        float distance = CalcNewDistance(scale);
        if(distance > minDistance)
        {
            position.z = minDistance;
        }
        else
        {
            position.z = distance;
        }

        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, position, Time.deltaTime * 30);

        prevScale = scale;
    }

    private void CalcFrustumDimnensionW()
    {
        float distance = 0.0f;
        distance = Mathf.Abs(Camera.main.transform.position.z);
        frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        frustumWidth = frustumHeight * Camera.main.aspect;
    }

    private float CalcScaling()
    {
        float width = maxW.x - minW.x;
        float height = maxW.y - minW.y;

        float scaleX = width / frustumWidth;
        float scaleY = height / frustumHeight;

        float scale = Mathf.Max(scaleX, scaleY);
        return scale;
    }

    private float CalcNewDistance(float scale)
    {
        if (scale == 0.0f)
            scale = 1.0f;
        float height = frustumHeight * scale;
        float distance = height * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return distance * -1.0f;
    }
}

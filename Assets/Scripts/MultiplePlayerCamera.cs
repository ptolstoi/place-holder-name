using UnityEngine;
using System.Collections;

public class MultiplePlayerCamera : MonoBehaviour {

    private GameObject[] players;
    private Vector2 minW, maxW;
    private float startDistance = 0.0f;
    private float frustumWidth, frustumHeight;
    public float minDistance = -10.0f;

	// Use this for initialization
	void Start () {
        players = GameObject.FindGameObjectsWithTag("Player");
        startDistance = Camera.main.transform.position.z;

        CalcFrustumDimnensionW();
	}
	
	// Update is called once per frame
	void Update () {
	    if(players.Length > 1)
        {
            CalculateBounds();

            CalculateCameraPosition();
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
                minW.x = pos.x;
            if (pos.x > maxW.x)
                maxW.x = pos.x;

            //y bounds
            if (pos.y < minW.y)
                minW.y = pos.y;
            if (pos.y > maxW.y)
                maxW.y = pos.y;
        }
    }

    void CalculateCameraPosition()
    {
        Vector3 center = (minW + maxW) * 0.5f;
        center.z = Camera.main.transform.position.z;

        float scale = CalcScaling();

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
        //Debug.Log(position.z);

        Camera.main.transform.position = position;
        //Vector3 pos = (min + max) * 0.5f;

        //Vector3 magnitudeVector = Vector3.Cross(pos, Vector3.up) * -1.0f;

        //pos.z = startDistance + magnitudeVector.z;
        //Camera.main.transform.position = pos;
        //Vector3 cameraDistance = Vector3.Cross(pos, Vector3.up) * -1.0f;
        //float magnitude = cameraDistance.magnitude;
        //pos.z = startDistance;

        //Vector3 minPoint = Camera.main.WorldToScreenPoint(minPos);
        //Vector3 maxPoint = Camera.main.WorldToScreenPoint(maxPos);

        //float zoom = ZoomCamera(minPoint, maxPoint);
        
        //if (minPoint != maxPoint)
        //{
        //    //float zoomFactor = ZoomCamera(minPoint, maxPoint);
        //    //Debug.Log(zoomFactor);
        ////    pos.z = startDistance * zoomFactor;
        //}
        ////Camera.main.transform.position = pos;
        //CalcAngle();
    }

    //private float ZoomCamera(Vector3 minScreen, Vector3 maxScreen)
    //{
    //    float width = Camera.main.pixelWidth;
    //    float height = Camera.main.pixelHeight;
    //    Vector3 pos = maxScreen - minScreen;
    //    float scaleX = Mathf.Abs( pos.x )/ width;
    //    float scaleY = Mathf.Abs( pos.y )/ height;

    //    float totalScale = Mathf.Max(scaleX, scaleY);
    //    return totalScale;
    //}

    //private float CalcAngle()
    //{
    //    float distance = Mathf.Abs(Camera.main.transform.position.z);
    //    float frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
    //    Debug.Log(frustumHeight);
    //    return frustumHeight;
    //}

    private void CalcFrustumDimnensionW()
    {
        float distance = Mathf.Abs(Camera.main.transform.position.z);
        frustumHeight = 2.0f * distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        frustumWidth = frustumHeight * Camera.main.aspect;
    }

    private float CalcScaling()
    {
        float width = maxW.x - minW.x;
        float height = maxW.y - minW.y;
        Debug.Log(new Vector2(width, height));

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
        //Debug.Log(height);
        float distance = height * 0.5f / Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        //Debug.Log(scale);
        return distance * -1.0f;
    }
}

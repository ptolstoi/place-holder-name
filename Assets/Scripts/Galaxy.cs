using UnityEngine;
using System.Collections;

public class Galaxy : MonoBehaviour
{

    public Vector2 velocity;
	
	// Update is called once per frame
	void Update ()
	{
        var offset = GetComponent<MeshRenderer>().material.mainTextureOffset;
        offset += velocity*Time.deltaTime;

        offset.x %= 1;
        offset.y %= 1;

	    GetComponent<MeshRenderer>().material.mainTextureOffset = offset;
	}
}

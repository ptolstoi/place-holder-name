using UnityEngine;
using System.Collections;

public class Galaxy : MonoBehaviour
{

    public Vector2 velocity;
	
	// Update is called once per frame
	void Update ()
	{
	    GetComponent<MeshRenderer>().material.mainTextureOffset += velocity*Time.deltaTime;
	}
}

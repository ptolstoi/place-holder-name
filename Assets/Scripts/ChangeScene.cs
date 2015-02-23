using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public int sceneNumber;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space)){

            AutoFade.LoadLevel(sceneNumber, 1, 1, Color.black);


        }
	
	}
}

using UnityEngine;
using System.Collections;

public class ScaleUp : MonoBehaviour {
    public float velocity;
    public float maxScale = 10;

    private void Awake()
    {
        DontDestroyOnLoad(this);
      }
    
    // Update is called once per frame
	void Update () {
        transform.localScale += Vector3.one * velocity * Time.deltaTime;

        if (transform.localScale.x >= maxScale)
        {
            Debug.Log("destroyd");
            Destroy(this.gameObject);
        }
	}
}

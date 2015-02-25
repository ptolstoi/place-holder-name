using UnityEngine;
using System.Collections;
using System.Linq;

public class FancyRotatedCamera : MonoBehaviour
{

    private Transform[] player;
    public float minZoom = 10;
    public float lerpSpeed = 10;

    private Vector3 lastPosition;

    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player").Select(a => a.transform).ToArray();
    }

	void Update ()
	{
	    var pos = player.Select(p => p.position).ToArray();

        var min = new Vector3(pos.Min(p => p.x), pos.Min(p => p.y));
        var max = new Vector3(pos.Max(p => p.x), pos.Max(p => p.y));

	    var center = (max - min)*0.5f + min;

	    Vector2 size = max - min;
	    size.y *= camera.aspect;

	    var maxSide = Mathf.Max(minZoom, Mathf.Max(size.x, size.y));

	    var targetPosition = center + Vector3.back*(Mathf.Sqrt(maxSide) * 4.2f + 0.24f * maxSide);

        lastPosition = Vector3.Lerp(lastPosition, targetPosition, Time.deltaTime * lerpSpeed);
	    transform.position = lastPosition;

        transform.rotation = Quaternion.identity;
        transform.RotateAround(center, Vector3.right, -20);
	}
}
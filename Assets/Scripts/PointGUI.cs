using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PointGUI : MonoBehaviour
{

    public Background Background;

    public Image[] Players;

	// Use this for initialization
	void Start () {
	    foreach (var stat in Background.ownership)
	    {
            print((int)stat.Key);
	        Players[(int) stat.Key].color = stat.Key.GetColor();
	    }
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	}
}

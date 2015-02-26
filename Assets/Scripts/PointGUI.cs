using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PointGUI : MonoBehaviour
{

    public Background Background;

    public RectTransform[] Players;

    public Text timeLeft;

    private RectTransform rectTransform;
    private float border;

    // Use this for initialization
	void Start () {
	    foreach (var stat in Background.ownership)
	    {
	        Players[(int) stat.Key].GetComponent<Image>().color = stat.Key.GetColor();
            Players[(int) stat.Key].SetWidth(0);
	    }

	    rectTransform = transform.GetChild(0).GetComponent<RectTransform>();

	    border = rectTransform.GetWidth()*0.01f;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    var totalWidth = 0.0f;
	    for (int i = 0; i < 4; ++i)
	    {
	        var player = (Player) i;
	        var rect = Players[i];
            float owned = Background.ownership[player] / (float)Background.createdPlanets;
	        var targetWidth = (rectTransform.GetWidth() - 3 * border)* owned;
	        rect.SetWidth(Mathf.Lerp(rect.GetWidth(), targetWidth, Time.deltaTime * 4));
	        totalWidth += rect.GetWidth() + border*(i != 3 && owned > 0 ? 1 : 0);
	    }

	    totalWidth /= 2;
	    totalWidth -= border/2;

	    for (int i = 0; i < 4; i++)
	    {
	        var rect = Players[i];
            rect.SetLeftTopPosition(-Vector2.right * totalWidth + Vector2.up * rect.GetHeight() * 0.5f);
	        totalWidth -= rect.GetWidth() + border;
	    }

	    var time = Mathf.FloorToInt(Background.TimeLeft);
        var mins = Mathf.FloorToInt(time / 60.0f);
        var secs = time - mins * 60;

	    timeLeft.text = String.Format("{0:00}:{1:00}", mins, secs);
	}
}

using UnityEngine;
using System.Collections;

public class TrailMaterialControler : MonoBehaviour
{

    public Texture2D ColorTexture;
    public Color Color;
    public Vector2 ColorVelocity;


    private Material material;

    private Vector2 offset;

    private float directionFactor;

    private bool pause;

    // Use this for initialization
    void Start()
    {
        material = GetComponent<Renderer>().material;
        material.SetTexture("_MainTex", ColorTexture);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!pause)
        {
            Vector2 textureOffset = GetComponent<Renderer>().material.GetTextureOffset("_MainTex");
            textureOffset.x = (textureOffset.x + Time.deltaTime * ColorVelocity.x) % 1;
            textureOffset.y = (textureOffset.y + Time.deltaTime * ColorVelocity.y * directionFactor) % 1;
            material.SetTextureOffset("_MainTex", textureOffset);

            material.SetColor("_Color", Color);

            GetComponent<Renderer>().material = material;
        }
    }

    public void SetDirection(float direction)
    {
        directionFactor = direction;
    }

    public void Pause()
    {
        pause = true;
    }

    public void UnPause()
    {
        pause = false;
    }
}

using UnityEngine;
using System.Collections;

public class TrailMaterialControler : MonoBehaviour
{

    public Texture2D ColorTexture;
    public Texture2D LightTexture;
    public Color Color;
    public Vector2 Speed;

    private Material material;

    private Vector2 offset;

    // Use this for initialization
    void Start()
    {
        material = renderer.material;
        material.SetTexture("_MainTex", ColorTexture);
        material.SetTexture("_LightTex", LightTexture);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 textureOffset = renderer.material.GetTextureOffset("_LightTex");
        textureOffset += Time.deltaTime * Speed;
        material.SetTextureOffset("_LightTex", textureOffset);

        material.SetColor("_Color", Color);

        renderer.material = material;
    }
}

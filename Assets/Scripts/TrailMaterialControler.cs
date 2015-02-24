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
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 textureOffset = material.GetTextureOffset("MainTex");
        textureOffset += Time.deltaTime * Speed;
    }
}

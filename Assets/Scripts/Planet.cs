using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Planet : MonoBehaviour
{

    public Transform OuterPlanet;
    public Transform InnerPlanet;
    public MeshRenderer Glow;
    public AnimationCurve PlayerChangeAnimation;
    public AnimationCurve WobbleAnimation;

    public float TransitionDuration = 1;
    public float MaxWobbleTime = 1;
    [Range(0, 1)]
    public float wobbleAmount = 0.75f;
    [Range(0, 1)]
    public float dragAmount = 0.75f;

    public GameObject[] Decorations;
    private Transform DecorationRoot;

    private float rotationSpeed;
    private bool hasMoons;
    private Vector3[] rotationAxis;

    private Material outerPlanetMaterial;
    private Material ringMaterial;
    private Material glowMaterial;

    public Player Owner { get; protected set; }

    protected PlayerController GrappledPlayer { get; set; }
    protected PlayerController lastGrappledPlayer;
    protected float wobbleTime;

    protected Quaternion defaultUp;
    public bool Celestial;

    public void ChangeOwner(PlayerController newOwner)
    {
        if (Owner != newOwner.Player && !Celestial)
        {
            ChangeColor(newOwner.Player.GetColor(), TransitionDuration);
            Owner = newOwner.Player;

            glowMaterial.mainTextureOffset += new Vector2(0.5f,0);

            SoundSystem.Instance.PlayChord(newOwner.audio, ChordType.GrabNew);
        }
        else
        {
            SoundSystem.Instance.PlayChord(newOwner.audio, ChordType.GrabOld);
        }
    }

    public void Grapple(PlayerController player)
    {
        GrappledPlayer = player;
    }

    public void ReleaseGrapple(PlayerController player)
    {
        GrappledPlayer = null;
    }

    void Start()
    {
        Owner = Player.PlayerNone;

        foreach (var ring in Decorations)
        {
            ring.SetActive(false);
        }

        GenerateDecorations();
        rotationSpeed = Random.Range(-90, 90) + 120;

        transform.localScale *= 3 + Random.Range(-0.5f, 0.5f);
        if (Celestial)
        {
            transform.localScale = 6 * Vector3.one;
            ChangeColor(new Color(1, 1, 0.9f), 0.1f);
            glowMaterial.color = new Color(1, 1, 0.9f, 0f);
            glowMaterial.SetFloat("_Alpha", 0.5f);
            Glow.transform.localScale *= 1.2f;
        }

        if (!Celestial)
        {
            var color = ColorPalette.CalcColorCold();
            ChangeColor(color, 0);
            defaultUp = Quaternion.Euler(40, 0, 0);
            transform.rotation = defaultUp;
            glowMaterial.SetFloat("_Alpha", 0.42f);

            ringMaterial.color = color;
            glowMaterial.color = color;
        }
        glowMaterial.mainTextureOffset += Vector2.right*Random.Range(0f, 1f);
        Glow.transform.SetParent(null);
        Glow.transform.eulerAngles = new Vector3(180, 0, 0);
        Glow.transform.position += Vector3.up*0.5f;
    }

    void GenerateDecorations()
    {
        var go = new GameObject("DecorationRoot");
        DecorationRoot = go.transform;
        DecorationRoot.SetParent(transform);
        DecorationRoot.localPosition = OuterPlanet.localPosition;

        hasMoons = false;

        var rng = Random.Range(0, 6);

        outerPlanetMaterial = new Material(OuterPlanet.GetComponent<MeshRenderer>().material);
        OuterPlanet.GetComponent<MeshRenderer>().material = outerPlanetMaterial;

        ringMaterial = new Material(Decorations[0].GetComponent<MeshRenderer>().material);
        Decorations[0].GetComponent<MeshRenderer    >().material = ringMaterial;
        Decorations[1].GetComponent<MeshRenderer>().material = ringMaterial;
        Decorations[2].GetComponent<MeshRenderer>().material = ringMaterial;

        glowMaterial = new Material(Glow.material);
        Glow.material = glowMaterial;

        if (Celestial)
        {
            return;
        }

        if (rng == 0 || rng == 1 || rng == 2)
        {
            var ring = GenerateDecoration(Decorations[rng]);
            //            ring.Rotate(Vector3.right, 20f);

            if (rng == 2)
            {
                OuterPlanet.localScale *= 0.5f;
            }
        }
        else if (rng == 3)
        {
            var ring1 = GenerateDecoration(Decorations[1], true);
            var ring2 = GenerateDecoration(Decorations[1]);
            var angle = Random.Range(0, 30) + 10 * (Random.value < 0.5f ? -1 : 1);

            ring2.localEulerAngles = ring1.localEulerAngles;
            ring1.localEulerAngles += new Vector3(0, angle);
            ring2.localEulerAngles += new Vector3(0, -angle);
        }
        else if (rng == 4)
        {
            var moonFar = GenerateDecoration(Decorations[3]);
            moonFar.localRotation = Random.rotation;
            var moonNear = GenerateDecoration(Decorations[4]);
            moonNear.localRotation = Random.rotation;
            hasMoons = true;
            rotationAxis =
            new[] {
                Random.value * (Vector3.left + Vector3.up),
                Random.value * (Vector3.left + Vector3.up)
            };

            moonFar.GetComponent<MeshRenderer>().material = outerPlanetMaterial;
            moonNear.GetComponent<MeshRenderer>().material = outerPlanetMaterial;
        }
    }

    Transform GenerateDecoration(GameObject go, bool clone = false)
    {
        var ring = clone ? Instantiate(go) as GameObject : go;

        ring.transform.SetParent(DecorationRoot.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        ring.SetActive(true);

        return ring.transform;
    }

    void Update()
    {
        glowMaterial.mainTextureOffset += new Vector2(0.25f * Time.deltaTime, 0);

        if (Celestial)
        {
            return;
        }

        if (!hasMoons)
        {
            DecorationRoot.transform.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);
        }
        else
        {
            var i = 0;
            foreach (Transform decoration in DecorationRoot)
            {
                decoration.transform.Rotate(rotationAxis[i], rotationSpeed * Time.deltaTime);
                ++i;
            }
        }

        Glow.transform.position = transform.position + Vector3.up * transform.localScale.x * 0.5f + Vector3.forward * 0.5f;

        if (GrappledPlayer != null)
        {
            var targetRotation = Quaternion.FromToRotation(Vector3.back, (GrappledPlayer.transform.position - transform.position));
            targetRotation = Quaternion.Slerp(targetRotation, defaultUp, 1 - dragAmount);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 20);
            wobbleTime = 0;
            lastGrappledPlayer = GrappledPlayer;
        }

        if (GrappledPlayer == null && lastGrappledPlayer != null)
        {
            var startRotation = Quaternion.FromToRotation(Vector3.back,
                (lastGrappledPlayer.transform.position - transform.position).normalized);
            startRotation = Quaternion.Slerp(startRotation, defaultUp, 1 - wobbleAmount);

            var targetWobbleRotation = Quaternion.FromToRotation(Vector3.back,
                (transform.position - lastGrappledPlayer.transform.position).normalized);
            targetWobbleRotation = Quaternion.Slerp(targetWobbleRotation, defaultUp, 1 - wobbleAmount);

            transform.localRotation = Quaternion.Slerp(startRotation, targetWobbleRotation,
                WobbleAnimation.Evaluate(wobbleTime / MaxWobbleTime));
            wobbleTime += Time.deltaTime;
        }

        Vector2 bias = (InnerPlanet.transform.position - transform.position);
        Glow.transform.position += (Vector3)bias;
    }

    void ChangeColor(Color newColor, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(PulsateColor(newColor, 0.6f));
    }

    IEnumerator ChangeColorCoroutine(Color newColor, float duration)
    {
        InnerPlanet.localScale = Vector3.zero;
        float timePassed = 0;

        var mr = InnerPlanet.GetComponent<MeshRenderer>();
        var oldColor = !Celestial ? ringMaterial.color : Color.clear;
        glowMaterial.color = newColor;

        while (timePassed < duration)
        {
            yield return null;

            mr.material.color = newColor;
            var lrp = PlayerChangeAnimation.Evaluate(timePassed / duration);
            InnerPlanet.localScale = Vector3.one * lrp * 2;

            if (!Celestial)
            {
                var ringColor = newColor.FromColor();
                ringColor.b += 0.3f;
                ringMaterial.color = Color.Lerp(oldColor, ringColor.ToColor(), timePassed/duration);
            }

            timePassed += Time.deltaTime;
        }

        outerPlanetMaterial.color = newColor;

        InnerPlanet.localScale = Vector3.zero;
    }

    IEnumerator PulsateColor(Color baseColor, float duration)
    {
        yield return StartCoroutine(ChangeColorCoroutine(baseColor, duration));
        while (true)
        {
            var rnd = baseColor.FromColor();
            rnd.b += Random.Range(0, 0.4f);
            Color newColor = rnd.ToColor();
            yield return StartCoroutine(ChangeColorCoroutine(newColor, duration));
        }
    }

    public void SetCentralPlanet()
    {
        GetComponent<CircleCollider2D>().center = Vector2.zero;
    }
}

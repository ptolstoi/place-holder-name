using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Planet : MonoBehaviour
{

    public Transform OuterPlanet;
    public Transform InnerPlanet;
    public AnimationCurve PlayerChangeAnimation;
    public AnimationCurve WobbleAnimation;

    public float TransitionDuration = 1;
    public float MaxWobbleTime = 1;
    [Range(0,1)]
    public float wobbleAmount = 0.75f;

    public GameObject[] Decorations;
    private Transform DecorationRoot;

    private float rotationSpeed;
    private bool hasMoons;
    private Vector3[] rotationAxis;

    private Material outerPlanetMaterial;

    public Player Owner { get; protected set; }

    protected PlayerController GrappledPlayer { get; set; }
    protected PlayerController lastGrappledPlayer;
    protected float wobbleTime;

    public void ChangeOwner(Player newOwner)
    {
        if (Owner != newOwner)
        {
            ChangeColor(newOwner.GetColor(), TransitionDuration);
            Owner = newOwner;
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
        foreach (var ring in Decorations)
        {
            ring.SetActive(false);
        }

        GenerateDecorations();
        rotationSpeed = Random.Range(-90, 90) + 180;

        transform.localScale *= 1 + Random.Range(-0.5f, 0.5f);
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

        if (rng == 0 || rng == 1 || rng == 2)
        {
            var ring = GenerateDecoration(Decorations[rng]);
            ring.Rotate(Vector3.right, Random.Range(-20f, 20f));

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
        if (!hasMoons)
        {
            DecorationRoot.transform.Rotate(Vector3.back, rotationSpeed*Time.deltaTime);
        }
        else
        {
            var i = 0;
            foreach (Transform decoration in DecorationRoot)
            {
                decoration.transform.Rotate(rotationAxis[i], rotationSpeed*Time.deltaTime);
                ++i;
            }
        }

        if (GrappledPlayer != null)
        {
            var targetRotation = Quaternion.FromToRotation(Vector3.back, (GrappledPlayer.transform.position - transform.position));
            targetRotation = Quaternion.Slerp(targetRotation, Quaternion.identity, 1 - wobbleAmount);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 20);
            wobbleTime = 0;
            lastGrappledPlayer = GrappledPlayer;
        }

        if (GrappledPlayer == null && lastGrappledPlayer != null)
        {
            var startRotation = Quaternion.FromToRotation(Vector3.back,
                (lastGrappledPlayer.transform.position - transform.position).normalized);
            startRotation = Quaternion.Slerp(startRotation, Quaternion.identity, 1 - wobbleAmount);

            var targetWobbleRotation = Quaternion.FromToRotation(Vector3.back,
                (transform.position - lastGrappledPlayer.transform.position).normalized);
            targetWobbleRotation = Quaternion.Slerp(targetWobbleRotation, Quaternion.identity, 1 - wobbleAmount);
        
            transform.localRotation = Quaternion.Slerp(startRotation, targetWobbleRotation,
                WobbleAnimation.Evaluate(wobbleTime / MaxWobbleTime));
             wobbleTime += Time.deltaTime;
        }
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

        while (timePassed < duration)
        {
            yield return null;

            mr.material.color = newColor;
            InnerPlanet.localScale = Vector3.one*PlayerChangeAnimation.Evaluate(timePassed/duration) * 2;

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
            var rnd = Random.Range(-0.3f, 0.3f);
            Color newColor = (Vector4)baseColor + new Vector4(0, 0, 0, 1) +
                (Vector4)(Vector3.one * rnd);

            yield return StartCoroutine(ChangeColorCoroutine(newColor, duration));
        }
    }
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trail : MonoBehaviour
{
    public float height = 2.0f;
    public float time = 2.0f;
    public float minDistance = 0.1f;
    public int filterWindowSize = 10;

    private List<TrailSection> sections = new List<TrailSection>();

    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private Vector2[] uv;
    int[] triangles;

    private Vector2 position;
    private TrailSection currentSection;

    private TrailSection[] window;

    void Start()
    {
        window = new TrailSection[filterWindowSize];
    }

    void LateUpdate()
    {
        position = transform.position;

        while (sections.Count > 0 && Time.time > sections[0].Time + time)
        {
            sections.RemoveAt(0);
        }


        int lastIndex = sections.Count - 1;

        if (sections.Count == 0 || (position - sections[lastIndex].Position).sqrMagnitude > minDistance * minDistance)
        {
            TrailSection section = new TrailSection();

            section.Bitangent = transform.right;
            section.Position = position;
            section.Time = Time.time;
            sections.Add(section);
        }

        lastIndex = sections.Count - 1;

        mesh = GetComponent<MeshFilter>().mesh;
        if (sections.Count > 2)
        {

            vertices = new Vector3[sections.Count * 2];
            colors = new Color[sections.Count * 2];
            uv = new Vector2[sections.Count * 2];

            currentSection = sections[0];

            Color interpolatedColor;
            float u;

            Vector2 smoothPosition = sections[lastIndex].Position;
            Vector2 smoothBitangent = sections[lastIndex].Bitangent;

            float smoothingLerp = 0.9f;

            Matrix4x4 localSpaceTransform = transform.worldToLocalMatrix;

            for (int i = 0; i < window.Length; i++)
            {
                window[i] = sections[lastIndex];
            }

            TrailSection smoothedSection = new TrailSection();

            float inverseLength = 1.0f / window.Length;

            for (int i = sections.Count - 1; i >= 0; i--)
            {
                currentSection = sections[i];

                for (int j = 1; j < window.Length; j++)
                {
                    TrailSection nextSection = window[j];

                    window[j - 1] = window[j];
                    smoothedSection.Position += nextSection.Position;
                    smoothedSection.Bitangent += nextSection.Bitangent;
                }

                smoothedSection.Position += currentSection.Position;
                smoothedSection.Bitangent += currentSection.Bitangent;

                window[window.Length - 1] = currentSection;

                smoothedSection.Position *= inverseLength;
                smoothedSection.Bitangent *= inverseLength;

                smoothPosition = Vector2.Lerp(currentSection.Position, smoothPosition, smoothingLerp);
                smoothBitangent = Vector2.Lerp(currentSection.Bitangent, smoothBitangent, smoothingLerp).normalized;


                vertices[i * 2 + 0] = localSpaceTransform.MultiplyPoint(smoothPosition + smoothBitangent * (height / 2f));
                vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(smoothPosition - smoothBitangent * (height / 2f));

                u = i > 0 ? Mathf.Clamp01((Time.time - currentSection.Time) / time) : 1.0f;
                uv[i * 2 + 0] = new Vector2(u, 0);
                uv[i * 2 + 1] = new Vector2(u, 1);

                interpolatedColor = Color.Lerp(Color.white, new Color(1, 1, 1, 0), u);
                colors[i * 2 + 0] = interpolatedColor;
                colors[i * 2 + 1] = interpolatedColor;
            }

            triangles = new int[(sections.Count - 1) * 2 * 3];

            for (int i = 0; i < triangles.Length / 6; i++)
            {
                triangles[i * 6 + 0] = i * 2;
                triangles[i * 6 + 1] = i * 2 + 1;
                triangles[i * 6 + 2] = i * 2 + 2;

                triangles[i * 6 + 3] = i * 2 + 2;
                triangles[i * 6 + 4] = i * 2 + 1;
                triangles[i * 6 + 5] = i * 2 + 3;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.uv = uv;
            mesh.triangles = triangles;
        }
    }

    public void Reset()
    {
        sections.Clear();
    }

    private class TrailSection
    {
        public Vector2 Position;
        public Vector2 Bitangent;
        public float Time;
    }

}

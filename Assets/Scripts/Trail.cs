using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trail : MonoBehaviour
{
    public float height = 2.0f;
    public float time = 2.0f;
    public float minDistance = 0.1f;

    private List<TrailSection> sections = new List<TrailSection>();

    void LateUpdate()
    {
        Vector3 position = transform.position;
        float now = Time.time;

        // Remove old sections
        while (sections.Count > 0 && now > sections[0].Time + time)
        {
            sections.RemoveAt(0);
        }

        // Add a new trail section
        if (sections.Count == 0 || (sections[0].Point - position).sqrMagnitude > minDistance * minDistance)
        {
            TrailSection section = new TrailSection();
            section.Point = position;
            section.UpDirection = transform.TransformDirection(-Vector3.right);
            section.Time = now;
            sections.Add(section);
        }
        // Rebuild the mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        // We need at least 2 sections to create the line
        if (sections.Count < 2)
            return;

        Vector3[] vertices = new Vector3[sections.Count * 2];
        Color[] colors = new Color[sections.Count * 2];
        Vector2[] uv = new Vector2[sections.Count * 2];
        TrailSection currentSection = sections[0];

        // Use matrix instead of transform.TransformPoint for performance reasons
        Matrix4x4 localSpaceTransform = transform.worldToLocalMatrix;

        // Generate vertex, uv and colors
        for (int i = 0; i < sections.Count; i++)
        {
            currentSection = sections[i];
            // Calculate u for texture uv and color interpolation
            float u = 1.0f;
            if (i != 0)
                u = Mathf.Clamp01((Time.time - currentSection.Time) / time);
            // Calculate upwards direction
            Vector3 upDir = currentSection.UpDirection;
            // Generate vertices

            float x = (sections.Count - i) / sections.Count;

            vertices[i * 2 + 0] = localSpaceTransform.MultiplyPoint(currentSection.Point - upDir * height / 2f * x);
            vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(currentSection.Point + upDir * height / 2f * x);
            uv[i * 2 + 0] = new Vector2(u, 0);
            uv[i * 2 + 1] = new Vector2(u, 1);
        }

        // Generate triangles indices
        int[] triangles = new int[(sections.Count - 1) * 2 * 3];
        for (int i = 0; i < triangles.Length / 6; i++)
        {
            triangles[i * 6 + 0] = i * 2;
            triangles[i * 6 + 1] = i * 2 + 1;
            triangles[i * 6 + 2] = i * 2 + 2;

            triangles[i * 6 + 3] = i * 2 + 2;
            triangles[i * 6 + 4] = i * 2 + 1;
            triangles[i * 6 + 5] = i * 2 + 3;
        }

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uv;
        mesh.triangles = triangles;

    }

    private class TrailSection
    {
        public Vector3 Point;
        public Vector3 UpDirection;
        public float Time;
    }

}

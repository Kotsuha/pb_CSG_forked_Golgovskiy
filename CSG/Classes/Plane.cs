using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
    /// <summary>
    /// Represents a plane in 3D space.
    /// <remarks>Does not include position.</remarks>
    /// </summary>
    sealed class Plane
    {
        public Vector3 normal; // Normal vector of the plane
        public float w;        // Distance from the origin along the normal vector

        [System.Flags]
        enum EPolygonType
        {
            Coplanar    = 0,    // Polygon lies coplanar with the plane
            Front       = 1,    // Polygon lies entirely in front of the plane
            Back        = 2,    // Polygon lies entirely behind the plane
            Spanning    = 3     // Polygon spans across the plane
        };

        public Plane()
        {
            normal = Vector3.zero;
            w = 0f;
        }

        /// <summary>
        /// Constructs a plane from three points, calculating the normal and distance.
        /// </summary>
        /// <param name="a">First point on the plane.</param>
        /// <param name="b">Second point on the plane.</param>
        /// <param name="c">Third point on the plane.</param>
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            // Calculate the normal vector using the cross product of vectors formed by the points
            normal = Vector3.Cross(b - a, c - a);
            normal.Normalize(); // Normalize the normal vector
            w = Vector3.Dot(normal, a); // Calculate the distance from the origin along the normal vector
        }

        /// <summary>
        /// Returns a string representation of the plane in the format "normal w".
        /// </summary>
        public override string ToString() => $"{normal} {w}";

        /// <summary>
        /// Checks if the plane's normal vector is valid (non-zero length).
        /// </summary>
        public bool Valid()
        {
            return normal.magnitude > 0f;
        }

        /// <summary>
        /// Flips the orientation of the plane by reversing the normal vector and distance.
        /// </summary>
        public void Flip()
        {
            normal = -normal;
            w = -w;
        }

        /// <summary>
        /// Splits a given polygon by this plane and categorizes fragments into appropriate lists.
        /// </summary>
        /// <param name="polygon">Polygon to split.</param>
        /// <param name="coplanarFront">List to collect coplanar polygons in front of the plane.</param>
        /// <param name="coplanarBack">List to collect coplanar polygons behind the plane.</param>
        /// <param name="front">List to collect polygons entirely in front of the plane.</param>
        /// <param name="back">List to collect polygons entirely behind the plane.</param>
        public void SplitPolygon(Polygon polygon, List<Polygon> coplanarFront, List<Polygon> coplanarBack, List<Polygon> front, List<Polygon> back)
        {
            // Classify each point of the polygon relative to the plane
            EPolygonType polygonType = 0;
            List<EPolygonType> types = new List<EPolygonType>();

            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                float t = Vector3.Dot(this.normal, polygon.vertices[i].position) - this.w;
                EPolygonType type = (t < -CSG.epsilon) ? EPolygonType.Back : ((t > CSG.epsilon) ? EPolygonType.Front : EPolygonType.Coplanar);
                polygonType |= type; // Update polygon type based on current vertex classification
                types.Add(type); // Store vertex type
            }

            // Place the polygon or its fragments into appropriate lists based on classification
            switch (polygonType)
            {
                case EPolygonType.Coplanar:
                    {
                        // Determine if the polygon is front or back relative to the plane
                        if (Vector3.Dot(this.normal, polygon.plane.normal) > 0)
                            coplanarFront.Add(polygon);
                        else
                            coplanarBack.Add(polygon);
                    }
                    break;

                case EPolygonType.Front:
                    {
                        front.Add(polygon);
                    }
                    break;

                case EPolygonType.Back:
                    {
                        back.Add(polygon);
                    }
                    break;

                case EPolygonType.Spanning:
                    {
                        List<Vertex> f = new List<Vertex>(); // Vertices in front of the plane
                        List<Vertex> b = new List<Vertex>(); // Vertices behind the plane

                        for (int i = 0; i < polygon.vertices.Count; i++)
                        {
                            int j = (i + 1) % polygon.vertices.Count;

                            EPolygonType ti = types[i], tj = types[j];
                            Vertex vi = polygon.vertices[i], vj = polygon.vertices[j];

                            if (ti != EPolygonType.Back)
                                f.Add(vi);

                            if (ti != EPolygonType.Front)
                                b.Add(vi);

                            if ((ti | tj) == EPolygonType.Spanning)
                            {
                                // Calculate intersection point along the edge
                                float t = (this.w - Vector3.Dot(this.normal, vi.position)) / Vector3.Dot(this.normal, vj.position - vi.position);
                                Vertex v = VertexUtility.Mix(vi, vj, t); // Interpolate vertex

                                f.Add(v); // Add vertex to front list
                                b.Add(v); // Add vertex to back list
                            }
                        }

                        // Create new polygons from front and back lists if they have enough vertices
                        if (f.Count >= 3)
                            front.Add(new Polygon(f, polygon.material));

                        if (b.Count >= 3)
                            back.Add(new Polygon(b, polygon.material));
                    }
                    break;
            }   // End switch(polygonType)
        }
    }
}

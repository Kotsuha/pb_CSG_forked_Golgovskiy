using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
    /// <summary>
    /// Represents a polygon face with an arbitrary number of vertices.
    /// </summary>
    sealed class Polygon
    {
        public List<Vertex> vertices; // List of vertices that define the polygon
        public Plane plane;           // Plane in which the polygon lies
        public Material material;     // Material associated with the polygon

        /// <summary>
        /// Constructs a polygon from a list of vertices and a material.
        /// </summary>
        /// <param name="list">List of vertices that define the polygon.</param>
        /// <param name="mat">Material associated with the polygon.</param>
        public Polygon(List<Vertex> list, Material mat)
        {
            vertices = list; // Initialize the vertices list
            // Initialize the plane using the first three vertices of the polygon
            plane = new Plane(list[0].position, list[1].position, list[2].position);
            material = mat; // Initialize the material
        }

        /// <summary>
        /// Flips the orientation of the polygon.
        /// </summary>
        public void Flip()
        {
            // Reverse the order of vertices to flip the polygon orientation
            vertices.Reverse();

            // Flip each vertex individually
            for (int i = 0; i < vertices.Count; i++)
                vertices[i].Flip();

            // Flip the plane associated with the polygon
            plane.Flip();
        }

        /// <summary>
        /// Returns a string representation of the polygon.
        /// </summary>
        /// <returns>A string in the format "[vertex count] plane normal".</returns>
        public override string ToString()
        {
            return $"[{vertices.Count}] {plane.normal}"; // Returns the number of vertices and the plane's normal vector
        }
    }
}

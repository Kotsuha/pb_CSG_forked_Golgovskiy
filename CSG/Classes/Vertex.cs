using System;
using UnityEngine;

namespace Parabox.CSG
{
    /// <summary>
    /// Holds information about a single vertex, and provides methods for averaging between many.
    /// <remarks>All values are optional. Where not present a default value will be substituted if necessary.</remarks>
    /// </summary>
    public struct Vertex
    {
        // Private fields for vertex attributes
        Vector3 m_Position;   // Position in model space
        Color m_Color;        // Vertex color
        Vector3 m_Normal;     // Normal vector
        Vector4 m_Tangent;    // Tangent vector
        Vector2 m_UV0;        // First UV channel
        Vector2 m_UV2;        // Second UV channel
        Vector4 m_UV3;        // Third UV channel
        Vector4 m_UV4;        // Fourth UV channel
        VertexAttributes m_Attributes; // Bitmask of set attributes

        /// <value>
        /// The position in model space.
        /// </value>
        public Vector3 position
        {
            get { return m_Position; }
            set
            {
                hasPosition = true; // Mark that position is set
                m_Position = value; // Set position
            }
        }

        /// <value>
        /// Vertex color.
        /// </value>
        public Color color
        {
            get { return m_Color; }
            set
            {
                hasColor = true; // Mark that color is set
                m_Color = value; // Set color
            }
        }

        /// <value>
        /// Unit vector normal.
        /// </value>
        public Vector3 normal
        {
            get { return m_Normal; }
            set
            {
                hasNormal = true; // Mark that normal is set
                m_Normal = value; // Set normal
            }
        }

        /// <value>
        /// Vertex tangent (sometimes called binormal).
        /// </value>
        public Vector4 tangent
        {
            get { return m_Tangent; }
            set
            {
                hasTangent = true; // Mark that tangent is set
                m_Tangent = value; // Set tangent
            }
        }

        /// <value>
        /// UV 0 channel. Also called textures.
        /// </value>
        public Vector2 uv0
        {
            get { return m_UV0; }
            set
            {
                hasUV0 = true; // Mark that UV0 is set
                m_UV0 = value; // Set UV0
            }
        }

        /// <value>
        /// UV 2 channel.
        /// </value>
        public Vector2 uv2
        {
            get { return m_UV2; }
            set
            {
                hasUV2 = true; // Mark that UV2 is set
                m_UV2 = value; // Set UV2
            }
        }

        /// <value>
        /// UV 3 channel.
        /// </value>
        public Vector4 uv3
        {
            get { return m_UV3; }
            set
            {
                hasUV3 = true; // Mark that UV3 is set
                m_UV3 = value; // Set UV3
            }
        }

        /// <value>
        /// UV 4 channel.
        /// </value>
        public Vector4 uv4
        {
            get { return m_UV4; }
            set
            {
                hasUV4 = true; // Mark that UV4 is set
                m_UV4 = value; // Set UV4
            }
        }

        /// <summary>
        /// Check if a vertex attribute has been set.
        /// </summary>
        /// <param name="attribute">The attribute or attributes to test for.</param>
        /// <returns>True if this vertex has the specified attributes set, false if they are default values.</returns>
        public bool HasArrays(VertexAttributes attribute)
        {
            // Return true if the specified attribute is set in m_Attributes
            return (m_Attributes & attribute) == attribute;
        }

        // Property to check if position is set
        public bool hasPosition
        {
            get { return (m_Attributes & VertexAttributes.Position) == VertexAttributes.Position; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Position) : (m_Attributes & ~(VertexAttributes.Position)); }
        }

        // Property to check if color is set
        public bool hasColor
        {
            get { return (m_Attributes & VertexAttributes.Color) == VertexAttributes.Color; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Color) : (m_Attributes & ~(VertexAttributes.Color)); }
        }

        // Property to check if normal is set
        public bool hasNormal
        {
            get { return (m_Attributes & VertexAttributes.Normal) == VertexAttributes.Normal; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Normal) : (m_Attributes & ~(VertexAttributes.Normal)); }
        }

        // Property to check if tangent is set
        public bool hasTangent
        {
            get { return (m_Attributes & VertexAttributes.Tangent) == VertexAttributes.Tangent; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Tangent) : (m_Attributes & ~(VertexAttributes.Tangent)); }
        }

        // Property to check if UV0 is set
        public bool hasUV0
        {
            get { return (m_Attributes & VertexAttributes.Texture0) == VertexAttributes.Texture0; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Texture0) : (m_Attributes & ~(VertexAttributes.Texture0)); }
        }

        // Property to check if UV2 is set
        public bool hasUV2
        {
            get { return (m_Attributes & VertexAttributes.Texture1) == VertexAttributes.Texture1; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Texture1) : (m_Attributes & ~(VertexAttributes.Texture1)); }
        }

        // Property to check if UV3 is set
        public bool hasUV3
        {
            get { return (m_Attributes & VertexAttributes.Texture2) == VertexAttributes.Texture2; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Texture2) : (m_Attributes & ~(VertexAttributes.Texture2)); }
        }

        // Property to check if UV4 is set
        public bool hasUV4
        {
            get { return (m_Attributes & VertexAttributes.Texture3) == VertexAttributes.Texture3; }
            private set { m_Attributes = value ? (m_Attributes | VertexAttributes.Texture3) : (m_Attributes & ~(VertexAttributes.Texture3)); }
        }

        /// <summary>
        /// Flips the normal and tangent vectors of the vertex.
        /// </summary>
        public void Flip()
        {
            if(hasNormal)
                m_Normal *= -1f; // Invert the normal vector

            if (hasTangent)
                m_Tangent *= -1f; // Invert the tangent vector
        }
    }
}

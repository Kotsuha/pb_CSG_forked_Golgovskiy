using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Parabox.CSG
{
    sealed class Node
    {
        public List<Polygon> polygons;  // List of polygons in this node

        public Node front;  // Front node (subtree) of the BSP tree
        public Node back;   // Back node (subtree) of the BSP tree

        public Plane plane; // Plane used for partitioning polygons

        public Node()
        {
            front = null;
            back = null;
        }

        public Node(List<Polygon> list)
        {
            Build(list);
        }

        public Node(List<Polygon> list, Plane plane, Node front, Node back)
        {
            this.polygons = list;
            this.plane = plane;
            this.front = front;
            this.back = back;
        }

        public Node Clone()
        {
            Node clone = new Node(this.polygons, this.plane, this.front, this.back);

            return clone;
        }

        // Remove all polygons in this BSP tree that are inside the other BSP tree
        // `other`.
        public void ClipTo(Node other)
        {
            this.polygons = other.ClipPolygons(this.polygons);

            if (this.front != null)
            {
                this.front.ClipTo(other);
            }

            if (this.back != null)
            {
                this.back.ClipTo(other);
            }
        }

        // Convert solid space to empty space and empty space to solid space.
        public void Invert()
        {
            for (int i = 0; i < this.polygons.Count; i++)
                this.polygons[i].Flip();

            this.plane.Flip();

            if (this.front != null)
            {
                this.front.Invert();
            }

            if (this.back != null)
            {
                this.back.Invert();
            }

            Node tmp = this.front;
            this.front = this.back;
            this.back = tmp;
        }

        // Build a BSP tree out of `polygons`. When called on an existing tree, the
        // new polygons are filtered down to the bottom of the tree and become new
        // nodes there. Each set of polygons is partitioned using the first polygon
        // (no heuristic is used to pick a good split).
        public void Build(List<Polygon> list)
        {
            if (list.Count < 1)
                return;

            // Check if this node is a new node or needs initialization
            bool newNode = plane == null || !plane.Valid(); 

            // If it's a new node, initialize the plane
            if (newNode)
            {
                plane = new Plane();
                plane.normal = list[0].plane.normal;
                plane.w = list[0].plane.w;
            }

            // Initialize the polygons list if it's null
            if (polygons == null)
                polygons = new List<Polygon>();
                
            var listFront = new List<Polygon>();
            var listBack = new List<Polygon>();

            // Split each polygon in list into front and back lists based on the plane
            for (int i = 0; i < list.Count; i++)
                plane.SplitPolygon(list[i], polygons, polygons, listFront, listBack);
            
            // Build the front subtree with polygons in listFront
            if (listFront.Count > 0)
            {    
                // SplitPolygon can fail to correctly identify coplanar planes when the epsilon value is too low. When
                // this happens, the front or back list will be filled and built into a new node recursively. This 
                // check catches that case and sorts the front/back lists into the coplanar polygons collection.
                if (newNode && list.SequenceEqual(listFront))
                    polygons.AddRange(listFront);
                else
                    (front ?? (front = new Node())).Build(listFront);
            }

            // Build the back subtree with polygons in listBack
            if (listBack.Count > 0)
            {
                if (newNode && list.SequenceEqual(listBack))
                    polygons.AddRange(listBack);
                else
                    (back ?? (back = new Node())).Build(listBack);
            }
        }

        // Recursively remove all polygons in `polygons` that are inside this BSP tree.
        public List<Polygon> ClipPolygons(List<Polygon> list)
        {
            if (!this.plane.Valid())
            {
                return list;
            }

            List<Polygon> list_front = new List<Polygon>();
            List<Polygon> list_back = new List<Polygon>();

            // Split each polygon in list into front and back lists based on the plane
            for (int i = 0; i < list.Count; i++)
            {
                this.plane.SplitPolygon(list[i], list_front, list_back, list_front, list_back);
            }

            // Recursively clip front and back polygons
            if (this.front != null)
            {
                list_front = this.front.ClipPolygons(list_front);
            }

            if (this.back != null)
            {
                list_back = this.back.ClipPolygons(list_back);
            }
            else
            {
                list_back.Clear();
            }

            // Concatenate front and back lists
            list_front.AddRange(list_back);

            return list_front;
        }

        // Return a list of all polygons in this BSP tree.
        public List<Polygon> AllPolygons()
        {
            List<Polygon> list = polygons != null ? new List<Polygon>(polygons) : new List<Polygon>();
            List<Polygon> list_front = new List<Polygon>(), list_back = new List<Polygon>();

            // Recursively get all polygons in front subtree
            if (this.front != null)
            {
                list_front = this.front.AllPolygons();
            }

            // Recursively get all polygons in back subtree
            if (this.back != null)
            {
                list_back = this.back.AllPolygons();
            }

            // Concatenate all lists
            list.AddRange(list_front);
            list.AddRange(list_back);

            return list;
        }

        #region STATIC OPERATIONS

        // Return a new CSG solid representing space in either this solid or in the
        // solid `b`. Neither this solid nor the solid `b` are modified.
        public static Node Union(Node a1, Node b1)
        {
            // Clone the input nodes to prevent modification
            Node a = a1.Clone();
            Node b = b1.Clone();
        
            // Clip node a to node b and vice versa
            a.ClipTo(b);
            b.ClipTo(a);
            
            // Invert node b and clip it to node a again
            b.Invert();
            b.ClipTo(a);
            b.Invert();
        
            // Rebuild node a with all polygons from both nodes
            a.Build(b.AllPolygons());
        
            Node ret = new Node(a.AllPolygons());
        
            return ret;
        }
        
        // Return a new CSG solid representing space in this solid but not in the
        // solid `b`. Neither this solid nor the solid `b` are modified.
        public static Node Subtract(Node a1, Node b1)
        {
            // Clone the input nodes to prevent modification
            Node a = a1.Clone();
            Node b = b1.Clone();
        
            // Invert node a and clip it to node b
            a.Invert();
            a.ClipTo(b);
            
            // Clip node b to inverted node a and invert node b
            b.ClipTo(a);
            b.Invert();
            b.ClipTo(a);
            b.Invert();
            
            // Rebuild node a with all polygons from clipped node b
            a.Build(b.AllPolygons());
            a.Invert();
        
            Node ret = new Node(a.AllPolygons());
        
            return ret;
        }

        // Return a new CSG solid representing space both in this solid and in the
        // solid `b`. Neither this solid nor the solid `b` are modified.
        public static Node Intersect(Node a1, Node b1)
        {
            // Clone the input nodes to prevent modification
            Node a = a1.Clone();
            Node b = b1.Clone();

            // Invert node a and clip node b to it
            a.Invert();
            b.ClipTo(a);
            
            // Invert node b and clip node a to it
            b.Invert();
            a.ClipTo(b);

            // Rebuild node a with all polygons from clipped node b
            a.Build(b.AllPolygons());
            a.Invert();

            Node ret = new Node(a.AllPolygons());

            return ret;
        }

        #endregion
    }
}

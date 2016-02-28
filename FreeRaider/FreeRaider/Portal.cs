using System;
using System.Collections.Generic;
using OpenTK;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int PORTAL_NORMAL = 0x00;
        public const int PORTAL_FICTIVE = 0x01;

        public const int SPLIT_EMPTY = 0x00;
        public const int SPLIT_SUCCESS = 0x01;
    }

    public class Portal
    {
        public List<Vector3> Vertices { get; set; }

        public Plane Normal { get; set; }

        public Vector3 Centre { get; set; }

        public Room DestRoom { get; set; }

        public Room CurrentRoom { get; set; }

        public uint Flag { get; set; }

        public void Move(Vector3 mv)
        {
            Centre += mv;
            for (var i = 0; i < Vertices.Count; i++)
                Vertices[i] += mv;
            Normal.MoveTo(Vertices[0]);
        }

        public bool RayIntersect(Vector3 ray, Vector3 rayStart)
        {
            if (Math.Abs(Normal.Normal.Dot(ray)) < 0.02) return false;
            if (-Normal.Distance(rayStart) <= 0) return false;

            var T = rayStart - Vertices[0];
            var edge = Vertices[1] - Vertices[0];
            for (var i = 2; i < Vertices.Count; i++)
            {
                var prev = edge;
                edge = Vertices[i] - Vertices[0];
                var P = ray.Cross(edge);
                var Q = T.Cross(prev);
                var t = P.Dot(prev);
                var u = P.Dot(T) / t;
                var v = Q.Dot(ray) / t;
                t = 1.0f - u - v;
                if (u.IsBetween(0.0f, 1.0f) && v.IsBetween(0.0f, 1.0f) && t.IsBetween(0.0f, 1.0f))
                    return true;
            }

            return false;
        }

        public void GenNormal()
        {
            Assert.That(Vertices.Count > 3);
            var v1 = Vertices[1] - Vertices[0];
            var v2 = Vertices[2] - Vertices[1];
            Normal.Assign(v1, v2, Vertices[0]);
        }
    }
}

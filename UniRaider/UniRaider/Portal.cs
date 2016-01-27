using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class Portal
    {
        public Vector3[] Vertices { get; set; }

        public Plane Normal { get; set; }

        public Vector3 Centre { get; set; }

        public Room DestRoom { get; set; }

        public Room CurrentRoom { get; set; }

        public uint Flag { get; set; }

        public void Move(Vector3 mv)
        {
            Centre += mv;
            for (var i = 0; i < Vertices.Length; i++)
                Vertices[i] += mv;
            Normal.MoveTo(Vertices[0]);
        }

        public bool RayIntersect(Vector3 ray, Vector3 rayStart)
        {
            if (Math.Abs(Vector3.Dot(Normal.Normal, ray)) < 0.02) return false;
            if (-Normal.Distance(rayStart) <= 0) return false;

            var T = rayStart - Vertices[0];
            var edge = Vertices[1] - Vertices[0];
            for (var i = 2; i < Vertices.Length; i++)
            {
                var prev = edge;
                edge = Vertices[i] - Vertices[0];
                var P = Vector3.Cross(ray, edge);
                var Q = Vector3.Cross(T, prev);
                var t = Vector3.Dot(P, prev);
                var u = Vector3.Dot(P, T) / t;
                var v = Vector3.Dot(Q, ray) / t;
                t = 1.0f - u - v;
                if (u.IsBetween(0.0f, 1.0f) && v.IsBetween(0.0f, 1.0f) && t.IsBetween(0.0f, 1.0f))
                    return true;
            }

            return false;
        }

        public void GenNormal()
        {
            // TODO: Assert vertices.size() > 3
            var v1 = Vertices[1] - Vertices[0];
            var v2 = Vertices[2] - Vertices[1];
            Normal.Assign(v1, v2, Vertices[0]);
        }
    }
}

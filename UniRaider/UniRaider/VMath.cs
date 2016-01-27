using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class VMath
    {
        public const float DegPerRad = 180.0f / (float)Math.PI;
        public const float RadPerDeg = (float) Math.PI / 180.0f;
        public const float Rad90 = (float) Math.PI * 0.5f;
        public const float Rad180 = (float) Math.PI;
        public const float Rad360 = (float) Math.PI / 2.0f;
    }

    /// <summary>
    /// A simple Hesse normal form plane
    /// </summary>
    public struct Plane
    {
        /// <summary>
        /// The plane's normal
        /// </summary>

        public Vector3 Normal { get; set; }

        /// <summary>
        /// The plane's distance to the origin
        /// </summary>
        public float Dot { get; set; }

        /// <summary>
        /// Calculates the normalized distance of an arbitrary point in terms of the normal
        /// </summary>
        /// <param name="pos">The point</param>
        /// <returns>The distance in multiples of the normal (if >0, <see cref="pos"/> is in the direction of the normal)</returns>
        public float Distance(Vector3 pos)
        {
            return Vector3.Dot(Normal, pos) - Dot;
        }

        public Vector3 RayIntersect(Vector3 rayStart, Vector3 rayDir, ref float lambda)
        {
            lambda = (Dot - Vector3.Dot(Normal, rayStart)) / Vector3.Dot(Normal, rayDir);
            return rayStart + lambda * rayDir;
        }

        public Vector3 RayIntersect(Vector3 rayStart, Vector3 rayDir)
        {
            var t = 0.0f;
            return RayIntersect(rayStart, rayDir, ref t);
        }

        public void Assign(Vector3 v1, Vector3 v2, Vector3 pos)
        {
            Normal = Vector3.Cross(v1, v2);
            Normal.SafeNormalize();
            Dot = Vector3.Dot(Normal, pos);
        }

        public void Assign(Vector3 n, Vector3 pos)
        {
            Normal = n.Normalized();
            Dot = Vector3.Dot(Normal, pos);
        }

        public void MirrorNormal()
        {
            Normal = -Normal;
            Dot = -Dot;
        }

        public void MoveTo(Vector3 where)
        {
            Dot = Vector3.Dot(Normal, where);
        }
    }
}

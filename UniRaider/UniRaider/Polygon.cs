using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using UniRaider.Loader;

namespace UniRaider
{
    public enum PolygonSplit : byte
    {
        Front = 0,
        Back = 1,
        InPlane = 2,
        InBoth = 3
    }

    public partial class Constants
    {
        public const float SPLIT_EPSILON = 0.02f;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        /// <summary>
        /// [Length 4]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] Color;

        /// <summary>
        /// [Length 2]
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public float[] TexCoord;

        public VertexStruct ToStruct()
        {
            return new VertexStruct
            {
                Position = Position,
                Normal = Normal,
                Color = Color,
                TexCoord = TexCoord
            };
        }
    }

    /// <summary>
    /// TODO: UGLY!!!!!
    /// </summary>
    public struct VertexStruct
    {
        public Vector3 Position;

        public Vector3 Normal;

        /// <summary>
        /// [Length 4]
        /// </summary>
        public float[] Color;

        /// <summary>
        /// [Length 2]
        /// </summary>
        public float[] TexCoord;
    }

    public class Polygon
    {
        /// <summary>
        /// Vertices data [Length 4]
        /// </summary>
        public List<Vertex> Vertices { get; set; } 

        /// <summary>
        /// Texture index
        /// </summary>
        public ushort TexIndex { get; set; }

        /// <summary>
        /// Anim texture ID
        /// </summary>
        public ushort AnimID { get; set; }

        /// <summary>
        /// Anim texture frame offset
        /// </summary>
        public ushort FrameOffset { get; set; }

        /// <summary>
        /// Transparency information
        /// </summary>
        public Loader.BlendingMode BlendMode { get; set; }

        /// <summary>
        /// Double side flag
        /// </summary>
        public bool DoubleSide { get; set; }

        /// <summary>
        /// Polygon plane equation
        /// </summary>
        public Plane Plane { get; set; }

        public Polygon()
        {
            Vertices = new List<Vertex>();
            Plane = new Plane();
        }

        public Polygon(Polygon rhs)
        {
            Vertices = rhs.Vertices;
            TexIndex = rhs.TexIndex;
            AnimID = rhs.AnimID;
            FrameOffset = rhs.FrameOffset;
            BlendMode = rhs.BlendMode;
            DoubleSide = rhs.DoubleSide;
            Plane = rhs.Plane;
        }

        public bool IsBroken
        {
            get
            {
                if (Vertices.Count < 3)
                    return true;

                var dif0 = Plane.Normal.LengthSquared;
                if (dif0 < 0.999f || dif0 > 1.001f)
                    return true;

                var curr_v = Vertices.Last();
                foreach (var v in Vertices)
                {
                    var dif = v.Position - curr_v.Position;
                    if(dif.LengthSquared < 0.0001f)
                    {
                        return true;
                    }

                    curr_v = v;
                }

                return false;
            }
        }

        public void MoveSelf(Vector3 move)
        {
            foreach (var v in Vertices)
            {
                v.Position += move;
            }
            Plane.MoveTo(Vertices[0].Position);
        }

        public void Move(Polygon src, Vector3 move)
        {
            for (var i = 0; i < src.Vertices.Count; i++)
            {
                Vertices[i].Position = src.Vertices[i].Position + move;
            }
            Plane = src.Plane;
            Plane.MoveTo(Vertices[0].Position);
        }

        public void VTransform(Polygon src, Transform tr)
        {
            Plane.Normal = tr.Basis.MultiplyByVector(src.Plane.Normal);
            for (var i = 0; i < src.Vertices.Count; i++)
            {
                Vertices[i].Position = tr * src.Vertices[i].Position;
            }
            Plane.MoveTo(Vertices[0].Position);
        }

        public void Transform(Polygon src, Transform tr)
        {
            Vertices.Resize(src.Vertices.Count);

            Plane.Normal = tr.Basis.MultiplyByVector(src.Plane.Normal);
            for (var i = 0; i < src.Vertices.Count; i++)
            {
                Vertices[i].Position = tr * src.Vertices[i].Position;
                Vertices[i].Normal = tr.Basis.MultiplyByVector(src.Vertices[i].Normal);
            }
            Plane.MoveTo(Vertices[0].Position);
        }

        public void TransformSelf(Transform tr)
        {
            Plane.Normal = tr.Basis.MultiplyByVector(Plane.Normal);
            foreach (var t in Vertices)
            {
                t.Position *= tr;
                t.Normal = tr.Basis.MultiplyByVector(t.Normal);
            }
            Plane.MoveTo(Vertices[0].Position);
        }

        public void FindNormal()
        {
            var v1 = Vertices[0].Position - Vertices[1].Position;
            var v2 = Vertices[2].Position - Vertices[1].Position;
            Plane.Assign(v1, v2, Vector3.Zero);
        }

        public bool RayIntersect(Vector3 rayDir, Vector3 dot, ref float lambda)
        {
            var u = Plane.Normal.Dot(rayDir);
            if(Math.Abs(u) < 0.001)
            {
                return false; // plane is parallel to the ray - no intersection
            }
            lambda = -Plane.Distance(dot) / u;

            var vp = 0; // current polygon index
            var T = dot - Vertices[0].Position;

            var E2 = Vertices[1].Position - Vertices[0].Position;

            for (var i = 0; i < Vertices.Count - 2; i++, vp++)
            {
                var E1 = E2; // PREV
                E2 = Vertices[vp + 2].Position - Vertices[0].Position; // NEXT

                var P = rayDir.Cross(E2);
                var Q = T.Cross(E1);

                var tt = P.Dot(E1);
                u = P.Dot(T);
                u /= tt;
                var v = Q.Dot(rayDir);
                v /= tt;
                tt = 1.0f - u - v;
                if(u.IsBetween(0, 1) && v.IsBetween(0, 1) && tt.IsBetween(0, 1))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IntersectPolygon(Polygon p2)
        {
            if(SplitClassify(p2.Plane) != PolygonSplit.InBoth || p2.SplitClassify(Plane) != PolygonSplit.InBoth)
            {
                return false; // Quick check
            }

            var resultBuf = new List<Vector3>();

            // Intersection of polygon p1 and plane p2
            var prev_v = Vertices.Last();
            var dist0 = p2.Plane.Distance(prev_v.Position);
            foreach (var curr_v in Vertices)
            {
                var dist1 = p2.Plane.Distance(curr_v.Position);
                if ((dist1 > Constants.SPLIT_EPSILON && dist0 < -Constants.SPLIT_EPSILON)
                    || (dist1 < -Constants.SPLIT_EPSILON && dist0 > Constants.SPLIT_EPSILON))
                {
                    resultBuf.Add(p2.Plane.RayIntersect(prev_v.Position, curr_v.Position - prev_v.Position));
                }
                else
                {
                    resultBuf.Add(curr_v.Position);
                }

                if (resultBuf.Count >= 2) break;

                dist0 = dist1;
                prev_v = curr_v;
            }

            // Splitting p2 by p1 split plane
            prev_v = Vertices.Last();
            dist0 = p2.Plane.Distance(prev_v.Position);
            foreach (var curr_v in Vertices)
            {
                var dist1 = p2.Plane.Distance(curr_v.Position);
                if ((dist1 > Constants.SPLIT_EPSILON && dist0 < -Constants.SPLIT_EPSILON)
                    || (dist1 < -Constants.SPLIT_EPSILON && dist0 > Constants.SPLIT_EPSILON))
                {
                    resultBuf.Add(p2.Plane.RayIntersect(prev_v.Position, curr_v.Position - prev_v.Position));
                }
                else
                {
                    resultBuf.Add(curr_v.Position);
                }

                if (resultBuf.Count >= 4) break;

                dist0 = dist1;
                prev_v = curr_v;
            }

            var dir = Plane.Normal.Cross(p2.Plane.Normal); // vector of two planes intersection line
            var t = Math.Abs(dir.X);
            dist0 = Math.Abs(dir.Y);
            var dist1_ = Math.Abs(dir.Z);
            var dist2 = 0.0f;
            var pn = Constants.PLANE_X;
            if(t < dist0)
            {
                t = dist0;
                pn = Constants.PLANE_Y;
            }
            if(t < dist1_)
            {
                pn = Constants.PLANE_Z;
            }

            switch(pn)
            {
                case Constants.PLANE_X:
                    dist0 = (resultBuf[1][0] - resultBuf[0][0]) / dir[0];
                    dist1_ = (resultBuf[2][0] - resultBuf[0][0]) / dir[0];
                    dist2 = (resultBuf[3][0] - resultBuf[0][0]) / dir[0];
                    break;

                case Constants.PLANE_Y:
                    dist0 = (resultBuf[1][1] - resultBuf[0][1]) / dir[1];
                    dist1_ = (resultBuf[2][1] - resultBuf[0][1]) / dir[1];
                    dist2 = (resultBuf[3][1] - resultBuf[0][1]) / dir[1];
                    break;

                case Constants.PLANE_Z:
                    dist0 = (resultBuf[1][2] - resultBuf[0][2]) / dir[2];
                    dist1_ = (resultBuf[2][2] - resultBuf[0][2]) / dir[2];
                    dist2 = (resultBuf[3][2] - resultBuf[0][2]) / dir[2];
                    break;
            }

            if(dist0 > 0)
            {
                return !((dist1_ < 0 && dist2 < 0) || (dist1_ > dist0 && dist2 > dist0));
            }

            return !((dist1_ < dist0 && dist2 < dist0) || (dist1_ > 0 && dist2 > 0));
        }

        public PolygonSplit SplitClassify(Plane plane)
        {
            var pos = Vertices.Select(v => plane.Distance(v.Position));
            var positive = pos.Count(dist => dist > Constants.SPLIT_EPSILON);
            var negative = pos.Count(dist => dist < -Constants.SPLIT_EPSILON);

            if(positive > 0 && negative == 0) return PolygonSplit.Front;
            if(positive == 0 && negative > 0) return PolygonSplit.Back;
            if(positive < 1 && negative < 1) return PolygonSplit.InPlane;
            return PolygonSplit.InBoth;
        }

        public void Split(Plane n, ref Polygon front, ref Polygon back)
        {
            front.Plane = Plane;
            front.AnimID = AnimID;
            front.FrameOffset = FrameOffset;
            front.DoubleSide = DoubleSide;
            front.TexIndex = TexIndex;
            front.BlendMode = BlendMode;

            back.Plane = Plane;
            back.AnimID = AnimID;
            back.FrameOffset = FrameOffset;
            back.DoubleSide = DoubleSide;
            back.TexIndex = TexIndex;
            back.BlendMode = BlendMode;

            var prev_v = Vertices.Last();
            var dist0 = n.Distance(prev_v.Position);
            foreach (var curr_v in Vertices)
            {
                var dist1 = n.Distance(curr_v.Position);

                if(Math.Abs(dist1) > Constants.SPLIT_EPSILON)
                {
                    if ((dist1 > Constants.SPLIT_EPSILON && dist0 < -Constants.SPLIT_EPSILON)
                        || (dist1 < -Constants.SPLIT_EPSILON && dist0 > Constants.SPLIT_EPSILON))
                    {
                        var dir = curr_v.Position - prev_v.Position;
                        float t;
                        var tv = new Vertex
                        {
                            Position = n.RayIntersect(prev_v.Position, dir, out t),
                            Normal = prev_v.Normal.Lerp(curr_v.Normal, t).Normalized(),
                            Color = new[]
                            {
                                prev_v.Color[0] + t * (curr_v.Color[0] - prev_v.Color[0]),
                                prev_v.Color[1] + t * (curr_v.Color[1] - prev_v.Color[1]),
                                prev_v.Color[2] + t * (curr_v.Color[2] - prev_v.Color[2]),
                                prev_v.Color[3] + t * (curr_v.Color[3] - prev_v.Color[3])
                            },
                            TexCoord = new[]
                            {
                                prev_v.TexCoord[0] + t * (curr_v.TexCoord[0] - prev_v.TexCoord[0]),
                                prev_v.TexCoord[1] + t * (curr_v.TexCoord[1] - prev_v.TexCoord[1])
                            }
                        };

                        front.Vertices.Add(tv);
                        back.Vertices.Add(tv);
                    }

                    if (dist1 > Constants.SPLIT_EPSILON)
                        front.Vertices.Add(curr_v);
                    else
                        back.Vertices.Add(curr_v);
                }
                else
                {
                    front.Vertices.Add(curr_v);
                    back.Vertices.Add(curr_v);
                }

                prev_v = curr_v;
                dist0 = dist1;
            }
        }

        public bool IsInsideBBox(Vector3 bbMin, Vector3 bbMax)
        {
            return Vertices.All(v => v.Position.IsBetween(bbMin, bbMax));
        }

        public bool IsInsideBQuad(Vector3 bbMin, Vector3 bbMax)
        {
            return Vertices.All(v => v.Position.X.IsBetween(bbMin.X, bbMax.X)
                                     && v.Position.Y.IsBetween(bbMin.Y, bbMax.Y));
        }
    }
}

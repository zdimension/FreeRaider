using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreeRaider
{
    public class Frustum
    {
        /// <summary>
        /// Clip planes
        /// </summary>
        public List<Plane> Planes { get; set; }

        /// <summary>
        /// Frustum vertices
        /// </summary>
        public List<Vector3> Vertices { get; set; }

        /// <summary>
        /// Main frustum clip plane (inv. plane of parent portal)
        /// </summary>
        public Plane Normal { get; set; }

        public int ParentsCount { get; set; } = 0;


        public Frustum Parent { get; set; } = null;

        public bool HasParent(Frustum parent)
        {
            var frustum = this;
            while(frustum != null)
            {
                if (parent == frustum)
                    return true;
                frustum = frustum.Parent;
            }
            return false;
        }

        /// <summary>
        /// Check polygon visibility through the portal.
        /// </summary>
        public bool IsPolyVisible(Polygon p, Camera cam)
        {
            if (!p.DoubleSide && p.Plane.Distance(cam.Position) < 0)
                return false;

            // Direction from the camera position to an arbitrary vertex frustum
            Assert.That(Vertices.Any());
            var dir = Vertices[0] - cam.Position;
            var lambda = 0.0f;

            // Polygon fits whole frustum (shouldn't happen, but we check anyway)
            if(p.RayIntersect(dir, cam.Position, ref lambda))
            {
                return true;
            }

            // Generate queue order
            var nextPlaneIdx = 0;
            // 3 neighboring clipping planes
            var currentPlane = Planes.Last();
            var prevPlane = Planes[Planes.Count - 2];
            // in case no intersection
            var ins = true;
            // iterate through all the planes of this frustum
            for (var i = 0; i < Vertices.Count; i++)
            {
                var nextPlane = Planes[nextPlaneIdx];

                // Queue vertices for testing
                var prevVertex = p.Vertices.Last();
                // signed distance from the current point to the previous plane
                var dist0 = currentPlane.Distance(prevVertex.Position);
                var outs = true;
                // iterate through all the vertices of the polygon
                foreach (var currentVertex in p.Vertices)
                {
                    var dist1 = currentPlane.Distance(currentVertex.Position);
                    // the split point in the plane
                    if(Math.Abs(dist0) < Constants.SPLIT_EPSILON)
                    {
                        if(prevPlane.Distance(prevVertex.Position) > -Constants.SPLIT_EPSILON
                            && nextPlane.Distance(prevVertex.Position) > -Constants.SPLIT_EPSILON
                            && Normal.Distance(prevVertex.Position) > -Constants.SPLIT_EPSILON)
                        {
                            // Frustum-vertex intersection test is passed
                            return true;
                        }
                    }

                    // vertices from different sides of the plane (or on it)
                    if(dist0 * dist1 < 0 && Math.Abs(dist1) >= Constants.SPLIT_EPSILON)
                    {
                        // vector connecting vertices
                        dir = currentVertex.Position - prevVertex.Position;
                        // We are looking for the point of intersection
                        var T = currentPlane.RayIntersect(prevVertex.Position, dir);
                        if(prevPlane.Distance(T) > -Constants.SPLIT_EPSILON && nextPlane.Distance(T) > -Constants.SPLIT_EPSILON)
                        {
                            // Frustum-ray intersection test is passed
                            return true;
                        }
                    }

                    // point is outside
                    if(dist1 < -Constants.SPLIT_EPSILON)
                    {
                        ins = false;
                    }
                    else
                    {
                        outs = false;
                    }

                    // We moved all the vertices of the polygon
                    prevVertex = currentVertex;
                    // We moved all distances
                    dist0 = dist1;
                    // finished with all polygon vertices
                }

                if(outs)
                {
                    // all points are outside of the current plane - definitely exit
                    return false;
                }
                // We moved all the clipping planes
                prevPlane = currentPlane;
                currentPlane = nextPlane;
                nextPlaneIdx++;
                // finished with all planes of this frustum
            }
            if(ins)
            {
                // all the vertices are inside - test is passed
                return true;
            }

            return false;
        }

        public bool IsAABBVisible(ref Vector3 bbMin, ref Vector3 bbMax, ref Camera cam)
        {
            var poly = new Polygon();
            poly.Vertices = new List<Vertex>(4);
            var ins = true;

            // X AXIS

            if(cam.Position.X < bbMin.X)
            {
                poly.Plane.Normal.X = -1.0f;
                poly.Plane.Dot = -bbMin.X;

                poly.Vertices[0].Position[0] = bbMin[0];
                poly.Vertices[0].Position[1] = bbMax[1];
                poly.Vertices[0].Position[2] = bbMax[2];

                poly.Vertices[1].Position[0] = bbMin[0];
                poly.Vertices[1].Position[1] = bbMin[1];
                poly.Vertices[1].Position[2] = bbMax[2];

                poly.Vertices[2].Position[0] = bbMin[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMin[2];

                poly.Vertices[3].Position[0] = bbMin[0];
                poly.Vertices[3].Position[1] = bbMax[1];
                poly.Vertices[3].Position[2] = bbMin[2];

                if(IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }
            else if(cam.Position.X > bbMax.X)
            {
                poly.Plane.Normal.X = 1.0f;
                poly.Plane.Dot = bbMax.X;

                poly.Vertices[0].Position[0] = bbMax[0];
                poly.Vertices[0].Position[1] = bbMax[1];
                poly.Vertices[0].Position[2] = bbMax[2];

                poly.Vertices[1].Position[0] = bbMax[0];
                poly.Vertices[1].Position[1] = bbMin[1];
                poly.Vertices[1].Position[2] = bbMax[2];

                poly.Vertices[2].Position[0] = bbMax[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMin[2];

                poly.Vertices[3].Position[0] = bbMax[0];
                poly.Vertices[3].Position[1] = bbMax[1];
                poly.Vertices[3].Position[2] = bbMin[2];

                if (IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }

            // Y AXIS

            poly.Plane.Normal.X = 0;
            poly.Plane.Normal.Z = 0;

            if (cam.Position.Y < bbMin.Y)
            {
                poly.Plane.Normal.Y = -1.0f;
                poly.Plane.Dot = -bbMin.Y;

                poly.Vertices[0].Position[0] = bbMax[0];
                poly.Vertices[0].Position[1] = bbMin[1];
                poly.Vertices[0].Position[2] = bbMax[2];

                poly.Vertices[1].Position[0] = bbMin[0];
                poly.Vertices[1].Position[1] = bbMin[1];
                poly.Vertices[1].Position[2] = bbMax[2];

                poly.Vertices[2].Position[0] = bbMin[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMin[2];

                poly.Vertices[3].Position[0] = bbMax[0];
                poly.Vertices[3].Position[1] = bbMin[1];
                poly.Vertices[3].Position[2] = bbMin[2];

                if (IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }
            else if (cam.Position.Y > bbMax.Y)
            {
                poly.Plane.Normal.Y = 1.0f;
                poly.Plane.Dot = -bbMax.Y;

                poly.Vertices[0].Position[0] = bbMax[0];
                poly.Vertices[0].Position[1] = bbMax[1];
                poly.Vertices[0].Position[2] = bbMax[2];

                poly.Vertices[1].Position[0] = bbMax[0];
                poly.Vertices[1].Position[1] = bbMin[1];
                poly.Vertices[1].Position[2] = bbMax[2];

                poly.Vertices[2].Position[0] = bbMax[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMin[2];

                poly.Vertices[3].Position[0] = bbMax[0];
                poly.Vertices[3].Position[1] = bbMax[1];
                poly.Vertices[3].Position[2] = bbMin[2];

                if (IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }

            // Z AXIS

            poly.Plane.Normal.X = 0;
            poly.Plane.Normal.Y = 0;

            if (cam.Position.Z < bbMin.Z)
            {
                poly.Plane.Normal.Z = -1.0f;
                poly.Plane.Dot = -bbMin.Z;

                poly.Vertices[0].Position[0] = bbMax[0];
                poly.Vertices[0].Position[1] = bbMax[1];
                poly.Vertices[0].Position[2] = bbMin[2];

                poly.Vertices[1].Position[0] = bbMin[0];
                poly.Vertices[1].Position[1] = bbMax[1];
                poly.Vertices[1].Position[2] = bbMin[2];

                poly.Vertices[2].Position[0] = bbMin[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMin[2];

                poly.Vertices[3].Position[0] = bbMax[0];
                poly.Vertices[3].Position[1] = bbMin[1];
                poly.Vertices[3].Position[2] = bbMin[2];

                if (IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }
            else if (cam.Position.Z > bbMax.Z)
            {
                poly.Plane.Normal.Z = 1.0f;
                poly.Plane.Dot = -bbMax.Z;

                poly.Vertices[0].Position[0] = bbMax[0];
                poly.Vertices[0].Position[1] = bbMax[1];
                poly.Vertices[0].Position[2] = bbMax[2];

                poly.Vertices[1].Position[0] = bbMin[0];
                poly.Vertices[1].Position[1] = bbMax[1];
                poly.Vertices[1].Position[2] = bbMax[2];

                poly.Vertices[2].Position[0] = bbMin[0];
                poly.Vertices[2].Position[1] = bbMin[1];
                poly.Vertices[2].Position[2] = bbMax[2];

                poly.Vertices[3].Position[0] = bbMax[0];
                poly.Vertices[3].Position[1] = bbMin[1];
                poly.Vertices[3].Position[2] = bbMax[2];

                if (IsPolyVisible(poly, cam))
                {
                    return true;
                }
                ins = false;
            }

            return ins;
        }

        public bool IsOBBVisible(OBB obb, Camera cam)
        {
            var ins = false;
            var p = obb.Polygons;
            for (int i = 0; i < 6; i++, p++)
            {
                var t = p.Plane.Distance(cam.Position);
                if(t > 0.0 && IsPolyVisible(p, cam))
                {
                    return true;
                }
                if(ins && t > 0)
                {
                    ins = false;
                }
            }

            return ins;
        }

        public static Frustum PortalFrustumIntersect(ref Portal portal, Frustum emitter, Render render)
        {
            if (portal.DestRoom == null)
                return null;

            if (portal.Normal.Distance(render.Camera.Position) < Constants.SPLIT_EPSILON)
                // non face or degenerate to the line portal
                return null;

            if (portal.DestRoom.Frustum.Count > 0 && emitter.HasParent(portal.DestRoom.Frustum[0]))
                return null; // Abort infinite loop!

            bool inDist = false;
            bool inFace = false;
            foreach (var v in portal.Vertices)
            {
                if (!inDist && render.Camera.Frustum.Normal.Distance(v) < render.Camera.DistanceFar)
                    inDist = true;
                if (!inFace && emitter.Normal.Distance(v) > 0.0)
                    inFace = true;
                if (inDist && inFace)
                    break;
            }

            if (!inDist || !inDist)
                return null;

            // Search for the first free room's frustum
            portal.DestRoom.Frustum.Add(new Frustum());
            var currentGen = portal.DestRoom.Frustum.Last();

            currentGen.SplitPrepare(portal); // prepare for clipping

            if (currentGen.SplitByPlane(emitter.Normal)) // splitting by main frustum clip plane
            {
                for (var i = 0; i < emitter.Vertices.Count; i++)
                {
                    if(!currentGen.SplitByPlane(emitter.Planes[i]))
                    {
                        portal.DestRoom.Frustum.RemoveAt(portal.DestRoom.Frustum.Count - 1);
                        return null;
                    }
                }

                currentGen.GenClipPlanes(render.Camera); // all is OK, let's generate clip planes

                currentGen.Parent = emitter; // add parent pointer
                currentGen.ParentsCount = emitter.ParentsCount + 1;

                portal.DestRoom.MaxPath = Math.Max(portal.DestRoom.MaxPath, currentGen.ParentsCount); // maximum path to the room
                return currentGen;
            }

            portal.DestRoom.Frustum.RemoveAt(portal.DestRoom.Frustum.Count - 1);

            return null;
        }

        public void SplitPrepare(Portal p)
        {
            Vertices = p.Vertices;
            Normal = p.Normal;
            Normal.MirrorNormal();
            Parent = null;
        }

        public void GenClipPlanes(ref Camera cam)
        {
            if (Vertices.Count == 0)
                return;

            Planes.Resize(Vertices.Count);

            var curr_v = Vertices.Last();
            var prev_v = Vertices[Vertices.Count - 2];

            for (var i = 0; i < Vertices.Count; i++)
            {
                var V1 = prev_v - cam.Position; // POV-vertx vector
                var V2 = prev_v - curr_v; // vector connecting neighbor vertices
                V1.Normalize();
                V2.Normalize();
                Planes[i].Assign(V1, V2, curr_v);

                prev_v = curr_v;
                curr_v = Vertices[i];
            }
        }

        public bool SplitByPlane(Plane splitPlane)
        {
            Assert.That(Vertices.Any());

            var buf = new List<Vector3>();

            var currentVertex = Vertices.Last();

            var dist = new float[2];
            dist[0] = splitPlane.Distance(currentVertex);

            foreach (var nextVertex in Vertices)
            {
                dist[1] = splitPlane.Distance(nextVertex);

                if(dist[1] > Constants.SPLIT_EPSILON)
                {
                    if(dist[0] < -Constants.SPLIT_EPSILON)
                    {
                        buf.Add(splitPlane.RayIntersect(currentVertex, nextVertex - currentVertex)); // Shifting
                    }
                    buf.Add(nextVertex); // Adding
                }
                else if(dist[1] < -Constants.SPLIT_EPSILON)
                {
                    if(dist[0] > Constants.SPLIT_EPSILON)
                    {
                        buf.Add(splitPlane.RayIntersect(currentVertex, nextVertex - currentVertex));
                    }
                }
                else
                {
                    buf.Add(nextVertex); // Adding
                }

                currentVertex = nextVertex;
                dist[0] = dist[1];
            }

            Vertices.Clear();

            if (buf.Count <= 2) // Nothing was added or degenerative
            {
                return false;
            }

            currentVertex = buf.Last();
            foreach (var nextVertex in buf)
            {
                if(currentVertex.Distance2(nextVertex) > Constants.SPLIT_EPSILON.Square())
                {
                    Vertices.Add(nextVertex);
                }
                currentVertex = nextVertex;
            }

            if(Vertices.Count <= 2)
            {
                Vertices.Clear();
                return false;
            }

            return true;
        }
    }
}

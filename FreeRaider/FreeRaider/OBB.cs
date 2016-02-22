using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreeRaider
{
    public partial class Constants
    {
        public const float DEFAULT_OBB_TEST_OVERLAP = 1.2f;
    }

    /// <summary>
    /// Oriented bounding box
    /// </summary>
    public class OBB
    {
        /// <summary>
        /// Length = 6
        /// </summary>
        public Polygon[] BasePolygons; // bv base surface

        /// <summary>
        /// Length = 6
        /// </summary>
        public Polygon[] Polygons; // bv world coordinate surface

        public Transform Transform; // Object transform matrix

        public float Radius;

        public Vector3 BaseCentre;

        public Vector3 Centre;

        public Vector3 Extent;

        public OBB()
        {
            BasePolygons = new Polygon[6];
            Polygons = new Polygon[6];
            Transform = new Transform();
        }

        public void DoTransform()
        {
            if(Transform != null)
            {
                for (var i = 0; i < 6; i++)
                {
                    Polygons[i].VTransform(BasePolygons[i], Transform);
                }
                Centre = Transform * BaseCentre;
            }
            else
            {
                for (var i = 0; i < 6; i++)
                {
                    Polygons[i] = BasePolygons[i];
                }
                Centre = BaseCentre;
            }
        }

        public void Rebuild(ref Vector3 bbMin, ref Vector3 bbMax)
        {
            Extent = (bbMax - bbMin) / 2;
            BaseCentre = (bbMin + bbMax) / 2;
            Radius = Extent.Length;

            // UP
            var p_up = BasePolygons[0];


            // 0 1
            // 0 0
            BasePolygons[0].Vertices[0].Position = bbMax;

            // 1 0
            // 0 0
            BasePolygons[0].Vertices[1].Position = new Vector3(bbMin.X, bbMax.Y, bbMax.Z);

            // 0 0
            // 1 0
            BasePolygons[0].Vertices[2].Position = new Vector3(bbMin.X, bbMin.Y, bbMax.Z);

            // 0 0
            // 0 1
            BasePolygons[0].Vertices[3].Position = new Vector3(bbMax.X, bbMin.Y, bbMax.Z);

            BasePolygons[0].FindNormal();
            
            // DOWN
            var p_down = BasePolygons[1];

            // 0 1
            // 0 0
            BasePolygons[1].Vertices[0].Position = new Vector3(bbMax.X, bbMax.Y, bbMin.Z);

            // 1 0
            // 0 0
            BasePolygons[1].Vertices[1].Position = new Vector3(bbMax.X, bbMin.Y, bbMin.Z);

            // 0 0
            // 1 0
            BasePolygons[1].Vertices[2].Position = bbMin;

            // 0 0
            // 0 1
            BasePolygons[1].Vertices[3].Position = new Vector3(bbMin.X, bbMax.Y, bbMin.Z);

            BasePolygons[1].FindNormal();

            // RIGHT: OX+
            BasePolygons[2].Vertices[0].Position = p_up.Vertices[0].Position;
            BasePolygons[2].Vertices[1].Position = p_up.Vertices[3].Position;
            BasePolygons[2].Vertices[2].Position = p_down.Vertices[1].Position;
            BasePolygons[2].Vertices[3].Position = p_down.Vertices[0].Position;

            BasePolygons[2].FindNormal();

            // LEFT: OX-
            BasePolygons[3].Vertices[0].Position = p_up.Vertices[1].Position;
            BasePolygons[3].Vertices[3].Position = p_up.Vertices[2].Position;
            BasePolygons[3].Vertices[2].Position = p_down.Vertices[2].Position;
            BasePolygons[3].Vertices[1].Position = p_down.Vertices[3].Position;

            BasePolygons[3].FindNormal();

            // FORWARD: OY+
            BasePolygons[4].Vertices[0].Position = p_up.Vertices[0].Position;
            BasePolygons[4].Vertices[3].Position = p_up.Vertices[1].Position;
            BasePolygons[4].Vertices[2].Position = p_down.Vertices[3].Position;
            BasePolygons[4].Vertices[1].Position = p_down.Vertices[0].Position;

            BasePolygons[4].FindNormal();

            // BACKWARD: OY-
            BasePolygons[5].Vertices[0].Position = p_up.Vertices[3].Position;
            BasePolygons[5].Vertices[1].Position = p_up.Vertices[2].Position;
            BasePolygons[5].Vertices[2].Position = p_down.Vertices[2].Position;
            BasePolygons[5].Vertices[3].Position = p_down.Vertices[1].Position;

            BasePolygons[5].FindNormal();
        }

        public bool IsVisibleInRoom(Room room, Camera cam)
        {
            var polys = Polygons;

            if (room.Frustum.Count == 0)
            {
                var ins = true;
                foreach (var polygon in polys)
                {
                    var t = polygon.Plane.Distance(Global.EngineCamera.Position);
                    if (t > 0 && Global.EngineCamera.Frustum.IsPolyVisible(polygon, cam))
                    {
                        return true;
                    }
                    if (ins && t > 0)
                    {
                        ins = false;
                    }
                }
                return ins;
            }

            return room.Frustum.Any(frustum => (from polygon in polys
                                                let t = polygon.Plane.Distance(cam.Position)
                                                where t > 0 && frustum.IsPolyVisible(polygon, cam)
                                                select polygon).Any());
        }

        public static int OBB_Test(Entity e1, Entity e2, float overlap = Constants.DEFAULT_OBB_TEST_OVERLAP)
        {
            // translation, in parent frame

        }
    }
}

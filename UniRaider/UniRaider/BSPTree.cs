using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class BSPFaceRef
    {
        public Transform Transform { get; set; }

        public TransparentPolygonReference Polygon { get; set; }

        public BSPFaceRef(Transform matrix, TransparentPolygonReference polygon)
        {
            Transform = matrix;
            Polygon = polygon;
        }
    }

    public class BSPNode
    {
        public Plane Plane { get; set; }

        public List<BSPFaceRef> PolygonsFront { get; set; }
        
        public List<BSPFaceRef> PolygonsBack { get; set; }

        public BSPNode Front;

        public BSPNode Back;
    }

    public class DynamicBSP
    {
        private void addPolygon(ref BSPNode root, BSPFaceRef face, Polygon transformed)
        {
            if(root == null) root = new BSPNode();

            if(root.PolygonsFront.Count == 0)
            {
                // We though root.Front == null && root.Back == null
                root.Plane = transformed.Plane;
                root.PolygonsFront = new List<BSPFaceRef> { face };
                return;
            }

            var positive = 0;
            var negative = 0;
            var inPlane = 0;

            foreach(Vertex v in transformed.Vertices)
            {
                var dist = root.Plane.Distance(v.Position);
                if (dist > Constants.SPLIT_EPSILON)
                    positive++;
                else if (dist < -Constants.SPLIT_EPSILON)
                    negative++;
                else
                    inPlane++;
            }

            if(positive > 0 && negative == 0) // SPLIT_FRONT
            {
                addPolygon(ref root.Front, face, transformed);
            }
            else if(positive == 0 && negative > 0) // SPLIT_BACK
            {
                addPolygon(ref root.Back, face, transformed);
            }
            else // SPLIT_IN_PLANE
            {
                if(transformed.Plane.Normal.Dot(root.Plane.Normal) > 0.9)
                {
                    root.PolygonsFront.Add(face);
                }
                else
                {
                    root.PolygonsBack.Add(face);
                }
            }
        }

        public void AddNewPolygonList(List<TransparentPolygonReference> p, ref Transform transform, Frustum frustum, Camera cam)
        {
            foreach (var pp in p)
            {
                var transformed = new Polygon();
                transformed.Vertices.Resize(pp.Polygon.Vertices.Count);
                transformed.Transform(pp.Polygon, transform);
                transformed.DoubleSide = pp.Polygon.DoubleSide;

                //if(frustum)
            }
        }

        public BSPNode Root { get; private set; } = new BSPNode();

        public void Reset()
        {
            Root = new BSPNode();
        }
    }
}

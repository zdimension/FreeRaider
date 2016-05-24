using System.Collections.Generic;
using static FreeRaider.Constants;

namespace FreeRaider
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

        public List<BSPFaceRef> PolygonsFront { get; set; } = new List<BSPFaceRef>();

        public List<BSPFaceRef> PolygonsBack { get; set; } = new List<BSPFaceRef>();

        public BSPNode Front;

        public BSPNode Back;
    }

    public class DynamicBSP
    {
        private BSPNode _root = new BSPNode();

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

            foreach(var v in transformed.Vertices)
            {
                var dist = root.Plane.Distance(v.Position);
                if (dist > SPLIT_EPSILON)
                    positive++;
                else if (dist < -SPLIT_EPSILON)
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

        public void AddNewPolygonList(List<TransparentPolygonReference> p, Transform transform, Frustum frustum, Camera cam)
        {
            foreach (var pp in p)
            {
                var transformed = new Polygon();
                transformed.Vertices.Resize(pp.Polygon.Vertices.Count, () => new Vertex());
                transformed.Transform(pp.Polygon, transform);
                transformed.DoubleSide = pp.Polygon.DoubleSide;

                if(frustum.IsPolyVisible(transformed, cam))
                {
                    addPolygon(ref _root, new BSPFaceRef(transform, pp), transformed);
                }
            }
        }

        public BSPNode Root
        {
            get { return _root; }
            private set { _root = value; }
        }

        public void Reset()
        {
            Root = new BSPNode();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class Frustum
    {
        /// <summary>
        /// Clip planes
        /// </summary>
        public Plane[] Planes { get; set; }

        /// <summary>
        /// Frustum vertices
        /// </summary>
        public Vector3[] Vertices { get; set; }

        /// <summary>
        /// Main frustum clip plane (inv. plane of parent portal)
        /// </summary>
        public Plane Normal { get; set; }

        public ushort ParentsCount { get; set; } = 0;


        public Frustum Parent { get; set; } = null;

        public void SplitPrepare(Portal p)
        {
            Vertices = p.Vertices;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class Camera
    {
        /// <summary>
        /// Camera position
        /// </summary>
        public Vector3 Position { get; set; } = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// Camera view direction
        /// </summary>
        public Vector3 ViewDirection => new Vector3(0.0f, 0.0f, 1.0f);

        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 UpDirection => new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// Strafe vector
        /// </summary>
        public Vector3 RightDirection => new Vector3(1.0f, 0.0f, 0.0f);

        private Vector3 previousPosition = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 angle;

        private Matrix4 glViewMat = new Matrix4();
        private Matrix4 glProjMat = new Matrix4();
        private Matrix4 glViewProjMat = new Matrix4();

        private Plane[] clipPlanes = new Plane[4];
        private frust
    }
}

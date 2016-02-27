using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace FreeRaider
{
    public partial class Constants
    {
        public const float RD_DEFAULT_SLEEPING_THRESHOLD = 10.0f;
    }

    // Joint setup struct is used to parse joint script entry to
    // actual joint.

    public class RDJointSetup
    {
        public enum Type
        {
            Point = 0,
            Hinde = 1,
            Cone = 2
        }

        /// <summary>
        /// Primary body index
        /// </summary>
        public ushort BodyIndex;

        public Type JointType;

        /// <summary>
        /// Primary pivot point offset
        /// </summary>
        public Vector3 Body1Offset;

        /// <summary>
        /// Secondary pivot point offset
        /// </summary>
        public Vector3 Body2Offset;

        /// <summary>
        /// Primary pivot point angle
        /// </summary>
        public Vector3 Body1Angle;

        /// <summary>
        /// Secondary pivot point angle
        /// </summary>
        public Vector3 Body2Angle;

        /// <summary>
        /// Only first two are used for hinge constraints
        /// </summary>
        public float[] JointLimit = new float[3];
    }

    // Ragdoll body setup is used to modify body properties for ragdoll needs.

    public class RDBodySetup
    {
        public float Mass;

        public float[] Damping = new float[2];

        public float Restitution;

        public float Friction;
    }

    // Ragdoll setup struct is an unified structure which contains settings
    // for ALL joints and bodies of a given ragdoll.

    public class RDSetup
    {
        /// <summary>
        /// Constraint force mixing (joint softness)
        /// </summary>
        public float JointCfm = 0.0f;

        /// <summary>
        /// Error reduction parameter (joint "inertia")
        /// </summary>
        public float JointErp;

        public List<RDJointSetup> JointSetup =new List<RDJointSetup>();
        
        public List<RDBodySetup> BodySetup = new List<RDBodySetup>();

        /// <summary>
        /// Later to be implemented as hit callback function
        /// </summary>
        public string HitFunc;

        public bool GetSetup(int ragdollIndex);

        public void ClearSetup()
        {
            BodySetup.Clear();
            JointSetup.Clear();
            HitFunc = "";
        }
    }
}

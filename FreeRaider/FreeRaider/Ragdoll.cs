using System.Collections.Generic;
using NLua;
using OpenTK;
using static FreeRaider.Global;

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
            Hinge = 1,
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

        public List<RDJointSetup> JointSetup = new List<RDJointSetup>();

        public List<RDBodySetup> BodySetup = new List<RDBodySetup>();

        /// <summary>
        /// Later to be implemented as hit callback function
        /// </summary>
        public string HitFunc;

        public bool GetSetup(int ragdollIndex)
        {
            var rds2 = (EngineLua["getRagdollSetup"] as LuaFunction).Call(ragdollIndex)[0];
            if (!(rds2 is LuaTable))
                return false;
            dynamic rds = (LuaTable) rds2;

            HitFunc = rds["hit_callback"];

            JointSetup.Resize((int) rds["joint_count"], () => new RDJointSetup());

            BodySetup.Resize((int) rds["body_count"], () => new RDBodySetup());

            JointCfm = (float)rds["joint_cfm"];
            JointErp = (float)rds["joint_erp"];

            for (var i = 0; i < BodySetup.Count; i++)
            {
                var b = rds["body"][i + 1];
                BodySetup[i].Mass = (float)b["mass"];
                BodySetup[i].Restitution = (float)b["restitution"];
                BodySetup[i].Friction = (float)b["friction"];
                var damp = b["damping"];
                if (damp is LuaTable)
                {
                    BodySetup[i].Damping[0] = (float)damp[1];
                    BodySetup[i].Damping[1] = (float)damp[2];
                }
            }

            for (var i = 0; i < JointSetup.Count; i++)
            {
                var j = rds["joint"][i + 1];
                JointSetup[i].BodyIndex = (ushort)j["body_index"];
                JointSetup[i].JointType = (RDJointSetup.Type) j["joint_type"];
                if(j["body1_offset"] is LuaTable)
                {
                    for (var k = 0; k < 3; k++)
                        JointSetup[i].Body1Offset[k] = (float)j["body1_offset"][k + 1];
                }
                if (j["body2_offset"] is LuaTable)
                {
                    for (var k = 0; k < 3; k++)
                        JointSetup[i].Body2Offset[k] = (float)j["body2_offset"][k + 1];
                }
                if (j["body1_angle"] is LuaTable)
                {
                    for (var k = 0; k < 3; k++)
                        JointSetup[i].Body1Angle[k] = (float)j["body1_angle"][k + 1];
                }
                if (j["body2_angle"] is LuaTable)
                {
                    for (var k = 0; k < 3; k++)
                        JointSetup[i].Body2Angle[k] = (float)j["body2_angle"][k + 1];
                }
                if (j["joint_limit"] is LuaTable)
                {
                    for (var k = 0; k < 3; k++)
                        JointSetup[i].JointLimit[k] = (float)j["joint_limit"][k + 1];
                }
            }

            return true;
        }

        public void ClearSetup()
        {
            BodySetup.Clear();
            JointSetup.Clear();
            HitFunc = "";
        }
    }
}

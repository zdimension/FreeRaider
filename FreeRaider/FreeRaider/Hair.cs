using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using NLua;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static FreeRaider.StaticFuncs;

namespace FreeRaider
{
    public enum HairType
    {
        TR1 = 0,
        TR2 = 1,
        TR3 = 1,
        TR4_Kid1 = 3,
        TR4_Kid2 = 4,
        TR4_Old = 5,
        TR5_Kid1 = 6,
        TR5_Kid2 = 7,
        TR5_Old = 8
    }

    public partial class Constants
    {
        public const int HAIR_DISCARD_ROOT_FACE = 0;

        public const int HAIR_DISCARD_TAIL_FACE = 5;
    }

    public class HairElement
    {
        /// <summary>
        /// Pointer to rendered mesh.
        /// </summary>
        public BaseMesh Mesh;

        /// <summary>
        /// Pointer to collision shape.
        /// </summary>
        public CollisionShape Shape;

        /// <summary>
        /// Pointer to dynamic body.
        /// </summary>
        public RigidBody Body;

        /// <summary>
        /// Position of this hair element
        /// </summary>
        public Vector3 Position;
    }

    public class Hair : IDisposable
    {
        public EngineContainer Container;

        /// <summary>
        /// Entity who owns this hair.
        /// </summary>
        public Entity OwnerChar;

        /// <summary>
        /// Owner entity's body ID.
        /// </summary>
        public uint OwnerBody;

        /// <summary>
        /// ransform from owner body to root of hair start
        /// </summary>
        public Transform OwnerBodyHairRoot;

        /// <summary>
        /// Index of "root" element.
        /// </summary>
        public byte RootIndex;

        /// <summary>
        /// Index of "tail" element.
        /// </summary>
        public byte TailIndex;

        /// <summary>
        /// Array of elements.
        /// </summary>
        public List<HairElement> Elements;

        /// <summary>
        /// Array of joints.
        /// </summary>
        public List<Generic6DofConstraint> Joints;

        /// <summary>
        /// Mesh containing all vertices of all parts of this hair object.
        /// </summary>
        public BaseMesh Mesh;

        public void Dispose()
        {
            foreach (var joint in Joints.Where(joint => joint != null))
            {
                BtEngineDynamicsWorld.RemoveConstraint(joint);
            }

            foreach (var element in Elements.Where(element => element.Body != null))
            {
                element.Body.UserObject = null;
                BtEngineDynamicsWorld.RemoveRigidBody(element.Body);
            }
        }

        /// <summary>
        /// Creates hair into allocated hair structure, using previously defined setup and entity index.
        /// </summary>
        public bool Create(HairSetup setup, Entity parentEntity)
        {
            // No setup or parent to link to - bypass function.
            if (parentEntity == null || setup == null || setup.LinkBody >= parentEntity.Bf.BoneTags.Count ||
                parentEntity.Bt.BtBody[(int)setup.LinkBody] == null)
                return false;

            var model = EngineWorld.GetModelByID(setup.Model);

            // No model to link to - bypass function.
            if (model == null || model.MeshCount == 0) return false;

            // Setup engine container. FIXME: DOESN'T WORK PROPERLY ATM.
            Container = new EngineContainer();
            Container.Room = parentEntity.Self.Room;
            Container.ObjectType = OBJECT_TYPE.Hair;
            Container.Object = this;

            // Setup initial hair parameters.
            OwnerChar = parentEntity; // Entity to refer to.
            OwnerBody = setup.LinkBody; // Entity body to refer to.

            // Setup initial position / angles.
            var ownerBodyTransform = parentEntity.Transform * parentEntity.Bf.BoneTags[(int) OwnerBody].FullTransform;

            // Number of elements (bodies) is equal to number of hair meshes.
            Elements = new List<HairElement>();
            Elements.Resize(model.MeshCount, () => new HairElement());

            // Root index should be always zero, as it is how engine determines that it is
            // connected to head and renders it properly. Tail index should be always the
            // last element of the hair, as it indicates absence of "child" constraint.
            RootIndex = 0;
            TailIndex = (byte)(Elements.Count - 1);

            // Weight step is needed to determine the weight of each hair body.
            // It is derived from root body weight and tail body weight.
            var weightStep = (setup.RootWeight - setup.TailWeight) / Elements.Count;
            var currentWeight = setup.RootWeight;

            for (var i = 0; i < Elements.Count; i++)
            {
                // Point to corresponding mesh.
                Elements[i].Mesh = model.MeshTree[i].MeshBase;

                // Begin creating ACTUAL physical hair mesh.
                var localInertia = BulletSharp.Math.Vector3.Zero;

                // Make collision shape out of mesh.
                Elements[i].Shape = BT_CSfromMesh(Elements[i].Mesh, true, true, false);
                Elements[i].Shape.CalculateLocalInertia(currentWeight * setup.HairInertia, out localInertia);

                // Decrease next body weight to weight_step parameter.
                currentWeight -= weightStep;

                // Initialize motion state for body.
                var startTransform = ownerBodyTransform;
                var motionState = new DefaultMotionState(((Matrix4)startTransform).ToBullet());

                // Make rigid body.
                Elements[i].Body = new RigidBody(new RigidBodyConstructionInfo(currentWeight, motionState, Elements[i].Shape, localInertia));

                // Damping makes body stop in space by itself, to prevent it from continous movement.
                Elements[i].Body.SetDamping(setup.HairDamping[0], setup.HairDamping[1]);

                // Restitution and friction parameters define "bounciness" and "dullness" of hair.
                Elements[i].Body.Restitution = setup.HairRestitution;
                Elements[i].Body.Friction = setup.HairFriction;

                // Since hair is always moving with Lara, even if she's in still state (like, hanging
                // on a ledge), hair bodies shouldn't deactivate over time.
                Elements[i].Body.ForceActivationState(ActivationState.DisableDeactivation);

                // Hair bodies must not collide with each other, and also collide ONLY with kinematic
                // bodies (e. g. animated meshes), or else Lara's ghost object or anything else will be able to
                // collide with hair!
                Elements[i].Body.UserObject = Container;
                BtEngineDynamicsWorld.AddRigidBody(Elements[i].Body, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.KinematicFilter);

                Elements[i].Body.Activate();
            }

            // GENERATE CONSTRAINTS.
            // All constraints are generic 6-DOF type, as they seem perfect fit for hair.

            // Joint count is calculated from overall body amount multiplied by per-body constraint
            // count.
            Joints = new List<Generic6DofConstraint>();
            Joints.Resize(Elements.Count);

            // If multiple joints per body is specified, joints are placed in circular manner,
            // with obvious step of (SIMD_2_PI) / joint count. It means that all joints will form
            // circle-like figure.
            var currJoint = 0;

            for (var i = 0; i < Elements.Count; i++)
            {
                float bodyLength;
                var localA = new Transform();
                localA.SetIdentity();
                var localB = new Transform();
                localB.SetIdentity();

                var jointX = 0.0f;
                var jointY = 0.0f;

                RigidBody prevBody;
                if(i == 0) // First joint group
                {
                    // Adjust pivot point A to parent body.
                    localA.Origin = setup.HeadOffset + new Vector3(jointX, 0.0f, jointY);
                    Helper.SetEulerZYX(ref localA.Basis, setup.RootAngle.X, setup.RootAngle.Y, setup.RootAngle.Z);
                    // Stealing this calculation because I need it for drawing
                    OwnerBodyHairRoot = localA;

                    localB.Origin = new Vector3(jointX, 0.0f, jointY);
                    Helper.SetEulerZYX(ref localB.Basis, 0, -HalfPI, 0);

                    prevBody = parentEntity.Bt.BtBody[(int) OwnerBody]; // Previous body is parent body.
                }
                else
                {
                    // Adjust pivot point A to previous mesh's length, considering mesh overlap multiplier.
                    bodyLength = Math.Abs(Elements[i - 1].Mesh.BBMax.Y - Elements[i - 1].Mesh.BBMin.Y) *
                                 setup.JointOverlap;

                    localA.Origin = new Vector3(jointX, bodyLength, jointY);
                    Helper.SetEulerZYX(ref localA.Basis, 0, -HalfPI, 0);

                    // Pivot point B is automatically adjusted by Bullet.
                    localB.Origin = new Vector3(jointX, 0.0f, jointY);
                    Helper.SetEulerZYX(ref localB.Basis, 0, -HalfPI, 0);

                    prevBody = Elements[i - 1].Body; // Previous body is preceding hair mesh.
                }

                // Create 6DOF constraint.
                Joints[currJoint] = new Generic6DofConstraint(prevBody, Elements[i].Body, ((Matrix4) localA).ToBullet(),
                    ((Matrix4) localB).ToBullet(), true);

                // CFM and ERP parameters are critical for making joint "hard" and link
                // to Lara's head. With wrong values, constraints may become "elastic".
                for (var axis = 0; axis < 6; axis++)
                {
                    Joints[currJoint].SetParam(ConstraintParam.StopCfm, setup.JointCfm, axis);
                    Joints[currJoint].SetParam(ConstraintParam.StopErp, setup.JointErp, axis);
                }

                Joints[currJoint].LinearLowerLimit = BulletSharp.Math.Vector3.Zero;
                Joints[currJoint].LinearUpperLimit = BulletSharp.Math.Vector3.Zero;

                if(i == 0)
                {
                    // First joint group should be more limited in motion, as it is connected
                    // right to the head. NB: Should we make it scriptable as well?
                    Joints[currJoint].AngularLowerLimit = new BulletSharp.Math.Vector3(-HalfPI, 0.0f, -HalfPI * 0.4f);
                    Joints[currJoint].AngularLowerLimit = new BulletSharp.Math.Vector3(-HalfPI * 0.3f, 0.0f, HalfPI * 0.4f);

                    // Increased solver iterations make constraint even more stable.
                    Joints[currJoint].OverrideNumSolverIterations = 100;
                }
                else
                {
                    // Normal joint with more movement freedom.
                    Joints[currJoint].AngularLowerLimit = new BulletSharp.Math.Vector3(-HalfPI * 0.5f, 0.0f, -HalfPI * 0.5f);
                    Joints[currJoint].AngularLowerLimit = new BulletSharp.Math.Vector3(HalfPI * 0.5f, 0.0f, HalfPI * 0.5f);
                }

                Joints[currJoint].DebugDrawSize = 5.0f; // Draw constraint axes.

                // Add constraint to the world.
                BtEngineDynamicsWorld.AddConstraint(Joints[currJoint], true);

                currJoint++; // Point to the next joint.
            }

            createHairMesh(model);

            return true;
        }


        private void createHairMesh(SkeletalModel model)
        {
            Mesh = new BaseMesh();
            Mesh.ElementsPerTexture.Resize(EngineWorld.Textures.Count);
            var totalElements = 0;

            // Gather size information
            for (var i = 0; i < model.MeshCount; i++)
            {
                var original = model.MeshTree[i].MeshBase;

                Mesh.TexturePageCount = Math.Max(Mesh.TexturePageCount, original.TexturePageCount);

                for (var j = 0; j < original.TexturePageCount; j++)
                {
                    Mesh.ElementsPerTexture[j] += original.ElementsPerTexture[j];
                    totalElements += (int) original.ElementsPerTexture[j];
                }
            }

            // Create arrays
            Mesh.Elements.Resize(totalElements);

            // - with matrix index information
            Mesh.MatrixIndices.Resize(Mesh.Vertices.Count, () => new BaseMesh.MatrixIndex());

            // Copy information
            var elementsStartPerTexture = new List<uint>();
            elementsStartPerTexture.Resize((int)Mesh.TexturePageCount);
            Mesh.Vertices.Clear();
            for (var i = 0; i < model.MeshCount; i++)
            {
                var original = model.MeshTree[i].MeshBase;

                // Copy vertices
                var verticesStart = Mesh.Vertices.Count; // TODO: Wut... size == 0 (cf. L328)
                Mesh.Vertices.AddRange(original.Vertices);

                // Copy elements
                var originalElementsStart = 0;
                for (var page = 0; page < original.TexturePageCount; page++)
                {
                    if (original.ElementsPerTexture[page] == 0)
                        continue;

                    Assert(originalElementsStart < original.Elements.Count);
                    Assert(originalElementsStart + original.ElementsPerTexture[page] <= original.Elements.Count);

                    Assert(elementsStartPerTexture[page] < Mesh.Elements.Count);
                    Assert(elementsStartPerTexture[page] + original.ElementsPerTexture[page] <= Mesh.Elements.Count);

                    Helper.ListCopy(original.Elements, originalElementsStart, Mesh.Elements,
                        (int) elementsStartPerTexture[page], (int) original.ElementsPerTexture[page]);

                    for (var j = 0; j < original.ElementsPerTexture[page]; j++)
                    {
                        Mesh.Elements[(int) elementsStartPerTexture[page]] =
                            (uint) (verticesStart + original.Elements[originalElementsStart]);
                        originalElementsStart++;
                        elementsStartPerTexture[page]++;
                    }
                }

                // Apply total offset from parent.
                // The resulting mesh will have all the hair in default position
                // (i.e. as one big rope). The shader and matrix then transform it
                // correctly.
                Elements[i].Position = model.MeshTree[i].Offset;
                if(i > 0)
                {
                    // TODO: This assumes the parent is always the preceding mesh.
                    // True for hair, obviously wrong for everything else. Can stay
                    // here, but must go when we start generalizing the whole thing.
                    Elements[i].Position += Elements[i - 1].Position;
                }

                // And create vertex data (including matrix indices)
                for (var j = 0; j < original.Vertices.Count; j++)
                {
                    Mesh.MatrixIndices.Add(new BaseMesh.MatrixIndex());
                    Assert(Mesh.MatrixIndices.Count > verticesStart + j);
                    if (original.Vertices[j].Position[1] <= 0)
                    {
                        Mesh.MatrixIndices[verticesStart + j].I = (sbyte) i;
                        Mesh.MatrixIndices[verticesStart + j].J = (sbyte) (i + 1);
                    }
                    else
                    {
                        Mesh.MatrixIndices[verticesStart + j].I = (sbyte) (i + 1);
                        Mesh.MatrixIndices[verticesStart + j].J = Math.Min((sbyte) (i + 2), (sbyte) model.MeshCount);
                    }

                    // Now move all the hair vertices
                    Mesh.Vertices[verticesStart + j].Position += Elements[i].Position;

                    // If the normal isn't fully in y direction, cancel its y component
                    // This is perhaps a bit dubious.
                    if (Mesh.Vertices[verticesStart + j].Normal.X != 0 || Mesh.Vertices[verticesStart + j].Normal.Z != 0)
                    {
                        Mesh.Vertices[verticesStart + j].Normal.Y = 0;
                        Mesh.Vertices[verticesStart + j].Normal.Normalize();
                    }
                }
            }

            Mesh.GenVBO(Renderer);
        }
    }

    public class HairSetup
    {
        /// <summary>
        /// Hair model ID
        /// </summary>
        public uint Model;

        /// <summary>
        /// Lara's head mesh index
        /// </summary>
        public uint LinkBody;

        /// <summary>
        /// Root and tail hair body weight. Intermediate body
        /// </summary>
        public float RootWeight;

        /// <summary>
        /// Weights are calculated from these two parameters
        /// </summary>
        public float TailWeight;

        /// <summary>
        /// Damping affects hair "plasticity"
        /// </summary>
        public float[] HairDamping = new float[2];

        /// <summary>
        /// Inertia affects hair "responsiveness"
        /// </summary>
        public float HairInertia;

        /// <summary>
        /// "Bounciness" of the hair
        /// </summary>
        public float HairRestitution;

        /// <summary>
        /// How much other bodies will affect hair trajectory
        /// </summary>
        public float HairFriction;

        /// <summary>
        /// How much two hair bodies overlap each other
        /// </summary>
        public float JointOverlap;

        /// <summary>
        /// Constraint force mixing (joint softness)
        /// </summary>
        public float JointCfm;

        /// <summary>
        /// Error reduction parameter (joint "inertia")
        /// </summary>
        public float JointErp;

        /// <summary>
        /// Linear offset to place hair to
        /// </summary>
        public Vector3 HeadOffset;

        /// <summary>
        /// First constraint set angle (to align hair angle)
        /// </summary>
        public Vector3 RootAngle;

        /// <summary>
        /// Gets scripted hair set-up to specified hair set-up structure.
        /// </summary>
        public void GetSetup(uint hairEntryIndex)
        {
            var res2 = ((LuaFunction) EngineLua["getHairSetup"]).Call(hairEntryIndex)[0];
            if (!(res2 is LuaTable))
                return;
            dynamic res = (LuaTable)res2;

            Model = (uint)res["model"];
            LinkBody = (uint)res["link_body"];
            RootWeight = (float)res["props"]["root_weight"];
            TailWeight = (float)res["props"]["tail_weight"];
            HairInertia = (float)res["props"]["hair_inertia"];
            HairFriction = (float)res["props"]["hair_friction"];
            HairRestitution = (float)res["props"]["hair_bouncing"];
            JointOverlap = (float)res["props"]["joint_overlap"];
            JointCfm = (float)res["props"]["joint_cfm"];
            JointErp = (float)res["props"]["joint_erp"];
            HairDamping[0] = (float)res["props"]["hair_damping"][1];
            HairDamping[1] = (float)res["props"]["hair_damping"][2];
            dynamic off = res["offset"];
            HeadOffset = new Vector3((float)off[1], (float)off[2], (float)off[3]);
            dynamic ra = res["root_angle"];
            RootAngle = new Vector3((float)ra[1], (float)ra[2], (float)ra[3]);
        }
    }
}

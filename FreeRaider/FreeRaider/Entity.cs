using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BulletSharp;
using OpenTK;
using SharpFont;

namespace FreeRaider
{
    public enum ENTITY_ANIM : short
    {
        None = 0x00,
        NewFrame = 0x01,
        NewAnim = 0x02
    }

    [Flags]
    public enum ENTITY_TYPE : ushort
    {
        /// <summary>
        /// Just an animating.
        /// </summary>
        Generic = 0x0000,
        /// <summary>
        /// Can respond to other entity's commands.
        /// </summary>
        Interactive = 0x0001,
        /// <summary>
        /// Can activate triggers.
        /// </summary>
        TriggerActivator = 0x0002,
        /// <summary>
        /// Can activate heavy triggers.
        /// </summary>
        HeavyTriggerActivator = 0x0004,
        /// <summary>
        /// Can be picked up.
        /// </summary>
        Pickable = 0x0008,
        /// <summary>
        /// Can be pushed/pulled.
        /// </summary>
        Traverse = 0x0010,
        /// <summary>
        /// Can be walked upon.
        /// </summary>
        TraverseFloor = 0x0020,
        /// <summary>
        /// Acts as a physical dynamic object.
        /// </summary>
        Dynamic = 0x0040,
        /// <summary>
        /// Is actor.
        /// </summary>
        Actor = 0x0080,
        /// <summary>
        /// Does collision checks for itself.
        /// </summary>
        CollCheck = 0x0100,
        /// <summary>
        /// Was spawned.
        /// </summary>
        Spawned = 0x8000
    }

    [Flags]
    public enum ENTITY_CALLBACK : uint
    {
        None = 0x00000000,
        Activate = 0x00000001,
        Deactivate = 0x00000002,
        Collision = 0x00000004,
        Stand = 0x00000008,
        Hit = 0x00000010,
        RoomCollision = 0x00000020
    }

    public enum Substance
    {
        None,
        WaterShallow,
        WaterWade,
        WaterSwim,
        QuicksandShallow,
        QuicksandConsumed
    }

    public enum ENTITY_TLAYOUT : byte
    {
        /// <summary>
        /// Activation mask
        /// </summary>
        Mask = 0x1F,
        /// <summary>
        /// Last trigger event
        /// </summary>
        Event = 0x20,
        /// <summary>
        /// Activity lock
        /// </summary>
        Lock = 0x40,
        /// <summary>
        /// Sector status
        /// </summary>
        SectorStatus = 0x80
    }

    public class EntityCollisionNode
    {
        public List<CollisionObject> Obj;
    }

    public class BtEntityData
    {
        public bool NoFixAll;

        public BODY_PART NoFixBodyParts;

        /// <summary>
        /// Like Bullet character controller for penetration resolving.
        /// </summary>
        public List<PairCachingGhostObject> GhostObjects;

        /// <summary>
        /// Keep track of the contact manifolds
        /// </summary>
        public AlignedManifoldArray ManifoldArray;

        public List<CollisionShape> Shapes;

        public List<RigidBody> BtBody;

        /// <summary>
        /// Ragdoll joints
        /// </summary>
        public List<TypedConstraint> BtJoints;

        public List<EntityCollisionNode> LastCollisions;
    }

    /// <summary>
    /// Entity movement types
    /// </summary>
    public enum MoveType
    {
        StaticPos,
        Kinematic,
        OnFloor,
        Wade,
        Quicksand,
        OnWater,
        Underwater,
        FreeFalling,
        Climbing,
        Monkeyswing,
        WallsClimb,
        Dozy
    }

    public class Entity
    {
        /// <summary>
        /// Unique entity ID
        /// </summary>
        public uint ID { get; }

        /// <summary>
        /// Object code bit (since TR4)
        /// </summary>
        public int OCB;

        /// <summary>
        /// Mask + once + event + sector status flags
        /// </summary>
        public ENTITY_TLAYOUT TriggerLayout;

        /// <summary>
        /// Set by "timer" trigger field
        /// </summary>
        public float Timer;

        /// <summary>
        /// Information about script callbacks
        /// </summary>
        public ENTITY_CALLBACK CallbackFlags;

        public ENTITY_TYPE TypeFlags = ENTITY_TYPE.Generic;

        public bool Enabled = true;

        public bool Active = true;

        public bool Visible = true;

        /// <summary>
        /// Move direction
        /// </summary>
        public ENT_MOVE DirFlag = ENT_MOVE.Stay;

        /// <summary>
        /// On floor / free fall / swim / .....
        /// </summary>
        public MoveType MoveType = MoveType.StaticPos;

        /// <summary>
        /// Render once per frame trigger
        /// </summary>
        public bool WasRendered;

        /// <summary>
        /// Same for debug lines
        /// </summary>
        public bool WasRenderedLines;

        /// <summary>
        /// Current linear speed from animation info
        /// </summary>
        public float CurrentSpeed;

        /// <summary>
        /// Speed of the entity XYZ
        /// </summary>
        public Vector3 Speed;

        public float SpeedMult = Constants.TR_FRAME_RATE;

        /// <summary>
        /// Linear inertia
        /// </summary>
        public float InertiaLinear;

        /// <summary>
        /// Angular inertia - X and Y axes
        /// </summary>
        public Vector2 InertiaAngular;

        /// <summary>
        /// Current bone frame with full frame information
        /// </summary>
        public SSBoneFrame Bf;

        public BtEntityData Bt;

        public Vector3 Angles;

        /// <summary>
        /// GL transformation matrix
        /// </summary>
        public Transform Transform;

        public Vector3 Scaling = Vector3.One;

        /// <summary>
        /// Oriented bounding box
        /// </summary>
        public OBB OBB;

        public RoomSector CurrentSector;

        public RoomSector LastSector;

        public EngineContainer Self;

        /// <summary>
        /// Where we can activate object (dx, dy, dz)
        /// </summary>
        public Vector3 ActivationOffset = new Vector3(0, 256, 0);

        public float ActivationRadius = 128;

        public Entity(uint id)
        {
            ID = id;
            MoveType = MoveType.OnFloor;
            Self = new EngineContainer();

            Transform.SetIdentity();
            Self.Object = this;
            Self.ObjectType = OBJECT_TYPE.Entity;
            Self.Room = null;
            Self.CollisionType = COLLISION_TYPE.None;
            OBB.Transform = Transform;
            Bt.BtBody.Clear();
            Bt.BtJoints.Clear();
            Bt.NoFixAll = false;
            Bt.NoFixBodyParts = 0x0000000;
            Bt.ManifoldArray = null;
            Bt.Shapes.Clear();
            Bt.GhostObjects.Clear();
            Bt.LastCollisions.Clear();

            Bf.Animations.Model = null;
            Bf.Animations.ClearOnFrame();
            Bf.Animations.FrameTime = 0.0f;
            Bf.Animations.LastState = TR_STATE.LaraWalkForward;
            Bf.Animations.NextState = TR_STATE.LaraWalkForward;
            Bf.Animations.Lerp = 0.0f;
            Bf.Animations.CurrentAnimation = TR_ANIMATION.LaraRun;
            Bf.Animations.CurrentFrame = 0;
            Bf.Animations.NextAnimation = TR_ANIMATION.LaraRun;
            Bf.Animations.NextFrame = 0;
            Bf.Animations.Next = null;
            Bf.BoneTags.Clear();
            Bf.BBMax = Vector3.Zero;
            Bf.BBMin = Vector3.Zero;
            Bf.Centre = Vector3.Zero;
            Bf.Position = Vector3.Zero;
            Speed = Vector3.Zero;
        }

        ~Entity()
        {
            Bt.LastCollisions.Clear();

            if(Bt.BtJoints.Count > 0)
            {
                DeleteRagdoll();
            }

            foreach (var ghost in Bt.GhostObjects)
            {
                ghost.UserObject = null;
                Global.BtEngineDynamicsWorld.RemoveCollisionObject(ghost);
            }
            Bt.GhostObjects.Clear();

            Bt.Shapes.Clear();

            Bt.ManifoldArray.Clear();

            if(Bt.BtBody.Count > 0)
            {
                foreach (var body in Bt.BtBody)
                {
                    if(body != null)
                    {
                        body.UserObject = null;
                        if(body.MotionState != null)
                        {
                            body.MotionState.Dispose();
                            body.MotionState = null;
                        }
                        body.CollisionShape = null;

                        Global.BtEngineDynamicsWorld.RemoveRigidBody(body);
                    }
                }
                Bt.BtBody.Clear();
            }

            Self = null;

            Bf.BoneTags.Clear();

            for (var ssAnim = Bf.Animations.Next; ssAnim != null;)
            {
                var ssAnimNext = ssAnim.Next;
                ssAnim.Next = null;
                ssAnim = ssAnimNext;
            }
            Bf.Animations.Next = null;
        }

        public void CreateGhosts()
        {
            if (Bf.Animations.Model == null || Bf.Animations.Model.MeshCount <= 0)
                return;

            Bt.ManifoldArray = new AlignedManifoldArray();
            Bt.Shapes.Clear();
            Bt.GhostObjects.Clear();
            Bt.LastCollisions.Clear();
            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                var box = Constants.COLLISION_GHOST_VOLUME_COEFFICIENT *
                          (Bf.BoneTags[i].MeshBase.BBMax - Bf.BoneTags[i].MeshBase.BBMin);
                Bt.Shapes.Add(new BoxShape(box));
                Bt.Shapes.Last().Margin = Constants.COLLISION_MARGIN_DEFAULT;
                Bf.BoneTags[i].MeshBase.Radius = Math.Min(Math.Min(box.X, box.Y), box.Z);

                var pcg = new PairCachingGhostObject();

                pcg.SetIgnoreCollisionCheck(Bt.BtBody[i], true);

                var gltr = Transform * Bf.BoneTags[i].FullTransform;
                gltr.Origin = gltr * Bf.BoneTags[i].MeshBase.Center;

                pcg.WorldTransform = (Matrix4)gltr;
                pcg.CollisionFlags |= CollisionFlags.NoContactResponse | CollisionFlags.CharacterObject;
                pcg.UserObject = Self;
                pcg.CollisionShape = Bt.Shapes.Last();
                Global.BtEngineDynamicsWorld.AddCollisionObject(pcg, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.AllFilter);
                Bt.GhostObjects.Add(pcg);

                Bt.LastCollisions.Add(null);
            }
        }

        public void Enable()
        {
            if(!Enabled)
            {
                EnableCollision();
                Enabled = Active = Visible = true;
            }
        }

        public void Disable()
        {
            if (Enabled)
            {
                DisableCollision();
                Enabled = Active = Visible = false;
            }
        }

        /// <summary>
        /// This function enables collision for entity in all cases except NULL models.
        /// If collision models does not exists, function will create them.
        /// </summary>
        public void EnableCollision()
        {
            foreach (var b in Bt.BtBody)
            {
                if(b != null && !b.IsInWorld)
                {
                    Global.BtEngineDynamicsWorld.AddRigidBody(b);
                }
            }
        }

        public void DisableCollision()
        {
            foreach (var b in Bt.BtBody)
            {
                if (b != null && b.IsInWorld)
                {
                    Global.BtEngineDynamicsWorld.RemoveRigidBody(b);
                }
            }
        }

        public void GenRigidBody()
        {
            if (Bf.Animations.Model == null || Self.CollisionType == COLLISION_TYPE.None)
                return;

            Bt.BtBody.Clear();

            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                var mesh = Bf.Animations.Model.MeshTree[i].MeshBase;
                CollisionShape cshape;
                switch(Self.CollisionShape)
                {
                    case COLLISION_SHAPE.Sphere:
                        cshape = CollisionShapeHelper.CSfromSphere(mesh.Radius);
                        break;

                    case COLLISION_SHAPE.TrimeshConvex:
                        cshape = CollisionShapeHelper.CSfromMesh(mesh, true, true, false);
                        break;

                    case COLLISION_SHAPE.Trimesh:
                        cshape = CollisionShapeHelper.CSfromMesh(mesh, true, true, true);
                        break;

                    case COLLISION_SHAPE.Box:
                    default:
                        cshape = CollisionShapeHelper.CSfromBBox(mesh.BBMin, mesh.BBMax, true, true);
                        break;
                }

                Bt.BtBody.Add(null);

                if(cshape != null)
                {
                    var localInertia = Vector3.Zero;
                    if (Self.CollisionShape != COLLISION_SHAPE.Trimesh)
                        cshape.CalculateLocalInertia(0.0f, out localInertia);

                    var startTransform = Transform * Bf.BoneTags[i].FullTransform;
                    var motionState = new DefaultMotionState((Matrix4)startTransform);
                    Bt.BtBody[Bt.BtBody.Count - 1] = new RigidBody(new RigidBodyConstructionInfo(0.0f, motionState, cshape, localInertia));

                    CollisionFlags cf = CollisionFlags.None;
                    switch (Self.CollisionType)
                    {
                        case COLLISION_TYPE.Kinematic:
                            Bt.BtBody.Last().CollisionFlags |= CollisionFlags.KinematicObject;
                            break;

                        case COLLISION_TYPE.Ghost:
                            Bt.BtBody.Last().CollisionFlags |= CollisionFlags.NoContactResponse;
                            break;

                        case COLLISION_TYPE.Actor:
                        case COLLISION_TYPE.Vehicle:
                            Bt.BtBody.Last().CollisionFlags |= CollisionFlags.CharacterObject;
                            break;

                        case COLLISION_TYPE.Static:
                        default:
                            Bt.BtBody.Last().CollisionFlags |= CollisionFlags.StaticObject;
                            break;
                    }

                    Global.BtEngineDynamicsWorld.AddRigidBody(Bt.BtBody[i], CollisionFilterGroups.KinematicFilter, CollisionFilterGroups.AllFilter);
                    Bt.BtBody.Last().UserObject = Self;
                }
            }
        }

        public void GhostUpdate()
        {
            if (Bt.GhostObjects.Count == 0)
                return;

            Assert.That(Bt.GhostObjects.Count == Bf.BoneTags.Count);

            if(TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                for(var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    var tr = Transform * Bf.BoneTags[i].FullTransform;
                    var v = Bf.Animations.Model.MeshTree[i].MeshBase.Center;
                    var pos = tr * v;
                    tr.Origin = pos;
                    Bt.GhostObjects[i].WorldTransform = (Matrix4)tr;
                }
            }
            else
            {
                for (var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    var tr = (Transform)Bt.BtBody[i].WorldTransform;
                    tr.Origin = Bf.Animations.Model.MeshTree[i].MeshBase.Center;
                    Bt.GhostObjects[i].WorldTransform = (Matrix4)tr;
                }
            }
        }

        public void UpdateCurrentCollisions();

        public int GetPenetrationFixVector(out Vector3 reaction, bool hasMove)
        {
            reaction = Vector3.Zero;
            if (Bt.GhostObjects.Count == 0 || Bt.NoFixAll)
                return 0;

            Assert.That(Bt.GhostObjects.Count == Bf.BoneTags.Count);

            var origPos = Transform.Origin;
            var ret = 0;
            for (var i = 0; i < Bf.Animations.Model.CollisionMap.Count; i++)
            {
                var m = Bf.Animations.Model.CollisionMap[i];
                var btag = Bf.BoneTags[m];

                if (btag.BodyPart.HasFlagUns(Bt.NoFixBodyParts))
                {
                    continue;
                }

                // antitunneling condition for main body parts, needs only in move case: ((move != NULL) && (btag->body_part & (BODY_PART_BODY_LOW | BODY_PART_BODY_UPPER)))
                Vector3 from;
                if (btag.Parent == null ||
                    (hasMove && btag.BodyPart.HasFlagUns(BODY_PART.BodyLow | BODY_PART.BodyUpper)))
                {
                    from = ((Transform) Bt.GhostObjects[m].WorldTransform).Origin;
                    from += Transform.Origin - origPos;
                }
                else
                {
                    var parentFrom = btag.Parent.FullTransform * btag.Parent.MeshBase.Center;
                    from = Transform * parentFrom;
                }

                var tr = Transform * btag.FullTransform;
                var to = tr * btag.MeshBase.Center;
                var curr = from;
                var move = to - from;
                var moveLen = move.Length;
                if (i == 0 && moveLen > 1024.0f)
                {
                    break;
                }
                var iter = (int) (4.0f * moveLen / btag.MeshBase.Radius + 1);
                move /= iter;

                for(var j = 0; j <= iter; j++)
                {
                    tr.Origin = curr;
                    var trCurrent = tr;
                    Bt.GhostObjects[m].WorldTransform = (Matrix4) trCurrent;
                    Vector3 tmp;
                    if(StaticFuncs.GhostGetPenetrationFixVector(Bt.GhostObjects[m], Bt.ManifoldArray, out tmp).ToBool())
                    {
                        Transform.Origin += tmp;
                        curr += tmp;
                        from += tmp;
                        ret++;
                    }
                    curr += move;
                }
            }
            reaction = Transform.Origin - origPos;
            Transform.Origin = origPos;

            return ret;
        }

        public void CheckCollisionCallbacks()
        {
            if (Bt.GhostObjects.Count == 0)
                return;

            CollisionObject cobj;
            uint currFlag;
            UpdateCurrentCollisions();
            while((cobj = GetRemoveCollisionBodyParts(0xFFFFFFFF, out currFlag)) != null)
            {
                // do callbacks here:
                var type = OBJECT_TYPE.None;
                var cont = (EngineContainer) cobj.UserObject;
                if(cont != null)
                {
                    type = cont.ObjectType;
                }

                if(type == OBJECT_TYPE.Entity)
                {
                    var activator = (Entity) cont.Object;

                    if(activator.CallbackFlags.HasFlag(ENTITY_CALLBACK.Collision))
                    {
                        // Activator and entity IDs are swapped in case of collision callback.
                        Global.EngineLua.ExecEntity((int)ENTITY_CALLBACK.Collision, (int)activator.ID, (int)ID);
                    }
                }
                else if(CallbackFlags.HasFlag(ENTITY_CALLBACK.RoomCollision) && type == OBJECT_TYPE.RoomBase)
                {
                    var activator = (Room) cont.Object;
                    Global.EngineLua.ExecEntity((int)ENTITY_CALLBACK.RoomCollision, (int)ID, (int)activator.ID);
                }
            }
        }

        public bool WasCollisionBodyParts(uint partsFlags)
        {
            return Bt.LastCollisions.Count != 0 && Bf.BoneTags.Where((t, i) => t.BodyPart.HasFlagUns(partsFlags) && Bt.LastCollisions[i].Obj.Count > 0).Any();
        }

        public void CleanCollisionAllBodyParts()
        {
            foreach (var coll in Bt.LastCollisions)
            {
                coll.Obj.Clear();
            }
        }

        public void CleanCollisionBodyParts(uint partsFlags)
        {
            if (Bt.LastCollisions.Count == 0)
                return;

            for (var i = 0; i < Bf.BoneTags.Count; i++)
            {
                if(Bf.BoneTags[i].BodyPart.HasFlagUns(partsFlags))
                {
                    Bt.LastCollisions[i].Obj.Clear();
                }
            }
        }

        public CollisionObject GetRemoveCollisionBodyParts(uint partsFlags, out uint currFlag)
        {
            currFlag = 0x00;
            if (Bt.LastCollisions.Count == 0)
                return null;

            for (var i = 0; i < Bf.BoneTags.Count; i++)
            {
                if (Bf.BoneTags[i].BodyPart.HasFlagUns(partsFlags))
                {
                    var cn = Bt.LastCollisions[i];
                    if(cn.Obj.Count > 0)
                    {
                        currFlag = Bf.BoneTags[i].BodyPart;
                        var res = cn.Obj.Last();
                        cn.Obj.RemoveAt(cn.Obj.Count - 1);
                        return res;
                    }
                }
            }

            return null;
        }

        public void UpdateRoomPos()
        {
            var pos = GetRoomPos();
            var newRoom = Room.FindPosCogerrence(pos, Self.Room);
            if(newRoom == null)
            {
                CurrentSector = null;
                return;
            }

            var newSector = newRoom.GetSectorXYZ(pos);
            newRoom = newSector.OwnerRoom;
            
            TransferToRoom(newRoom);

            Self.Room = newRoom;
            LastSector = CurrentSector;

            if(CurrentSector != newSector)
            {
                TriggerLayout &= ~ENTITY_TLAYOUT.SectorStatus; // Reset sector status.
                CurrentSector = newSector;
            }
        }

        public void UpdateRigidBody(bool force)
        {
            if(TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                Transform = (Transform)Bt.BtBody[0].WorldTransform;
                UpdateRoomPos();
                for(var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    Bf.BoneTags[i].FullTransform = Transform.Inverse * Bt.BtBody[i].WorldTransform;
                }

                // that cycle is necessary only for skinning models
                foreach (var t in Bf.BoneTags)
                {
                    t.Transform = t.Parent != null ? t.Parent.FullTransform.Inverse * t.FullTransform : t.FullTransform;
                }

                UpdateGhostRigidBody();

                Bf.BBMin = Bf.BoneTags[0].MeshBase.BBMin;
                Bf.BBMax = Bf.BoneTags[0].MeshBase.BBMax;
                if(Bf.BoneTags.Count > 1)
                {
                    foreach (var b in Bf.BoneTags)
                    {
                        var pos = b.FullTransform.Origin;
                        var bbmin = b.MeshBase.BBMin;
                        var bbmax = b.MeshBase.BBMax;
                        var r = bbmax.X - bbmin.X;
                        var t = bbmax.Y - bbmin.Y;
                        r = Math.Max(t, r);
                        t = bbmax.Z - bbmin.Z;
                        r = Math.Max(t, r);
                        r *= 0.5f;

                        Bf.BBMin = Helper.Vec3Min(Bf.BBMin, pos.AddF(-r));

                        Bf.BBMax = Helper.Vec3Max(Bf.BBMax, pos.AddF(r));
                    }
                }
            }
            else
            {
                if (Bf.Animations.Model == null || Bt.BtBody.Count == 0 ||
                    !force && Bf.Animations.Model.Animations.Count == 1 &&
                    Bf.Animations.Model.Animations[0].Frames.Count == 1)
                {
                    return;
                }

                UpdateRoomPos();
                if(Self.CollisionType != COLLISION_TYPE.Static)
                {
                    for(var i = 0; i < Bf.BoneTags.Count; i++)
                    {
                        if(Bt.BtBody[i] != null)
                        {
                            Bt.BtBody[i].WorldTransform = (Matrix4)(Transform * Bf.BoneTags[i].FullTransform);
                        }
                    }
                }
            }
            RebuildBV();
        }

        public void RebuildBV();

        public int GetAnimDispatchCase(uint id)
        {
            var anim = Bf.Animations.Model.Animations[(int) Bf.Animations.CurrentAnimation];

            foreach (var stc in anim.StateChange)
            {
                if(stc.ID == id)
                {
                    for (var j = 0; j < stc.AnimDispatch.Count; j++)
                    {
                        var disp = stc.AnimDispatch[j];
                        if (disp.FrameHigh >= disp.FrameLow && Bf.Animations.CurrentFrame.IsBetween(disp.FrameLow, disp.FrameHigh))
                        {
                            return j;
                        }
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Next frame and next anim calculation function.
        /// </summary>
        public static void GetNextFrame(SSBoneFrame bf, float time, StateChange stc, out short frame, out TR_ANIMATION anim,
            AnimControlFlags animFlags)
        {
            var currAnim = bf.Animations.Model.Animations[(int) bf.Animations.CurrentAnimation];

            frame = (short)((bf.Animations.FrameTime + time) / bf.Animations.Period);
            frame = Math.Max(frame, (short)0); // paranoid checking
            anim = bf.Animations.CurrentAnimation;

            // Flag has a highest priority
            if(animFlags == AnimControlFlags.LoopLastFrame)
            {
                if(frame >= currAnim.Frames.Count - 1)
                {
                    frame = (short) (currAnim.Frames.Count - 1);
                    anim = bf.Animations.CurrentAnimation; // paranoid duplicate
                }
                return;
            }
            else if(animFlags == AnimControlFlags.Lock)
            {
                frame = 0;
                anim = bf.Animations.CurrentAnimation;
                return;
            }

            // Check next anim if frame >= frames.Count
            if(frame >= currAnim.Frames.Count)
            {
                if(currAnim.NextAnim != null)
                {
                    frame = (short)currAnim.NextFrame;
                    anim = currAnim.NextAnim.ID;
                    return;
                }

                frame = (short)(frame % currAnim.Frames.Count);
                anim = bf.Animations.CurrentAnimation; // paranoid duplicate
                return;
            }

            // State change check
            if(stc != null)
            {
                foreach (var disp in stc.AnimDispatch)
                {
                    if(disp.FrameHigh >= disp.FrameLow && frame.IsBetween(disp.FrameLow, disp.FrameHigh))
                    {
                        anim = disp.NextAnim;
                        frame = (short) disp.NextFrame;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Process frame + trying to change state
        /// </summary>
        public ENTITY_ANIM Frame(float time);

        public virtual void UpdateTransform()
        {
            Angles = Angles.WrapAngle();

            Helper.SetEulerZYX(ref Transform.Basis, Angles.Y * Constants.RadPerDeg, Angles.Z * Constants.RadPerDeg,
                Angles.X * Constants.RadPerDeg);

            FixPenetrations(Vector3.Zero);
        }

        public void UpdateCurrentSpeed(bool zeroVz = false)
        {
            var t = CurrentSpeed * SpeedMult;
            var vz = zeroVz ? 0.0f : Speed.Z;

            if(DirFlag.HasFlag(ENT_MOVE.MoveForward))
            {
                Speed = Transform.Basis.Column1 * t;
            }
            else if (DirFlag.HasFlag(ENT_MOVE.MoveBackward))
            {
                Speed = Transform.Basis.Column1 * -t;
            }
            else if (DirFlag.HasFlag(ENT_MOVE.MoveLeft))
            {
                Speed = Transform.Basis.Column0 * -t;
            }
            else if (DirFlag.HasFlag(ENT_MOVE.MoveRight))
            {
                Speed = Transform.Basis.Column0 * t;
            }
            else
            {
                Speed = Vector3.Zero;
            }

            Speed.Z = vz;
        }

        public void AddOverrideAnim(int modelID)
        {
            var sm = Global.EngineWorld.GetModelByID((uint)modelID);

            if(sm != null && sm.MeshCount == Bf.BoneTags.Count)
            {
                var ssAnim = new SSAnimation();

                ssAnim.Model = sm;
                ssAnim.ClearOnFrame();
                ssAnim.Next = Bf.Animations.Next;
                Bf.Animations.Next = ssAnim;

                // TODO: Useless assign to 0?
                ssAnim.FrameTime = 0.0f;
                ssAnim.NextState = TR_STATE.LaraWalkForward;
                ssAnim.Lerp = 0.0f;
                ssAnim.CurrentAnimation = TR_ANIMATION.LaraRun;
                ssAnim.CurrentFrame = 0;
                ssAnim.CurrentAnimation = TR_ANIMATION.LaraRun;
                ssAnim.NextFrame = 0;
                ssAnim.Period = 1.0f / Constants.TR_FRAME_RATE;
            }
        }

        public void CheckActivators();

        public virtual Substance GetSubstanceState()
        {
            return Substance.None;
        }

        public static void UpdateCurrentBoneFrame(SSBoneFrame bf, Transform etr);

        public void DoAnimCommands(SSAnimation ssAnim, int changing);

        public void ProcessSector();

        public void SetAnimation(TR_ANIMATION animation, int frame = 0, int anotherModel = -1)
        {
            if(Bf.Animations.Model == null || (int)animation >= Bf.Animations.Model.Animations.Count)
            {
                return;
            }

            animation = animation < 0 ? 0 : animation;
            Bt.NoFixAll = false;

            if(anotherModel >= 0)
            {
                var model = Global.EngineWorld.GetModelByID((uint) anotherModel);
                if (model == null || (int) animation >= model.Animations.Count)
                    return;
                Bf.Animations.Model = model;
            }

            var anim = Bf.Animations.Model.Animations[(int) animation];

            Bf.Animations.Lerp = 0.0f;
            frame %= anim.Frames.Count;
            frame = frame >= 0 ? frame : anim.Frames.Count - 1 + frame;
            Bf.Animations.Period = 1.0f / Constants.TR_FRAME_RATE;

            Bf.Animations.LastState = anim.StateID;
            Bf.Animations.NextState = anim.StateID;
            Bf.Animations.CurrentAnimation = animation;
            Bf.Animations.CurrentFrame = (short)frame;
            Bf.Animations.NextAnimation = animation;
            Bf.Animations.NextFrame = (short)frame;

            Bf.Animations.FrameTime = frame * Bf.Animations.Period;

            UpdateCurrentBoneFrame(Bf, Transform);
            UpdateRigidBody(false);
        }

        public void MoveForward(float dist)
        {
            Transform.Origin += Transform.Basis.Column1 * dist;
        }

        public void MoveStrafe(float dist)
        {
            Transform.Origin += Transform.Basis.Column0 * dist;
        }

        public void MoveVertical(float dist)
        {
            Transform.Origin += Transform.Basis.Column2 * dist;
        }

        public float FindDistance(Entity entity2)
        {
            return (Transform.Origin - entity2.Transform.Origin).Length;
        }

        /// <summary>
        /// Constantly updates some specific parameterd to keep hair aligned to entity
        /// </summary>
        public virtual void UpdateHair()
        {
        }

        public bool CreateRagdoll(RDSetup setup);

        public bool DeleteRagdoll()
        {
            if (Bt.BtJoints.Count == 0)
                return false;

            for (var i = 0; i < Bt.BtJoints.Count; i++)
            {
                if(Bt.BtJoints[i] != null)
                {
                    Global.BtEngineDynamicsWorld.RemoveConstraint(Bt.BtJoints[i]);
                    Bt.BtJoints[i] = null;
                }
            }

            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                Global.BtEngineDynamicsWorld.RemoveRigidBody(Bt.BtBody[i]);
                Bt.BtBody[i].SetMassProps(0, Vector3.Zero);
                Global.BtEngineDynamicsWorld.AddRigidBody(Bt.BtBody[i], CollisionFilterGroups.KinematicFilter, CollisionFilterGroups.AllFilter);
                if(i < Bt.GhostObjects.Count && Bt.GhostObjects[i] != null)
                {
                    Global.BtEngineDynamicsWorld.RemoveCollisionObject(Bt.GhostObjects[i]);
                    Global.BtEngineDynamicsWorld.AddCollisionObject(Bt.GhostObjects[i], CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.AllFilter);
                }
            }

            Bt.BtJoints.Clear();

            TypeFlags &= ~ENTITY_TYPE.Dynamic;

            return true;
        }

        public virtual void FixPenetrations(Vector3 move)
        {
            if (Bt.GhostObjects.Count == 0)
                return;

            if(TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                return;
            }

            if(Bt.NoFixAll)
            {
                GhostUpdate();
                return;
            }

            Vector3 reaction;
            GetPenetrationFixVector(out reaction, move != Vector3.Zero);
            Transform.Origin += reaction;

            GhostUpdate();
        }

        public virtual Vector3 GetRoomPos()
        {
            return Transform * ((Bf.BBMin + Bf.BBMax) / 2);
        }

        public virtual void TransferToRoom(Room room)
        {
            if(Self.Room != null && !Self.Room.IsOverlapped(room))
            {
                Self.Room.RemoveEntity(this);
                room?.AddEntity(this);
            }
        }

        public virtual void FrameImpl(float time, short frame, ENTITY_ANIM state)
        {
            Bf.Animations.CurrentFrame = frame;
        }

        public virtual void ProcessSectorImpl()
        {
        }

        public virtual void Jump(float vert, float hor)
        {
        }

        public virtual void Kill()
        {
        }

        public virtual void UpdateGhostRigidBody()
        {
        }

        public virtual BtEngineClosestConvexResultCallback CallbackForCamera()
        {
            var cb = new BtEngineClosestConvexResultCallback(Self);
            cb.CollisionFilterMask = CollisionFilterGroups.StaticFilter | CollisionFilterGroups.KinematicFilter;
            return cb;
        }

        public virtual Vector3 CamPosForFollowing(float dz)
        {
            var camPos = Transform * Bf.BoneTags[0].FullTransform.Origin;
            camPos[2] += dz;
            return camPos;
        }
        
        public virtual void UpdatePlatformPreStep()
        {
        }

        public Vector3 ApplyGravity(float time)
        {
            var gravityAcceleration = Global.BtEngineDynamicsWorld.Gravity;
            var gravitySpeed = gravityAcceleration * time;
            var move = (Speed + gravitySpeed / 2) * time;
            Speed += gravitySpeed;
            return move;
        }

        private void doAnimMove(ref TR_ANIMATION anim, ref short frame)
        {
            if(Bf.Animations.Model != null)
            {
                var currAf = Bf.Animations.Model.Animations[(int) Bf.Animations.CurrentAnimation];
                var currBf = currAf.Frames[Bf.Animations.CurrentFrame];

                if(currBf.Command.HasFlagUns(ANIM_CMD.Jump))
                {
                    Jump(-currBf.V_Vertical, currBf.V_Horizontal);
                }
                if(currBf.Command.HasFlagUns(ANIM_CMD.ChangeDirection))
                {
                    Angles.X += 180.0f;
                    if(MoveType == MoveType.Underwater)
                    {
                        Angles.Y = -Angles.Y; // for underwater case
                    }
                    if(DirFlag == ENT_MOVE.MoveBackward)
                    {
                        DirFlag = ENT_MOVE.MoveForward;
                    }
                    else if(DirFlag == ENT_MOVE.MoveForward)
                    {
                        DirFlag = ENT_MOVE.MoveBackward;
                    }
                    UpdateTransform();
                    SetAnimation(currAf.NextAnim.ID, currAf.NextFrame);
                    anim = Bf.Animations.CurrentAnimation;
                    frame = Bf.Animations.CurrentFrame;
                }
                if(currBf.Command.HasFlagUns(ANIM_CMD.Move))
                {
                    Transform.Origin += Transform.Basis.MultiplyByVector(currBf.Move);
                }
            }
        }

        private static float GetInnerBBRadius(Vector3 bbMin, Vector3 bbMax)
        {
            var d = bbMax - bbMin;
            return Math.Min(d.X, Math.Min(d.Y, d.Z));
        }
    }

    public partial class StaticFuncs
    {
        /// <summary>
        /// It is from bullet_character_controller
        /// </summary>
        public static int GhostGetPenetrationFixVector(PairCachingGhostObject ghost, AlignedManifoldArray manifoldArray,
            out Vector3 correction)
        {
            // Here we must refresh the overlapping paircache as the penetrating movement itself or the
            // previous recovery iteration might have used setWorldTransform and pushed us into an object
            // that is not in the previous cache contents from the last timestep, as will happen if we
            // are pushed into a new AABB overlap. Unhandled this means the next convex sweep gets stuck.
            //
            // Do this by calling the broadphase's setAabb with the moved AABB, this will update the broadphase
            // paircache and the ghostobject's internal paircache at the same time.    /BW

            var ret = 0;
            var pairArray = ghost.OverlappingPairCache.OverlappingPairArray;
            Vector3 aabb_min, aabb_max, t;

            ghost.CollisionShape.GetAabb(ghost.WorldTransform, out aabb_min, out aabb_max);
            Global.BtEngineDynamicsWorld.Broadphase.SetAabb(ghost.BroadphaseHandle, aabb_min, aabb_max,
                Global.BtEngineDynamicsWorld.Dispatcher);
            Global.BtEngineDynamicsWorld.Dispatcher.DispatchAllCollisionPairs(ghost.OverlappingPairCache,
                Global.BtEngineDynamicsWorld.DispatchInfo, Global.BtEngineDynamicsWorld.Dispatcher);

            correction = Vector3.Zero;
            var numPairs = ghost.OverlappingPairCache.NumOverlappingPairs;
            for (var i = 0; i < numPairs; i++)
            {
                manifoldArray.Clear();
                // do not use commented code: it prevents to collision skips.
                //var pair = pairArray[i];
                //var collisionPair = Global.BtEngineDynamicsWorld.PairCache.FindPair(pair.Proxy0, pair.Proxy1);
                var collisionPair = pairArray[i];

                if(collisionPair == null)
                {
                    continue;
                }

                collisionPair.Algorithm?.GetAllContactManifolds(manifoldArray);

                foreach (var manifold in manifoldArray)
                {
                    var directionSign = manifold.Body0 == ghost ? -1.0f : 1.0f;
                    var cont0 = (EngineContainer) manifold.Body0.UserObject;
                    var cont1 = (EngineContainer) manifold.Body1.UserObject;
                    if(cont0.CollisionType == COLLISION_TYPE.Ghost && cont1.CollisionType == COLLISION_TYPE.Ghost)
                    {
                        continue;
                    }
                    for(var k = 0; k < manifold.NumContacts; k++)
                    {
                        var pt = manifold.GetContactPoint(k);
                        var dist = pt.Distance;

                        if(dist < 0.0f)
                        {
                            t = pt.NormalWorldOnB * dist * directionSign;
                            correction += t;
                            ret++;
                        }
                    }
                }
            }

            return ret;
        }

        public static StateChange Anim_FindStateChangeByAnim(AnimationFrame anim, TR_ANIMATION stateChangeAnim)
        {
            return stateChangeAnim >= 0 ? anim.StateChange.FirstOrDefault(x => x.AnimDispatch.Any(y => y.NextAnim == stateChangeAnim)) : null;
        }

        public static StateChange Anim_FindStateChangeByID(AnimationFrame anim, uint id)
        {
            return anim.StateChange.FirstOrDefault(x => x.ID == id);
        }
    }
}


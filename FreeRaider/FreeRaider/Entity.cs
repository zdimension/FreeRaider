using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using NLua;
using NLua.Exceptions;
using OpenTK;
using static FreeRaider.Constants;
using static FreeRaider.Global;
using static FreeRaider.StaticFuncs;

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

    [Flags]
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
        public List<CollisionObject> Obj = new List<CollisionObject>();
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

    public class Entity : IDisposable
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

        public float SpeedMult = TR_FRAME_RATE;

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

            Transform = new Transform();
            Transform.SetIdentity();
            Self.Object = this;
            Self.ObjectType = OBJECT_TYPE.Entity;
            Self.Room = null;
            Self.CollisionType = COLLISION_TYPE.None;
            OBB = new OBB();
            OBB.Transform = Transform;
            Bt = new BtEntityData();
            Bt.BtBody = new List<RigidBody>();
            Bt.BtJoints = new List<TypedConstraint>();
            Bt.NoFixAll = false;
            Bt.NoFixBodyParts = 0x0000000;
            Bt.ManifoldArray = null;
            Bt.Shapes = new List<CollisionShape>();
            Bt.GhostObjects = new List<PairCachingGhostObject>();
            Bt.LastCollisions = new List<EntityCollisionNode>();

            Bf = new SSBoneFrame();
            Bf.Animations = new SSAnimation();
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
            Bf.BoneTags = new List<SSBoneTag>();
            Bf.BBMax = Vector3.Zero;
            Bf.BBMin = Vector3.Zero;
            Bf.Centre = Vector3.Zero;
            Bf.Position = Vector3.Zero;
            Speed = Vector3.Zero;
        }

        public void Dispose()
        {
            Bt.LastCollisions.Clear();

            if (Bt.BtJoints.Count > 0)
            {
                DeleteRagdoll();
            }

            foreach (var ghost in Bt.GhostObjects)
            {
                ghost.UserObject = null;
                BtEngineDynamicsWorld?.RemoveCollisionObject(ghost);
            }
            Bt.GhostObjects.Clear();

            Bt.Shapes.Clear();

            Bt.ManifoldArray?.Clear();

            if (Bt.BtBody.Count > 0)
            {
                foreach (var body in Bt.BtBody)
                {
                    if (body != null)
                    {
                        body.UserObject = null;
                        if (body.MotionState != null)
                        {
                            body.MotionState.Dispose();
                            body.MotionState = null;
                        }
                        body.CollisionShape = null;

                        BtEngineDynamicsWorld.RemoveRigidBody(body);
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
                var box = COLLISION_GHOST_VOLUME_COEFFICIENT *
                          (Bf.BoneTags[i].MeshBase.BBMax - Bf.BoneTags[i].MeshBase.BBMin);
                Bt.Shapes.Add(new BoxShape(box.ToBullet()));
                Bt.Shapes.Last().Margin = COLLISION_MARGIN_DEFAULT;
                Bf.BoneTags[i].MeshBase.Radius = Math.Min(Math.Min(box.X, box.Y), box.Z);

                var pcg = new PairCachingGhostObject();

                pcg.SetIgnoreCollisionCheck(Bt.BtBody[i], true);

                var gltr = Transform * Bf.BoneTags[i].FullTransform;
                gltr.Origin = gltr * Bf.BoneTags[i].MeshBase.Center;

                pcg.WorldTransform = ((Matrix4)gltr).ToBullet();
                pcg.CollisionFlags |= CollisionFlags.NoContactResponse | CollisionFlags.CharacterObject;
                pcg.UserObject = Self;
                pcg.CollisionShape = Bt.Shapes.Last();
                BtEngineDynamicsWorld.AddCollisionObject(pcg, CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.AllFilter);
                Bt.GhostObjects.Add(pcg);

                Bt.LastCollisions.Add(new EntityCollisionNode());
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
                    BtEngineDynamicsWorld.AddRigidBody(b);
                }
            }
        }

        public void DisableCollision()
        {
            foreach (var b in Bt.BtBody)
            {
                if (b != null && b.IsInWorld)
                {
                    BtEngineDynamicsWorld.RemoveRigidBody(b);
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
                        cshape = BT_CSfromSphere(mesh.Radius);
                        break;

                    case COLLISION_SHAPE.TrimeshConvex:
                        cshape = BT_CSfromMesh(mesh, true, true, false);
                        break;

                    case COLLISION_SHAPE.Trimesh:
                        cshape = BT_CSfromMesh(mesh, true, true, true);
                        break;

                    case COLLISION_SHAPE.Box:
                    default:
                        cshape = BT_CSfromBBox(mesh.BBMin, mesh.BBMax, true, true);
                        break;
                }

                Bt.BtBody.Add(null);

                if(cshape != null)
                {
                    var localInertia = BulletSharp.Math.Vector3.Zero;
                    if (Self.CollisionShape != COLLISION_SHAPE.Trimesh)
                        cshape.CalculateLocalInertia(0.0f, out localInertia);

                    var startTransform = Transform * Bf.BoneTags[i].FullTransform;
                    var motionState = new DefaultMotionState(((Matrix4)startTransform).ToBullet());
                    Bt.BtBody[Bt.BtBody.Count - 1] = new RigidBody(new RigidBodyConstructionInfo(0.0f, motionState, cshape, localInertia));

                    CollisionFlags cf = CollisionFlags.None;
                    switch (Self.CollisionType)
                    {
                        case COLLISION_TYPE.Kinematic:
                            Bt.BtBody[Bt.BtBody.Count - 1].CollisionFlags |= CollisionFlags.KinematicObject;
                            break;

                        case COLLISION_TYPE.Ghost:
                            Bt.BtBody[Bt.BtBody.Count - 1].CollisionFlags |= CollisionFlags.NoContactResponse;
                            break;

                        case COLLISION_TYPE.Actor:
                        case COLLISION_TYPE.Vehicle:
                            Bt.BtBody[Bt.BtBody.Count - 1].CollisionFlags |= CollisionFlags.CharacterObject;
                            break;

                        case COLLISION_TYPE.Static:
                        default:
                            Bt.BtBody[Bt.BtBody.Count - 1].CollisionFlags |= CollisionFlags.StaticObject;
                            break;
                    }

                    BtEngineDynamicsWorld.AddRigidBody(Bt.BtBody[i], CollisionFilterGroups.KinematicFilter, CollisionFilterGroups.AllFilter);
                    Bt.BtBody.Last().UserObject = Self;
                }
            }
        }

        public void GhostUpdate()
        {
            if (Bt.GhostObjects.Count == 0)
                return;

            Assert(Bt.GhostObjects.Count == Bf.BoneTags.Count);

            if(TypeFlags.HasFlag(ENTITY_TYPE.Dynamic))
            {
                for(var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    var tr = Transform * Bf.BoneTags[i].FullTransform;
                    var v = Bf.Animations.Model.MeshTree[i].MeshBase.Center;
                    var pos = tr * v;
                    tr.Origin = pos;
                    Bt.GhostObjects[i].WorldTransform = ((Matrix4)tr).ToBullet();
                }
            }
            else
            {
                for (var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    var tr = (Transform)Bt.BtBody[i].WorldTransform.ToOpenTK();
                    tr.Origin = Bf.Animations.Model.MeshTree[i].MeshBase.Center;
                    Bt.GhostObjects[i].WorldTransform = ((Matrix4)tr).ToBullet();
                }
            }
        }

        public void UpdateCurrentCollisions()
        {
            if (Bt.GhostObjects.Count == 0)
                return;

            Assert(Bt.GhostObjects.Count == Bf.BoneTags.Count);

            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                var ghost = Bt.GhostObjects[i];
                var cn = Bt.LastCollisions[i];

                cn.Obj.Clear();
                var tr = Transform * Bf.BoneTags[i].FullTransform;
                var v = Bf.Animations.Model.MeshTree[i].MeshBase.Center;
                var origTr = ghost.WorldTransform;
                var _wt = tr.Clone();
                var pos = tr * v;
                _wt.Origin = pos;
                ghost.WorldTransform = ((Matrix4) _wt).ToBullet();

                var pairArray = ghost.OverlappingPairCache.OverlappingPairArray;
                BulletSharp.Math.Vector3 aabb_min, aabb_max;

                ghost.CollisionShape.GetAabb(ghost.WorldTransform, out aabb_min, out aabb_max);
                BtEngineDynamicsWorld.Broadphase.SetAabb(ghost.BroadphaseHandle, aabb_min, aabb_max, BtEngineDynamicsWorld.Dispatcher);
                BtEngineDynamicsWorld.Dispatcher.DispatchAllCollisionPairs(ghost.OverlappingPairCache,
                    BtEngineDynamicsWorld.DispatchInfo, BtEngineDynamicsWorld.Dispatcher);

                var numPairs = ghost.OverlappingPairCache.NumOverlappingPairs;
                for(var j = 0; j < numPairs; j++)
                {
                    Bt.ManifoldArray.Clear();
                    var collisionPair = pairArray[j];

                    collisionPair?.Algorithm?.GetAllContactManifolds(Bt.ManifoldArray);

                    foreach (var manifold in Bt.ManifoldArray)
                    {
                        for(var c = 0; c < manifold.NumContacts; c++)
                        {
                            if(manifold.GetContactPoint(c).Distance < 0.0f)
                            {
                                var u = manifold.Body0;
                                if(Self == u.UserObject)
                                {
                                    u = manifold.Body1;
                                }
                                cn.Obj.Add(u);
                                break;
                            }
                        }
                    }
                }
                ghost.WorldTransform = origTr;
            }
        }

        public int GetPenetrationFixVector(out Vector3 reaction, bool hasMove)
        {
            reaction = Vector3.Zero;
            if (Bt.GhostObjects.Count == 0 || Bt.NoFixAll)
                return 0;

            Assert(Bt.GhostObjects.Count == Bf.BoneTags.Count);

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
                    from = ((Transform) Bt.GhostObjects[m].WorldTransform.ToOpenTK()).Origin;
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
                    Bt.GhostObjects[m].WorldTransform = ((Matrix4) trCurrent).ToBullet();
                    Vector3 tmp;
                    if(GhostGetPenetrationFixVector(Bt.GhostObjects[m], Bt.ManifoldArray, out tmp).ToBool())
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
                        EngineLua.ExecEntity((int)ENTITY_CALLBACK.Collision, (int)activator.ID, (int)ID);
                    }
                }
                else if(CallbackFlags.HasFlag(ENTITY_CALLBACK.RoomCollision) && type == OBJECT_TYPE.RoomBase)
                {
                    var activator = (Room) cont.Object;
                    EngineLua.ExecEntity((int)ENTITY_CALLBACK.RoomCollision, (int)ID, (int)activator.ID);
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
                Transform = (Transform)Bt.BtBody[0].WorldTransform.ToOpenTK();
                UpdateRoomPos();
                for(var i = 0; i < Bf.BoneTags.Count; i++)
                {
                    Bf.BoneTags[i].FullTransform = Transform.Inverse * Bt.BtBody[i].WorldTransform.ToOpenTK();
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
                            Bt.BtBody[i].WorldTransform = ((Matrix4)(Transform * Bf.BoneTags[i].FullTransform)).ToBullet();
                        }
                    }
                }
            }
            RebuildBV();
        }

        /// <summary>
        /// The function rebuild / renew entity's BV
        /// </summary>
        public void RebuildBV()
        {
            if(Bf.Animations.Model != null)
            {
                // get current BB from animation
                OBB.Rebuild(Bf.BBMin, Bf.BBMax);
                OBB.DoTransform();
            }
        }

        public int GetAnimDispatchCase(TR_STATE id)
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
        public ENTITY_ANIM Frame(float time)
        {
            short frame;
            TR_ANIMATION anim;
            var ret = ENTITY_ANIM.None;

            if (TypeFlags.HasFlag(ENTITY_TYPE.Dynamic) || !Active || !Enabled || Bf.Animations.Model == null ||
                (Bf.Animations.Model.Animations.Count == 1 && Bf.Animations.Model.Animations[0].Frames.Count == 1))
            {
                return ENTITY_ANIM.None;
            }

            if(Bf.Animations.AnimFlags.HasFlag(AnimControlFlags.Lock)) return ENTITY_ANIM.NewFrame; // penetration fix will be applyed in Character.Move... functions

            var ssAnim = Bf.Animations;

            GhostUpdate();

            Bf.Animations.Lerp = 0.0f;
            var stc = Anim_FindStateChangeByID(Bf.Animations.Model.Animations[(int) Bf.Animations.CurrentAnimation],
                Bf.Animations.NextState);
            GetNextFrame(Bf, time, stc, out frame, out anim, Bf.Animations.AnimFlags);
            if(Bf.Animations.CurrentAnimation != anim)
            {
                Bf.Animations.LastAnimation = Bf.Animations.CurrentAnimation;

                ret = ENTITY_ANIM.NewAnim;
                DoAnimCommands(Bf.Animations, (int)ret);
                doAnimMove(ref anim, ref frame);

                SetAnimation(anim, frame);
                stc = Anim_FindStateChangeByID(Bf.Animations.Model.Animations[(int) Bf.Animations.CurrentAnimation],
                    Bf.Animations.NextState);
            }
            else if(Bf.Animations.CurrentFrame != frame)
            {
                if(Bf.Animations.CurrentFrame == 0)
                {
                    Bf.Animations.LastAnimation = Bf.Animations.CurrentAnimation;
                }

                ret = ENTITY_ANIM.NewFrame;
                DoAnimCommands(Bf.Animations, (int)ret);
                doAnimMove(ref anim, ref frame);
            }

            Bf.Animations.FrameTime = time;

            var t = (long)(Bf.Animations.FrameTime / Bf.Animations.Period);
            var dt = Bf.Animations.FrameTime - t * Bf.Animations.Period;
            Bf.Animations.FrameTime = frame * Bf.Animations.Period + dt;
            Bf.Animations.Lerp = dt / Bf.Animations.Period;
            GetNextFrame(Bf, Bf.Animations.Period, stc, out Bf.Animations.NextFrame, out Bf.Animations.NextAnimation, ssAnim.AnimFlags);

            FrameImpl(time, frame, ret);

            UpdateCurrentBoneFrame(Bf, Transform);
            FixPenetrations(Vector3.Zero);

            return ret;
        }

        public virtual void UpdateTransform()
        {
            Angles = Angles.WrapAngle();

            Helper.SetEulerZYX(ref Transform.Basis, Angles.Y * RadPerDeg, Angles.Z * RadPerDeg,
                Angles.X * RadPerDeg);

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
            var sm = EngineWorld.GetModelByID((uint)modelID);

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
                ssAnim.Period = 1.0f / TR_FRAME_RATE;
            }
        }

        public void CheckActivators()
        {
            if (Self.Room == null)
                return;

            var ppos = Transform.Origin + Transform.Basis.Column1 * Bf.BBMax.Y;
            foreach (var cont in Self.Room.Containers)
            {
                if (cont.ObjectType != OBJECT_TYPE.Entity || cont.Object == null)
                    continue;

                var e = (Entity) cont.Object;
                if (!e.Enabled)
                    continue;

                if(e.TypeFlags.HasFlag(ENTITY_TYPE.Interactive))
                {
                    if(e != this && OBB.OBB_Test(e, this) == 1)
                    {
                        EngineLua.ExecEntity((int)ENTITY_CALLBACK.Activate, (int)e.ID, (int)ID);
                    }
                }
                else if (e.TypeFlags.HasFlag(ENTITY_TYPE.Pickable))
                {
                    var r = e.ActivationRadius;
                    r *= r;
                    var v = Transform.Origin;
                    if (e != this
                        && ((v.X - ppos.X) * (v.X - ppos.X) + (v.Y - ppos.Y) * (v.Y - ppos.Y) < r)
                        && (v.Z + 32.0 > Transform.Origin.Z + Bf.BBMin.Z)
                        && (v.Z - 32.0 < Transform.Origin.Z + Bf.BBMax.Z))
                    {
                        EngineLua.ExecEntity((int)ENTITY_CALLBACK.Activate, (int)e.ID, (int)ID);
                    }
                }
            }
        }

        public virtual Substance GetSubstanceState()
        {
            return Substance.None;
        }

        public uint SubstanceStateUInt32 => (uint) GetSubstanceState();

        public static void UpdateCurrentBoneFrame(SSBoneFrame bf, Transform etr)
        {
            var btag = bf.BoneTags[0];
            var model = bf.Animations.Model;

            var nextBf = model.Animations[(int) bf.Animations.NextAnimation].Frames[bf.Animations.NextFrame];
            var currBf = model.Animations[(int) bf.Animations.CurrentAnimation].Frames[bf.Animations.CurrentFrame];

            var tr = Vector3.Zero;
            var cmd_tr = Vector3.Zero;
            if (etr != null && currBf.Command.HasFlagUns(ANIM_CMD.Move))
            {
                tr = etr.Basis.MultiplyByVector(currBf.Move);
                cmd_tr = tr * bf.Animations.Lerp;
            }

            bf.BBMax = currBf.BBMax.Lerp(nextBf.BBMax, bf.Animations.Lerp) + cmd_tr;
            bf.BBMin = currBf.BBMin.Lerp(nextBf.BBMin, bf.Animations.Lerp) + cmd_tr;
            bf.Centre = currBf.Centre.Lerp(nextBf.Centre, bf.Animations.Lerp) + cmd_tr;
            bf.Position = currBf.Position.Lerp(nextBf.Position, bf.Animations.Lerp) + cmd_tr;

            var next_btag = nextBf.BoneTags[0];
            var src_btag = currBf.BoneTags[0];
            for (var k = 0;
                k < currBf.BoneTags.Count;)
            {
                btag.Offset = src_btag.Offset.Lerp(next_btag.Offset, bf.Animations.Lerp);
                btag.Transform.Origin = btag.Offset;
                btag.Transform.Origin.Z = 1.0f;
                if(k == 0)
                {
                    btag.Transform.Origin += bf.Position;
                    btag.QRotate = Quaternion.Slerp(src_btag.QRotate, next_btag.QRotate, bf.Animations.Lerp);
                }
                else
                {
                    var ov_src_btag = src_btag;
                    var ov_next_btag = next_btag;
                    var ov_lerp = bf.Animations.Lerp;
                    for (var ov_anim = bf.Animations.Next; ov_anim != null; ov_anim = ov_anim.Next)
                    {
                        if (ov_anim.Model != null && ov_anim.Model.MeshTree[k].ReplaceAnim != 0)
                        {
                            var ov_curr_bf =
                                ov_anim.Model.Animations[(int) ov_anim.CurrentAnimation].Frames[ov_anim.CurrentFrame];
                            var ov_next_bf =
                                ov_anim.Model.Animations[(int) ov_anim.NextAnimation].Frames[ov_anim.NextFrame];
                            ov_src_btag = ov_curr_bf.BoneTags[k];
                            ov_next_btag = ov_next_bf.BoneTags[k];
                            ov_lerp = ov_anim.Lerp;
                            break;
                        }
                    }
                    btag.QRotate = Quaternion.Slerp(ov_src_btag.QRotate, ov_next_btag.QRotate, ov_lerp);
                }
                btag.Transform.Rotation = btag.QRotate;
                k++;
                if (k < bf.BoneTags.Count)
                {
                    btag = bf.BoneTags[k];
                    src_btag = currBf.BoneTags[k];
                    next_btag = nextBf.BoneTags[k];
                }
            }

            // build absolute coordinate matrix system
            var btagI = 0;
            btag = bf.BoneTags[btagI];
            btag.FullTransform = btag.Transform;
            var incBtag = new Action(() =>
            {
                btagI++;
                btag = btagI < bf.BoneTags.Count ? bf.BoneTags[btagI] : null;
            });
            for(var k = 1; k < currBf.BoneTags.Count; k++, incBtag())
            {
                btag.FullTransform = (btag.Parent?.FullTransform ?? new Transform()) * btag.Transform;
            }
        }

        public void DoAnimCommands(SSAnimation ssAnim, int changing)
        {
            if (EngineWorld.AnimCommands.Length == 0 || ssAnim.Model == null)
            {
                return; // If no anim commands
            }

            var af = ssAnim.Model.Animations[(int) ssAnim.CurrentAnimation];
            if (af.NumAnimCommands.IsBetween(0, 255, IB.aEbI))
            {
                Assert(af.AnimCommand < EngineWorld.AnimCommands.Length);

                unsafe
                {
                    fixed (short* tmp = &EngineWorld.AnimCommands[(int)af.AnimCommand])
                    {
                        fixed (short* back = &EngineWorld.AnimCommands[EngineWorld.AnimCommands.Length - 1])
                        {
                            var pointer = tmp;
                            for (uint count = 0; count < af.NumAnimCommands; count++)
                            {
                                Assert(pointer <= back);
                                var command = *pointer;
                                ++pointer;
                                switch ((TR_ANIMCOMMAND) command)
                                {
                                    case TR_ANIMCOMMAND.SetPosition:
                                        // This command executes ONLY at the end of animation.
                                        pointer += 3; // Parse through 3 operands.
                                        break;

                                    case TR_ANIMCOMMAND.JumpDistance:
                                        // This command executes ONLY at the end of animation.
                                        pointer += 2; // Parse through 2 operands.
                                        break;

                                    case TR_ANIMCOMMAND.EmptyHands:
                                        // FIXME: Behaviour is yet to be discovered.
                                        break;

                                    case TR_ANIMCOMMAND.Kill:
                                        // This command executes ONLY at the end of animation.
                                        if (ssAnim.CurrentFrame == af.Frames.Count - 1)
                                        {
                                            Kill();
                                        }

                                        break;

                                    case TR_ANIMCOMMAND.PlaySound:
                                        if (ssAnim.CurrentFrame == pointer[0])
                                        {
                                            var soundIndex = pointer[1] & 0x3FFF;

                                            // Quick workaround for TR3 quicksand.
                                            if (GetSubstanceState().IsAnyOf(Substance.QuicksandConsumed, Substance.QuicksandShallow))
                                            {
                                                soundIndex = 18;
                                            }

                                            if (pointer[1].HasFlagSig(TR_ANIMCOMMAND_CONDITION.Water))
                                            {
                                                if (GetSubstanceState() == Substance.WaterShallow)
                                                    Audio.Send((uint)soundIndex, TR_AUDIO_EMITTER.Entity, (int)ID);
                                            }
                                            else if (pointer[1].HasFlagSig(TR_ANIMCOMMAND_CONDITION.Land))
                                            {
                                                if (GetSubstanceState() != Substance.WaterShallow)
                                                    Audio.Send((uint)soundIndex, TR_AUDIO_EMITTER.Entity, (int)ID);
                                            }
                                            else
                                            {
                                                Audio.Send((uint)soundIndex, TR_AUDIO_EMITTER.Entity, (int)ID);
                                            }
                                        }
                                        pointer += 2;
                                        break;

                                    case TR_ANIMCOMMAND.PlayEffect:
                                        if (ssAnim.CurrentFrame == pointer[0])
                                        {
                                            var effectID = pointer[1] & 0x3FFF;
                                            if (effectID > 0)
                                                EngineLua.ExecEffect(effectID, (int)ID);
                                        }
                                        pointer += 2;
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ProcessSector()
        {
            if (CurrentSector == null) return;

            // Calculate both above and below sectors for further usage.
            // Sector below is generally needed for getting proper trigger index,
            // as many triggers tend to be called from the lowest room in a row
            // (e.g. first trapdoor in The Great Wall, etc.)
            // Sector above primarily needed for paranoid cases of monkeyswing.

            var lowestSector = CurrentSector.GetLowestSector();
            Assert(lowestSector != null);

            ProcessSectorImpl();

            // If entity either marked as trigger activator (Lara) or heavytrigger activator (other entities),
            // we try to execute a trigger for this sector.

            if(TypeFlags.HasFlag(ENTITY_TYPE.TriggerActivator) || TypeFlags.HasFlag(ENTITY_TYPE.HeavyTriggerActivator))
            {
                // Look up trigger function table and run trigger if it exists.
                try
                {
                    (EngineLua["tlist_RunTrigger"] as LuaFunction)?.Call(lowestSector.TrigIndex, Bf.Animations.Model.ID == 0 ? ActivatorType.Lara : ActivatorType.Misc, ID); // TODO: Maybe cast to int before calling?
                }
                catch(LuaException e)
                {
                    Sys.DebugLog(LUA_LOG_FILENAME, "{0}", e.Message);
                }
            }
        }

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
                var model = EngineWorld.GetModelByID((uint) anotherModel);
                if (model == null || (int) animation >= model.Animations.Count)
                    return;
                Bf.Animations.Model = model;
            }

            var anim = Bf.Animations.Model.Animations[(int) animation];

            Bf.Animations.Lerp = 0.0f;
            frame %= anim.Frames.Count;
            frame = frame >= 0 ? frame : anim.Frames.Count - 1 + frame;
            Bf.Animations.Period = 1.0f / TR_FRAME_RATE;

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

        public bool CreateRagdoll(RDSetup setup)
        {
            // No entity, setup or body count overflow - bypass function.

            if (setup == null || setup.BodySetup.Count > Bf.BoneTags.Count)
            {
                return false;
            }

            var result = true;

            // If ragdoll already exists, overwrite it with new one.

            if (Bt.BtJoints.Count > 0)
            {
                result = DeleteRagdoll();
            }

            // Setup bodies.
            Bt.BtJoints.Clear();
            // update current character animation and full fix body to avoid starting ragdoll partially inside the wall or floor...
            UpdateCurrentBoneFrame(Bf, Transform);
            Bt.NoFixAll = false;
            Bt.NoFixBodyParts = 0x00000000;
#if NOPE
            int map_size = m_bf.animations.model->collision_map.size();             // does not works, strange...
            m_bf.animations.model->collision_map.size() = m_bf.animations.model->mesh_count;
            fixPenetrations(nullptr);
            m_bf.animations.model->collision_map.size() = map_size;
#else
            FixPenetrations(Vector3.Zero);
#endif

            for(var i = 0; i < setup.BodySetup.Count; i++)
            {
                // TODO: First check useless?
                if(i >= Bf.BoneTags.Count || Bt.BtBody[i] == null)
                {
                    result = false;
                    continue; // If body is absent, return false and bypass this body setup.
                }

                var inertia = BulletSharp.Math.Vector3.Zero;
                var mass = setup.BodySetup[i].Mass;

                BtEngineDynamicsWorld.RemoveRigidBody(Bt.BtBody[i]);

                Bt.BtBody[i].CollisionShape.CalculateLocalInertia(mass, out inertia);
                Bt.BtBody[i].SetMassProps(mass, inertia);

                Bt.BtBody[i].UpdateInertiaTensor();
                Bt.BtBody[i].ClearForces();

                Bt.BtBody[i].LinearFactor = BulletSharp.Math.Vector3.One;
                Bt.BtBody[i].AngularFactor = BulletSharp.Math.Vector3.One;

                Bt.BtBody[i].SetDamping(setup.BodySetup[i].Damping[0], setup.BodySetup[i].Damping[1]);
                Bt.BtBody[i].Restitution = setup.BodySetup[i].Restitution;
                Bt.BtBody[i].Friction = setup.BodySetup[i].Friction;

                Bt.BtBody[i].SetSleepingThresholds(RD_DEFAULT_SLEEPING_THRESHOLD, RD_DEFAULT_SLEEPING_THRESHOLD);

                if(Bf.BoneTags[i].Parent == null)
                {
                    var r = GetInnerBBRadius(Bf.BoneTags[i].MeshBase.BBMin, Bf.BoneTags[i].MeshBase.BBMax);
                    Bt.BtBody[i].CcdMotionThreshold = 0.8f * r;
                    Bt.BtBody[i].CcdSweptSphereRadius = r;
                }
            }

            UpdateRigidBody(true);
            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                BtEngineDynamicsWorld.AddRigidBody(Bt.BtBody[i]);
                Bt.BtBody[i].Activate();
                Bt.BtBody[i].LinearVelocity = Speed.ToBullet();
                if (i < Bt.GhostObjects.Count && Bt.GhostObjects[i] != null)
                {
                    BtEngineDynamicsWorld.RemoveCollisionObject(Bt.GhostObjects[i]);
                    BtEngineDynamicsWorld.AddCollisionObject(Bt.GhostObjects[i], CollisionFilterGroups.None, CollisionFilterGroups.None);
                }
            }

            // Setup constraints.
            Bt.BtJoints.Resize(setup.JointSetup.Count);

            for(var i = 0; i < setup.JointSetup.Count; i++)
            {
                if(setup.JointSetup[i].BodyIndex >= Bf.BoneTags.Count || Bt.BtBody[setup.JointSetup[i].BodyIndex] == null)
                {
                    result = false;
                    break; // If body 1 or body 2 are absent, return false and bypass this joint.
                }

                var localA = new Transform();
                var localB = new Transform();
                var btB = Bf.BoneTags[setup.JointSetup[i].BodyIndex];
                var btA = btB.Parent;
                if(btA == null)
                {
                    result = false;
                    break;
                }
#if NOPE        
                localA.setFromOpenGLMatrix(btB->transform);
                localB.setIdentity();
#else
                Helper.SetEulerZYX(ref localA.Basis, setup.JointSetup[i].Body1Angle);
                //localA.Origin = setup.JointSetup[i].Body1Offset;
                localA.Origin = btB.Transform.Origin;

                Helper.SetEulerZYX(ref localB.Basis, setup.JointSetup[i].Body2Angle);
                //localB.Origin = setup.JointSetup[i].Body2Offset;
                localB.Origin = Vector3.Zero;
#endif

                switch(setup.JointSetup[i].JointType)
                {
                    case RDJointSetup.Type.Point:
                        Bt.BtJoints[i] = new Point2PointConstraint(Bt.BtBody[btA.Index], Bt.BtBody[btB.Index],
                            localA.Origin.ToBullet(), localB.Origin.ToBullet());
                        break;

                        case RDJointSetup.Type.Hinge:
                        var hingeC = new HingeConstraint(Bt.BtBody[btA.Index], Bt.BtBody[btB.Index], ((Matrix4) localA).ToBullet(),
                            ((Matrix4) localB).ToBullet());
                        hingeC.SetLimit(setup.JointSetup[i].JointLimit[0], setup.JointSetup[i].JointLimit[1], 0.9f, 0.3f, 0.3f);
                        Bt.BtJoints[i] = hingeC;
                        break;

                    case RDJointSetup.Type.Cone:
                        var coneC = new ConeTwistConstraint(Bt.BtBody[btA.Index], Bt.BtBody[btB.Index], ((Matrix4)localA).ToBullet(),
                            ((Matrix4)localB).ToBullet());
                        coneC.SetLimit(setup.JointSetup[i].JointLimit[0], setup.JointSetup[i].JointLimit[1], setup.JointSetup[i].JointLimit[2], 0.9f, 0.3f, 0.7f);
                        Bt.BtJoints[i] = coneC;
                        break;
                }

                Bt.BtJoints[i].SetParam(ConstraintParam.StopCfm, setup.JointCfm, -1);
                Bt.BtJoints[i].SetParam(ConstraintParam.StopErp, setup.JointErp, -1);

                Bt.BtJoints[i].DebugDrawSize = 64.0f;
                BtEngineDynamicsWorld.AddConstraint(Bt.BtJoints[i], true);
            }

            if(!result)
            {
                DeleteRagdoll(); // PARANOID: Clean up the mess, if something went wrong.
            }
            else
            {
                TypeFlags |= ENTITY_TYPE.Dynamic;
            }
            return result;
        }

        public bool DeleteRagdoll()
        {
            if (Bt.BtJoints.Count == 0)
                return false;

            for (var i = 0; i < Bt.BtJoints.Count; i++)
            {
                if(Bt.BtJoints[i] != null)
                {
                    BtEngineDynamicsWorld.RemoveConstraint(Bt.BtJoints[i]);
                    Bt.BtJoints[i] = null;
                }
            }

            for(var i = 0; i < Bf.BoneTags.Count; i++)
            {
                BtEngineDynamicsWorld.RemoveRigidBody(Bt.BtBody[i]);
                Bt.BtBody[i].SetMassProps(0, BulletSharp.Math.Vector3.Zero);
                BtEngineDynamicsWorld.AddRigidBody(Bt.BtBody[i], CollisionFilterGroups.KinematicFilter, CollisionFilterGroups.AllFilter);
                if(i < Bt.GhostObjects.Count && Bt.GhostObjects[i] != null)
                {
                    BtEngineDynamicsWorld.RemoveCollisionObject(Bt.GhostObjects[i]);
                    BtEngineDynamicsWorld.AddCollisionObject(Bt.GhostObjects[i], CollisionFilterGroups.CharacterFilter, CollisionFilterGroups.AllFilter);
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
            cb.CollisionFilterMask = (short) (CollisionFilterGroups.StaticFilter | CollisionFilterGroups.KinematicFilter);
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
            var gravityAcceleration = BtEngineDynamicsWorld.Gravity;
            var gravitySpeed = gravityAcceleration * time;
            var move = (Speed + gravitySpeed.ToOpenTK() / 2) * time;
            Speed += gravitySpeed.ToOpenTK();
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
            BulletSharp.Math.Vector3 aabb_min, aabb_max, t;

            ghost.CollisionShape.GetAabb(ghost.WorldTransform, out aabb_min, out aabb_max);
            BtEngineDynamicsWorld.Broadphase.SetAabb(ghost.BroadphaseHandle, aabb_min, aabb_max,
                BtEngineDynamicsWorld.Dispatcher);
            BtEngineDynamicsWorld.Dispatcher.DispatchAllCollisionPairs(ghost.OverlappingPairCache,
                BtEngineDynamicsWorld.DispatchInfo, BtEngineDynamicsWorld.Dispatcher);

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
                            correction += t.ToOpenTK();
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

        public static StateChange Anim_FindStateChangeByID(AnimationFrame anim, TR_STATE id)
        {
            return anim.StateChange.FirstOrDefault(x => x.ID == id);
        }
    }
}


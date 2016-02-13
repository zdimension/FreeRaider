using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics.ES30;

namespace UniRaider
{
    public partial class Constants
    {
        /// <summary>
        /// Fully opaque object (all polygons are opaque: all t.flags &lt; 0x02)
        /// </summary>
        public const byte MESH_FULL_OPAQUE = 0x00;

        /// <summary>
        /// Fully transparency or has transparency and opaque polygon / object
        /// </summary>
        public const byte MESH_HAS_TRANSPARENCY = 0x01;
    }

    [Flags]
    public enum ANIM_CMD : ushort
    {
        Move = 0x01,
        ChangeDirection = 0x02,
        Jump = 0x04
    }

    public struct TransparentPolygonReference
    {
        public Polygon Polygon;

        public VertexArray UsedVertexArray;

        public uint SizeIndex;

        public uint Count;

        public bool IsAnimated;
    }

    /// <summary>
    /// Animated version of vertex. Does not contain texture coordinate, because that is in a different VBO.
    /// </summary>
    public struct AnimatedVertex
    {
        public Vector3 Position;

        /// <summary>
        /// Length 4
        /// </summary>
        public float[] Color;

        public Vector3 Normal;
    }

    /// <summary>
    /// Base mesh, uses everywhere
    /// </summary>
    public struct BaseMesh
    {
        /// <summary>
        /// Mesh's ID
        /// </summary>
        public uint ID;

        /// <summary>
        /// Does this mesh have prebaked vertex lighting
        /// </summary>
        public bool UsesVertexColors;

        /// <summary>
        /// Polygons data
        /// </summary>
        public List<Polygon> Polygons;

        /// <summary>
        /// Transparency mesh's polygons list
        /// </summary>
        public List<Polygon> TransparencyPolygons;

        /// <summary>
        /// Face without structure wrapping
        /// </summary>
        public uint TexturePageCount;

        public List<uint> ElementsPerTexture;

        public List<uint> Elements;

        public uint AlphaElements;

        public List<Vertex> Vertices;

        public uint AnimatedElementCount;

        public uint AlphaAnimatedElementCount;

        public List<uint> AllAnimatedElements;

        public List<AnimatedVertex> AnimatedVertices;

        public List<TransparentPolygonReference> TransparentPolygons;

        /// <summary>
        /// Geometry centre of mesh
        /// </summary>
        public Vector3 Center;

        /// <summary>
        /// AABB bounding volume
        /// </summary>
        public Vector3 BBMin;

        /// <summary>
        /// AABB bounding volume
        /// </summary>
        public Vector3 BBMax;

        /// <summary>
        /// Radius of the bounding sphere
        /// </summary>
        public float Radius;

        [StructLayout(LayoutKind.Auto, Pack = 1)]
        public struct MatrixIndex
        {
            public sbyte I;

            public sbyte J;
        }

        /// <summary>
        /// Vertices map for skin mesh
        /// </summary>
        public List<MatrixIndex> MatrixIndices;

        public uint VBOVertexArray;

        public uint VBOIndexArray;

        public uint VBOSkinArray;

        public VertexArray MainVertexArray;


        // Buffers for animated polygons
        // The first contains position, normal and color.
        // The second contains the texture coordinates. It gets updated every frame.

        public uint AnimatedVBOVertexArray;

        public uint AnimatedVBOTexCoordArray;

        public uint AnimatedVBOIndexArray;

        public VertexArray AnimatedVertexArray;

        public void Destructor()
        {
            Clear();
        }

        public void Clear();

        public void FindBB();

        public void GenVBO(Render renderer);

        public void GenFaces();

        public uint AddVertex(Vertex v);

        public uint AddAnimatedVertex(Vertex v);

        public void PolySortInMesh();
    }

    /// <summary>
    /// Base sprite structure
    /// </summary>
    public struct Sprite
    {
        /// <summary>
        /// Object's ID
        /// </summary>
        public uint ID;

        /// <summary>
        /// Texture number
        /// </summary>
        public uint Texture;

        /// <summary>
        /// Texture coordinates [Length 8]
        /// </summary>
        public float[] TexCoord;

        public uint Flag;

        // World sprite's gabarites
        public float Left;

        public float Right;

        public float Top;

        public float Bottom;
    }

    /// <summary>
    /// Structure for all the sprites in a room
    /// </summary>
    public struct SpriteBuffer
    {
        /// <summary>
        /// Vertex data for the sprites
        /// </summary>
        public VertexArray Data;

        /// <summary>
        /// How many sub-ranges the element_array_buffer contains. It has one for each texture listed.
        /// </summary>
        public uint NumTexturePages;

        /// <summary>
        /// The element count for each sub-range.
        /// </summary>
        public List<uint> ElementCountPerTexture;
    }

    public struct Light
    {
        /// <summary>
        /// World position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// RGBA value [Length 4]
        /// </summary>
        public float[] Colour;

        public float Inner;

        public float Outer;

        public float Length;

        public float Cutoff;

        public float Falloff;

        public Loader.LightType LightType;
    }

    /// <summary>
    /// Animated sequence. Used globally with animated textures to refer its parameters and frame numbers.
    /// </summary>
    public struct TexFrame
    {
        /// <summary>
        /// Length 4
        /// </summary>
        public float[] Mat;

        /// <summary>
        /// Length 2
        /// </summary>
        public float[] Move;

        public ushort TextureIndex;
    }

    public struct AnimSeq
    {
        /// <summary>
        /// UVRotate mode flag.
        /// </summary>
        public bool UVRotate;

        /// <summary>
        /// Single frame mode. Needed for TR4-5 compatible UVRotate.
        /// </summary>
        public bool FrameLock;

        /// <summary>
        /// Blend flag. Reserved for future use!
        /// </summary>
        public bool Blend;

        /// <summary>
        /// Blend rate. Reserved for future use!
        /// </summary>
        public float BlendRate;

        /// <summary>
        /// Blend value. Reserved for future use!
        /// </summary>
        public float BlendTime;

        /// <summary>
        /// 0 = normal, 1 = back, 2 = reverse.
        /// </summary>
        public sbyte AnimType;

        /// <summary>
        /// Used only with type 2 to identify current animation direction.
        /// </summary>
        public bool ReverseDirection;

        /// <summary>
        /// Time passed since last frame update.
        /// </summary>
        public float FrameTime;

        /// <summary>
        /// Current frame for this sequence.
        /// </summary>
        public ushort CurrentFrame;

        /// <summary>
        /// For types 0-1, specifies framerate, for type 3, should specify rotation speed.
        /// </summary>
        public float FrameRate;

        /// <summary>
        /// Speed of UVRotation, in seconds.
        /// </summary>
        public float UVRotateSpeed;

        /// <summary>
        /// Reference value used to restart rotation.
        /// </summary>
        public float UVRotateMax;

        /// <summary>
        /// Current coordinate window position.
        /// </summary>
        public float CurrentUVRotate;

        public List<TexFrame> Frames;

        /// <summary>
        /// Offset into anim textures frame list.
        /// </summary>
        public List<uint> FrameList;
    }

    /// <summary>
    /// Room static mesh
    /// </summary>
    public struct StaticMesh
    {
        public uint ObjectID;

        /// <summary>
        /// 0 - was not rendered, 1 - opaque, 2 - transparency, 3 - full rendered
        /// </summary>
        public byte WasRendered;

        /// <summary>
        /// 0 - was not rendered, 1 - opaque, 2 - transparency, 3 - full rendered
        /// </summary>
        public byte WasRenderedLines;

        /// <summary>
        /// Disable static mesh rendering
        /// </summary>
        public bool Hide;

        /// <summary>
        /// Model position
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Model angles
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// Model tint [Length 4]
        /// </summary>
        public float[] Tint; // TODO: Replace all float[]s like this by some Color structure

        /// <summary>
        /// Visible bounding box
        /// </summary>
        public Vector3 VBBMin;

        /// <summary>
        /// Visible bounding box
        /// </summary>
        public Vector3 VBBMax;

        /// <summary>
        /// Collision bounding box
        /// </summary>
        public Vector3 CBBMin;

        /// <summary>
        /// Collision bounding box
        /// </summary>
        public Vector3 CBBMax;

        /// <summary>
        /// GL transformation matrix
        /// </summary>
        public Transform Transform;

        public OBB OBB;

        public EngineContainer Self;

        /// <summary>
        /// Base model
        /// </summary>
        public BaseMesh Mesh;

        public RigidBody BtBody;
    }

    /*
     * Animated skeletal model. Taken from openraider.
     * model -> animation -> frame -> bone
     * thanks to Terry 'Mongoose' Hendrix II
     */

    /*
     * SMOOTHED ANIMATIONS STRUCTURES
     * stack matrices are needed for skinned mesh transformations.
     */

    public class SSBoneTag
    {
        public SSBoneTag Parent;

        public ushort Index;

        /// <summary>
        /// First mesh in array
        /// </summary>
        public BaseMesh MeshBase;

        /// <summary>
        /// Base skinned mesh for TR4+
        /// </summary>
        public BaseMesh MeshSkin;

        public BaseMesh MeshSlot;

        /// <summary>
        /// Model position offset
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// Quaternion rotation
        /// </summary>
        public Quaternion QRotate;

        /// <summary>
        /// 4x4 OpenGL matrix for stack usage
        /// </summary>
        public Transform Transform;

        /// <summary>
        /// 4x4 OpenGL matrix for global usage
        /// </summary>
        public Transform FullTransform;

        /// <summary>
        /// Flag: BODY, LEFT_LEG_1, RIGHT_HAND_2, HEAD...
        /// </summary>
        public uint BodyPart;
    }

    public class SSAnimation
    {
        public short LastState;

        public short NextState;

        public short LastAnimation;

        public short CurrentAnimation;

        public short NextAnimation;

        public short CurrentFrame;

        public short NextFrame;

        /// <summary>
        /// Additional animation control param
        /// </summary>
        public ushort AnimFlags;

        /// <summary>
        /// One frame change period
        /// </summary>
        public float Period = 1.0f / 30;

        /// <summary>
        /// Current time
        /// </summary>
        public float FrameTime;

        public float Lerp;

        public event OnFrameHandler OnFrame = delegate {};

        public delegate void OnFrameHandler(Character ent, SSAnimation ssAnim, int state);

        /// <summary>
        /// Base model
        /// </summary>
        public SkeletalModel Model;

        public SSAnimation Next;
    }

    /// <summary>
    /// Base frame of animated skeletal model
    /// </summary>
    public struct SSBoneFrame
    {
        /// <summary>
        /// Array of bones
        /// </summary>
        public List<SSBoneTag> BoneTags;

        /// <summary>
        /// Position (base offset)
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Bounding box min coordinates
        /// </summary>
        public Vector3 BBMin;

        /// <summary>
        /// Bounding box max coordinates
        /// </summary>
        public Vector3 BBMax;

        /// <summary>
        /// Bounding box centre
        /// </summary>
        public Vector3 Centre;

        /// <summary>
        /// Animations list
        /// </summary>
        public SSAnimation Animations;

        /// <summary>
        /// Whether any skinned meshes need rendering
        /// </summary>
        public bool HasSkin;

        public void FromModel(SkeletalModel model);
    }

    public struct BoneTag
    {
        /// <summary>
        /// Bone vector
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// Rotation quaternion
        /// </summary>
        public Quaternion QRotate;
    }

    /// <summary>
    /// Base frame of animated skeletal model
    /// </summary>
    public struct BoneFrame
    {
        /// <summary>
        /// 0x01 - move need, 0x02 - 180 rotate need
        /// </summary>
        public ushort Command;

        /// <summary>
        /// Array of bones
        /// </summary>
        public List<BoneTag> BoneTags;

        /// <summary>
        /// Position (base offset)
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Bounding box min coordinates
        /// </summary>
        public Vector3 BBMin;

        /// <summary>
        /// Bounding box max coordinates
        /// </summary>
        public Vector3 BBMax;

        /// <summary>
        /// Bounding box centre
        /// </summary>
        public Vector3 Centre;

        /// <summary>
        /// Move command data
        /// </summary>
        public Vector3 Move;

        /// <summary>
        /// Jump command data
        /// </summary>
        public float V_Vertical;

        /// <summary>
        /// Jump command data
        /// </summary>
        public float V_Horizontal;

        public static void Copy(BoneFrame dst, BoneFrame src);
    }

    /// <summary>
    /// Mesh tree base element structure
    /// </summary>
    public struct MeshTreeTag
    {
        /// <summary>
        /// Base mesh - pointer to the first mesh in array
        /// </summary>
        public BaseMesh MeshBase;

        /// <summary>
        /// Base skinned mesh for ТR4+
        /// </summary>
        public BaseMesh MeshSkin;

        /// <summary>
        /// Model position offset
        /// </summary>
        public Vector3 Offset;

        /// <summary>
        /// 0x0001 = POP, 0x0002 = PUSH, 0x0003 = RESET
        /// </summary>
        public ushort Flag;

        public uint BodyPart;

        /// <summary>
        /// Flag for shoot / guns animations (0x00, 0x01, 0x02, 0x03)
        /// </summary>
        public byte ReplaceMesh;

        public byte ReplaceAnim;
    }

    /// <summary>
    /// Animation switching control structure
    /// </summary>
    public struct AnimDispatch
    {
        /// <summary>
        /// "switch to" animation
        /// </summary>
        public ushort NextAnim;

        /// <summary>
        /// "switch to" frame
        /// </summary>
        public ushort NextFrame;

        /// <summary>
        /// Low border of state change condition
        /// </summary>
        public ushort FrameLow;

        /// <summary>
        /// High border of state change condition
        /// </summary>
        public ushort FrameHigh;
    }

    public struct StateChange
    {
        public uint ID;

        public List<AnimDispatch> AnimDispatch;
    }

    /// <summary>
    /// One animation frame structure
    /// </summary>
    public class AnimationFrame
    {
        public uint ID;

        public byte OriginalFrameRate;

        /// <summary>
        /// Forward-backward speed
        /// </summary>
        public int SpeedX;

        /// <summary>
        /// Forward-backward accel
        /// </summary>
        public int AccelX;

        /// <summary>
        /// Left-right speed
        /// </summary>
        public int SpeedY;

        /// <summary>
        /// Left-right accel
        /// </summary>
        public int AccelY;

        public uint AnimCommand;

        public uint NumAnimCommands;

        public ushort StateID;

        /// <summary>
        /// Frame data
        /// </summary>
        public List<BoneFrame> Frames;

        /// <summary>
        /// Animation statechanges data
        /// </summary>
        public List<StateChange> StateChange;

        /// <summary>
        /// Next default animation
        /// </summary>
        public AnimationFrame NextAnim;

        /// <summary>
        /// Next default frame
        /// </summary>
        public int NextFrame;
    }

    /// <summary>
    /// Skeletal model with animations data
    /// </summary>
    public struct SkeletalModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public uint ID;

        /// <summary>
        /// Transparency flags; 0 - opaque; 1 - alpha test; other - blending mode
        /// </summary>
        public byte TransparencyFlags;

        /// <summary>
        /// Bounding box min coordinates
        /// </summary>
        public Vector3 BBMin;

        /// <summary>
        /// Bounding box max coordinates
        /// </summary>
        public Vector3 BBMax;

        /// <summary>
        /// Bounding box centre
        /// </summary>
        public Vector3 Centre;

        /// <summary>
        /// Animations data
        /// </summary>
        public List<AnimationFrame> Animations;

        /// <summary>
        /// Number of model meshes
        /// </summary>
        public ushort MeshCount;

        /// <summary>
        /// Base mesh tree
        /// </summary>
        public List<MeshTreeTag> MeshTree;

        public List<ushort> CollisionMap;

        public void Clear();

        public void FillTransparency();

        public void InterpolateFrames();

        public void FillSkinnedMeshMap();
    }

    public partial class StaticFuncs
    {
        public static MeshTreeTag SkeletonClone(MeshTreeTag src, int tagsCount);

        public static void SkeletonCopyMeshes(MeshTreeTag dst, MeshTreeTag src, int tagsCount);

        public static void SkeletonCopyMeshes2(MeshTreeTag dst, MeshTreeTag src, int tagsCount);
    }

    public static partial class Extensions
    {
        public static collis
    }
}

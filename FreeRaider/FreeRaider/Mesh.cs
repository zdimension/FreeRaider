using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics.ES30;
using FreeRaider.Loader;
using VertexAttribPointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType;

namespace FreeRaider
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

    public class TransparentPolygonReference
    {
        public Polygon Polygon;

        public VertexArray UsedVertexArray;

        public uint FirstIndex;

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
    public class BaseMesh
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class MatrixIndex
        {
            public sbyte I;

            public sbyte J;

            public MatrixIndexStruct ToStruct()
            {
                return new MatrixIndexStruct {I = I, J = J};
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MatrixIndexStruct
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

        ~BaseMesh()
        {
            Clear();
        }

        public void Clear()
        {
            if (VBOVertexArray != 0)
            {
                GL.DeleteBuffer(VBOVertexArray);
                VBOVertexArray = 0;
            }

            if (VBOIndexArray != 0)
            {
                GL.DeleteBuffer(VBOIndexArray);
                VBOIndexArray = 0;
            }

            Polygons.Clear();
            TransparencyPolygons.Clear();
            Vertices.Clear();
            MatrixIndices.Clear();
            ElementsPerTexture.Clear();
            Elements.Clear();
        }

        /// <summary>
        /// Bounding box calculation
        /// </summary>
        public void FindBB()
        {
            if (Vertices.Count > 0)
            {
                var vecs = Vertices.Select(x => x.Position);
                BBMin = new Vector3(vecs.Min(x => x.X), vecs.Min(x => x.Y), vecs.Min(x => x.Z));
                BBMax = new Vector3(vecs.Max(x => x.X), vecs.Max(x => x.Y), vecs.Max(x => x.Z));

                Center = (BBMin + BBMax) / 2;
            }
        }

        public void GenVBO(Render renderer)
        {
            if (new[] {VBOIndexArray, VBOVertexArray, VBOSkinArray}.Contains((uint) 0))
                return;

            // now, begin VBO filling!
            VBOVertexArray = (uint) GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOVertexArray);
            var vsa = Vertices.Select(x => x.ToStruct()).ToArray();
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (Marshal.SizeOf(typeof (VertexStruct)) * vsa.Length), vsa,
                BufferUsageHint.StaticDraw);

            // Store additional skinning information
            if (MatrixIndices.Count > 0)
            {
                VBOSkinArray = (uint) GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBOSkinArray);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr) (Marshal.SizeOf(typeof (MatrixIndexStruct)) * MatrixIndices.Count),
                    MatrixIndices.Select(x => x.ToStruct()).ToArray(),
                    BufferUsageHint.StaticDraw);
            }

            // Fill indices vbo
            VBOIndexArray = (uint) GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndexArray);

            long elementsSize = sizeof (uint) * AlphaElements;
            for (var i = 0; i < TexturePageCount; i++)
            {
                elementsSize += sizeof (uint) * (long) ElementsPerTexture[i];
            }
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) elementsSize, Elements.ToArray(),
                BufferUsageHint.StaticDraw);

            // Prepare vertex array
            var attribs = new[]
            {
                new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Position, 3,
                    VertexAttribPointerType.Float, false, VBOVertexArray, Marshal.SizeOf(typeof (Vertex)),
                    (int) Marshal.OffsetOf(typeof (Vertex), "Position")),
                new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Normal, 3,
                    VertexAttribPointerType.Float, false, VBOVertexArray, Marshal.SizeOf(typeof (Vertex)),
                    (int) Marshal.OffsetOf(typeof (Vertex), "Normal")),
                new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Color, 4,
                    VertexAttribPointerType.Float, false, VBOVertexArray, Marshal.SizeOf(typeof (Vertex)),
                    (int) Marshal.OffsetOf(typeof (Vertex), "Color")),
                new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.TexCoord, 2,
                    VertexAttribPointerType.Float, false, VBOVertexArray, Marshal.SizeOf(typeof (Vertex)),
                    (int) Marshal.OffsetOf(typeof (Vertex), "TexCoord")),
                // Only used for skinned meshes
                new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.MatrixIndex, 2,
                    VertexAttribPointerType.UnsignedByte, false, VBOSkinArray, 2, 0)
            };
            var numAttribs = MatrixIndices.Count == 0 ? 4 : 5;
            MainVertexArray = new VertexArray(VBOIndexArray, attribs);

            // Now for animated polygons, if any
            if (AllAnimatedElements.Count > 0)
            {
                // And upload.
                AnimatedVBOVertexArray = (uint) GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, AnimatedVBOVertexArray);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr) (Marshal.SizeOf(typeof (AnimatedVertex)) * AnimatedVertices.Count),
                    AnimatedVertices.ToArray(), BufferUsageHint.StaticDraw);

                AnimatedVBOIndexArray = (uint) GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, AnimatedVBOIndexArray);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr) (sizeof (uint) * AllAnimatedElements.Count),
                    AllAnimatedElements.ToArray(), BufferUsageHint.StaticDraw);

                // Prepare empty buffer for tex coords
                AnimatedVBOTexCoordArray = (uint) GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, AnimatedVBOTexCoordArray);
                GL.BufferData(BufferTarget.ArrayBuffer,
                    (IntPtr) (Marshal.SizeOf(new float[2]) * AnimatedVertices.Count),
                    IntPtr.Zero, BufferUsageHint.StreamDraw);

                var attribs2 = new[]
                {
                    new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Position, 3,
                        VertexAttribPointerType.Float, false, AnimatedVBOVertexArray,
                        Marshal.SizeOf(typeof (AnimatedVertex)),
                        (int) Marshal.OffsetOf(typeof (AnimatedVertex), "Position")),
                    new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Color, 4,
                        VertexAttribPointerType.Float, false, AnimatedVBOVertexArray,
                        Marshal.SizeOf(typeof (AnimatedVertex)),
                        (int) Marshal.OffsetOf(typeof (AnimatedVertex), "Color")),
                    new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Normal, 3,
                        VertexAttribPointerType.Float, false, AnimatedVBOVertexArray,
                        Marshal.SizeOf(typeof (AnimatedVertex)),
                        (int) Marshal.OffsetOf(typeof (AnimatedVertex), "Normal")),
                    new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.TexCoord, 2,
                        VertexAttribPointerType.Float, false, AnimatedVBOTexCoordArray, Marshal.SizeOf(new float[2]), 0)
                };
                AnimatedVertexArray = new VertexArray(AnimatedVBOIndexArray, attribs2);
            }
            else
            {
                // No animated data
                AnimatedVBOVertexArray = 0;
                AnimatedVBOTexCoordArray = 0;
                AnimatedVertexArray = null;
            }

            // Update references for transparent polygons
            for (var i = 0; i < TransparentPolygons.Count; i++)
            {
                var p = TransparentPolygons[i];
                p.UsedVertexArray = p.IsAnimated ? AnimatedVertexArray : MainVertexArray;
                TransparentPolygons[i] = p;
            }
        }

        public void GenFaces()
        {
            ElementsPerTexture.Resize((int) TexturePageCount);

            /*
             * Layout of the buffers:
             *
             * Normal vertex buffer:
             * - vertices of polygons in order, skipping only animated.
             * Animated vertex buffer:
             * - vertices (without tex coords) of polygons in order, skipping only
             *   non-animated.
             * Animated texture buffer:
             * - tex coords of polygons in order, skipping only non-animated.
             *   stream, initially empty.
             *
             * Normal elements:
             * - elements for texture[0]
             * ...
             * - elements for texture[n]
             * - elements for alpha
             * Animated elements:
             * - animated elements (opaque)
             * - animated elements (blended)
             */

            // Do a first pass to find the numbers of everything
            AlphaElements = 0;
            uint numNormalElements = 0;
            AnimatedVertices.Clear();
            AnimatedElementCount = 0;
            AlphaAnimatedElementCount = 0;

            var transparent = 0;
            foreach (var p in Polygons.Where(p => !p.IsBroken))
            {
                var elementCount = (uint) (p.Vertices.Count - 2) * 3;
                if (p.DoubleSide) elementCount *= 2;

                if (p.AnimID == 0)
                {
                    if (p.BlendMode == BlendingMode.Opaque || p.BlendMode == BlendingMode.Transparent)
                    {
                        ElementsPerTexture[p.TexIndex] += elementCount;
                        numNormalElements += elementCount;
                    }
                    else
                    {
                        AlphaElements += elementCount;
                        transparent++;
                    }
                }
                else
                {
                    if (p.BlendMode == BlendingMode.Opaque || p.BlendMode == BlendingMode.Transparent)
                    {
                        AnimatedElementCount += elementCount;
                    }
                    else
                    {
                        AlphaAnimatedElementCount += elementCount;
                        transparent++;
                    }
                }
            }

            Elements.Resize((int) (numNormalElements + AlphaElements));
            uint elementOffset = 0;
            var startPerTexture = Helper.FillArray((uint) 0, (int) TexturePageCount);
            for (var i = 0; i < TexturePageCount; i++)
            {
                startPerTexture[i] = elementOffset;
                elementOffset += ElementsPerTexture[i];
            }
            var startTransparent = elementOffset;

            AllAnimatedElements.Resize((int) (AnimatedElementCount + AlphaAnimatedElementCount));
            uint animatedStart = 0;
            var animatedStartTransparent = AnimatedElementCount;

            TransparentPolygons.Resize(transparent);
            var transparentPolygonStart = 0;

            foreach (var p in Polygons.Where(p => !p.IsBroken))
            {
                var elementCount = (uint) (p.Vertices.Count - 2) * 3;
                var backwardsStartOffset = elementCount;
                if (p.DoubleSide) elementCount *= 2;

                if (p.AnimID == 0)
                {
                    // Not animated
                    var texture = p.TexIndex;

                    uint oldStart;
                    if (p.BlendMode == BlendingMode.Opaque || p.BlendMode == BlendingMode.Transparent)
                    {
                        oldStart = startPerTexture[texture];
                        startPerTexture[texture] += elementCount;
                    }
                    else
                    {
                        oldStart = startTransparent;
                        startTransparent += elementCount;
                        TransparentPolygons[transparentPolygonStart].FirstIndex = oldStart;
                        TransparentPolygons[transparentPolygonStart].Count = elementCount;
                        TransparentPolygons[transparentPolygonStart].Polygon = p;
                        TransparentPolygons[transparentPolygonStart].IsAnimated = false;
                        transparentPolygonStart++;
                    }
                    var backwardsStart = oldStart + backwardsStartOffset;

                    // Render the polygon as a triangle fan. That is obviously correct for
                    // a triangle and also correct for any quad.
                    var startElement = AddVertex(p.Vertices[0]);
                    var previousElement = AddVertex(p.Vertices[1]);

                    for (var j = 2; j < p.Vertices.Count; j++)
                    {
                        var thisElement = AddVertex(p.Vertices[j]);

                        var offset1 = (int) oldStart + (j - 2) * 3;
                        Elements[offset1 + 0] = startElement;
                        Elements[offset1 + 1] = previousElement;
                        Elements[offset1 + 2] = thisElement;

                        if (p.DoubleSide)
                        {
                            var offset2 = (int) backwardsStart + (j - 2) * 3;
                            Elements[offset2 + 0] = startElement;
                            Elements[offset2 + 1] = thisElement;
                            Elements[offset2 + 2] = previousElement;
                        }

                        previousElement = thisElement;
                    }
                }
                else
                {
                    // Animated
                    uint oldStart;
                    if (p.BlendMode == BlendingMode.Opaque || p.BlendMode == BlendingMode.Transparent)
                    {
                        oldStart = animatedStart;
                        animatedStart += elementCount;
                    }
                    else
                    {
                        oldStart = animatedStartTransparent;
                        animatedStartTransparent += elementCount;
                        TransparentPolygons[transparentPolygonStart].FirstIndex = oldStart;
                        TransparentPolygons[transparentPolygonStart].Count = elementCount;
                        TransparentPolygons[transparentPolygonStart].Polygon = p;
                        TransparentPolygons[transparentPolygonStart].IsAnimated = true;
                        transparentPolygonStart++;
                    }
                    var backwardsStart = oldStart + backwardsStartOffset;

                    // Render the polygon as a triangle fan. That is obviously correct for
                    // a triangle and also correct for any quad.
                    var startElement = AddAnimatedVertex(p.Vertices[0]);
                    var previousElement = AddAnimatedVertex(p.Vertices[1]);

                    for (var j = 2; j < p.Vertices.Count; j++)
                    {
                        var thisElement = AddAnimatedVertex(p.Vertices[j]);

                        var offset1 = (int) oldStart + (j - 2) * 3;
                        Elements[offset1 + 0] = startElement;
                        Elements[offset1 + 1] = previousElement;
                        Elements[offset1 + 2] = thisElement;

                        if (p.DoubleSide)
                        {
                            var offset2 = (int) backwardsStart + (j - 2) * 3;
                            Elements[offset2 + 0] = startElement;
                            Elements[offset2 + 1] = thisElement;
                            Elements[offset2 + 2] = previousElement;
                        }

                        previousElement = thisElement;
                    }
                }
            }

            // Now same for animated triangles
        }

        public uint AddVertex(Vertex vertex)
        {
            for (var ind = 0; ind < Vertices.Count; ind++)
            {
                var v = Vertices[ind];
                if (v.Position == vertex.Position && v.TexCoord.SequenceEqual(vertex.TexCoord))
                {
                    return (uint) ind;
                }
            }

            Vertices.Add(new Vertex
            {
                Position = vertex.Position,
                Normal = vertex.Normal,
                Color = vertex.Color,
                TexCoord = vertex.TexCoord
            });

            return (uint) Vertices.Count - 1;
        }

        public uint AddAnimatedVertex(Vertex v)
        {
            // Skip search for equal vertex; tex coords may differ but aren't stored in
            // animated_vertex_s

            AnimatedVertices.Add(new AnimatedVertex
            {
                Position = v.Position,
                Color = v.Color,
                Normal = v.Normal
            });

            return (uint) AnimatedVertices.Count - 1;
        }

        public void PolySortInMesh()
        {
            foreach (var p in Polygons)
            {
                if(p.AnimID > 0 && p.AnimID <= Global.EngineWorld.AnimSequences.Count)
                {
                    var seq = Global.EngineWorld.AnimSequences[p.AnimID - 1];
                    // set tex coordinates to the first frame for correct texture transform in renderer
                    Global.EngineWorld.TextureAtlas.GetCoordinates(seq.FrameList[0], false, p, 0, seq.UVRotate);
                }

                if(p.BlendMode != BlendingMode.Opaque && p.BlendMode != BlendingMode.Transparent)
                {
                    TransparencyPolygons.Add(p);
                }
            }
        }
    }

    /// <summary>
    /// Base sprite structure
    /// </summary>
    public class Sprite
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
    public class SpriteBuffer
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

    public class Light
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
    public class TexFrame
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

    public class AnimSeq
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
    public class StaticMesh
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

        public event OnFrameHandler OnFrame = delegate { };

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
    public class SSBoneFrame
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

        public void FromModel(SkeletalModel model)
        {
            HasSkin = false;
            BBMin = Vector3.Zero;
            BBMax = Vector3.Zero;
            Centre = Vector3.Zero;
            Position = Vector3.Zero;
            Animations = new SSAnimation();

            /*Animations.Next = null; TODO: Not needed
            Animations.OnFrame = null;*/
            Animations.Model = model;
            BoneTags.Resize(model.MeshCount);

            var stack = 0;
            var parents = new List<SSBoneTag>(BoneTags.Count);
            parents[0] = null;
            BoneTags[0].Parent = null;
            for (ushort i = 0; i < BoneTags.Count; i++)
            {
                BoneTags[i].Index = i;
                BoneTags[i].MeshBase = model.MeshTree[i].MeshBase;
                BoneTags[i].MeshSkin = model.MeshTree[i].MeshSkin;
                if (BoneTags[i].MeshSkin != null)
                    HasSkin = true;
                BoneTags[i].MeshSlot = null;
                BoneTags[i].BodyPart = model.MeshTree[i].BodyPart;

                BoneTags[i].Offset = model.MeshTree[i].Offset;
                BoneTags[i].QRotate = new Quaternion(0, 0, 0, 0);
                BoneTags[i].Transform.SetIdentity();
                BoneTags[i].FullTransform.SetIdentity();

                if (i > 0)
                {
                    BoneTags[i].Parent = BoneTags[i - 1];
                    if (model.MeshTree[i].Flag.HasFlagUns(0x01)) // POP
                    {
                        if (stack > 0)
                        {
                            BoneTags[i].Parent = parents[stack];
                            stack--;
                        }
                    }
                    if (model.MeshTree[i].Flag.HasFlagUns(0x02)) // PUSH
                    {
                        if (stack + 1 < (short) model.MeshCount)
                        {
                            stack++;
                            parents[stack] = BoneTags[i].Parent;
                        }
                    }
                }
            }
        }
    }

    public class BoneTag
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
    public class BoneFrame
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

        public static void Copy(BoneFrame dst, BoneFrame src)
        {
            dst.BoneTags.Resize(src.BoneTags.Count);
            dst.Position = src.Position;
            dst.Centre = src.Centre;
            dst.BBMax = src.BBMax;
            dst.BBMin = src.BBMin;

            dst.Command = src.Command;
            dst.Move = src.Move;

            for (var i = 0; i < dst.BoneTags.Count; i++)
            {
                dst.BoneTags[i] = new BoneTag
                {
                    QRotate = src.BoneTags[i].QRotate,
                    Offset = src.BoneTags[i].Offset
                };
            }
        }
    }

    /// <summary>
    /// Mesh tree base element structure
    /// </summary>
    public class MeshTreeTag
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
    public class AnimDispatch
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

    public class StateChange
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
    public class SkeletalModel
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

        public void Clear()
        {
            MeshTree.Clear();
            CollisionMap.Clear();
            Animations.Clear();
        }

        public void FillTransparency()
        {
            TransparencyFlags = Constants.MESH_FULL_OPAQUE;
            for (var i = 0; i < MeshCount; i++)
            {
                if (MeshTree[i].MeshBase.TransparencyPolygons.Count > 0)
                {
                    TransparencyFlags = Constants.MESH_HAS_TRANSPARENCY;
                    return;
                }
            }
        }

        public void InterpolateFrames()
        {
            foreach (var anim in Animations)
            {
                if (anim.Frames.Count > 1 && anim.OriginalFrameRate > 1) // we can't interpolate one frame or rate < 2!
                {
                    var newBoneFrames = new BoneFrame[anim.OriginalFrameRate * (anim.Frames.Count - 1) + 1].ToList();

                    // the first frame does not change
                    var bfi = 0;
                    var bf = newBoneFrames[bfi];
                    bf.BoneTags.Resize(MeshCount);
                    bf.Position = Vector3.Zero;
                    bf.Move = Vector3.Zero;
                    bf.Command = 0x00;
                    bf.Centre = anim.Frames[0].Centre;
                    bf.Position = anim.Frames[0].Position;
                    bf.BBMax = anim.Frames[0].BBMax;
                    bf.BBMin = anim.Frames[0].BBMin;
                    for (var k = 0; k < MeshCount; k++)
                    {
                        bf.BoneTags[k].Offset = anim.Frames[0].BoneTags[k].Offset;
                        bf.BoneTags[k].QRotate = anim.Frames[0].BoneTags[k].QRotate;
                    }

                    bfi++;
                    bf = newBoneFrames[bfi];

                    for (var j = 1; j < anim.Frames.Count; j++)
                    {
                        for (var l = 1; l <= anim.OriginalFrameRate; l++)
                        {
                            bf.Position = Vector3.Zero;
                            bf.Move = Vector3.Zero;
                            bf.Command = 0x00;
                            var lerp = (float) l / anim.OriginalFrameRate;
                            var t = 1 - lerp;

                            bf.BoneTags.Resize(MeshCount);

                            var prev = anim.Frames[j - 1];
                            var cur = anim.Frames[j];

                            bf.Centre = t * prev.Centre + lerp * cur.Centre;

                            /*
                            bf.Centre[0] = t * prev.Centre[0] + lerp * cur.Centre[0];
                            bf.Centre[1] = t * prev.Centre[1] + lerp * cur.Centre[1];
                            bf.Centre[2] = t * prev.Centre[2] + lerp * cur.Centre[2];
                            */

                            bf.Position = t * prev.Position + lerp * cur.Position;

                            bf.BBMax = t * prev.BBMax + lerp * cur.BBMax;

                            bf.BBMin = t * prev.BBMin + lerp * cur.BBMax;

                            for (var k = 0; k < MeshCount; k++)
                            {
                                bf.BoneTags[k].Offset = prev.BoneTags[k].Offset.Lerp(cur.BoneTags[k].Offset, lerp);
                                bf.BoneTags[k].QRotate = Quaternion.Slerp(prev.BoneTags[k].QRotate,
                                    cur.BoneTags[k].QRotate, lerp);
                            }

                            bfi++;
                            bf = newBoneFrames[bfi];
                        }
                    }

                    // swap old and new animation bone frames
                    // free old bone frames
                    newBoneFrames.MoveTo(anim.Frames);
                }
            }
        }

        public void FillSkinnedMeshMap()
        {
            var treeTagI = 0;
            var treeTag = MeshTree[treeTagI];

            for (var i = 0; i < MeshCount; i++, treeTag = MeshTree[++treeTagI])
            {
                if (treeTag.MeshSkin == null)
                {
                    return;
                }

                treeTag.MeshSkin.MatrixIndices.Resize(treeTag.MeshSkin.Vertices.Count);
                var chI = 0;
                var ch = treeTag.MeshSkin.MatrixIndices[chI];
                var vI = 0;
                var v = treeTag.MeshSkin.Vertices[vI];
                for (var k = 0;
                    k < treeTag.MeshSkin.Vertices.Count;
                    k++, v = treeTag.MeshSkin.Vertices[++vI], ch = treeTag.MeshSkin.MatrixIndices[++chI])
                {
                    var rv = StaticFuncs.FindVertexInMesh(treeTag.MeshBase, v.Position);
                    if (rv != null)
                    {
                        ch.I = 0;
                        ch.J = 0;
                        v.Position = rv.Position;
                        v.Normal = rv.Normal;
                    }
                    else
                    {
                        ch.I = 0;
                        ch.J = 1;
                        var tv = v.Position + treeTag.Offset;
                        var prevTreeTagI = 0;
                        var prevTreeTag = MeshTree[prevTreeTagI];
                        for (var l = 0; l < MeshCount; l++, prevTreeTag = MeshTree[++prevTreeTagI])
                        {
                            rv = StaticFuncs.FindVertexInMesh(prevTreeTag.MeshBase, tv);
                            if (rv != null)
                            {
                                ch.I = 0;
                                ch.J = 0;
                                v.Position = rv.Position - treeTag.Offset;
                                v.Normal = rv.Normal;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    public partial class StaticFuncs
    {
        public static MeshTreeTag[] SkeletonClone(MeshTreeTag[] src, int tagsCount)
        {
            var ret = new MeshTreeTag[tagsCount];

            for (var i = 0; i < tagsCount; i++)
            {
                ret[i] = new MeshTreeTag
                {
                    MeshBase = src[i].MeshBase,
                    MeshSkin = src[i].MeshSkin,
                    Flag = src[i].Flag,
                    Offset = src[i].Offset,
                    ReplaceAnim = src[i].ReplaceAnim,
                    ReplaceMesh = src[i].ReplaceMesh
                };
            }

            return ret;
        }

        public static void SkeletonCopyMeshes(MeshTreeTag[] dst, MeshTreeTag[] src, int tagsCount)
        {
            for (var i = 0; i < tagsCount; i++)
            {
                dst[i].MeshBase = src[i].MeshBase;
            }
        }

        public static void SkeletonCopyMeshes2(MeshTreeTag[] dst, MeshTreeTag[] src, int tagsCount)
        {
            for (var i = 0; i < tagsCount; i++)
            {
                dst[i].MeshSkin = src[i].MeshBase;
            }
        }

        public static Vertex FindVertexInMesh(BaseMesh mesh, Vector3 v)
        {
            return mesh.Vertices.FirstOrDefault(mv => (v - mv.Position).LengthSquared < 4.0f);
        }
    }

    public class CollisionShapeHelper
    {
        public static CollisionShape CSfromSphere(float radius)
        {
            if (radius == 0) return null;

            var ret = new SphereShape(radius);
            ret.Margin = Constants.COLLISION_MARGIN_RIGIDBODY;

            return ret;
        }

        public static CollisionShape CSfromBBox(Vector3 bbMin, Vector3 bbMax, bool useCompression, bool buildBvh)
        {
            var trimesh = new TriangleMesh();
            var cnt = 0;

            var obb = new OBB();
            var pI = 0;
            var p = obb.Polygons[pI];
            for (var i = 0; i < 6; i++, p = obb.Polygons[++pI])
            {
                if (p.IsBroken) continue;

                for (var j = 1; j + 1 < p.Vertices.Count; j++)
                {
                    var v0 = p.Vertices[j + 1].Position;
                    var v1 = p.Vertices[j].Position;
                    var v2 = p.Vertices[0].Position;
                    trimesh.AddTriangle(v0, v1, v2, true);
                }
                cnt++;
            }

            if (cnt == 0)
            {
                trimesh = null;
                return null;
            }


            CollisionShape ret = new ConvexTriangleMeshShape(trimesh, true);
            ret.Margin = Constants.COLLISION_MARGIN_RIGIDBODY;

            return ret;
        }

        public static CollisionShape CSfromMesh(ref BaseMesh mesh, bool useCompression, bool buildBvh,
            bool isStatic = true)
        {
            var cnt = 0;
            var trimesh = new TriangleMesh();
            CollisionShape ret;

            foreach (var p in mesh.Polygons.Where(p => !p.IsBroken))
            {
                for (var j = 1; j + 1 < p.Vertices.Count; j++)
                {
                    var v0 = p.Vertices[j + 1].Position;
                    var v1 = p.Vertices[j].Position;
                    var v2 = p.Vertices[0].Position;
                    trimesh.AddTriangle(v0, v1, v2, true);
                }
                cnt++;
            }

            if (cnt == 0)
            {
                trimesh = null;
                return null;
            }

            if (isStatic)
            {
                ret = new BvhTriangleMeshShape(trimesh, useCompression, buildBvh);
            }
            else
            {
                ret = new ConvexTriangleMeshShape(trimesh, true);
            }
            ret.Margin = Constants.COLLISION_MARGIN_RIGIDBODY;

            return ret;
        }

        public static CollisionShape CSfromHeightmap(List<RoomSector> heightmap, List<SectorTween> tweens,
            bool useCompression, bool buildBvh)
        {
            var cnt = 0;
            var r = heightmap[0].OwnerRoom;
            var trimesh = new TriangleMesh();

            for (var i = 0; i < r.Sectors.Count; i++)
            {
                var hm = heightmap[i];
                if (hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Ghost &&
                    hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                {
                    if (hm.FloorDiagonalType == TR_SECTOR_DIAGONAL_TYPE.None ||
                        hm.FloorDiagonalType == TR_SECTOR_DIAGONAL_TYPE.NorthWest)
                    {
                        if (hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalA)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[3],
                                hm.FloorCorners[2],
                                hm.FloorCorners[0],
                                true);
                            cnt++;
                        }

                        if (hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalB)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[2],
                                hm.FloorCorners[1],
                                hm.FloorCorners[0],
                                true);
                            cnt++;
                        }
                    }
                    else
                    {
                        if (hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalA)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[3],
                                hm.FloorCorners[2],
                                hm.FloorCorners[1],
                                true);
                            cnt++;
                        }

                        if (hm.FloorPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalB)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[3],
                                hm.FloorCorners[1],
                                hm.FloorCorners[0],
                                true);
                            cnt++;
                        }
                    }
                }

                if (hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.Ghost &&
                    hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                {
                    if (hm.CeilingDiagonalType == TR_SECTOR_DIAGONAL_TYPE.None ||
                        hm.CeilingDiagonalType == TR_SECTOR_DIAGONAL_TYPE.NorthWest)
                    {
                        if (hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalA)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[0],
                                hm.FloorCorners[2],
                                hm.FloorCorners[3],
                                true);
                            cnt++;
                        }

                        if (hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalB)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[0],
                                hm.FloorCorners[1],
                                hm.FloorCorners[2],
                                true);
                            cnt++;
                        }
                    }
                    else
                    {
                        if (hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalA)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[0],
                                hm.FloorCorners[1],
                                hm.FloorCorners[3],
                                true);
                            cnt++;
                        }

                        if (hm.CeilingPenetrationConfig != TR_PENETRATION_CONFIG.DoorVerticalB)
                        {
                            trimesh.AddTriangle(
                                hm.FloorCorners[1],
                                hm.FloorCorners[2],
                                hm.FloorCorners[3],
                                true);
                            cnt++;
                        }
                    }
                }
            }

            foreach (var tween in tweens)
            {
                switch (tween.CeilingTweenType)
                {
                    case SectorTweenType.TwoTriangles:
                        var t = Math.Abs(
                            (tween.CeilingCorners[2][2] - tween.CeilingCorners[3][2]) /
                            (tween.CeilingCorners[0][2] - tween.CeilingCorners[1][2]));
                        t = 1.0f / (1.0f + t);
                        var o = new Vector3();
                        Helper.SetInterpolate3(ref o, tween.CeilingCorners[0], tween.CeilingCorners[2], t);
                        trimesh.AddTriangle(
                            tween.CeilingCorners[0],
                            tween.CeilingCorners[1],
                            o,
                            true);
                        trimesh.AddTriangle(
                            tween.CeilingCorners[3],
                            tween.CeilingCorners[2],
                            o,
                            true);
                        cnt += 2;
                        break;
                    case SectorTweenType.TriangleLeft:
                        trimesh.AddTriangle(
                            tween.CeilingCorners[0],
                            tween.CeilingCorners[1],
                            tween.CeilingCorners[3],
                            true);
                        cnt++;
                        break;
                    case SectorTweenType.TriangleRight:
                        trimesh.AddTriangle(
                            tween.CeilingCorners[2],
                            tween.CeilingCorners[1],
                            tween.CeilingCorners[3],
                            true);
                        cnt++;
                        break;
                    case SectorTweenType.Quad:
                        trimesh.AddTriangle(
                            tween.CeilingCorners[0],
                            tween.CeilingCorners[1],
                            tween.CeilingCorners[3],
                            true);
                        trimesh.AddTriangle(
                            tween.CeilingCorners[2],
                            tween.CeilingCorners[1],
                            tween.CeilingCorners[3],
                            true);
                        cnt += 2;
                        break;
                }

                switch (tween.FloorTweenType)
                {
                    case SectorTweenType.TwoTriangles:
                        var t = Math.Abs(
                            (tween.FloorCorners[2][2] - tween.FloorCorners[3][2]) /
                            (tween.FloorCorners[0][2] - tween.FloorCorners[1][2]));
                        t = 1.0f / (1.0f + t);
                        var o = new Vector3();
                        Helper.SetInterpolate3(ref o, tween.FloorCorners[0], tween.FloorCorners[2], t);
                        trimesh.AddTriangle(
                            tween.FloorCorners[0],
                            tween.FloorCorners[1],
                            o,
                            true);
                        trimesh.AddTriangle(
                            tween.FloorCorners[3],
                            tween.FloorCorners[2],
                            o,
                            true);
                        cnt += 2;
                        break;
                    case SectorTweenType.TriangleLeft:
                        trimesh.AddTriangle(
                            tween.FloorCorners[0],
                            tween.FloorCorners[1],
                            tween.FloorCorners[3],
                            true);
                        cnt++;
                        break;
                    case SectorTweenType.TriangleRight:
                        trimesh.AddTriangle(
                            tween.FloorCorners[2],
                            tween.FloorCorners[1],
                            tween.FloorCorners[3],
                            true);
                        cnt++;
                        break;
                    case SectorTweenType.Quad:
                        trimesh.AddTriangle(
                            tween.FloorCorners[0],
                            tween.FloorCorners[1],
                            tween.FloorCorners[3],
                            true);
                        trimesh.AddTriangle(
                            tween.FloorCorners[2],
                            tween.FloorCorners[1],
                            tween.FloorCorners[3],
                            true);
                        cnt += 2;
                        break;
                }
            }

            if(cnt == 0)
            {
                trimesh = null;
                return null;
            }

            var ret = new BvhTriangleMeshShape(trimesh, useCompression, buildBvh);
            ret.Margin = Constants.COLLISION_MARGIN_RIGIDBODY;
            return ret;
        }
    }
}

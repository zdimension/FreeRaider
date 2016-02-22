using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using FreeRaider.Loader;

namespace FreeRaider.Game
{
    public class LevelManager
    {
        public static Color4[] Palette16 { get; set; }
        public static Color4[] Palette8 { get; set; }






        public static void SetLevel(string filename)
        {
            var l = TombLevelParser.ParseFile(filename);
            if(l.GameVersion == TR2LevelVersion.TR1 || l.GameVersion == TR2LevelVersion.TR1UnfinishedBusiness)
            {
                loadTR1((TR1Level)l);
            }
            else if(l.GameVersion == TR2LevelVersion.TR2)
            {
                loadTR2((TR2Level)l);
            }
            else if(l.GameVersion == TR2LevelVersion.TR3)
            {
                loadTR3((TR3Level)l);
            }
        }
        
        private static void loadTR1(TR1Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();

            #region Textures
            foreach (var tr2Textile8 in lvl.Textile8)
            {
                Texture8.ToGLTexture(tr2Textile8);
            }
            #endregion
        }

        private static void loadTR2(TR2Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();
            Palette16 = lvl.Palette16.Select(x => new Color4(x.Red, x.Green, x.Blue, x.Unused)).ToArray();

            #region Textures
            foreach (var tr2Textile8 in lvl.Textile8)
            {
                Texture8.ToGLTexture(tr2Textile8);
            }
            foreach (var tr2Textile16 in lvl.Textile16)
            {
                Texture16.ToGLTexture(tr2Textile16);
            }
            #endregion

            #region Rooms
            for(var i = 0; i < lvl.Rooms.Length; i++)
            {
                var r = lvl.Rooms[i];

                var roomPos = new Vector3(r.info.x, 0.0f, r.info.z);

                var boundingBox = new Vector3[]
                {
                    Vector3.Zero,
                    Vector3.Zero
                };

                var vertices = new List<Vector3>();

                for(var j = 0; j < r.RoomData.Vertices.Length; j++)
                {
                    var current = r.RoomData.Vertices[j];
                    var vert = current.Vertex.ToVector3();
                    vertices.Add(vert);
                    if(j == 0)
                    {
                        boundingBox[0] = boundingBox[1] = vert;
                    }
                    else
                    {
                        boundingBox[0].X = Math.Min(boundingBox[0].X, vert.X);
                        boundingBox[1].X = Math.Min(boundingBox[1].X, vert.X);
                        boundingBox[0].Y = Math.Min(boundingBox[0].Y, vert.Y);
                        boundingBox[1].Y = Math.Min(boundingBox[1].Y, vert.Y);
                        boundingBox[0].Z = Math.Min(boundingBox[0].Z, vert.Z);
                        boundingBox[1].Z = Math.Min(boundingBox[1].Z, vert.Z);
                    }
                }

                boundingBox[0] += roomPos;
                boundingBox[1] += roomPos;

                var spr = r.RoomData.Sprites.Select(s => new RoomSprite(vertices[s.Vertex] + roomPos, (ushort) s.Texture)).ToList();


            }
            #endregion
        }

        private static void loadTR3(TR3Level lvl)
        {
            Palette8 = lvl.Palette.Select(x => new Color4(x.Red, x.Green, x.Blue, 0xFF)).ToArray();
            Palette16 = lvl.Palette16.Select(x => new Color4(x.Red, x.Green, x.Blue, x.Unused)).ToArray();

            #region Textures
            foreach (var tr2Textile8 in lvl.Textile8)
            {
                Texture8.ToGLTexture(tr2Textile8);
            }
            foreach (var tr2Textile16 in lvl.Textile16)
            {
                Texture16.ToGLTexture(tr2Textile16);
            }
            #endregion
        }
    }
}

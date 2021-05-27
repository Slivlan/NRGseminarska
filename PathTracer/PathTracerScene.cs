using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace PathTracer
{
    public class PathTracerScene
    {
        #region Constructors

        public PathTracerScene()
        {
            this.Triangles = new List<PathTracerTriangle>();
        }

        #endregion

        #region Properties

        public Action<int> Animator { get; set; }

        public PathTracerMaterial BackgroundMaterial { get; set; }

        public PathTracerCamera Camera { get; set; }

        public PathTracerColor FogColor { get; set; }

        public float FogDistance { get; set; }

        public List<PathTracerTriangle> Triangles { get; set; }

        #endregion

        public void ReadObject(string objPath, string texturePath = null, float scale = 1f, PathTracerMaterial material = null) {

            Bitmap texture = null;
            if(texturePath != null) {
                texture = (Bitmap)Bitmap.FromFile(texturePath).Clone();
            }

            List<Vector3> vertices = new List<Vector3>();
            Vector3[] verticesArray = null;
            bool newVertex = false;

            System.IO.StreamReader file = new System.IO.StreamReader(objPath);
            string line;
            while((line = file.ReadLine()) != null) {
                if (line.Length == 0)
                    continue;
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith("v ")) {
                    string[] split = line.Split(' ');
                    Vector3 v = new Vector3();
                    v.X = float.Parse(split[1]) * scale;
                    v.Y = float.Parse(split[2]) * scale;
                    v.Z = float.Parse(split[3]) * scale;
                    vertices.Add(v);
                    //Console.WriteLine("{0} {1} {2}", v.X, v.Y, v.Z);
                    newVertex = true;
                }

                 

                if (line.StartsWith("f ")) {
                    if(newVertex || verticesArray == null) {
                        newVertex = false;
                        verticesArray = vertices.ToArray();
                    }

                    string[] split = line.Split(' ');
                    
                    Vector3 V0 = verticesArray[int.Parse(split[1].Split("/")[0])-1];
                    Vector3 V1 = verticesArray[int.Parse(split[2].Split("/")[0])-1];
                    Vector3 V2 = verticesArray[int.Parse(split[3].Split("/")[0]) - 1];
                    PathTracerTriangle t = new PathTracerTriangle(V0, V1, V2, material == null ? new PathTracerMaterial() : material, texture: texture != null ? texture : null);
                    t.Clockwise = false;
                    if(material == null)
                        t.Material.Color = new PathTracerColor(1, 0.5f, 0.5f, 0.5f);
                    this.Triangles.Add(t);
                }

            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PathTracer {
    public class PathTracerSphere {
        public float Radius;
        public Vector3 Position;
        public PathTracerMaterial Material;

        public PathTracerSphere() {
            return;
        }

        public PathTracerSphere (float r, Vector3 pos, PathTracerMaterial mat) {
            Material = mat;
            Position = pos;
            Radius = r;
            return;
        }

        public PathTracerHit GetHit(PathTracerRay ray) {
            PathTracerRay objectSpaceRay = new PathTracerRay();
            objectSpaceRay.Direction = ray.Direction;
            objectSpaceRay.Origin = ray.Origin - Position;

            double dist;
            Vector3 crossPoint;
            Vector3 normal;
            double a = Vector3.Dot(objectSpaceRay.Direction, objectSpaceRay.Direction);
            double b = 2 * Vector3.Dot(objectSpaceRay.Origin, objectSpaceRay.Direction);
            double c = Vector3.Dot(objectSpaceRay.Origin, objectSpaceRay.Origin) - Radius * Radius;
            double discriminant = b * b - 4 * a * c;
            if (discriminant >= 0) {
                /*Console.WriteLine("Found intersection between ray that starts in: {0}, {1}, {2} with direction {3}, {4}, {5} and sphere with radius {2}",
                                    r.o.x, r.o.y, r.o.z, r.d.x, r.d.y, r.d.z, Radius);*/

                //Console.WriteLine("Hit sphere. ");
                double x1 = (-b + Math.Sqrt(discriminant)) / (2 * a);
                double x2 = (-b - Math.Sqrt(discriminant)) / (2 * a);

                if (x1 < x2) {
                    dist = x1;
                }
                else {
                    dist = x2;
                }

                if(dist < 0.001f) {
                    return PathTracerHit.Miss;
                }

                crossPoint = objectSpaceRay.Origin + ((float)dist * objectSpaceRay.Direction);

                //Console.WriteLine("{0} --- {1}", crossPoint + Position, ray.Origin + (float)dist * ray.Direction);

                //Console.WriteLine("In point: {0}, {1}, {2}", crossPoint.X, crossPoint.Y, crossPoint.Z);
                //Console.WriteLine("Distance " + dist);
                normal = crossPoint;
                normal = Vector3.Normalize(normal);

                var hit = new PathTracerHit();
                hit.IsHit = true;
                hit.Material = Material;
                hit.Normal = normal;
                hit.Position = crossPoint+Position;
                hit.Distance = (float)dist;

                return hit;
                
            }
            else {
                return PathTracerHit.Miss;
            }
            
        }
    }
}

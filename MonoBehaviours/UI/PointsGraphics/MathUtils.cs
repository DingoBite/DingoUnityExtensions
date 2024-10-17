using System;
using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.MonoBehaviours.UI.PointsGraphics
{
    public static class MathUtils
    {
        public static Vector2 GetLookClosestPoint(float radius, Vector2 p1, Vector2 p2)
        {
            var alpha = Vector2.SignedAngle(Vector2.right, p2 - p1) * Mathf.Deg2Rad;
            var angleOffset = new Vector2((float)Math.Cos(alpha), (float)Math.Sin(alpha));
            return angleOffset * radius;
        }

        public static Vector2 GetTouchPoint(float radius, Vector2 p1, Vector2 p2)
        {
            var angleOffset = GetLookClosestPoint(radius, p1, p2);
            var pd = Vector2.Perpendicular(angleOffset);
            return pd.normalized * radius;
        }

        public static List<Vector2> CalculateElasticPoint_Old(Vector2 p1, float r1, Vector2 p2, float r2, float step = 0.01f)
        {
            var touchP1 = GetTouchPoint(r1, p1, p2);
            var touchP2 = GetTouchPoint(r2, p1, p2);

            return CalculateTangentPoints(p1, r1, p2, r2, step, touchP1, - touchP1, touchP2, - touchP2);
        }

        public static float FullAngle(Vector2 vector) => FullAngle(vector, Vector2.right);
        
        public static float FullAngle(Vector2 vector, Vector2 direction)
        {
            var angle = Vector2.SignedAngle(direction, vector) + 360;
            return (angle % 360) * Mathf.Deg2Rad;
        }

        public static float FullAngleDeg(Vector2 vector) => FullAngleDeg(vector, Vector2.right);
        
        public static float FullAngleDeg(Vector2 vector, Vector2 direction)
        {
            var angle = Vector2.SignedAngle(direction, vector) + 360;
            return angle % 360;
        }
        
        public static float GetAbsSignedAngle(Vector2 v1, Vector2 v2) => Math.Abs(Vector2.SignedAngle(v1, v2));
        
        private static List<Vector2> CalculateTangentPoints(Vector2 p1, float r1, Vector2 p2, float r2, float step, Vector2 tStartP1, Vector2 tEndP1, Vector2 tStartP2, Vector2 tEndP2)
        {
            var points = new List<Vector2>();
            step = Math.Max(step, 0.0001f);
            // Perpendicular tangent check
            var inverseSegments = (tStartP1 - tStartP2).y <= -5e-4 && (tEndP1 - tEndP2).y >= -5e-4;

            var startP1 = p1 + tStartP1;
            var endP1 = p1 + tEndP1;
            var aStartP1 = FullAngle(tStartP1);
            var aEndP1 = FullAngle(tEndP1);

            var startP2 = p2 + tStartP2;
            var endP2 = p2 + tEndP2;
            var aStartP2 = FullAngle(tStartP2);
            var aEndP2 = FullAngle(tEndP2);

            points.Add(startP2);
            var (start, end) = inverseSegments ? (aStartP1, aEndP1) : (aStartP1, aEndP1 - Math.PI * 2);
            var s = Math.Sign(end - start) * step;
            var i = 0;
            for (var a = start; Math.Abs(a - end) >= step; a += s)
            {
                i++;
                if (i > 1000)
                    break;
                var x = (float)Math.Cos(a);
                var y = (float)Math.Sin(a);
                var p = new Vector2(x, y);
                p *= r1;
                points.Add(p1 + p);
            }
            points.Add(endP1);
            (start, end) = !inverseSegments ? (aEndP2, aStartP2) : (aEndP2, aStartP2 - (float) Math.PI * 2);
            s = Math.Sign(end - start) * step;
            i = 0;
            for (var a = start; Math.Abs(a - end) >= step; a += s)
            {
                i++;
                if (i > 1000)
                    break;
                var x = (float)Math.Cos(a);
                var y = (float)Math.Sin(a);
                var p = new Vector2(x, y);
                p *= r2;
                points.Add(p2 + p);
            }
            points.Add(startP2);
            points.Add(startP2 + (startP1 - startP2) * 0.001f);

            return points;
        }

        // https://en.wikipedia.org/wiki/Tangent_lines_to_circles
        public static List<Vector2> CalculateElasticPoint(Vector2 p1, float r1, Vector2 p2, float r2, float step = 0.01f)
        {
            var isR1 = r1 > r2;

            var originP = isR1 ? p1 : p2;
            var originR = isR1 ? r1 : r2;
            var targetP = isR1 ? p2 : p1;
            var targetR = isR1 ? r2 : r1;
            var dir = originP - targetP;
            var d = dir.magnitude;
            
            var r3 = originR - targetR;
            if (d <= r3 + 5e-4)
                return CalculateCircle(originR, originP, step);

            var b = Math.Asin(r3 / d);
            var g = -Math.Atan2(dir.y, dir.x);

            var cosStart = (float) Math.Cos(g - b);
            var sinStart = (float) Math.Sin(g - b);
            var cosEnd = -(float) Math.Cos(g + b);
            var sinEnd = -(float) Math.Sin(g + b);

            var tStartOrigin = new Vector2(sinStart, cosStart) * originR;
            var tEndOrigin = new Vector2(sinEnd, cosEnd) * originR;
            
            var tStartTarget = new Vector2(sinStart, cosStart) * targetR;
            var tEndTarget = new Vector2(sinEnd, cosEnd) * targetR;
            
            return CalculateTangentPoints(originP, originR, targetP, targetR, step, tStartOrigin, tEndOrigin, tStartTarget, tEndTarget);
        }
        
        public static List<Vector2> CalculateCircle(float radius, Vector2 uvCenter, float step)
        {
            var points = new List<Vector2>();
            var i = 0;
            for (var a = -3 * Math.PI * 0.5f; a <= Math.PI * 0.5f + float.Epsilon; a += step)
            {
                var x = (float)(radius * Math.Cos(a));
                var y = (float)(radius * Math.Sin(a));
                var point = new Vector2(x, y) + uvCenter;
                points.Add(point);
                if (i == 0)
                    points.Add(point);
                i++;
            }
            points.Add(points[0]);
            return points;
        }
    }
}
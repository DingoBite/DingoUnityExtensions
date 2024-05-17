using System;
using System.Collections.Generic;
using UnityEngine;

namespace DingoUnityExtensions.MathAndGeometry
{
    public struct Frustum
    {
        private float _fov;

        private float _near;
        private float _far;

        private float _aspect;
        private Quaternion _rotation;

        public Frustum(float fov = 50)
        {
            _near = 0;
            _far = 10;
            _aspect = 16f / 9;
            _fov = fov;
            _rotation = Quaternion.identity;
            Position = Vector3.zero;
            Forward = Vector3.forward;
            Right = Vector3.right;
            Up = Vector3.up;
        }

        public Vector3 Position { get; set; }

        public Quaternion Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                Forward = _rotation * Vector3.forward;
                Right = _rotation * Vector3.right;
                Up = _rotation * Vector3.up;

                Forward.Normalize();
                Right.Normalize();
                Up.Normalize();
            }
        }

        public Vector3 Forward { get; private set; }
        public Vector3 Right { get; private set; }
        public Vector3 Up { get; private set; }

        public Vector3 NearClipCenter => Position + Forward * _near;
        public Vector3 FarClipCenter => Position + Forward * _far;

        public Plane NearPlane => new(FarClipCenter - NearClipCenter, NearClipCenter);

        public float HeightNear => 2 * Near * Mathf.Tan(FOV * Mathf.Deg2Rad * 0.5f);
        public float WidthNear => HeightNear * _aspect;

        public float HeightFar => 2 * Far * Mathf.Tan(FOV * Mathf.Deg2Rad * 0.5f);
        public float WidthFar => HeightFar * _aspect;

        public Vector2 Dimensions => new(WidthNear, HeightNear);

        public float Near
        {
            get => _near;
            set => _near = Mathf.Max(0, value);
        }

        public float Far
        {
            get => _far;
            set => _far = Mathf.Max(0, value);
        }

        public float Aspect
        {
            get => _aspect;
            set => _aspect = Mathf.Max(value, Vector2.kEpsilon);
        }

        public float FOV
        {
            get => _fov;
            set => _fov = Mathf.Clamp(value, Vector2.kEpsilon, 180);
        }

        public void SetHeightFar(float value)
        {
            // value = 2 * Far * Mathf.Tan(FOV * Mathf.Deg2Rad * 0.5f);
            // Far = value / (2 * Mathf.Tan(FOV * Mathf.Deg2Rad * 0.5f));

            Far = value / (2 * Mathf.Tan(FOV * Mathf.Deg2Rad * 0.5f));
        }

        public Vector3 GetMinRangeWidthLine()
        {
            var halfHeight = Mathf.Tan(_fov * Mathf.Deg2Rad * 0.5f) * _near;
            var halfWidth = halfHeight * _aspect;
            var direction = Right * halfWidth;

            return direction;
        }

        public Vector3 GetMinRangeHeightLine()
        {
            var halfHeight = Mathf.Tan(_fov * Mathf.Deg2Rad * 0.5f) * _near;
            var direction = Up * halfHeight;

            return direction;
        }

        public Frustum MirrorPosition()
        {
            var frustum = this;
            frustum.Position = new Vector3(-Position.x, Position.y, Position.z);
            return frustum;
        }
        
        public Frustum MirrorRotation()
        {
            var frustum = this;
            frustum.Rotation = _rotation * Quaternion.Euler(0, 180, 0);
            return frustum;
        }
        
        public static float MaxDistanceToFrustum(Vector3 point, IReadOnlyList<Plane> planes)
        {
            if (planes.Count != 6)
                throw new ArgumentException($"Planes count {planes.Count} != 6");
            var maxDistance = float.MinValue;
            for (var i = 0; i < 6; i++)
            {
                var distance = System.Math.Abs(planes[i].GetDistanceToPoint(point));
                if (distance > maxDistance)
                    maxDistance = distance;
            }

            return maxDistance;
        }

        public static float MaxDistanceToFrustum(Vector3 point, float near, float far, float aspect, float fov, Vector3 forward, Vector3 position, Vector3 up)
        {
            var planes = GetFrustumPlanes(position, near, far, aspect, fov, forward, up);
            return MaxDistanceToFrustum(point, planes);
        }
        
        public static Plane[] GetFrustumPlanes(Vector3 position, float near, float far, float aspect, float fov, Vector3 forward, Vector3 up)
        {
            var halfHeight = near * Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f);
            var halfWidth = halfHeight * aspect;

            var right = Vector3.Cross(forward, up);

            var nearCenter = position + forward * near;
            var farCenter = position + forward * far;

            var corners = new Vector3[8];
            corners[0] = nearCenter - halfHeight * up - halfWidth * right; // near bottom left
            corners[1] = nearCenter + halfHeight * up - halfWidth * right; // near top left
            corners[2] = nearCenter + halfHeight * up + halfWidth * right; // near top right
            corners[3] = nearCenter - halfHeight * up + halfWidth * right; // near bottom right

            corners[4] = farCenter - halfHeight * far / near * up - halfWidth * far / near * right; // far bottom left
            corners[5] = farCenter + halfHeight * far / near * up - halfWidth * far / near * right; // far top left
            corners[6] = farCenter + halfHeight * far / near * up + halfWidth * far / near * right; // far top right
            corners[7] = farCenter - halfHeight * far / near * up + halfWidth * far / near * right; // far bottom right

            var planes = new Plane[6];
            planes[0] = new Plane(corners[0], corners[1], corners[2]); // Near
            planes[1] = new Plane(corners[4], corners[7], corners[6]); // Far
            planes[2] = new Plane(corners[0], corners[4], corners[5]); // Left
            planes[3] = new Plane(corners[3], corners[2], corners[6]); // Right
            planes[4] = new Plane(corners[1], corners[5], corners[6]); // Top
            planes[5] = new Plane(corners[0], corners[3], corners[7]); // Bottom
            return planes;
        }
    }
}
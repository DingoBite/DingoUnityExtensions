using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace DingoUnityExtensions.Extensions
{
    public enum SpaceType
    {
        Local,
        World
    }

    [Flags]
    public enum TransformComponent
    {
        None = 0,
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2,
        
        Everything = ~0,
    }

    [Serializable, Preserve]
    public class TransformTargetPair
    {
        [field: SerializeField] public Transform Transform { get; private set; }
        [field: SerializeField] public SpaceType SpaceType { get; private set; }
        [field: SerializeField] public Transform Target { get; private set; }
        [field: SerializeField] public SpaceType TargetSpaceType { get; private set; }
        [field: SerializeField] public TransformComponent TransformComponent { get; private set; }

        public float Distance => Transform.GetDistance(Target, SpaceType, TargetSpaceType, TransformComponent);

        public void Move(float time)
        {
            Transform.SetComponents(Target, SpaceType, TargetSpaceType, TransformComponent, time);
        }
        
        public bool TryMove(float time, float moveThreshold)
        {
            Transform.SetComponents(Target, SpaceType, TargetSpaceType, TransformComponent, out var distance, time);
            return distance < moveThreshold;
        }
    }
    
    public static class TransformExtensions
    {
        private const float ANGLE_DELIMITER = 1f / 180f;

        public static float GetDistance(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType, TransformComponent transformComponent)
        {
            if (transformComponent == TransformComponent.None)
                return 0;
            
            var distance = 0f;
            var components = 0;
            var rotationMultiplier = 1f;
            if (transformComponent.HasFlag(TransformComponent.Scale))
            {
                distance += ScaleDistance(transform, target, spaceType, targetSpaceType);
                components++;
            }
            if (transformComponent.HasFlag(TransformComponent.Rotation))
            {
                rotationMultiplier = RotationDistanceMultiplier(transform, target, spaceType, targetSpaceType);
            }
            if (transformComponent.HasFlag(TransformComponent.Position))
            {
                distance += PositionDistance(transform, target, spaceType, targetSpaceType);
                components++;
            }

            components = components == 0 ? 1 : components;
            return distance * rotationMultiplier / components;
        }
        
        public static void SetComponents(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType, TransformComponent transformComponent, out float distance, float time = 1, bool distanceAfter = true)
        {
            distance = 0f;
            if (transformComponent == TransformComponent.None)
                return;
            
            var components = 0;
            var rotationMultiplier = 1f;
            if (transformComponent.HasFlag(TransformComponent.Scale))
            {
                if (!distanceAfter)
                    distance += ScaleDistance(transform, target, spaceType, targetSpaceType);
                transform.SetScale(spaceType, target.GetScale(targetSpaceType), time);
                if (distanceAfter)
                    distance += ScaleDistance(transform, target, spaceType, targetSpaceType);
                components++;
            }
            if (transformComponent.HasFlag(TransformComponent.Rotation))
            {
                if (!distanceAfter)
                    rotationMultiplier = RotationDistanceMultiplier(transform, target, spaceType, targetSpaceType);
                transform.SetRotation(spaceType, target.GetRotation(targetSpaceType), time);
                if (distanceAfter)
                    rotationMultiplier = RotationDistanceMultiplier(transform, target, spaceType, targetSpaceType);
            }
            if (transformComponent.HasFlag(TransformComponent.Position))
            {
                if (!distanceAfter)
                    distance += PositionDistance(transform, target, spaceType, targetSpaceType);
                transform.SetPosition(spaceType, target.GetPosition(targetSpaceType), time);
                if (distanceAfter)
                    distance += PositionDistance(transform, target, spaceType, targetSpaceType);
                components++;
            }
            
            components = components == 0 ? 1 : components;
            distance *= rotationMultiplier / components;
        }

        public static void SetComponents(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType, TransformComponent transformComponent, float time = 1)
        {
            if (transformComponent == TransformComponent.None)
                return;
            if (transformComponent == TransformComponent.Everything)
            {
                SetTRS(transform, spaceType, target.localToWorldMatrix);
                return;
            }
            
            if (transformComponent.HasFlag(TransformComponent.Scale))
                transform.SetScale(spaceType, target.GetScale(targetSpaceType), time);
            if (transformComponent.HasFlag(TransformComponent.Rotation))
                transform.SetRotation(spaceType, target.GetRotation(targetSpaceType), time);
            if (transformComponent.HasFlag(TransformComponent.Position))
                transform.SetPosition(spaceType, target.GetPosition(targetSpaceType), time);
        }

        public static Vector3 GetPosition(this Transform transform, SpaceType spaceType)
        {
            return spaceType switch
            {
                SpaceType.Local => transform.localPosition,
                SpaceType.World => transform.position,
                _ => throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null)
            };
        }
        
        public static void SetPosition(this Transform transform, SpaceType spaceType, Vector3 position, float time = 1)
        {
            switch (spaceType)
            {
                case SpaceType.Local:
                    transform.localPosition = Vector3.Lerp(transform.localPosition, position, time);
                    break;
                case SpaceType.World:
                    transform.position = Vector3.Lerp(transform.position, position, time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null);
            }
        }
        
        public static Quaternion GetRotation(this Transform transform, SpaceType spaceType)
        {
            return spaceType switch
            {
                SpaceType.Local => transform.localRotation,
                SpaceType.World => transform.rotation,
                _ => throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null)
            };
        }
        
        public static void SetRotation(this Transform transform, SpaceType spaceType, Quaternion rotation, float time = 1)
        {
            switch (spaceType)
            {
                case SpaceType.Local:
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, rotation, time);
                    break;
                case SpaceType.World:
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotation, time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null);
            }
        }

        public static Vector3 GetScale(this Transform transform, SpaceType spaceType)
        {
            return spaceType switch
            {
                SpaceType.Local => transform.lossyScale,
                SpaceType.World => transform.lossyScale,
                _ => throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null)
            };
        }
        
        public static void SetScale(this Transform transform, SpaceType spaceType, Vector3 scale, float time = 1)
        {
            switch (spaceType)
            {
                case SpaceType.Local:
                    transform.localScale = Vector3.Lerp(transform.localScale, scale, time);
                    break;
                case SpaceType.World:
                    var prevScale = transform.lossyScale;
                    transform.localScale = Vector3.one;
                    var lossyScaleX = transform.lossyScale.x;
                    var lossyScaleY = transform.lossyScale.y;
                    var lossyScaleZ = transform.lossyScale.z;
                    var x = Mathf.Approximately(lossyScaleX, 0) ? lossyScaleX : scale.x / lossyScaleX;
                    var y = Mathf.Approximately(lossyScaleY, 0) ? lossyScaleY : scale.y / lossyScaleY;
                    var z = Mathf.Approximately(lossyScaleZ, 0) ? lossyScaleZ : scale.z / lossyScaleZ;
                    transform.localScale = Vector3.Lerp(prevScale, new Vector3 (x, y, z), time);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(spaceType), spaceType, null);
            }
        }
        
        public static float PositionDistance(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType) => 
            (transform.GetPosition(spaceType) - target.GetPosition(targetSpaceType)).magnitude;

        public static float RotationDistanceMultiplier(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType) => 
            1 + Quaternion.Angle(transform.GetRotation(spaceType), target.GetRotation(targetSpaceType)) * ANGLE_DELIMITER;

        public static float ScaleDistance(this Transform transform, Transform target, SpaceType spaceType, SpaceType targetSpaceType) => 
            (transform.GetScale(spaceType) - target.GetScale(targetSpaceType)).magnitude;

        public static void SetTRS(this Transform transform, SpaceType spaceType, Matrix4x4 trs)
        {
            transform.SetScale(spaceType, trs.lossyScale);
            transform.SetRotation(spaceType, trs.rotation);
            transform.SetPosition(spaceType, trs.GetPosition());
        }
        
        public static void SetXZPos(this Transform transform, Vector2 xzVector)
        {
            var pos = transform.position;
            pos.x = xzVector.x;
            pos.z = xzVector.y;
            transform.localPosition = pos;
        }
        
        public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
        {
            transform.localScale = matrix.lossyScale;
            transform.localRotation = matrix.rotation;
            transform.localPosition = matrix.GetPosition();
        }
        
        public static (Vector3 position, Quaternion rotation, Vector3 localScale) FromMatrix(this Matrix4x4 matrix)
        {
            var localScale = matrix.lossyScale;
            var rotation = matrix.rotation;
            var position = matrix.GetPosition();
            return (position, rotation, localScale);
        }

        public static Rect GetNormalizedRect(this RectTransform childRect, RectTransform parentRect)
        {
            var childCorners = new Vector3[4];
            childRect.GetWorldCorners(childCorners);

            for (var i = 0; i < 4; i++)
            {
                childCorners[i] = parentRect.InverseTransformPoint(childCorners[i]);
            }

            var minPoint = new Vector2(
                Mathf.Min(childCorners[0].x, childCorners[2].x),
                Mathf.Min(childCorners[0].y, childCorners[2].y)
            );

            var maxPoint = new Vector2(
                Mathf.Max(childCorners[0].x, childCorners[2].x),
                Mathf.Max(childCorners[0].y, childCorners[2].y)
            );

            var size = maxPoint - minPoint;

            var normalizedMin = new Vector2(
                0.5f + minPoint.x / parentRect.rect.width,
                0.5f + minPoint.y / parentRect.rect.height
            );

            var normalizedSize = new Vector2(
                size.x / parentRect.rect.width,
                size.y / parentRect.rect.height
            );

            return new Rect(normalizedMin, normalizedSize);
        }
    }
}
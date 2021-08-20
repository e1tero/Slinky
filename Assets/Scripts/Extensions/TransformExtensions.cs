using UnityEngine;

namespace Extentions
{
    public static class TransformExtensions
    {
        public static void MoveArroundX(this Transform transform, in Vector3 target, in float angle, in float radius = 1, in float height = 1)
        {
            var position = target + new Vector3(Mathf.Sin(angle) * radius, height, Mathf.Cos(angle) * radius);
            transform.position = position;
        }
        
        public static void MoveArroundY(this Transform transform, in Vector3 target, in float angle, in float radius = 1)
        {
            var position = target + new Vector3(0, Mathf.Sin(angle) * radius, 0);
            transform.position = position;
        }
    }
}
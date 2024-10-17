using DingoUnityExtensions.Pools;
using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class DevDrawerView : MonoBehaviour
    {
        [SerializeField] private LinePool _linePool;
        [SerializeField] private PointPool _pointPool;

        public Pool<Point> Points => _pointPool;
        public Pool<Line> Lines => _linePool;
    }
}
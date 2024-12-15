using DingoUnityExtensions.Pools;
using UnityEngine;

namespace DingoUnityExtensions.DevView
{
    public class DevDrawerView : MonoBehaviour
    {
        [SerializeField] private LinePool _linePool;
        [SerializeField] private PointPool _pointPool;

        public PoolBehaviour<Point> Points => _pointPool;
        public PoolBehaviour<Line> Lines => _linePool;
    }
}
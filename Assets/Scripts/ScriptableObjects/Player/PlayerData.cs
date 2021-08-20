using UnityEngine;

namespace ScriptableObjects.Player
{
    [CreateAssetMenu(menuName = "Create Player Data", fileName = "New Player Data")]
    public class PlayerData : ScriptableObject
    {
        public AnimationCurve _animationCurve;
        public float _time;
        public float _height;
        public float _gravity;
        public int _distance;
        public int _offset;
    }
}
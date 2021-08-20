using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [Serializable]
    public class PlayerView
    {
        [SerializeField] private Gradient _gradient;

        public void Init(IReadOnlyList<FollowSystem> parts)
        {
            var gradientEvaluate = 0f;
            var inceaseValue = 1f / parts.Count;

            for (int i = 0; i < parts.Count; i++)
            {
                parts[i].GetComponentInChildren<Renderer>().material.color = _gradient.Evaluate(gradientEvaluate);
                gradientEvaluate += inceaseValue;
            }
        }
    }
}
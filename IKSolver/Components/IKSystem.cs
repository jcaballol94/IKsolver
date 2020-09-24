using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/System")]
    public class IKSystem : MonoBehaviour
    {
        [SerializeField] private int _numIterations = 10;
        [SerializeField] [Range(0f, 1f)] private float _rootPull = 1f;

        [Header("Debug")]
        public bool RunNormally = true;
        public bool RunOneIteration = false;
        public bool PullEnds = false;
        public bool PullRoots = false;

        private Bone _root;
        private Vector3 _rootBasePosition;
        private Quaternion _rootBaseRotation;

        private void Start()
        {
            // Find the first bone of the hierarchy
            _root = GetComponentInChildren<Bone>();
            // Explore the rest of the hierarchy from there
            var constraints = new List<Constraint>();
            _root.ExploreHierarchy(constraints);
            for (int i = 0; i < constraints.Count; ++i)
            {
                constraints[i].RegisterMessages();
            }

            // Initialize all the bones
            _root.Initialize(transform.forward);

            _rootBasePosition = transform.InverseTransformPoint(_root.Position);
            _rootBaseRotation = Quaternion.Inverse(transform.rotation) * _root.Rotation;
        }

        private void LateUpdate()
        {
            if (RunNormally) RunOneIteration = true;
            if (RunOneIteration) PullEnds = PullRoots = true;

            for (int i = 0; i < _numIterations; ++i)
            {
                if (PullEnds)
                {
                    _root.PullEnds();
                    PullEnds = false;
                }

                if (PullRoots)
                {
                    _root.Position = Vector3.Lerp(_root.Position, transform.TransformPoint(_rootBasePosition), _rootPull);
                    _root.Rotation = Quaternion.Slerp(_root.Rotation, transform.rotation * _rootBaseRotation, _rootPull);
                    _root.PullRoot();
                    PullRoots = false;
                    _root.transform.position = _root.Position;
                }

                RunOneIteration = false;

                if (!RunNormally) break;
            }
            _root.ApplyTransform();
        }
    }
}
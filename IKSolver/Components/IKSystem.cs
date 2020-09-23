using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/System")]
    public class IKSystem : MonoBehaviour
    {
        [SerializeField] private int _numIterations = 10;

        [Header("Debug")]
        public bool RunNormally = true;
        public bool RunOneIteration = false;
        public bool PullEnds = false;
        public bool PullRoots = false;

        private Bone _root;
        private Vector3 _rootBasePosition;
        private Quaternion _rootBaseRotation;

        private Constraint[] _constraints;

        private void Start()
        {
            // Find all the constraints
            _constraints = GetComponents<Constraint>();

            // Find the first bone of the hierarchy
            _root = GetComponentInChildren<Bone>();
            // Explore the rest of the hierarchy from there
            _root.ExploreHierarchy();

            // Now that we know the hierarchy, register the messages accordingly
            for (int i = 0; i < _constraints.Length; ++i)
            {
                _constraints[i].RegisterMessages();
            }

            // Initialize all the bones
            _root.Initialize();

            _rootBasePosition = _root.transform.parent.InverseTransformPoint(_root.Position);
            _rootBaseRotation = Quaternion.Inverse(_root.transform.parent.rotation) * _root.Rotation;
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
                    _root.Position = _root.transform.parent.TransformPoint(_rootBasePosition);
                    _root.transform.localRotation = _root.transform.parent.rotation * _rootBaseRotation;
                    _root.PullRoot();
                    PullRoots = false;
                }

                RunOneIteration = false;

                if (!RunNormally) break;
            }
            _root.ApplyTransform();
        }
    }
}
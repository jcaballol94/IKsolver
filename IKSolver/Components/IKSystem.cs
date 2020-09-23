using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/System")]
    public class IKSystem : MonoBehaviour
    {
        [SerializeField] private int _numIterations = 10;

        private Bone _root;
        private Vector3 _rootBasePosition;
        private Quaternion _rootBaseRotation;

        private void Start()
        {
            // Find the first bone of the hierarchy
            _root = GetComponentInChildren<Bone>();
            // Explore the rest of the hierarchy from there
            _root.ExploreHierarchy();
            // Initialize all the bones
            _root.Initialize();

            _rootBasePosition = _root.transform.parent.InverseTransformPoint(_root.Position);
            _rootBaseRotation = Quaternion.Inverse(_root.transform.parent.rotation) * _root.Rotation;
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _numIterations; ++i)
            {
                _root.PullEnds();
                _root.Position = _root.transform.parent.TransformPoint(_rootBasePosition);
                _root.transform.localRotation = _root.transform.parent.rotation * _rootBaseRotation;
                _root.PullRoot();
            }
            _root.ApplyTransform();
        }
    }
}
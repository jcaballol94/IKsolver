using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKSolver
{
    public abstract class IKJoint : MonoBehaviour
    {
        private IKSolver _solver;
        private IKBoneBase _parent;

        private Vector3 _position;

        private Quaternion _baseOrientation;
        private float _length;

        private void ExploreHierarchy (IKSolver parentSolver) 
        {
            _solver = parentSolver;
        }

        private void Initialize ()
        {
            _position = transform.position;
            _baseOrientation = Quaternion.Inverse(GetRotation()) * transform.rotation;

            _length = Vector3.Distance(_position, _parent._position);
        }

        private Vector3 GetDesiredParentPosition ()
        {
            var toParent = (_parent._position - _position).normalized;
            return _position + toParent * _length;
        }

        private void PullEnds () { }

        private void PullRoot (Vector3 desiredPos)
        {
            _position = desiredPos;
        }

        private Quaternion GetRotation()
        {
            return Quaternion.identity;
        }

        private void ApplyTransform ()
        {
            transform.position = _position;
            transform.rotation = GetRotation() * _baseOrientation;
        }
    }
}
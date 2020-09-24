using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraints/Copy Orientation")]
    public class CopyOrientation : Constraint
    {
        [SerializeField] private Bone _target;
        [SerializeField] private Vector3 _referenceAxis = Vector3.up;
        [SerializeField] private bool _setAsUp = true;

        public override void RegisterMessages()
        {
            _bone.onPostPullEnds.AddListener(SetOrientation);
        }

        private void SetOrientation()
        {
            var forwardVector = _bone.Rotation * Vector3.forward;
            var upVector = _target.Rotation * _referenceAxis;
            
            if (!_setAsUp)
            {
                upVector = Vector3.Cross(forwardVector, upVector).normalized;
            }

            _bone.Rotation = Quaternion.LookRotation(forwardVector, upVector);
        }
    }
}
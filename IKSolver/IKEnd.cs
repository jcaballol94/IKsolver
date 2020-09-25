using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone End")]
    public class IKEnd : Bone, IOrientationProvider
    {
        public Transform target;

        private Quaternion _targetLocalRotation;

        public override void Initialize()
        {
            orientationProvider = this;
            base.Initialize();
        }

        protected override void InitializeRotation()
        {
            var rotation = Quaternion.LookRotation(Parent.GetForwardVector(), Parent.GetUpVector());
            _targetLocalRotation = Quaternion.Inverse(target.rotation) * rotation;

            base.InitializeRotation();
        }

        public override void PullEnds()
        {
            // Match the position of the target
            Position = target.position;
        }

        public Vector3 UpVector
        {
            get
            {
                return target.rotation * _targetLocalRotation * Vector3.up;
            }
        }

        public override Vector3 GetForwardVector()
        {
            return target.rotation * _targetLocalRotation * Vector3.forward;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (target)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position, 0.05f);
            }
        }
    }
}
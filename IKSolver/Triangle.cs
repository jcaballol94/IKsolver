using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone Triangle")]
    public class Triangle : Bone, IOrientationProvider
    {
        [SerializeField] private Bone _boneA;
        [SerializeField] private Bone _boneB;

        private float _halfBase;
        private float _tipToBase;

        public override void Initialize()
        {
            orientationProvider = this;
            _boneA.orientationProvider = this;
            _boneB.orientationProvider = this;

            base.Initialize();

            _halfBase = Vector3.Distance(_boneA.Position, _boneB.Position) * 0.5f;
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            _tipToBase = Vector3.Distance(Position, midPoint);
        }

        public override void PullEnds()
        {
            // First update the children
            for (int i = 0; i < children.Count; ++i)
            {
                children[i].PullEnds();
            }

            // Get the desired position for the midpoint
            var AtoB = _boneB.Position - _boneA.Position;
            var midPoint = _boneA.Position + AtoB * 0.5f;

            // Get the desired position for the tip
            var up = GetUpVector();
            var toTip = Vector3.Cross(up, AtoB).normalized;
            var desiredTip = midPoint + toTip * _tipToBase;

            // Add the influence of the other children
            if (children.Count > 2)
            {
                desiredTip *= 2f;
                for (int i = 0; i < children.Count; ++i)
                {
                    if (children[i] != _boneA && children[i] != _boneB)
                    {
                        desiredTip += children[i].GetDesiredParentPosition();
                    }
                }
                desiredTip /= (children.Count);
            }
            Position = desiredTip;

            // Calculate the final midpoint
            var toMidPoint = (midPoint - Position).normalized;
            midPoint = Position + toMidPoint * _tipToBase;

            // Calculate the final position of the children
            AtoB = Vector3.Cross(GetUpVector(), toMidPoint);
            _boneA.Position = midPoint - AtoB * _halfBase;
            _boneB.Position = midPoint + AtoB * _halfBase;
        }

        public override void PullRoot()
        {
            if (Parent)
            {
                var fromParent = (Position - Parent.Position).normalized;
                Position = Parent.Position + fromParent * _length;
            }

            var forward = GetForwardVector();
            var midPoint = Position + forward * _tipToBase;
            var up = GetUpVector();
            var AtoB = Vector3.Cross(up, forward);
            _boneA.Position = midPoint - AtoB * _halfBase;
            _boneB.Position = midPoint + AtoB * _halfBase;

            for (int i = 0; i < children.Count; ++i)
            {
                // Allow them to pull their own children
                children[i].PullRoot();
            }
        }

        public Vector3 UpVector
        {
            get
            {
                var toA = _boneA.Position - Position;
                var toB = _boneB.Position - Position;
                return Vector3.Cross(toA, toB).normalized;
            }
        }
        public override Vector3 GetForwardVector()
        {
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            return (midPoint - Position).normalized;
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            Gizmos.color = Color.white;
            if (Application.isPlaying)
            {
                Gizmos.DrawLine(_boneA.Position, _boneB.Position);
            }
            else if (_boneA && _boneB)
            {
                Gizmos.DrawLine(_boneA.transform.position, _boneB.transform.position);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraints/Triangle")]
    public class ConstraintTriangle : Constraint
    {
        [SerializeField] private Bone _boneA;
        [SerializeField] private Bone _boneB;

        private float _halfBase;
        private float _tipToBase;

        private Quaternion _tipBaseRotation;
        private Quaternion _boneABaseRotation;
        private Quaternion _boneBBaseRotation;

        public override void RegisterMessages()
        {
            _bone.onPostInitialize.AddListener(InitializeData);
            _bone.onPrePullEnds.AddListener(PullFromBase);
            _bone.onPrePullRoot.AddListener(PullFromTip);
        }

        private void UpdateBonesRotation (Vector3 up)
        {
            UpdateBoneRotation(_boneA, up, _boneABaseRotation);
            UpdateBoneRotation(_boneB, up, _boneBBaseRotation);
        }

        private static void UpdateBoneRotation(Bone bone, Vector3 up, Quaternion boneBaseRotation)
        {
            var target = bone.GetTargetPoint();
            bone.Rotation = Quaternion.LookRotation(target - bone.Position, up) * boneBaseRotation;
        }

        private void CalculateBonesBaseRotation (Vector3 up)
        {
            _boneABaseRotation = CalculateBoneBaseRotation(_boneA, up);
            _boneBBaseRotation = CalculateBoneBaseRotation(_boneB, up);
        }

        private static Quaternion CalculateBoneBaseRotation (Bone bone, Vector3 up)
        {
            var target = bone.GetTargetPoint();
            return Quaternion.Inverse(Quaternion.LookRotation(target - bone.Position, up)) * bone.Rotation;
        }

        private void InitializeData ()
        {
            _halfBase = Vector3.Distance(_boneA.Position, _boneB.Position) * 0.5f;
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            _tipToBase = Vector3.Distance(_bone.Position, midPoint);

            var up = CalculateUpVector();
            _tipBaseRotation = Quaternion.Inverse(Quaternion.LookRotation(midPoint - _bone.Position, up)) * _bone.Rotation;

            CalculateBonesBaseRotation(up);
        }

        private void PullFromBase ()
        {
            var up = CalculateUpVector();

            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            var toA = (_boneA.Position - midPoint).normalized;
            var toTip = Vector3.Cross(toA, up).normalized;

            _bone.Position = midPoint + toTip * _tipToBase;
            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;

            _bone.Rotation = Quaternion.LookRotation(midPoint - _bone.Position, up) * _tipBaseRotation;
            UpdateBonesRotation(up);
        }

        public void PullFromTip ()
        {
            var up = _bone.Rotation * Vector3.up;
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;

            var toMidPoint = (midPoint - _bone.Position).normalized;
            midPoint = _bone.Position + toMidPoint * _tipToBase;

            _bone.Rotation = Quaternion.LookRotation(toMidPoint, up);
            var toA = _bone.Rotation * Vector3.left;

            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;
            UpdateBonesRotation(up);

            _bone.Rotation *= _tipBaseRotation;
        }

        private Vector3 CalculateUpVector ()
        {
            var toA = (_boneA.Position - _bone.Position).normalized;
            var toB = (_boneB.Position - _bone.Position).normalized;

            var up = Vector3.Cross(toA, toB);
            return up.normalized;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(_boneA.Position, _boneB.Position);
            }
        }
    }
}
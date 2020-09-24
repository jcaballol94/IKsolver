﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraints/Quad")]
    public class ConstraintQuad : Constraint
    {
        [SerializeField] private Bone _boneA;
        [SerializeField] private Bone _boneB;
        [SerializeField] private Bone _boneC;

        private float _halfBase;
        private float _tipToBase;
        private float _baseToC;
        private float _CenterToBase;

        public override void RegisterMessages()
        {
            _bone.onPostInitialize.AddListener(InitializeData);
            _bone.onPrePullEnds.AddListener(PullFromBase);
            _bone.onPrePullRoot.AddListener(PullFromTip);

            _boneA.onPostInitialize.AddListener(() => OrienteBone(_boneA));
            _boneB.onPostInitialize.AddListener(() => OrienteBone(_boneB));
            _boneC.onPostInitialize.AddListener(() => OrienteBone(_boneC));
        }

        private void OrienteBone (Bone bone)
        {
            bone.Rotation = Quaternion.LookRotation(bone.Rotation * Vector3.forward, CalculateUpVector());
        }

        private void UpdateBonesRotation (Vector3 up)
        {
            UpdateBoneRotation(_boneA, up);
            UpdateBoneRotation(_boneB, up);
            UpdateBoneRotation(_boneC, up);
        }

        private static void UpdateBoneRotation (Bone bone, Vector3 up)
        {
            var target = bone.GetTargetPoint();
            bone.Rotation = Quaternion.LookRotation(target - bone.Position, up);
        }

        private void InitializeData ()
        {
            _halfBase = Vector3.Distance(_boneA.Position, _boneB.Position) * 0.5f;
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            _tipToBase = Vector3.Distance(_bone.Position, midPoint);
            _baseToC = Vector3.Distance(_boneC.Position, midPoint);

            var center = (_boneA.Position + _boneB.Position + _boneC.Position) * 0.33333f;
            var toMidPoint = (midPoint - _bone.Position).normalized;
            var toCenter = (midPoint - center);
            _CenterToBase = Vector3.Dot(toMidPoint, toCenter);

            var up = CalculateUpVector();
            _bone.Rotation = Quaternion.LookRotation(midPoint - _bone.Position, up);
        }

        private void PullFromBase ()
        {
            var up = CalculateUpVector();

            var center = (_boneA.Position + _boneB.Position + _boneC.Position) * 0.3333f;
            var toA = (_boneA.Position - _boneB.Position).normalized;
            var toTip = Vector3.Cross(toA, up).normalized;

            var midPoint = center - toTip * _CenterToBase;

            _bone.Position = midPoint + toTip * _tipToBase;
            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;
            _boneC.Position = midPoint - toTip * _baseToC;

            _bone.Rotation = Quaternion.LookRotation(midPoint - _bone.Position, up);
            UpdateBonesRotation(up);
        }

        private void PullFromTip ()
        {
            var up = _bone.Rotation * Vector3.up;
            var center = (_boneA.Position + _boneB.Position + _boneC.Position) * 0.3333f;
            
            var toMidPoint = (center - _bone.Position).normalized;
            var midPoint = _bone.Position + toMidPoint * _tipToBase;

            _bone.Rotation = Quaternion.LookRotation(toMidPoint, up);
            var toA = _bone.Rotation * Vector3.left;

            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;
            _boneC.Position = midPoint + toMidPoint * _baseToC;
            UpdateBonesRotation(up);
        }

        private Vector3 CalculateUpVector ()
        {
            var center = (_boneA.Position + _boneB.Position + _boneC.Position) * 0.3333f;

            var toCenter = (center - _bone.Position).normalized;
            var toB = (_boneB.Position - _boneA.Position).normalized;

            var up = Vector3.Cross(toCenter, toB);
            return up.normalized;
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(_boneA.Position, _boneB.Position);
                Gizmos.DrawLine(_boneA.Position, _boneC.Position);
                Gizmos.DrawLine(_boneC.Position, _boneB.Position);
            }
        }
    }
}
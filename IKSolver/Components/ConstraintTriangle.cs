﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraints/Triangle")]
    public class ConstraintTriangle : Constraint
    {
        [SerializeField] private Bone _boneA;
        [SerializeField] private Bone _boneB;

        private Bone _tip;

        private float _halfBase;
        private float _tipToBase;

        private void Awake()
        {
            _tip = GetComponent<Bone>();
        }

        public override void RegisterMessages()
        {
            _tip.onPostInitialize.AddListener(InitializeData);
            _tip.onPrePullEnds.AddListener(PullFromBase);
            _tip.onPrePullRoot.AddListener(PullFromTip);

            _boneA.onPostInitialize.AddListener(() => OrienteBone(_boneA));
            _boneB.onPostInitialize.AddListener(() => OrienteBone(_boneB));
        }

        private void OrienteBone (Bone bone)
        {
            bone.Rotation = Quaternion.LookRotation(bone.Rotation * Vector3.forward, CalculateUpVector());
        }

        private void UpdateBonesRotation (Vector3 up)
        {
            UpdateBoneRotation(_boneA, up);
            UpdateBoneRotation(_boneB, up);
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
            _tipToBase = Vector3.Distance(_tip.Position, midPoint);

            var up = CalculateUpVector();
            _tip.Rotation = Quaternion.LookRotation(midPoint - _tip.Position, up);
        }

        private void PullFromBase ()
        {
            var up = CalculateUpVector();

            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            var toA = (_boneA.Position - midPoint).normalized;
            var toTip = Vector3.Cross(toA, up).normalized;

            _tip.Position = midPoint + toTip * _tipToBase;
            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;

            _tip.Rotation = Quaternion.LookRotation(midPoint - _tip.Position, up);
            UpdateBonesRotation(up);
        }

        public void PullFromTip ()
        {
            var up = _tip.Rotation * Vector3.up;
            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;

            var toMidPoint = (midPoint - _tip.Position).normalized;
            midPoint = _tip.Position + toMidPoint * _tipToBase;

            _tip.Rotation = Quaternion.LookRotation(toMidPoint, up);
            var toA = _tip.Rotation * Vector3.left;

            _boneA.Position = midPoint + toA * _halfBase;
            _boneB.Position = midPoint - toA * _halfBase;
            UpdateBonesRotation(up);
        }

        private Vector3 CalculateUpVector ()
        {
            var toA = (_boneA.Position - _tip.Position).normalized;
            var toB = (_boneB.Position - _tip.Position).normalized;

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
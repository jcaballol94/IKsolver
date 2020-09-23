using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraint: Parent To Children Triangle")]
    public class ParentToChildrenTriangleConstraint : Constraint
    {
        [SerializeField] private Bone _boneA;
        [SerializeField] private Bone _boneB;
        [SerializeField] private Bone _parent;

        private float _halfDistance;
        private float _baseToMidpointDistance;

        public override void RegisterMessages()
        {
            var parent = _boneA.Parent;
            parent.onPoseInitialized.AddListener(CalculateDistance);
            parent.onEndsPulled.AddListener(Execute);
            parent.onRootPulled.AddListener(Execute);
        }

        private void CalculateDistance()
        {
            _halfDistance = Vector3.Distance(_boneA.Position, _boneB.Position) * 0.5f;

            var midPoint = (_boneA.Position + _boneB.Position) * 0.5f;
            _baseToMidpointDistance = Vector3.Distance(_parent.Position, midPoint);

            UpdateUpVectors();
        }

        private void UpdateUpVectors()
        {
            var toA = _boneA.Position - _parent.Position;
            var toB = _boneB.Position - _parent.Position;
            var up = Vector3.Cross(toA, toB).normalized;

            SetUpVector(up, _parent);
            SetUpVector(up, _boneA);
            SetUpVector(up, _boneB);
        }

        private static void SetUpVector (Vector3 up, Bone bone)
        {
            var forward = bone.Rotation * Vector3.forward;
            bone.Rotation = Quaternion.LookRotation(forward, up);
        }

        private void Execute()
        {
            var baseMidPoint = (_boneA.Position + _boneB.Position) * 0.5f;

            var toMidPoint = (baseMidPoint - _parent.Position).normalized;
            var adjustedMidPoint = _parent.Position + toMidPoint * _baseToMidpointDistance;

            AdjustBone(_boneA, baseMidPoint, adjustedMidPoint);
            AdjustBone(_boneB, baseMidPoint, adjustedMidPoint);

            UpdateUpVectors();
        }

        private void AdjustBone (Bone bone, Vector3 baseMidPoint, Vector3 adjustedMidPoint)
        {
            var toBone = (bone.Position - baseMidPoint).normalized;
            bone.Position = adjustedMidPoint + toBone * _halfDistance;
            var target = bone.GetTargetPoint();
            bone.Rotation = Quaternion.LookRotation(target - bone.Position);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_boneA.Position, _boneB.Position);
                Gizmos.DrawLine(_parent.Position, _boneB.Position);
                Gizmos.DrawLine(_boneA.Position, _parent.Position);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_boneA.transform.position, _boneB.transform.position);
                Gizmos.DrawLine(_parent.transform.position, _boneB.transform.position);
                Gizmos.DrawLine(_boneA.transform.position, _parent.transform.position);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Constraint: Siblings Triangle")]
    public class SiblingsTriangleConstraint : Constraint
    {
        [SerializeField] private Bone _baseA;
        [SerializeField] private Bone _baseB;
        [SerializeField] private Bone _tip;

        private float _halfDistance;
        private float _tipToMidpointDistance;

        public override void RegisterMessages()
        {
            var parent = _baseA.Parent;
            parent.onPoseInitialized.AddListener(CalculateDistance);
            parent.onEndsPulled.AddListener(Execute);
            parent.onRootPulled.AddListener(Execute);
        }

        private void CalculateDistance()
        {
            _halfDistance = Vector3.Distance(_baseA.Position, _baseB.Position) * 0.5f;

            var midPoint = (_baseA.Position + _baseB.Position) * 0.5f;
            _tipToMidpointDistance = Vector3.Distance(_tip.Position, midPoint);

            UpdateUpVectors();
        }

        private void UpdateUpVectors()
        {
            var toA = _baseA.Position - _tip.Position;
            var toB = _baseB.Position - _tip.Position;
            var up = Vector3.Cross(toA, toB).normalized;

            SetUpVector(up, _tip);
            SetUpVector(up, _baseA);
            SetUpVector(up, _baseB);
        }

        private static void SetUpVector(Vector3 up, Bone bone)
        {
            var forward = bone.Rotation * Vector3.forward;
            bone.Rotation = Quaternion.LookRotation(forward, up);
        }

        private void Execute()
        {
            var baseMidPoint = (_baseA.Position + _baseB.Position) * 0.5f;
            var toParent = (_baseA.Parent.Position - _tip.Position).normalized;
            var tipMidPoint = 

            AdjustBone(_baseA, midPoint);
            AdjustBone(_baseB, midPoint);

            var toMidPoint = (midPoint - _baseA.Parent.Position).normalized;
            _tip.Position = midPoint + toMidPoint * _tipToMidpointDistance;
            var target = _tip.GetTargetPoint();
            _tip.Rotation = Quaternion.LookRotation(target - _tip.Position);

            UpdateUpVectors();
        }

        private void AdjustBone(Bone bone, Vector3 baseMidPoint, Vector3 adjustedMidPoint)
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
                Gizmos.DrawLine(_baseA.Position, _baseB.Position);
                Gizmos.DrawLine(_baseA.Position, _tip.Position);
                Gizmos.DrawLine(_tip.Position, _baseB.Position);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(_baseA.transform.position, _baseB.transform.position);
                Gizmos.DrawLine(_baseA.transform.position, _tip.transform.position);
                Gizmos.DrawLine(_tip.transform.position, _baseB.transform.position);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class ArmIKSolver : IKSolver
    {
        [Header("Shoulder")]
        public Transform shoulder;

        [Header("Elbow")]
        public Transform elbow;
        public Transform elbowReference;

        [Header("Wrist")]
        public Transform wrist;
        public Transform wristReference;

        private void Awake()
        {
            var shoulderGo = new GameObject("ShoulderIK");

            shoulderGo.transform.parent = transform;
            shoulderGo.transform.position = shoulder.position;
            shoulderGo.transform.rotation = Quaternion.LookRotation(elbow.position - shoulder.position, transform.up);

            _rootBone = shoulderGo.AddComponent<IKBone>();
            _rootBone.RealBone = shoulder;
            _rootBone.ConstraintType = IKBone.RotationConstraintType.NONE;

            var elbowGO = new GameObject("ElbowIK");

            elbowGO.transform.parent = shoulderGo.transform;
            elbowGO.transform.position = elbow.position;
            elbowGO.transform.rotation = Quaternion.LookRotation(wrist.position - elbow.position, shoulderGo.transform.up);

            var elbowBone = elbowGO.AddComponent<IKBone>();
            elbowBone.RealBone = elbow;
            elbowBone.ConstraintType = IKBone.RotationConstraintType.HINGE;
            var elbowReferenceVector = _rootBone.transform.InverseTransformPoint(elbowReference.position) - elbowBone.transform.localPosition;
            elbowBone.ConstraintAxis = Vector3.Cross(elbowReferenceVector, Vector3.forward).normalized;
            elbowBone.Parent = _rootBone;
            _rootBone.Child = elbowBone;

            var wristGo = new GameObject("WristIK");

            wristGo.transform.parent = elbowGO.transform;
            wristGo.transform.position = wrist.position;
            wristGo.transform.rotation = Quaternion.LookRotation(wrist.position - elbow.position, elbowGO.transform.up);

            _tipBone = wristGo.AddComponent<IKBone>();
            _tipBone.RealBone = wrist;
            _tipBone.ConstraintType = IKBone.RotationConstraintType.HINGE;
            var wristReferenceVector = elbowBone.transform.InverseTransformPoint(wristReference.position) - _tipBone.transform.localPosition;
            _tipBone.ConstraintAxis = Vector3.Cross(wristReferenceVector, Vector3.forward).normalized;
            _tipBone.Parent = elbowBone;
            elbowBone.Child = _tipBone;
        }
    }
}
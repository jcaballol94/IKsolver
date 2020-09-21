using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class ArmIKSolver : IKSolver
    {
        [Header("Shoulder")]
        public Transform shoulder;
        public Transform shoulderReference;
        public float shoulderLimit = 90f;
        public Vector2 shoulderRollLimit = new Vector2(-110f, 110f);

        [Header("Elbow")]
        public Transform elbow;
        public Transform elbowReference;
        public Vector2 elbowLimits = new Vector2(0f, 150f);
        public Vector2 armRollLimit = new Vector2(0f, 150f);

        [Header("Wrist")]
        public Transform wrist;
        public Transform wristReference;
        public Vector2 wristLimits = new Vector2(-60f, 60f);

        private void Awake()
        {
            var shoulderGo = new GameObject("ShoulderIK");

            shoulderGo.transform.parent = transform;
            shoulderGo.transform.position = shoulder.position;
            shoulderGo.transform.rotation = Quaternion.LookRotation(elbow.position - shoulder.position, transform.up);

            _rootBone = shoulderGo.AddComponent<IKBone>();
            _rootBone.realBone = shoulder;

            _rootBone.constraintType = IKBone.RotationConstraintType.BALL;
            _rootBone.constraintAxis = shoulderReference.localPosition - _rootBone.transform.localPosition;
            _rootBone.constraintRotationLimits = Vector2.one * shoulderLimit;
            _rootBone.useRollConstraint = true;
            _rootBone.constraintRollLimits = shoulderRollLimit;

            var elbowGO = new GameObject("ElbowIK");

            elbowGO.transform.parent = shoulderGo.transform;
            elbowGO.transform.position = elbow.position;
            elbowGO.transform.rotation = Quaternion.LookRotation(wrist.position - elbow.position, shoulderGo.transform.up);

            var elbowBone = elbowGO.AddComponent<IKBone>();
            elbowBone.realBone = elbow;
            elbowBone.parent = _rootBone;
            _rootBone.child = elbowBone;

            elbowBone.constraintType = IKBone.RotationConstraintType.HINGE;
            var elbowReferenceVector = _rootBone.transform.InverseTransformPoint(elbowReference.position) - elbowBone.transform.localPosition;
            elbowBone.constraintAxis = Vector3.Cross(elbowReferenceVector, Vector3.forward).normalized;
            elbowBone.constraintRotationLimits = elbowLimits;
            elbowBone.useRollConstraint = true;
            elbowBone.constraintRollLimits = armRollLimit;

            var wristGo = new GameObject("WristIK");

            wristGo.transform.parent = elbowGO.transform;
            wristGo.transform.position = wrist.position;
            wristGo.transform.rotation = Quaternion.LookRotation(wrist.position - elbow.position, elbowGO.transform.up);

            _tipBone = wristGo.AddComponent<IKBone>();
            _tipBone.realBone = wrist;
            _tipBone.parent = elbowBone;
            elbowBone.child = _tipBone;

            _tipBone.constraintType = IKBone.RotationConstraintType.HINGE;
            var wristReferenceVector = elbowBone.transform.InverseTransformPoint(wristReference.position) - _tipBone.transform.localPosition;
            _tipBone.constraintAxis = Vector3.Cross(wristReferenceVector, Vector3.forward).normalized;
            _tipBone.constraintRotationLimits = wristLimits;
            _tipBone.useRollConstraint = true;
            _tipBone.constraintRollLimits = Vector2.zero;
        }
    }
}
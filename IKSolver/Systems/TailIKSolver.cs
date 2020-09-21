using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class TailIKSolver : IKSolver
    {
        public Transform root;
        public Transform tip;
        public float maxRotation = 45f;
        public bool useRollConstraint = false;
        public Vector2 RollRange = Vector2.zero;

        private void Awake()
        {
            IKBone parent = null;
            Transform tail;
            Transform head = root;

            while (head != tip)
            {
                tail = head;
                head = GetChildThatLeadsToTip(tail, tip);

                var go = new GameObject(tail.name + "IK");
                go.transform.parent = parent ? parent.transform : transform;
                go.transform.position = tail.position;
                go.transform.rotation = Quaternion.LookRotation(head.position - tail.position, parent ? parent.transform.up : transform.up);

                var bone = go.AddComponent<IKBone>();
                bone.realBone = tail;
                bone.parent = parent;
                bone.constraintType = IKBone.RotationConstraintType.BALL;
                bone.constraintAxis = Vector3.forward;
                bone.constraintRotationLimits = Vector3.one * maxRotation;
                bone.useRollConstraint = useRollConstraint;
                bone.constraintRollLimits = RollRange;

                if (parent)
                {
                    parent.child = bone;
                }
                else
                {
                    _rootBone = bone;
                }

                parent = bone;
            }

            var tipGo = new GameObject(head.name + "IK");
            tipGo.transform.parent = parent.transform;
            tipGo.transform.position = head.position;
            tipGo.transform.rotation = parent.transform.rotation;

            var tipBone = tipGo.AddComponent<IKBone>();
            tipBone.realBone = head;
            tipBone.parent = parent;
            parent.child = tipBone;
            tipBone.constraintType = IKBone.RotationConstraintType.BALL;
            tipBone.constraintAxis = Vector3.forward;
            tipBone.constraintRotationLimits = Vector3.one * maxRotation;
            tipBone.useRollConstraint = useRollConstraint;
            tipBone.constraintRollLimits = RollRange;
            _tipBone = tipBone;
        }
    }
}
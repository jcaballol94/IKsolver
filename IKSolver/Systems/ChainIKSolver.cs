using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class ChainIKSolver : IKSolver
    {
        public Transform root;
        public Transform tip;

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
                bone.RealBone = tail;
                bone.Parent = parent;
                bone.ConstraintType = IKBone.RotationConstraintType.NONE;
                if (parent)
                {
                    parent.Child = bone;
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
            tipBone.RealBone = head;
            tipBone.Parent = parent;
            parent.Child = tipBone;
            tipBone.ConstraintType = IKBone.RotationConstraintType.NONE;
            _tipBone = tipBone;
        }
    }
}
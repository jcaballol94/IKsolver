using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    public class IKBone : MonoBehaviour
    {
        public Transform RealBone { get; set; }
        public IKBone Parent { get; set; }
        public IKBone Child { get; set; }

        private Quaternion _realBoneRotation;

        private void Start()
        {
            _realBoneRotation = Quaternion.Inverse(transform.rotation) * RealBone.transform.rotation;
        }

        public void ApplyPose ()
        {
            RealBone.rotation = transform.rotation * _realBoneRotation;

            if (Child)
            {
                Child.ApplyPose();
            }
        }

        public void IterateTargetPosition (Transform tip, Transform target)
        {
            if (transform != tip)
            {
                var toTip = tip.position - transform.position;
                var toTarget = target.position - transform.position;
                var desiredRotation = Quaternion.FromToRotation(toTip, toTarget);

                var desiredForward = desiredRotation * transform.forward;
                var desiredUp= desiredRotation * transform.up;
                var desiredRight = desiredRotation * transform.right;

                transform.rotation = Quaternion.LookRotation(desiredForward, desiredUp);
            }

            if (Parent)
            {
                Parent.IterateTargetPosition(tip, target);
            }
        }
    }
}
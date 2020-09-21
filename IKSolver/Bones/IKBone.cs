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
            Debug.LogError($"{name}: {transform.rotation} * {_realBoneRotation} = {transform.rotation * _realBoneRotation}");
            RealBone.rotation = transform.rotation * _realBoneRotation;

            if (Child)
            {
                Child.ApplyPose();
            }
        }
    }
}
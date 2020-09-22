using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [AddComponentMenu("IKSolver/Bone")]
    public class FabrikBoneDefinition : MonoBehaviour
    {
        [SerializeField] private Transform _target;

        public FabrikBone CreateBones(Transform bonesParent)
        {
            // Create a bone
            var go = new GameObject(name + "IK");
            go.transform.parent = bonesParent;
            go.transform.position = transform.position;

            // Setup the component
            var bone = go.AddComponent<FabrikBone>();
            bone.RealBone = transform;
            bone.Target = _target;

            // Recursively create the children
            for (int i = 0; i < transform.childCount; ++i)
            {
                var childDefinition = transform.GetChild(i).GetComponentInChildren<FabrikBoneDefinition>();
                if (childDefinition)
                {
                    var childBone = childDefinition.CreateBones(bonesParent);
                    childBone.Parent = bone;
                    bone.Children.Add(childBone);
                }
            }

            return bone;
        }
    }
}
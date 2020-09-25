using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolverDeprecated
{
    [RequireComponent(typeof(Bone))]
    public abstract class Constraint : MonoBehaviour
    {
        protected  Bone _bone;

        private void Awake()
        {
            _bone = GetComponent<Bone>();
        }

        public abstract void RegisterMessages();
    }
}
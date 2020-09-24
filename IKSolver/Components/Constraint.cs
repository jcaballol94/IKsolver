using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jCaballol94.IKsolver
{
    [RequireComponent(typeof(Bone))]
    public abstract class Constraint : MonoBehaviour
    {
        public abstract void RegisterMessages();
    }
}
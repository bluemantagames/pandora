using UnityEngine;
using CRclone;

namespace CRclone.Combat {
    public interface ProjectileBehaviour {
        Enemy target { get; set; }
        GameObject parent { get; set; }
    }
}
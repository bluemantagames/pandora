using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CRclone.Movement;

namespace CRclone
{

    public class UnitBehaviour : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var state = GetComponent<MovementComponent>().Move();

            Debug.Log("Movement state " + state);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Network
{
    public class NetworkExceptionInjector : MonoBehaviour
    {
        public bool InjectException = false;


        // Update is called once per frame
        void Update()
        {
            if (InjectException) {
                InjectException = false;

                NetworkControllerSingleton.InjectException = true;
            }

        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Pandora.UI.HUD
{
    public class ZoomOutAnimation : MonoBehaviour
    {
        CinemachineVirtualCamera vcam;
        public bool animationStarted = false;
        public float EndSize = 6, AnimationTimeSeconds = 5;
        float? timeStart = null, timeEnd = null, startSize = null;
        AnimationCurve zoomCurve = null;

        // Update is called once per frame
        void Update()
        {
            if (animationStarted && zoomCurve == null) {
                vcam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
                zoomCurve = AnimationCurve.EaseInOut(Time.time, vcam.m_Lens.OrthographicSize, Time.time + AnimationTimeSeconds, EndSize);
            }

            if (zoomCurve != null) {
                vcam.m_Lens.OrthographicSize = zoomCurve.Evaluate(Time.time);

                if (Time.time >= zoomCurve.keys[zoomCurve.length - 1].time) {
                    animationStarted = false;
                    zoomCurve = null;
                }
            }
        }

        public void StartMatchEndAnimation() {
            animationStarted = true;
        }
    }

}
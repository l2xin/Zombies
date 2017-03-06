namespace DebugUtil
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;

    /**
     * Author: chan
     * 性能展现
     * 
    **/
    public class ShowFPS : MonoBehaviour
    {
        private const float UPDATE_INTERVAL = 1.0f;

        private float lastTime;
        private int frameCnt = 0;
        private float fps;

        void Start()
        {
            lastTime = Time.realtimeSinceStartup;
            frameCnt = 0;
        }

        void Update()
        {
            frameCnt++;
            float time = Time.realtimeSinceStartup;

            if (time >= lastTime + UPDATE_INTERVAL)
            {
                fps = (float)(frameCnt / (time - lastTime));

                lastTime = time;
                frameCnt = 0;
            }
        }

        private void OnGUI()
        {
            if (fps >= 30)
            {
                GUI.color = Color.green;
            }
            else if (fps >= 20)
            {
                GUI.color = Color.yellow;
            }
            else
            {
                GUI.color = Color.red;
            }
            GUI.Label(new Rect(200, 5, 200, 20), "fps : " + fps.ToString("0.00"));

            GUI.color = Color.green;
        }
    }
}
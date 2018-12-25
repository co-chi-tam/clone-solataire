using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CUIOrientation : MonoBehaviour
{

    [Serializable]
    public class UnityEventOrientation : UnityEvent<DeviceOrientation> {}

    public DeviceOrientation currentOrientation = DeviceOrientation.Portrait;
    public UnityEventOrientation OnOrientationChange;

    protected DeviceOrientation lastOrientation = DeviceOrientation.Portrait;

    protected WaitForSeconds m_WaitForFixedUpdate = new WaitForSeconds(0.25f);

    protected virtual void Start()
    {
        StopAllCoroutines();
        StartCoroutine(this.HandleUpdateOrientation());
    }

    protected virtual IEnumerator HandleUpdateOrientation()
    {
        while (true)
        {
            this.currentOrientation = Input.deviceOrientation;
            yield return this.m_WaitForFixedUpdate;
            if (this.lastOrientation != this.currentOrientation)
            {
                this.lastOrientation = this.currentOrientation;
                if (this.OnOrientationChange != null)
                {
                    this.OnOrientationChange.Invoke(this.currentOrientation);
                }
            }
        }
    }

    protected virtual void OnGUI()
    {
        GUI.Label (new Rect(0f, 0f, 200f, 50f), this.currentOrientation.ToString());
    }

}

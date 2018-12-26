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

    protected WaitForSeconds m_WaitForSecond = new WaitForSeconds(0.2f);
    protected WaitForFixedUpdate m_WaitForFixedUpdate = new WaitForFixedUpdate();

    protected virtual void Start()
    {
        this.currentOrientation = this.lastOrientation = Input.deviceOrientation;
        StopAllCoroutines();
        StartCoroutine(this.HandleUpdateOrientation());
    }

    protected virtual IEnumerator HandleUpdateOrientation()
    {
        while (true)
        {
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft 
                || Input.deviceOrientation == DeviceOrientation.LandscapeRight
                || Input.deviceOrientation == DeviceOrientation.Portrait 
                || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                this.currentOrientation = Input.deviceOrientation;
                yield return this.m_WaitForSecond;
                if (this.lastOrientation != this.currentOrientation)
                {
                    this.lastOrientation = this.currentOrientation;
                    if (this.OnOrientationChange != null)
                    {
                        this.OnOrientationChange.Invoke(this.currentOrientation);
                    }
                }
            }
            yield return this.m_WaitForFixedUpdate;
        }
    }

    protected virtual void OnGUI()
    {
        GUI.Label (new Rect(0f, 0f, 200f, 50f), this.currentOrientation.ToString());
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TAnimated : MonoBehaviour {

    protected WaitForFixedUpdate m_WaitTo = new WaitForFixedUpdate();
    protected Coroutine m_MoveCoroutine;

    public virtual void Stop()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// Simple move with time.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="delay"></param>
    /// <param name="position_start"></param>
    /// <param name="position_end"></param>
    /// <param name="time"></param>
    /// <param name="callback"></param>
	public virtual void MoveWithV3(
		Transform target,
        float delay,
        Vector3 position_start, Vector3 position_end,
		float time,
        Action callback)
    {
        this.Stop();
        this.m_MoveCoroutine = StartCoroutine (this.HandleMoveWithV3 (
            target,
			delay,
            position_start, position_end,
			time,
            callback));
    }

    protected IEnumerator HandleMoveWithV3 (
		Transform target,
        float wait,
        Vector3 position_start, Vector3 position_end,
        float time,
        Action callback)
    {
		target.localPosition = position_start;
        yield return this.m_WaitTo;
        // DELAY
        var curTime = wait;
        while (curTime > 0f)
        {
            yield return this.m_WaitTo;
            curTime -= Time.fixedDeltaTime;
        }
        // EXCUTE
        curTime = 0f;
        var position = position_start;
        while (curTime < time)
        {
            position = Vector3.Lerp(position_start, position_end, curTime / time);
            target.localPosition = position;
            yield return this.m_WaitTo;
            curTime += Time.fixedDeltaTime;
        }
        // UPDATE
		target.localPosition = position_end;
        if (callback != null)
        {
            callback();
        }
    }

    protected AnimationCurve m_AnimationCurve;
    /// <summary>
    /// Move object with simple curve, 
    /// animationCurve (0f, 0f);
    /// animationCurve (0.25f, -0.25f);
    /// animationCurve (0.75f, 0.25f);
    /// animationCurve (1f, 0f);
    /// </summary>
    /// <param name="target"></param>
    /// <param name="delay"></param>
    /// <param name="position_start"></param>
    /// <param name="position_end"></param>
    /// <param name="time"></param>
    /// <param name="callback"></param>
    public virtual void MoveWithCurve (
		Transform target,
        float delay,
        Vector3 position_start, Vector3 position_end,
		float time,
        Action callback)
    {
        var animationCurve = new AnimationCurve();
        animationCurve.AddKey (0f, 0f);
        animationCurve.AddKey (0.25f, -0.25f);
        animationCurve.AddKey (0.75f, 0.25f);
        animationCurve.AddKey (1f, 0f);

        this.MoveWithCurve (
            animationCurve,
            target,
			delay,
            position_start, position_end,
			time,
            callback
        );
    }

    /// <summary>
    /// Move object with additive curve.
    /// </summary>
    /// <param name="animated"></param>
    /// <param name="target"></param>
    /// <param name="delay"></param>
    /// <param name="position_start"></param>
    /// <param name="position_end"></param>
    /// <param name="time"></param>
    /// <param name="callback"></param>
    public virtual void MoveWithCurve(
        AnimationCurve animated,
		Transform target,
        float delay,
        Vector3 position_start, Vector3 position_end,
		float time,
        Action callback)
    {
        this.m_AnimationCurve = animated;

        this.Stop();
        this.m_MoveCoroutine = StartCoroutine (this.HandleMoveWithCurve (
            target,
			delay,
            position_start, position_end,
			time,
            callback));
    }

    protected IEnumerator HandleMoveWithCurve (
		Transform target,
        float wait,
        Vector3 position_start, Vector3 position_end,
        float time,
        Action callback)
    {
		target.localPosition = position_start;
        yield return this.m_WaitTo;
        // DELAY
        var curTime = wait;
        while (curTime > 0f)
        {
            yield return this.m_WaitTo;
            curTime -= Time.fixedDeltaTime;
        }
        // EXCUTE
        curTime = 0f;
        var position = position_start;
        var amplitude = (position_end - position_start).magnitude;
        while (curTime < time)
        {
            var delta = curTime / time;
            position = Vector3.Lerp(position_start, position_end, delta);
            var y = this.m_AnimationCurve.Evaluate(delta);
            position.y += y * amplitude;
            target.localPosition = position;
            yield return this.m_WaitTo;
            curTime += Time.fixedDeltaTime;
        }
        // UPDATE
		target.localPosition = position_end;
        if (callback != null)
        {
            callback();
        }
    }
	
}

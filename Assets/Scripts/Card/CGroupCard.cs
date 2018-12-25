using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGroupCard : MonoBehaviour {

	#region Fields

	public CCard selectCard;

	protected Queue<CCard> cache = new Queue<CCard>();

	protected Transform m_Transform;

	#endregion

	#region Implementation Monobehaviour

	protected virtual void Awake()
	{
		this.m_Transform = this.transform;
	}

	#endregion

	#region Object pool
	
	public virtual CCard Get()
	{
		if (this.cache.Count == 0)
			return null;
		return this.cache.Dequeue();
	}

	public virtual void Set(CCard card)
	{
		this.cache.Enqueue (card);
		// SET PARENT
		card.transform.SetParent (this.m_Transform);
		card.transform.localPosition = Vector3.zero;
		card.transform.localRotation = Quaternion.identity;
		card.transform.localScale = Vector3.one;
	} 

	#endregion

}

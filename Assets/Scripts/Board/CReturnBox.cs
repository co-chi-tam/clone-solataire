using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CReturnBox : MonoBehaviour
{
    
    protected List <CCard> m_OnBoxCards;
    public List <CCard> onBoxCards
    {
        get { return this.m_OnBoxCards; }
    }

    protected int m_MaximumCardToDisplay = 13;
    public int maximumCardToDisplay 
    {
        get { return this.m_MaximumCardToDisplay; }
        set { this.m_MaximumCardToDisplay = value; }
    }

    [SerializeField]    protected float m_Spacing = 25f;

    public virtual void Init()
    {
        this.m_OnBoxCards = new List<CCard>();

        this.m_MaximumCardToDisplay = 13;
    }

    public virtual void AddReturnBox(CCard card)
    {
        this.m_OnBoxCards.Add (card);
    }

    public virtual void Clear()
    {
        this.m_OnBoxCards.Clear();
        this.m_OnBoxCards.TrimExcess();
    }

    public virtual Vector3 GetReturnPosition()
    {
        var positionBaseCards = this.transform.localPosition;
        var amountChildActive = Mathf.FloorToInt (this.m_OnBoxCards.Count / this.m_MaximumCardToDisplay);
        positionBaseCards.x = positionBaseCards.x + (amountChildActive * this.m_Spacing);
        return positionBaseCards;
    }

}

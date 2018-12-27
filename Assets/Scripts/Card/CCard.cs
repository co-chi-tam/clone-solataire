using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CCard : MonoBehaviour, 
	IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    public static float CARD_RATIO = 0.738f;
    public static Vector2 CARD_SIZE = new Vector2 (101.5f, 137.5f);

	// K, Q, J, 10, 9, 8, 7, 6, 5, 4, 3, 2, A
	// 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 
	// sum: 91

	public enum ECardState: int {
		NONE = 0,
		FACE_UP = 1,
		FACE_DOWN = 2
	}

    #region Fields

	[Header("Configs")]
	[SerializeField]	protected bool m_ActiveCard = false;
	public bool activeCard 
	{ 
		get { return this.m_ActiveCard; }
		set 
		{ 
			this.m_ActiveCard = value; 
			if (this.m_BGImage != null)
			{
				if (this.m_StateCard == ECardState.FACE_UP)
					this.m_BGImage.color = value ? new Color32 (255, 255, 255, 255) : new Color32 (128, 128, 128, 255);
				else
					this.m_BGImage.color = new Color32 (255, 255, 255, 255);
					
				this.m_BGImage.raycastTarget = this.m_StateCard == ECardState.FACE_UP;
			}
		}
	}
	public bool activeSimpleCard 
	{ 
		get { return this.m_ActiveCard; }
		set 
		{ 
			this.m_ActiveCard = value; 
			if (this.m_BGImage != null)
			{
				this.m_BGImage.raycastTarget = this.m_StateCard == ECardState.FACE_UP;
			}
		}
	}

	[SerializeField]	protected ECardState m_StateCard = ECardState.FACE_DOWN;
	public ECardState stateCard 
	{ 
		get { return this.m_StateCard; }
		set 
		{ 
			this.m_StateCard = value; 
			// CARD BACK
			var cardPath = "Cards/card-back";
			switch (value)
			{
				default:
				case ECardState.NONE:
				case ECardState.FACE_DOWN:
					cardPath = "Cards/card-back";
					break;
				case ECardState.FACE_UP:
					cardPath = string.Format("Cards/Clover/card_{0}_clover", this.m_Value);
					break;
			}
			if (this.m_BGImage != null)
			{
				this.m_BGImage.sprite = CGameSetting.GetSpriteWithPath(cardPath);
				this.m_BGImage.raycastTarget = this.m_StateCard == ECardState.FACE_UP;
			}
		}
	}

	[SerializeField]	protected bool m_IsDropped = false;
	public bool isDropped 
	{ 
		get { return this.m_IsDropped; }
		set { this.m_IsDropped = value; }
	}
	[SerializeField]	protected int m_Value = 0;
	public int cardValue 
	{ 
		get { return this.m_Value; }
		set 
		{ 
			if (value <= 0 || value > 13)
				return;
			this.m_Value = value; 
			// CARD BACK
			var cardPath = "Cards/card-back";
			this.m_BGImage.sprite = CGameSetting.GetSpriteWithPath(cardPath);
		}
	}

    [SerializeField]	protected CColumn m_Column;
	public CColumn column 
	{ 
		get { return this.m_Column; }
		set { this.m_Column = value; }
	}
	public float heightOffset
	{
		get 
		{  
			return this.m_Column == null ? 50f : this.m_Column.heighOffset;
		}
	}
    [SerializeField]	protected CCard m_ConnectWithCard;
	public CCard connectWithCard 
	{ 
		get { return this.m_ConnectWithCard; }
		set { this.m_ConnectWithCard = value; }
	}

    protected Image m_BGImage;
	protected RectTransform m_DragObject;
	public RectTransform dragObject 
	{ 
		get { return this.m_DragObject; }
		set { this.m_DragObject = value; }
	}
    private Vector2 originalLocalPointerPosition;
    private Vector2 originalPanelLocalPosition;
    private RectTransform dragArea = null;
    private RectTransform dragAreaInternal
    {
        get
        {
            if (dragArea == null)
            {
                RectTransform canvas = transform as RectTransform;
                while (canvas.parent != null && canvas.parent is RectTransform)
                {
                    canvas = canvas.parent as RectTransform;
                }
                dragArea = canvas; 
            }
            return dragArea;
        }
    }

	protected RectTransform m_RectTransform;
	public RectTransform rectTransform 
	{ 
		get { return this.m_RectTransform; }
	}
	protected CGroupCard m_Group;

	protected bool m_IsMoving = false;

	protected Animator m_Animator;
	protected TAnimated m_MoveAnimated;

	#endregion
    
    #region Implementation Monobehaviour

	protected virtual void Start()
	{
		
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		this.OnBeginDragCard (Input.mousePosition);
	}

	public void OnDrag(PointerEventData eventData)
	{
		this.OnDragCard (Input.mousePosition);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this.OnDropCard (Input.mousePosition);
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// UI
		this.m_RectTransform = this.transform as RectTransform;
		this.m_RectTransform.anchorMin = new Vector2(0.5f, 0.5f);
		this.m_RectTransform.anchorMax = new Vector2(0.5f, 0.5f);
		this.m_DragObject = this.GetComponent<RectTransform>();
        this.m_BGImage = this.GetComponent<Image>();
		this.originalPanelLocalPosition = dragObject.localPosition;
		// GROUP
		this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
		// DROP
		this.m_IsDropped = false;
		// ANIM
		this.m_MoveAnimated = this.GetComponent<TAnimated>();
		this.m_IsMoving = false;
		// SET SIZE
		this.m_RectTransform.sizeDelta = CARD_SIZE;
	}

	public virtual void Clear()
	{
		if (this.m_DragObject == null || this.m_BGImage == null)
			return;
		// UI
		this.m_DragObject.gameObject.SetActive (true);
		// DROP
		this.m_IsDropped = false;
		// ANIM
		this.m_IsMoving = false;

        this.m_BGImage.raycastTarget = true;
	}

	public virtual void Setup(int value)
	{
		this.cardValue = value;
		this.name = string.Format("Card_{0}", cardValue);
	}

	public virtual void Move (
		float delay,
        Vector3 position_start, Vector3 position_end,
		float time,
        Action callback,
		int moveType = 0)
	{
		if (moveType == 0)
		{
			this.m_MoveAnimated.MoveWithV3 (
				this.m_RectTransform, 
				delay,
				position_start, position_end,
				time,
				callback);
		}
		else if (moveType == 1)
		{
			this.m_MoveAnimated.MoveWithCurve (
				this.m_RectTransform,
				delay,
				position_start, position_end,
				time,
				callback);
		}
	}

	public virtual void StopMove()
	{
		this.m_MoveAnimated.Stop();
	}

	public virtual bool IsCanConnect(CCard card)
	{
		if (this.m_StateCard != ECardState.FACE_UP)
			return false;
		if (card.stateCard != ECardState.FACE_UP)
			return false;
		return (this.cardValue - 1) == card.cardValue;
	}

	public virtual void IsDropOnColumn(CColumn column)
	{
		if (column == null || this.m_RectTransform == null)
			return;
		this.m_Column = column;
        this.m_RectTransform.SetParent(this.m_Column.transform);
	}

	public virtual void IsDropOnColumn()
	{
        this.IsDropOnColumn (this.m_Column);
	}

	#endregion

	#region DRAG

	public virtual void OnBeginDragCard()
	{
        this.m_RectTransform.SetParent (this.m_Group.transform);
		originalPanelLocalPosition = dragObject.localPosition;
		// FOLLOWER
		if (this.m_ConnectWithCard != null)
		{
			var currentCard = this.m_ConnectWithCard;
			var index = 0;
			while(currentCard != null)
			{
				currentCard.OnFollowCard(this, index);
				currentCard = currentCard.connectWithCard;
				index++;
			}
		}
	}

	public virtual void OnBeginDragCard(Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		// ACTIVE
		if (this.m_ActiveCard == false)
			return;
		// MOVING
		if (this.m_IsMoving)
			return;
        this.m_RectTransform.SetParent (this.m_Group.transform);
		originalPanelLocalPosition = dragObject.localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragAreaInternal, 
            position, 
            Camera.main, 
            out originalLocalPointerPosition))
        {
            this.m_Group.selectCard = this;
        }
	}

	public virtual void OnDragCard(Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		// ACTIVE
		if (this.m_ActiveCard == false)
			return;
		// MOVING
		if (this.m_IsMoving)
			return;
		if (this.m_Group.selectCard != this)
			return;
		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragAreaInternal, 
            position, 
            Camera.main, 
            out localPointerPosition))
		{
			var offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
			var move = originalPanelLocalPosition + offsetToOriginal;
			var delta = Vector3.Lerp(dragObject.localPosition, move, CGameSetting.DRAG_SPEED_THREAHOLD);
			dragObject.localPosition = delta;
            this.m_BGImage.raycastTarget = false;
			// FOLLOWER
			if (this.m_ConnectWithCard != null)
			{
				var currentCard = this.m_ConnectWithCard;
				var index = 0;
				while(currentCard != null)
				{
					currentCard.OnFollowCard(this, index);
					currentCard = currentCard.connectWithCard;
					index++;
				}
			}
		}
	}

	public virtual void OnFollowCard(CCard parent, int index)
	{
		this.m_RectTransform.SetParent (parent.transform);
		var position = Vector3.zero;
		position.y -= (index + 1) * this.GetHeightOffset();
		dragObject.localPosition = position;
        this.m_BGImage.raycastTarget = false;
	}

	public virtual void OnDropCard(Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		// IS DROPPED
		if (this.m_IsDropped == false && this.m_Column != null)
		{
			this.m_Column.RepositionAllCards();
			this.m_Column.ReconnectAllCards();
		}
	}

	public virtual void OnDropCard()
	{
		// IS DROPPED
		if (this.m_Column != null)
		{
			this.m_Column.RepositionAllCardsNonCallback();
			this.m_Column.ReconnectAllCards();
		}
	}

	#endregion

	#region GETTER && SETTER

	public virtual void SetParentInCenter(Transform parent)
	{
		var transform = this.m_RectTransform == null ? this.transform : this.m_RectTransform;
		transform.SetParent (parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale 	= Vector3.one;
	}

	public virtual void SetPosition(Vector2 value)
	{
		if(this.m_RectTransform == null)
		{
			this.transform.localPosition = value;
			return;
		}
		this.m_RectTransform.localPosition = value;
	}

	public virtual Vector2 GetPosition()
	{
		if(this.m_RectTransform == null)
		{
			return this.transform.localPosition;
		}
		return this.m_RectTransform.localPosition;
	}

	public virtual void MoveToPosition(float delay, Vector3 value, float time, Action callback = null)
	{
		if(this.m_RectTransform == null)
		{
			this.transform.localPosition = value;
			return;
		}
		var direction = value - this.m_RectTransform.localPosition;
		if (direction.sqrMagnitude >= 0.1f)
			this.Move (delay, this.m_RectTransform.localPosition, value, time, callback);
		else
		{
			if (callback != null)
			{
				callback();
			}
		}
	}

	public virtual void MoveToPosition(Vector3 value, Action callback = null)
	{
		this.MoveToPosition(0f, value, 0.1f, callback);
	}

	public virtual void CurveToPosition(float delay, Vector3 value, float time, int type = 1, Action callback = null)
	{
		if(this.m_RectTransform == null)
		{
			this.transform.localPosition = value;
			return;
		}
		var direction = value - this.m_RectTransform.localPosition;
		if (direction.sqrMagnitude >= 0.1f)
			this.Move (delay, this.m_RectTransform.localPosition, value, time, callback, type);
		else
		{
			if (callback != null)
			{
				callback();
			}
		}
	}

	public float GetHeightOffset()
	{
		return this.m_Column == null ? 50f : this.m_Column.heighOffset;
	}

	#endregion

	#region ULTILITES

	#endregion

}

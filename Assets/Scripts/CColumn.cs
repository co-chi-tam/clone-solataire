using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CColumn : MonoBehaviour, 
	IDropHandler,
	IPointerEnterHandler, IPointerExitHandler
{

    public static int PORTRAIT_MAXIMUM_CARD = 26;
    public static float PORTRAIT_SPACING = 50f;
    public static int LANDSCAPE_MAXIMUM_CARD = 13;
    public static float LANDSCAPE_SPACING = 30f;

    #region Implementation Fields

	[Header("Configs")]
	[SerializeField]	protected float m_HeighOffset = 50f;
	public float heighOffset 
	{ 
		get { return this.m_HeighOffset; }	
		protected set { this.m_HeighOffset = value; }
	}

	[SerializeField]	protected List<CCard> m_Cards;
	public List<CCard> cards 
	{ 
		get { return this.m_Cards; }
		set { this.m_Cards = value; }
	}
	[SerializeField]	protected List<CCard> m_AvailableCards = new List<CCard>();
	/// <summary>
    /// REVERSE WITH m_Cards
    /// </summary>
	public List<CCard> availableCards 
	{ 
		get { return this.m_AvailableCards; }
	}

	protected CGroupCard m_Group;
	protected CBoard m_Board;

	protected VerticalLayoutGroup m_LayoutGroup;
	protected WaitForFixedUpdate m_WaitFixedUpdate = new WaitForFixedUpdate();
    protected RectTransform m_RectTransform = null;
	public RectTransform rectTransform 
	{ 
		get { return this.m_RectTransform; }
	}

	protected List<CCard> m_ReturnCards = new List<CCard>();
	public List<CCard> returnCards 
	{ 
		get { return this.m_ReturnCards; }
	}

	#endregion

    #region Implementation Monobehaviour

	protected virtual void Start()
	{
		
	}

	protected virtual void OnGUI()
	{
		
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// UI
		this.m_RectTransform = this.transform as RectTransform;
		this.m_RectTransform.pivot = new Vector2(0.5f, 1f);
		// GROUP
		this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		this.m_LayoutGroup = this.GetComponent<VerticalLayoutGroup>();
		this.m_LayoutGroup.enabled = false;
		// ORIENTATION
		this.CalculateBaseOrientation(Input.deviceOrientation);
		var orientationManager = GameObject.FindObjectOfType<CUIOrientation>();
		orientationManager.OnOrientationChange.AddListener (this.CalculateBaseOrientation);
		// CARDS
		this.m_Cards = new List<CCard>();
	}

	public virtual void CalculateBaseOrientation(DeviceOrientation orientation)
	{
		// CALCULATE HEIGH OFFSET
		if (orientation == DeviceOrientation.LandscapeLeft 
			|| orientation == DeviceOrientation.LandscapeRight)
		{
			this.m_HeighOffset = LANDSCAPE_SPACING;
		}
		else if (orientation == DeviceOrientation.Portrait 
			|| orientation == DeviceOrientation.PortraitUpsideDown)
		{
			this.m_HeighOffset = PORTRAIT_SPACING;
		}
		// UPDATE
		this.RepositionAllCards();
		this.ReconnectAllCards();
	}

	public virtual void Clear()
	{
		this.m_Cards.Clear();
		this.m_Cards.TrimExcess();

		this.m_ReturnCards.Clear();
		this.m_ReturnCards.TrimExcess();

		this.m_AvailableCards.Clear();
		this.m_AvailableCards.TrimExcess();
	}

	public virtual void AddCard(CCard card)
	{
        if (card == null)
            return;
        if (card.column != null)
        {
            card.column.RemoveCard(card);
			card.column.RepositionAllCards();
			card.column.ReconnectAllCards();
        }
        var currentCard = card;
        while(currentCard != null)
        {
            this.AddCardToColumn(currentCard);
            currentCard = currentCard.connectWithCard;
        }
		// UPDATE
		this.RepositionAllCards();
		this.ReconnectAllCards();
		
	}
    
    public virtual void RemoveCard(CCard card)
	{
        if (card == null)
            return;
        var currentCard = card;
        while(currentCard != null)
        {
			if (this.m_Cards.Contains(currentCard))
			{
            	this.m_Cards.Remove (currentCard);
			}
			
            currentCard = currentCard.connectWithCard;
        }
		if (this.m_AvailableCards.Contains(card))
		{
			this.m_AvailableCards.Clear();
			this.m_AvailableCards.TrimExcess();
		}
		if (this.m_ReturnCards.Contains(card))
		{
			this.m_ReturnCards.Clear();
			this.m_ReturnCards.TrimExcess();
		}
	}

    public virtual void AddCardToColumn(CCard card)
	{
        card.isDropped = true;
		card.column = this;
        card.transform.SetParent(this.m_RectTransform);
        card.MoveToPosition (this.GetLastCardPosition());
		card.Clear();
		// ADD TO LIST
        this.m_Cards.Add (card);
    }

	public virtual void AddCardToList(CCard card)
	{
        card.isDropped = true;
		card.column = this;
		card.Clear();
		// ADD TO LIST
        this.m_Cards.Add (card);
    }

	public virtual void RepositionAllCards()
	{
		if (this.m_Cards.Count == 0)
			return;
		lock (this.m_Cards)
		{
			var allCardsComplete = 0;
			var nextCard = this.m_Cards[0];
			nextCard.transform.SetParent(this.m_RectTransform);
			nextCard.MoveToPosition (this.GetCardPositionWith(0), () => {
				allCardsComplete++;
			});
			nextCard.Clear();
			var lastCard = this.m_Cards[this.m_Cards.Count - 1];
			for (int i = 1; i < this.m_Cards.Count; i++)
			{
				var card = this.m_Cards[i];
				card.transform.SetParent(this.m_RectTransform);
				card.MoveToPosition (this.GetCardPositionWith(i), () => {
					allCardsComplete++;
					if (allCardsComplete == this.m_Cards.Count)
					{
						// CHECK VALUES
						this.CheckAllCardValues();
					}
				});
				card.Clear();
				nextCard = card;
			}
		}
	}

	public virtual void RepositionAllCardsNonCallback()
	{
		if (this.m_Cards.Count == 0)
			return;
		lock (this.m_Cards)
		{
			for (int i = 0; i < this.m_Cards.Count; i++)
			{
				var card = this.m_Cards[i];
				card.StopMove();
				card.transform.SetParent(this.m_RectTransform);
				card.SetPosition (this.GetCardPositionWith(i));
				card.Clear();
			}
			// CHECK VALUES
			this.CheckAllCardValues();
		}
	}

	public virtual void HightLightCards()
	{
		// ALL CARD FREE
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			var card = this.m_Cards[i];
			card.activeCard = false;
		}
		// LAST CARD
		var nextCard = this.m_Cards[this.m_Cards.Count - 1];
		nextCard.activeCard = true;
		// ALL CARD CAN COONECT
		for (int i = this.m_Cards.Count - 2; i >= 0; i--)
		{
			var card = this.m_Cards[i];
			if (card.IsCanConnect (nextCard))
			{
				// RECONNECT
				card.connectWithCard = nextCard;
				card.activeCard = true;
				nextCard = card;
			}
			else
			{
				break;
			}
		}
	}

	public virtual void ReconnectAllCards()
	{
		if (this.m_Cards.Count == 0)
			return;
		// CLEAR AVAILABLE CARD
		this.m_AvailableCards.Clear();
		this.m_AvailableCards.TrimExcess();
		// ALL CARD FREE
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			var card = this.m_Cards[i];
			card.connectWithCard = null;
			card.IsDropOnColumn(this);
			card.Clear();
			card.activeCard = false;
		}
		// LAST CARD
		var nextCard = this.m_Cards[this.m_Cards.Count - 1];
		nextCard.activeCard = true;
		// AVAILABLE CARDS
		this.m_AvailableCards.Add(nextCard);
		// ALL CARD CAN COONECT
		for (int i = this.m_Cards.Count - 2; i >= 0; i--)
		{
			var card = this.m_Cards[i];
			card.IsDropOnColumn(this);
			card.Clear();
			if (card.IsCanConnect (nextCard))
			{
				// RECONNECT
				card.connectWithCard = nextCard;
				card.activeCard = true;
				// AVAILABLE CARDS
				this.m_AvailableCards.Add(card);
				nextCard = card;
			}
			else
			{
				break;
			}
		}
	}

	public virtual void CheckAllCardValues()
	{
		// CHECK ALL CARDS
		if (this.CheckAllCardValues(ref this.m_ReturnCards))
		{
			// BLOCK
			CBoard.BOARD_LOCK = true;
			var delay = 0.1f;
			var timer = 0.5f;
			var randomType = Random.Range(0, 2); // SIMPLE MOVE | CURVE MOVE
			var countComplete = 0;
			var maxComplete = this.m_ReturnCards.Count;
			var returnPoint = this.m_Board.GetReturnCardPoint();
			for (int i = 0; i < this.m_ReturnCards.Count; i++)
			{
				var resCard = this.m_ReturnCards[i];
				resCard.activeSimpleCard = false;
				resCard.transform.SetParent(this.m_Board.transform);
				resCard.CurveToPosition (timer + (delay * i), returnPoint, timer, randomType, () => {
					countComplete++;
					if (countComplete == maxComplete)
					{
						this.CheckAllCardValuesComplete();
					}
				});
				this.m_Cards.Remove (resCard);
				this.m_Board.ReturnCardToBox (resCard);
			}	
		}
	}

	public virtual void CheckAllCardValuesComplete()
	{
		// UPDATE
		// this.RepositionAllCards();
		this.ReconnectAllCards();
		// BLOCK
		CBoard.BOARD_LOCK = false;
	}

	public virtual bool CheckAllCardValues(ref List<CCard> cardResults)
	{
		return CheckAllCardValues(this.m_Cards, ref cardResults);
	}

	public virtual bool CheckAllCardValues(List<CCard> inputCards, ref List<CCard> cardResults)
	{
		if (cardResults != null)
		{
			cardResults.Clear();
			cardResults.TrimExcess();
		}
		if (inputCards.Count == 0)
			return false;

		var hasK = false;
		var result = 0;
		for (int i = 0, k = 0; i < inputCards.Count - 1; i++)
		{
			var card = inputCards[i];
			// START WITH K == 13
			if (card.cardValue == 13)
			{
				hasK = (i + 1) < inputCards.Count - 1;
				result = 13;
				if (cardResults != null)
				{
					cardResults.Clear();
					cardResults.TrimExcess();
					cardResults.Add (card);
				}
			}
			// HAS K AND LOOP
			if (hasK)
			{
				k = i + 1;
				var nextCard = inputCards[k];
				if (card.IsCanConnect(nextCard))
				{
					result += nextCard.cardValue;
					if (cardResults != null)
					{
						cardResults.Add (nextCard);
					}
				}
				else
				{
					hasK = false;
				}
			}
		}
		// 1 + 2 + 3 + ... + 12 + 13
		return result == 91;
	}

	public virtual bool IsAvailableCard(CCard card)
	{
		if (card == null)
			return false;
		if (this.m_Cards.Count == 0)
			return true;
		if (this.m_Cards.Contains (card))
			return false;
		var lastCard = this.m_Cards[this.m_Cards.Count - 1];
		return lastCard.IsCanConnect(card);
	}

	public virtual void OnCardDrop (Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		// DROPPED
		if (this.m_Group != null && this.m_Group.selectCard != null) // HAVE GROUP/ SELECT CARD
		{
			if (this.IsAvailableCard(this.m_Group.selectCard))
			{
				// ADD CARD
				this.AddCard (this.m_Group.selectCard);
			}
			else
			{
				// CANCEL
				var column = this.m_Group.selectCard.column;
				if (column != null)
					column.ReconnectAllCards();
			}
			this.m_Group.selectCard = null;
		}
		// HIGHLIGHT
		this.SetHighlight(false);
	}

	public virtual void OnCardOnPointerEnter (Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		// GROUP
		if (this.m_Group == null)
			return;
		if (this.m_Group.selectCard == null)
			return;
		this.SetHighlight(true);
	}

	public virtual void OnCardOnPointerExit (Vector2 position)
	{
		if (CBoard.BOARD_LOCK)
			return;
		this.SetHighlight(false);
	}

	#endregion

	#region IPointer

	public void OnDrop (PointerEventData eventData)
	{
		this.OnCardDrop (Input.mousePosition);
	}

	public void OnPointerEnter(PointerEventData eventData)
    {
		this.OnCardOnPointerEnter (Input.mousePosition);
	}

	public void OnPointerExit(PointerEventData eventData)
    {
		this.OnCardOnPointerExit (Input.mousePosition);
	}

	#endregion

	#region GETTER && SETTER

	public virtual Vector3 GetCardPositionWith(int index)
	{
		var lastPosition = CCard.CARD_SIZE;
		lastPosition.x = 0f; // lastPosition.x / 2f;
		lastPosition.y = -lastPosition.y / 2f - (index * this.m_HeighOffset);
		return lastPosition;
	}

	public virtual Vector3 GetLastCardPosition()
	{
		return this.GetCardPositionWith(this.m_Cards.Count);
	}

	public virtual Vector3 GetLastCardWorldPosition()
	{
		var localPosition = this.GetLastCardPosition();
		return this.m_RectTransform.TransformPoint(localPosition);
	}

	public virtual Vector3 GetCardWorldPosition(CCard card)
	{
		var index = this.m_Cards.IndexOf (card);
		return this.m_RectTransform.TransformPoint(this.GetCardPositionWith(index));
	}

	public virtual void SetHighlight(bool value)
	{
		// if (this.m_Cards.Count > 0)
		// {
		// 	for (int i = 0; i < this.m_Cards.Count; i++)
		// 	{
		// 		if (value)
		// 			this.m_Cards[i].SetHighlight(i == this.m_Cards.Count - 1);	
		// 		else
		// 			this.m_Cards[i].SetHighlight(false);
		// 	}
		// }
	}

	#endregion

}

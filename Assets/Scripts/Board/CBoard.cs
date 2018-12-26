using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CBoard : MonoBehaviour
{
    public static int PORTRAIT_MAXIMUM_CARD = 26;
    public static float PORTRAIT_SPACING = 50f;
    public static int LANDSCAPE_MAXIMUM_CARD = 13;
    public static float LANDSCAPE_SPACING = 30f;
    
    #region Fields

    [SerializeField]    protected CColumn[] m_Columns;
    [SerializeField]    protected CCard m_CardPrefab;

    protected int[] m_DataCards = new int[] 
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,

        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13
    };

    protected List <CCard> m_OnBoardCards;
    protected List <CCard> m_OnHintCards;

    protected CReturnBox m_ReturnBox;

    protected RectTransform m_SpawnCardPoint;

    protected Button m_DrawCardsToColumnsButton;
    protected Button m_ResetButton;
    protected Button m_HintButton;

    protected RectTransform m_RectTransform = null;
	public RectTransform rectTransform 
	{ 
		get { return this.m_RectTransform; }
	}

    protected CGroupCard m_Group;

    public static bool BOARD_LOCK = false;

    // HINT
    protected bool m_IsActiveHint = false;
	protected WaitForSeconds m_WaitShortTime = new WaitForSeconds(2f);
    protected Coroutine m_HintCoroutine;

    // ORIENTATION
    protected Coroutine m_HandleOrientation;

    #endregion

    #region Implementation Monobehaviour

    protected virtual void Start()
    {
        this.Init();

        Input.multiTouchEnabled = false;
    }

    public virtual void Update()
    {
        if (this.m_IsActiveHint)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.OnCancelHint();
            }
        }
    }

    #endregion

    #region Main methods

    public virtual void Init()
    {
        // UI
		this.m_RectTransform = this.transform as RectTransform;
        // GROUP
        this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
        // COLUMN
        this.m_Columns = this.transform.GetComponentsInChildren<CColumn>();
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            this.m_Columns[i].Init();
        }
        // CARD
        this.m_CardPrefab = Resources.Load<CCard>("Cards/Card");
        // POINTS
        this.m_ReturnBox = this.transform.Find("ReturnCardPoint").GetComponent<CReturnBox>();
        this.m_SpawnCardPoint = this.transform.Find("SpawnCardPoint").GetComponent<RectTransform>();
        this.m_DrawCardsToColumnsButton = this.transform.Find("DrawCardsToColumnsButton").GetComponent<Button>();
        this.m_DrawCardsToColumnsButton.onClick.AddListener(this.OnNextDraw);
        this.m_ResetButton = this.transform.Find("ResetButton").GetComponent<Button>();
        this.m_ResetButton.onClick.AddListener(this.OnResetMatch);
        this.m_HintButton = this.transform.Find("HintButton").GetComponent<Button>();
        this.m_HintButton.onClick.AddListener(this.OnActiveHint);
        // DRAW CARDS
        this.m_OnBoardCards = new List <CCard>();
        for (int i = 0; i < this.m_DataCards.Length; i++)
        {
            var value = this.m_DataCards[i];
            var card = Instantiate (this.m_CardPrefab);
            card.Init();
            card.Setup (value);
            card.SetParentInCenter (this.m_SpawnCardPoint);
            this.m_OnBoardCards.Add (card);
        }
        this.m_ReturnBox.Init();
        this.m_OnHintCards = new List<CCard>();
        // BLOCK
		CBoard.BOARD_LOCK = true;
		// ORIENTATION
		this.CalculateBaseOrientation(Input.deviceOrientation);
		var orientationManager = GameObject.FindObjectOfType<CUIOrientation>();
		orientationManager.OnOrientationChange.AddListener (this.CalculateBaseOrientation);
        // SHUFFLE
        this.ShuffleCards();
        // On START DRAW
        Invoke("OnStartDraw", 1f);
    }

    public virtual void Clear()
    {
        // COLUMN
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            var column = this.m_Columns[i];
            var columnCards = column.cards;
            this.m_OnBoardCards.AddRange (columnCards);
            column.Clear();
        }
        // ON BOX
        this.m_OnBoardCards.AddRange (this.m_ReturnBox.onBoxCards);
        this.m_ReturnBox.Clear();
        // BOARD
        for (int i = 0; i < this.m_OnBoardCards.Count; i++)
        {
            var card = this.m_OnBoardCards[i];
            card.StopMove();
            card.SetParentInCenter (this.m_SpawnCardPoint);
            card.Clear();
        }
    }

    public virtual void CalculateBaseOrientation(DeviceOrientation orientation)
	{
        if (this.m_HandleOrientation != null)
            StopCoroutine (this.m_HandleOrientation);
            
        this.m_HandleOrientation = StartCoroutine (this.HandleOnOrientationDevice(orientation));
	}

    protected virtual IEnumerator HandleOnOrientationDevice(DeviceOrientation orientation)
    {
        // LOCK
        yield return BOARD_LOCK;
        // DEVICE TO SCREEN
        Screen.orientation = CGameSetting.DeviceToScreenOrientation (orientation);
        // LOGIC
        var heighOffset = 50f;
        // CALCULATE HEIGH OFFSET
		if (orientation == DeviceOrientation.LandscapeLeft 
			|| orientation == DeviceOrientation.LandscapeRight)
		{
			heighOffset = LANDSCAPE_SPACING;
		}
		else if (orientation == DeviceOrientation.Portrait 
			|| orientation == DeviceOrientation.PortraitUpsideDown)
		{
			heighOffset = PORTRAIT_SPACING;
		}
		// UPDATE 
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            this.m_Columns[i].heighOffset = heighOffset;
            this.m_Columns[i].RepositionAllCardsNonCallback();
            // this.m_Columns[i].ReconnectAllCards();
        }
    }

    public virtual void OnStartDraw()
    {
        var toTransform = this.m_SpawnCardPoint;
        this.DrawCardsToColumns(54, toTransform, (i, card) => {
            if (i < 45)
                card.stateCard = CCard.ECardState.FACE_DOWN;
            else
                card.stateCard = CCard.ECardState.FACE_UP;
        });
    }

    public virtual void OnNextDraw()
    {
        // BLOCK
        if (BOARD_LOCK)
            return;
        var toTransform = this.m_DrawCardsToColumnsButton.transform;
        this.DrawCardsToColumns(10, toTransform, (i, card) => {
            card.stateCard = CCard.ECardState.FACE_UP;
        });
    }

    public virtual void OnResetMatch()
    {
        // BLOCK
        if (BOARD_LOCK)
            return;
        // BLOCK
		BOARD_LOCK = true;
        // CLEAR
        this.Clear();
        // SHUFFLE
        this.ShuffleCards();
        // On START DRAW
        Invoke("OnStartDraw", 1f);
    }

    public virtual void OnActiveHint()
    {
        // BLOCK
        if (BOARD_LOCK)
            return;
        // BLOCK
        BOARD_LOCK = true;
        this.m_IsActiveHint = true;
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            this.m_Columns[i].RepositionAllCards();
            this.m_Columns[i].ReconnectAllCards();
        }
        if (this.m_HintCoroutine != null)
            StopCoroutine(this.m_HintCoroutine);

        this.m_HintCoroutine = StartCoroutine(this.HandleOnActiveHint());
    }

    public virtual void OnCancelHint()
    {
        // BLOCK
        BOARD_LOCK = false;
        this.m_IsActiveHint = false;

        if (this.m_HintCoroutine != null)
            StopCoroutine(this.m_HintCoroutine);
            
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            this.m_Columns[i].RepositionAllCardsNonCallback();
            this.m_Columns[i].ReconnectAllCards();
        }
    }

    protected IEnumerator HandleOnActiveHint()
    {
        var haveNewPath = this.HaveNewPath();
        // PLAY ANIM
        if (haveNewPath)
        {
            var animActive = false;
            for (int i = 0; i < this.m_OnHintCards.Count; i += 2)
            {
                var curCard = this.m_OnHintCards[i];
                var lastCard = this.m_OnHintCards[i + 1];
                animActive = false;
                curCard.OnBeginDragCard();
                var heighOffset = lastCard.column != null ? lastCard.column.heighOffset : 50f;
                var lastPosition = this.m_Group.transform.InverseTransformPoint(lastCard.transform.position);
                lastPosition.y -= heighOffset;
                curCard.MoveToPosition (
                    0.1f, 
                    lastPosition, 
                    0.75f, 
                    () => {
                        animActive = true;
                    }
                );
                yield return animActive;
                yield return this.m_WaitShortTime;
                curCard.OnDropCard();
            }
        }
        // BLOCK
        BOARD_LOCK = false;
        this.m_IsActiveHint = false;
    }

    public virtual bool HaveCardsOnColumns()
    {
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            if (this.m_Columns[i].cards.Count > 0)
                return true;
        }
        return false;
    }
    
    public virtual bool HaveCardsOnBoard()
    {
        return this.m_OnBoardCards.Count > 0;
    }

    public virtual bool HaveNewPath()
    {
        var haveNewPath = false;
        this.m_OnHintCards.Clear();
        this.m_OnHintCards.TrimExcess();
        // CHECK COLUMN
        for (int c = 0; c < this.m_Columns.Length; c++)
        {
            var curColumn = this.m_Columns[c];
            var curAvailableCards = curColumn.availableCards;
            if (curAvailableCards.Count == 0)
            {
                haveNewPath = true;
                continue;
            }
            for (int x = 0; x < curAvailableCards.Count; x++)
            {
                var curCard = curAvailableCards[x];
                for (int i = 0; i < this.m_Columns.Length; i++)
                {
                    if (c == i)
                    {
                        continue;
                    }
                    var nextColumn = this.m_Columns[i];
                    var nextAvailableCards = nextColumn.availableCards;
                    if (nextAvailableCards.Count == 0)
                        continue;
                    var lastCard = nextAvailableCards[0];
                    if (lastCard.IsCanConnect(curCard))
                    {
                        haveNewPath = x == (curAvailableCards.Count - 1);
                        this.m_OnHintCards.Add (curCard);
                        this.m_OnHintCards.Add (lastCard);
                    }
                }
            }     
        }
        return haveNewPath;
    }

    public virtual void ShuffleCards(int amount = 2)
    {
        if (this.m_OnBoardCards == null || this.m_OnBoardCards.Count == 0)
            return;
        for (int a = 0; a < amount; a++)
        {
            for (int i = 0; i < this.m_OnBoardCards.Count; i ++)
            {
                var random = Random.Range(0, this.m_OnBoardCards.Count);

                var _obj = this.m_OnBoardCards [i];
                this.m_OnBoardCards[i] = this.m_OnBoardCards[random];
                this.m_OnBoardCards[random] = _obj;
            }
        }
    }

    public virtual void DrawCardsToColumns(int amount, Transform toTransform, System.Action<int, CCard> eachCardCallback = null)
    {
        // BLOCK
		BOARD_LOCK = true;
        if (this.m_OnBoardCards.Count == 0)
        {
            this.OnDrawCardsComplete();
            return;
        }
        // DATA
        var delay = 0.1f;
        var moveTime = 0.25f;
        var columnWait = 0.25f;

        var moveType = 0;
        var amountComplete = 0;
        var index = 0;
        var maxAmount = amount >= this.m_OnBoardCards.Count ? this.m_OnBoardCards.Count : amount;
        while (maxAmount > 0)
        {
            var column = this.m_Columns[index % this.m_Columns.Length];
            var card = this.m_OnBoardCards[0];
            column.AddCardToList (card);
            card.column = column;
            card.connectWithCard = null;
            card.activeCard = true;
            card.stateCard = CCard.ECardState.FACE_DOWN;
            card.SetParentInCenter (toTransform);
            card.CurveToPosition (
                    delay + (index / this.m_Columns.Length * columnWait), 
                    toTransform.InverseTransformPoint (column.GetCardWorldPosition(card)),  
                    moveTime, 
                    moveType, 
                    () => {
                        card.IsDropOnColumn ();
                        amountComplete++;
                        if (amountComplete == amount)
                        {
                            this.OnDrawCardsComplete();
                        }
                    });
            this.m_OnBoardCards.RemoveAt (0);
            index++;
            maxAmount--;
            // CALLBACK
            if (eachCardCallback != null)
            {
                eachCardCallback (index, card);
            }
            
        }
        this.m_OnBoardCards.TrimExcess ();
    }

    public virtual void OnDrawCardsComplete()
    {
        // BLOCK
		BOARD_LOCK = false;
        // COLUMNS
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            this.m_Columns[i].RepositionAllCards();
            this.m_Columns[i].ReconnectAllCards();
        }
    }

    public virtual void RefeshColumns()
    {
        for (int i = 0; i < this.m_Columns.Length; i++)
        {
            // this.m_Columns[i].RepositionAllCards();
            this.m_Columns[i].ReconnectAllCards();
        }
    }

    public virtual void ReturnCardToBox(CCard card)
    {
        this.m_ReturnBox.AddReturnBox (card);
    }

    #endregion

    #region Getter && Setter

    public virtual Vector3 GetReturnCardPoint()
    {
        return this.m_ReturnBox.GetReturnPosition();
    }

    public virtual Vector3 GetSpawnCardPoint()
    {
        return this.m_SpawnCardPoint.localPosition;
    }

    #endregion

}

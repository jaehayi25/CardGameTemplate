using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CardGame : MonoBehaviour
{
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI opponentHealthText;
    public TextMeshProUGUI opponentIntentText;
    public TextMeshProUGUI turnIndicatorText;
    public Button drawCardButton;
    public Button endTurnButton;
    public GameObject playerHandContainer;
    public GameObject cardPrefab;

    private HorizontalLayoutGroup horizontalLayoutGroup;

    public List<Card> allCards;

    private int playerHealth = 30;
    private int opponentHealth = 30;
    private int playerShield = 0;
    private int opponentShield = 0;
    private bool isPlayerTurn = true;

    private List<Card> playerDeck = new List<Card>();
    private List<Card> playerHand = new List<Card>();
    private List<Card> playerDiscardPile = new List<Card>();

    private List<string> opponentIntents = new List<string> { "Attack", "Shield" };
    private string currentOpponentIntent;

    void Start()
    {
        // Ensure the playerHandContainer has a HorizontalLayoutGroup
        horizontalLayoutGroup = playerHandContainer.GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayoutGroup == null)
        {
            horizontalLayoutGroup = playerHandContainer.AddComponent<HorizontalLayoutGroup>();
        }

        InitializeDeck();
        ShuffleDeck();
        DrawInitialHand();
        SetOpponentIntent();
        UpdateUI();
        drawCardButton.onClick.AddListener(DrawCard);
        endTurnButton.onClick.AddListener(EndPlayerTurn);
    }

    void InitializeDeck()
    {
        foreach (Card card in allCards)
        {
            playerDeck.Add(card);
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < playerDeck.Count; i++)
        {
            Card temp = playerDeck[i];
            int randomIndex = Random.Range(i, playerDeck.Count);
            playerDeck[i] = playerDeck[randomIndex];
            playerDeck[randomIndex] = temp;
        }
    }

    void DrawInitialHand()
    {
        for (int i = 0; i < 5; i++)
        {
            DrawCard();
        }
    }

    void DrawCard()
    {
        if (playerDeck.Count == 0)
        {
            playerDeck.AddRange(playerDiscardPile);
            playerDiscardPile.Clear();
            ShuffleDeck();
        }

        if (playerDeck.Count > 0 && playerHand.Count < 7)
        {
            Card drawnCard = playerDeck[0];
            playerDeck.RemoveAt(0);
            playerHand.Add(drawnCard);
            CreateCardVisual(drawnCard);
            UpdateUI();
        }
    }

    void CreateCardVisual(Card card)
    {
        GameObject cardObject = Instantiate(cardPrefab, playerHandContainer.transform);
        CardVisual cardVisual = cardObject.GetComponent<CardVisual>();
        cardVisual.SetCard(card);
        cardVisual.OnCardClicked += PlayCard;
    }

    void SetOpponentIntent()
    {
        currentOpponentIntent = opponentIntents[Random.Range(0, opponentIntents.Count)];
    }

    void UpdateUI()
    {
        playerHealthText.text = $"Player Health: {playerHealth} (Shield: {playerShield})";
        opponentHealthText.text = $"Opponent Health: {opponentHealth} (Shield: {opponentShield})";
        opponentIntentText.text = $"Opponent Intent: {(isPlayerTurn ? currentOpponentIntent : "?")}";
        turnIndicatorText.text = isPlayerTurn ? "Player's Turn" : "Opponent's Turn";

        drawCardButton.interactable = isPlayerTurn;
        endTurnButton.interactable = isPlayerTurn;
    }

    void PlayCard(Card playedCard)
    {
        if (!isPlayerTurn || !playerHand.Contains(playedCard)) return;

        playerHand.Remove(playedCard);

        if (playedCard.Type == "Attack")
        {
            int damage = playedCard.Value;
            if (opponentShield > 0)
            {
                opponentShield -= damage;
                if (opponentShield < 0)
                {
                    opponentHealth += opponentShield;
                    opponentShield = 0;
                }
            }
            else
            {
                opponentHealth -= damage;
            }
        }
        else if (playedCard.Type == "Shield")
        {
            playerShield += playedCard.Value;
        }

        playerDiscardPile.Add(playedCard);
        UpdateUI();
        RefreshPlayerHand();

        if (opponentHealth <= 0)
        {
            OnOpponentDeath();
        }
    }

    void RefreshPlayerHand()
    {
        foreach (Transform child in playerHandContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (Card card in playerHand)
        {
            CreateCardVisual(card);
        }
    }

    void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        isPlayerTurn = false;
        UpdateUI();

        StartOpponentTurn();
    }

    void StartOpponentTurn()
    {
        UpdateUI();
        Invoke("ProcessOpponentIntent", 1f);
    }

    void ProcessOpponentIntent()
    {
        if (currentOpponentIntent == "Attack")
        {
            int damage = Random.Range(3, 7);
            if (playerShield > 0)
            {
                playerShield -= damage;
                if (playerShield < 0)
                {
                    playerHealth += playerShield;
                    playerShield = 0;
                }
            }
            else
            {
                playerHealth -= damage;
            }
        }
        else if (currentOpponentIntent == "Shield")
        {
            opponentShield += Random.Range(3, 7);
        }

        UpdateUI();

        if (playerHealth <= 0)
        {
            OnPlayerDeath();
        }
        else
        {
            Invoke("EndOpponentTurn", 1f);
        }
    }

    void EndOpponentTurn()
    {
        isPlayerTurn = true;
        DrawCard();
        SetOpponentIntent();
        UpdateUI();
    }

    void OnPlayerDeath()
    {
        Debug.Log("Game Over: Player has died!");
        // Add game over logic here (e.g., show game over screen, restart button, etc.)
    }

    void OnOpponentDeath()
    {
        Debug.Log("Victory: Opponent has been defeated!");
        // Add victory logic here (e.g., show victory screen, next level button, etc.)
    }
}
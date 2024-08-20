using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardVisual : MonoBehaviour
{
    public Image cardImage;
    public TextMeshProUGUI cardText;
    private Card card;

    public event System.Action<Card> OnCardClicked;

    public void SetCard(Card newCard)
    {
        card = newCard; 
        cardImage.sprite = card.Image;
        cardText.text = $"{card.Type}\n{card.Value}";
    }

    public void OnClick()
    {
        OnCardClicked?.Invoke(card);
    }
}
using UnityEngine;

public class UIButtons : MonoBehaviour
{
    public GameObject highScorePanel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAgain()
    {
        highScorePanel.SetActive(false);
        ResetScene();
    }

    public void ResetScene()
    {
        // Find all the cards and remove them
        UpdateSprite[] cards = Object.FindObjectsByType<UpdateSprite>(FindObjectsSortMode.None);
        foreach (UpdateSprite card in cards)
        {
            Destroy(card.gameObject);
        }
        ClearTopValues();
        // Deal new cards
        FindFirstObjectByType<Solitaire>().PlayCards();
    }

    void ClearTopValues()
    {
        Selectable[] selectables = Object.FindObjectsByType<Selectable>(FindObjectsSortMode.None);
        foreach (Selectable selectable in selectables)
        {
            if (selectable.CompareTag("Top"))
            {
                selectable.suit = null;
                selectable.value = 0;
            }
        }
    }
}

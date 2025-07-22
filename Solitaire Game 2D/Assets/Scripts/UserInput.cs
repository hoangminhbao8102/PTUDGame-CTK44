using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    public GameObject slot1;
    private Solitaire solitaire;
    private float timer;
    private float doubleClickTime = 0.3f;
    private int clickCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        solitaire = FindFirstObjectByType<Solitaire>();
        slot1 = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (clickCount == 1)
        {
            timer += Time.deltaTime;
        }
        if (clickCount == 3)
        {
            timer = 0;
            clickCount = 1;
        }
        if (timer > doubleClickTime) 
        {
            timer = 0;
            clickCount = 0;
        }

        GetMouseClick();
    }

    void GetMouseClick()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickCount++;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -10));

            RaycastHit2D hit = Physics2D.Raycast(worldMousePosition, Vector2.zero);
            if (hit)
            {
                // What has been hit? Deck/Card/EmptySlot...
                if (hit.collider.CompareTag("Deck"))
                {
                    // Clicked deck
                    Deck();
                }
                else if (hit.collider.CompareTag("Card"))
                {
                    // Clicked card
                    Card(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Top"))
                {
                    // Clicked top
                    Top(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Bottom"))
                {
                    // Clicked bottom
                    Bottom(hit.collider.gameObject);
                }
            }
        }
    }

    void Deck()
    {
        // Deck click actions
        print("Clicked on Deck");
        solitaire.DealFromDeck();
        slot1 = this.gameObject;
    }

    void Card(GameObject selected)
    {
        // Card click actions
        print("Clicked on Card");

        if (!selected.GetComponent<Selectable>().faceUp) // If the card clicked on is facedown
        {
            if (!Blocked(selected)) // If the card clicked on is not blocked
            {
                // Flip it over
                selected.GetComponent<Selectable>().faceUp = true;
                slot1 = this.gameObject;
            }
        }
        else if (selected.GetComponent<Selectable>().inDeckPile) // If the card clicked on is in the deck pile with the trips
        {
            // If it is not blocked
            if (!Blocked(selected))
            {
                if (slot1 == selected) // If the same card is clicked twice
                {
                    if (DoubleClick())
                    {
                        // Attempt auto stack
                        AutoStack(selected);
                    }
                }
                else
                {
                    slot1 = selected;
                }
            }

        }
        else
        {
            // If the card is face up
            // If there is no card currently selected
            // Select the card

            if (slot1 == this.gameObject) // Not null because we pass in this gameObject instead
            {
                slot1 = selected;
            }

            // If there is already a card selected (and it is not the same card)

            else if (slot1 != selected)
            {
                // If the new card is eligable to stack on the old card
                if (Stackable(selected))
                {
                    Stack(selected);
                }
                else
                {
                    // Select the new card
                    slot1 = selected;
                }
            }

            else if (slot1 == selected) // If the same card is clicked twice
            {
                if (DoubleClick())
                {
                    // Attempt auto stack
                }
            }
        }
    }

    void Top(GameObject selected)
    {
        // Top click actions
        print("Clicked on Top");
        if (slot1.CompareTag("Card"))
        {
            // If the card is an ace and empty slot is top then stack
            if (slot1.GetComponent<Selectable>().value == 1)
            {
                Stack(selected);
            }
        }
    }

    void Bottom(GameObject selected)
    {
        // Bottom click actions
        print("Clicked on Bottom");
        // If the card is a king and the empty slot is top then stack

        if (slot1.CompareTag("Card"))
        {
            if (slot1.GetComponent<Selectable>().value == 13)
            {
                Stack(selected);
            }
        }
    }

    bool Stackable(GameObject selected)
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        // Compare them to see if they stack

        if (!s2.inDeckPile)
        {
            if (s2.top) // If in the top pile must stack suited Ace to King
            {
                if (s1.suit == s2.suit || (s1.value == 1 && s2.suit == null))
                {
                    if (s1.value == s2.value + 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else // If in the bottom pile must stack alternate colours King to Ace
            {
                if (s1.value == s2.value - 1)
                {
                    bool card1 = s1.suit != "C" && s1.suit != "S";
                    bool card2 = s2.suit != "C" && s2.suit != "S";

                    if (card1 == card2)
                        return false;
                    else
                        return true;
                }
            }
        }
        return false;
    }

    void Stack(GameObject selected) 
    {
        // If on top of king or empty bottom stack the cards in place
        // Else stack the cards with a negative y offset

        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        float yOffset = 0.3f;

        if (s2.top || (!s2.top && s1.value == 13))
        {
            yOffset = 0;
        }

        slot1.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y - yOffset, selected.transform.position.z - 0.01f);
        slot1.transform.parent = selected.transform; // This makes the children move with the parents

        if (s1.inDeckPile) // Removes the cards from the top pile to prevent duplicate cards
        {
            solitaire.tripsOnDisplay.Remove(slot1.name);
        }
        else if (s1.top && s2.top &s1.value == 1) // Allows movement of cards between top spots
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = 0;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = null;
        }
        else if (s1.top) // Kepps track of the current value of the top decks as a card has been removed
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value - 1;
        }
        else // Removes the card string from the appropriate bottom list
        {
            solitaire.bottoms[s1.row].Remove(slot1.name);
        }

        s1.inDeckPile = false; // You cannot add cards to the trips pile so this is always fine
        s1.row = s2.row;

        if (s2.top)
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = s1.suit;
            s1.top = true;
        }
        else
        {
            s1.top = false;
        }

        // After completing move reset slot1 to be essentially null as being null will break the logic
        slot1 = this.gameObject;
    }

    bool Blocked(GameObject selected)
    {
        Selectable s2 = selected.GetComponent<Selectable>();
        if (s2.inDeckPile)
        {
            return s2.name != solitaire.tripsOnDisplay.Last();
        }
        else
        {
            return s2.name != solitaire.bottoms[s2.row].Last();
        }
    }

    bool DoubleClick()
    {
        return timer < doubleClickTime && clickCount == 2;
    }

    void AutoStack(GameObject selected)
    {
        for (int i = 0; i < solitaire.topPos.Length; i++)
        {
            Selectable stack = solitaire.topPos[i].GetComponent<Selectable>();
            if (selected.GetComponent<Selectable>().value == 1) // If it is an Ace
            {
                if (solitaire.topPos[i].GetComponent<Selectable>().value == 0) // And the top position is empty
                {
                    slot1 = selected;
                    Stack(stack.gameObject); // Stack the Ace up top
                    break;                  // In the first empty position found
                }
            }
            else
            {
                if ((stack.suit == slot1.GetComponent<Selectable>().suit) && (stack.value == slot1.GetComponent<Selectable>().value - 1)) 
                {
                    // If it is the last card (if it has no children)
                    if (HasNoChildren(slot1))
                    {
                        slot1 = selected;
                        // Find a top spot that macthes the conditions for auto stacking if it exists
                        string lastCardName = stack.suit + (stack.value switch
                        {
                            1 => "A",
                            11 => "J",
                            12 => "Q",
                            13 => "K",
                            _ => stack.value.ToString()
                        });

                        GameObject lastCard = GameObject.Find(lastCardName);
                        Stack(lastCard);
                        break;
                    }
                }
            }
        }
    }

    bool HasNoChildren(GameObject card)
    {
        return card.transform.childCount == 0;
    }
}

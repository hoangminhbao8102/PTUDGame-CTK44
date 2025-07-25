﻿using System.Collections.Generic;
using UnityEngine;

public class UpdateSprite : MonoBehaviour
{
    public Sprite cardFace;
    public Sprite cardBack;
    private SpriteRenderer spriteRenderer;
    private Selectable selectable;
    private Solitaire solitaire;
    private UserInput userInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        List<string> deck = Solitaire.GenerateDeck();
        solitaire = FindFirstObjectByType<Solitaire>();
        userInput = FindFirstObjectByType<UserInput>();

        int i = 0;
        foreach (string card in deck)
        {
            if (this.name == card)
            {
                cardFace = solitaire.cardFaces[i];
                break;
            }
            i++;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        selectable = GetComponent<Selectable>();

        if (spriteRenderer == null)
            Debug.LogError("SpriteRenderer is missing on " + gameObject.name);
        if (selectable == null)
            Debug.LogError("Selectable is missing on " + gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (selectable.faceUp == true)
        {
            spriteRenderer.sprite = cardFace;
        }
        else
        {
            spriteRenderer.sprite = cardBack;
        }

        if (userInput.slot1)
        {
            if (name == userInput.slot1.name)
            {
                spriteRenderer.color = Color.yellow;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
    }
}

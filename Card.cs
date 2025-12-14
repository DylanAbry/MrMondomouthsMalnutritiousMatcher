using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public GameObject front;
    public string foodName;
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void PlayerGetsCardAnimation()
    {
        anim.Play("PlayerGetsCard");
    }
    public void CPUGetsCardAnimation()
    {
        anim.Play("CPUGetsCard");
    }

    public void OnCardClicked()
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm.isDiscardMode)
            gm.SelectCardForDiscard(this);
        else if (gm.isMatchMode)
        {
            if (gm.firstMatchCard == null)
                gm.SelectFirstMatchCard(this);
            else
                gm.SelectSecondMatchCard(this);
        }
        else
        {
            gm.SelectCard(this); // normal drawing
        }
    }
}

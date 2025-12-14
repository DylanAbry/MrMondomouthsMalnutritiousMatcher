using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public MatchManager matchManager;

    [SerializeField] private int maxHandSize = 6;
    [SerializeField] private int maxCardsPerTurn = 3;

    // Controls how random the CPU is: lower = more greedy, higher = more random
    [SerializeField] private float cpuTemperature = 0.7f;


    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI cpuScoreText;
    public TextMeshProUGUI turnTracker;
    public int playerScore, cpuScore, numPlayerCardsDrawn, numCPUCardsDrawn;
    public bool playerTurn;

    public List<Card> playerCards = new List<Card>();
    public List<Card> cpuCards = new List<Card>();

    public Card[] cards;

    public GameObject[] playerCardPlacers;
    public GameObject[] cpuCardPlacers;
    private GameObject currCard;
    public GameObject cardCollectionPanel;
    public GameObject foodPairsPanel;

    public GameObject cardParent;
    public GameObject stopDrawingButton;
    public GameObject drawHalter;

    private int totalCardsDrawn = 0;
    private List<Card> deckOrder = new List<Card>();

    
    private Card selectedCardToDiscard = null;

    
    public GameObject discardMenuPanel;
    public GameObject confirmDiscardPanel;
    public GameObject instructionsPanel;
    public GameObject instructionsButton;

 
    public bool isDiscardMode = false;

    public bool isMatchMode = false;
    public Card firstMatchCard = null;
    public Card secondMatchCard = null;

    public GameObject matchMenuPanel;
    public GameObject confirmMatchPanel;
    public GameObject discardedPanel;

    private Dictionary<Card, Color> originalCardColors = new Dictionary<Card, Color>();

    public TextMeshProUGUI panelDescription;
    public TextMeshProUGUI confirmDiscardDescription;
    public TextMeshProUGUI confirmMatchDescription;

    public GameObject[] discardedPlacers;
    public List<Card> discarded = new List<Card>();

    [SerializeField] private int lastCardPenalty = 3;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject tiePanel;

    public Button foodPairsButton;
    public Button matchPairsButton;
    public Button discardedButton;
    public Button discardButton;

    [SerializeField] private float cpuDiscardRandomness = 0.25f;
    // Start is called before the first frame update
    void Start()
    {
        if (matchManager == null)
        {
            matchManager = FindObjectOfType<MatchManager>();
        }

        int randTurn = Random.Range(0, 10);
        if (randTurn < 5)
        {
            playerTurn = true;
            instructionsButton.SetActive(true);
        }
        else
        {
            playerTurn = false;
            instructionsButton.SetActive(false);
            discardButton.enabled = false;
            matchPairsButton.enabled = false;
            discardedButton.enabled = false;
            foodPairsButton.enabled = false;
        }
        cardCollectionPanel.SetActive(false);
        stopDrawingButton.SetActive(false);

        foreach(Card c in cards)
        {
            c.front.SetActive(false);
        }
        drawHalter.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        tiePanel.SetActive(false);
        instructionsPanel.SetActive(false);
        ShuffleCards();
        SetupTurn();
    }

    // Update is called once per frame
    void Update()
    {
        playerScoreText.text = string.Format("Player Score: " + playerScore);
        cpuScoreText.text = string.Format("CPU Score: " + cpuScore);

        
        if (isDiscardMode)
        {
            panelDescription.text = string.Format("Click on a card to discard it!");
        }
        else
        {

        }       

        if (playerTurn)
        {
            turnTracker.text = string.Format("Player Go");
        }
        else
        {
            turnTracker.text = string.Format("CPU Go");
        }
    }

    public void ShuffleCards()
    {
        deckOrder.Clear();

        List<Card> temp = new List<Card>(cards);

        // Fisher-Yates proper shuffle
        for (int i = temp.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Card c = temp[i];
            temp[i] = temp[j];
            temp[j] = c;
        }

        // Build deckOrder from the shuffled list
        foreach (Card c in temp)
        {
            deckOrder.Add(c);
            c.transform.SetParent(cardParent.transform);  // visually group them
        }
    }

    public void SetupTurn()
    {
        if (playerTurn)
        {
            numPlayerCardsDrawn = 0;
            
            foreach (Card c in cards)
            {
                c.GetComponent<Button>().enabled = true;
            }

            if (playerCards.Count == 6)
            {
                drawHalter.SetActive(true);
            }
            instructionsButton.SetActive(true);
            discardButton.enabled = true;
            matchPairsButton.enabled = true;
            discardedButton.enabled = true;
            foodPairsButton.enabled = true;
        }
        else
        {
            numCPUCardsDrawn = 0;
            foreach(Card c in cards)
            {
                c.GetComponent<Button>().enabled = false;
            }

            discardButton.enabled = false;
            matchPairsButton.enabled = false;
            discardedButton.enabled = false;
            foodPairsButton.enabled = false;
            StartCoroutine(CPUTurn());
        }
    }

    public void SelectCard(Card card)
    {
        deckOrder.RemoveAt(deckOrder.LastIndexOf(card));
        currCard = card.gameObject;
        card.front.SetActive(true);
        playerCards.Add(card);
        numPlayerCardsDrawn++;
        stopDrawingButton.SetActive(false);
        instructionsButton.SetActive(false);
        discardButton.enabled = false;
        matchPairsButton.enabled = false;
        discardedButton.enabled = false;
        foodPairsButton.enabled = false;

        foreach (Card c in cards)
            c.GetComponent<Button>().enabled = false;

        card.PlayerGetsCardAnimation();
        StartCoroutine(PlayerDrawsCard());
    }

    public void StopDrawing()
    {
        if (playerTurn)
        {
            playerTurn = false;
            drawHalter.SetActive(false);
            stopDrawingButton.SetActive(false);
            instructionsButton.SetActive(false);
            SetupTurn();
        }
        else
        {
            playerTurn = true;
            SetupTurn();
        }
    }

    private IEnumerator PlayerDrawsCard()
    {
        yield return new WaitForSeconds(1.3f);
        
        currCard.transform.SetParent(cardCollectionPanel.transform);
        currCard.transform.position = playerCardPlacers[playerCards.Count - 1].transform.position;
        totalCardsDrawn++;

        if (numPlayerCardsDrawn < 3)
        {
            foreach (Card c in cards)
            {
                c.GetComponent<Button>().enabled = true;
            }
            if (playerCards.Count == 6)
            {
                drawHalter.SetActive(true);
            }
            stopDrawingButton.SetActive(true);
            instructionsButton.SetActive(true);
            discardButton.enabled = true;
            matchPairsButton.enabled = true;
            discardedButton.enabled = true;
            foodPairsButton.enabled = true;
        }
        else
        {
            StopDrawing();
        }
        if (deckOrder.Count == 0)
        {
            cpuScore += lastCardPenalty;

            EndGame();  

            StopDrawing();
            yield break;
        }
    }

    private IEnumerator CPUTurn()
    {
        yield return new WaitForSeconds(0.5f);
        numCPUCardsDrawn = 0;
        stopDrawingButton.SetActive(false);

        while (numCPUCardsDrawn < 3)
        {
            if (cpuCards.Count >= maxHandSize)
            {
                // CPU must discard one BEFORE drawing
                TryCPUDiscard();

                // Safety: reorganize CPU cards
                ReorganizeCPUCards();

                // After discard, still must stop drawing if draw limit reached
                if (cpuCards.Count >= maxHandSize)
                    break;
            }

            if (cpuCards.Count >= maxHandSize)
            {
                bool discardedOne = ForceCPUDiscardOne();
                if (!discardedOne)
                    yield break;
            }
            // ---------- DRAW A CARD ----------
            Card next = GetNextCardFromDeck();
            if (next == null)
                yield break; // deck is empty

            currCard = next.gameObject;
            next.front.SetActive(true);
            cpuCards.Add(next);
            numCPUCardsDrawn++;
            totalCardsDrawn++;

            if (deckOrder.Count == 0)
            {
                // Player gets 3 points because CPU drew the last card
                playerScore += lastCardPenalty;   // or +3 directly

                next.CPUGetsCardAnimation();
                yield return new WaitForSeconds(1.3f);

                next.transform.SetParent(cardCollectionPanel.transform);
                next.transform.position = cpuCardPlacers[cpuCards.Count - 1].transform.position;

                EndGame(); 

                yield break;
            }

            next.CPUGetsCardAnimation();
            yield return new WaitForSeconds(1.3f);

            next.transform.SetParent(cardCollectionPanel.transform);
            next.transform.position = cpuCardPlacers[cpuCards.Count - 1].transform.position;

            // ---------- STEP 1: MATCH LOOP ----------
            bool matched = true;
            while (matched)
            {
                matched = ResolveOneCPUPair();
                if (matched)
                    yield return new WaitForSeconds(0.4f);
            }

            // ---------- STEP 2: DISCARD LOOP ----------
            int safety = 10;     
            bool discarded = true;

            while (discarded && numCPUCardsDrawn < maxCardsPerTurn && safety-- > 0)
            {
                // If CPU is at max hand size (6 cards), it MUST discard one.
                if (cpuCards.Count >= maxHandSize)
                {
                    ForceCPUDiscardOne();
                    yield return new WaitForSeconds(0.4f);
                    continue;
                }

                // Otherwise, try normal discard logic
                discarded = TryCPUDiscard();

                if (discarded)
                    yield return new WaitForSeconds(0.4f);
            }

            // ---------- DECIDE IF DRAW AGAIN ----------
            if (numCPUCardsDrawn >= 3)
                break;

            if (!ShouldCPUDrawAgain())
                break;

            yield return new WaitForSeconds(0.4f);
        }

        // End CPU turn back to player
        playerTurn = true;
        SetupTurn();
    }

    private Card GetNextCardFromDeck()
    {
        if (deckOrder.Count == 0)
            return null;

        int last = deckOrder.Count - 1;
        Card top = deckOrder[last];

        deckOrder.RemoveAt(last);
        return top;
    }

    private bool ForceCPUDiscardOne()
    {
        if (cpuCards.Count == 0)
            return false;


        Card worst = ChooseCPUDiscardCard();
        if (worst == null)
            return false;

        cpuCards.Remove(worst);
        discarded.Add(worst);
        worst.GetComponent<Button>().enabled = false;

        worst.transform.SetParent(discardedPanel.transform);
        worst.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

        ReorganizeCPUCards();
        return true;
    }

    // CPU Decision heuristics

    private bool ArePair(Card a, Card b)
    {
        return matchManager.IsMatch(a.foodName, b.foodName);
    }

    private int EvaluateHand(List<Card> hand)
    {
        int count = 0;
        for (int i = 0; i < hand.Count; i++)
        {
            for (int j = i + 1; j < hand.Count; j++)
            {
                if (ArePair(hand[i], hand[j]))
                    count++;
            }
        }
        return count;
    }

    private float EstimatePairProbability()
    {
        int possible = 0;
        int remaining = deckOrder.Count;

        foreach (Card deckCard in deckOrder)
        {
            foreach (Card held in cpuCards)
            {
                if (ArePair(deckCard, held))
                {
                    possible++;
                    break;
                }
            }
        }

        return (remaining == 0) ? 0f : (float)possible / remaining;
    }

    private bool ShouldCPUDrawAgain()
    {
        // Hard limits
        if (numCPUCardsDrawn >= maxCardsPerTurn)
            return false;

        if (cpuCards.Count >= maxHandSize)
            return false;

        // Do NOT draw if only 1 card left (avoid penalty)
        if (deckOrder.Count == 1)
        {
            // Only draw if CPU is severely behind
            int scoreDiff = playerScore - cpuScore;
            if (scoreDiff >= 3)
                return true;   // desperate gamble
            return false;
        }

        // Base probability from remaining deck matches
        float pPair = EstimatePairProbability();

        // CPU gets bolder when behind
        int cpuPotential = EvaluateHand(cpuCards);
        int playerPotential = EvaluateHand(playerCards);

        if (cpuScore < playerScore)
            pPair += 0.25f;

        if (cpuPotential < playerPotential)
            pPair += 0.15f;

        // Already matched? Slight confidence boost
        if (cpuPotential > 0)
            pPair += 0.10f;

        // If CPU only has 1 card, draw aggressively
        if (cpuCards.Count == 1)
            pPair += 0.25f;

        // Mild aggressiveness multiplier
        pPair *= 1.2f;

        pPair = Mathf.Clamp01(pPair);

        return Random.value < pPair;
    }

    // Pair resolution

    private bool ResolveOneCPUPair()
    {
        for (int i = 0; i < cpuCards.Count; i++)
        {
            for (int j = i + 1; j < cpuCards.Count; j++)
            {
                if (ArePair(cpuCards[i], cpuCards[j]))
                {
                    cpuScore++;

                    Card a = cpuCards[i];
                    Card b = cpuCards[j];

                    cpuCards.RemoveAt(j);
                    cpuCards.RemoveAt(i);

                    discarded.Add(a);
                    a.GetComponent<Button>().enabled = false;
                    discarded.Add(b);
                    b.GetComponent<Button>().enabled = false;

                    a.transform.SetParent(discardedPanel.transform);
                    b.transform.SetParent(discardedPanel.transform);

                    a.transform.position = discardedPlacers[discarded.Count - 2].transform.position;
                    b.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

                    return true; // matched something
                }
            }
        }
        ReorganizeCPUCards();
        return false;
    }
    private void ReorganizeCPUCards()
    {
        for (int i = 0; i < cpuCards.Count; i++)
        {
            cpuCards[i].transform.position = cpuCardPlacers[i].transform.position;
        }
    }

    private void CPUDiscardCard()
    {
        if (cpuCards.Count == 0)
            return;

        // Pick card CPU wants to discard
        Card cardToDiscard = ChooseCPUDiscardCard();

        if (cardToDiscard == null)
            return;

        // Remove from CPU hand
        cpuCards.Remove(cardToDiscard);

        // Add to discard list
        discarded.Add(cardToDiscard);

        // Move to discarded panel
        cardToDiscard.transform.SetParent(discardedPanel.transform);
        cardToDiscard.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

        // Hide CPU's discarded card face
        cardToDiscard.front.SetActive(false);
    }

    private Card ChooseCPUDiscardCard()
    {
        // 25% chance to discard randomly instead of optimal
        if (Random.value < cpuDiscardRandomness)
        {
            int randomIndex = Random.Range(0, cpuCards.Count);
            return cpuCards[randomIndex];
        }

        // Otherwise discard worst card based on compatibility
        Card worstCard = null;
        int worstScore = int.MaxValue;

        foreach (Card c in cpuCards)
        {
            int compatibility = 0;

            foreach (Card other in cpuCards)
            {
                if (other == c) continue;
                if (ArePair(c, other))
                    compatibility++;
            }

            if (compatibility < worstScore)
            {
                worstScore = compatibility;
                worstCard = c;
            }
        }

        return worstCard;
    }

    private bool TryCPUDiscard()
    {
        if (cpuCards.Count <= 2)
            return false; // too few cards to justify discarding

        Card discardChoice = ChooseCPUDiscardCard();
        if (discardChoice == null)
            return false;

        // Only discard if it's low-value
        if (!ShouldCPUDiscard(discardChoice))
            return false;

        cpuCards.Remove(discardChoice);
        discarded.Add(discardChoice);

        discardChoice.GetComponent<Button>().enabled = false;
        discardChoice.transform.SetParent(discardedPanel.transform);
        discardChoice.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

        ReorganizeCPUCards();
        return true;
    }

    private bool ShouldCPUDiscard(Card card)
    {
        // If CPU has 6 cards, discard ANY non-matching card
        if (cpuCards.Count >= maxHandSize)
            return true;

        // Count in-hand compatibility
        int compatibility = 0;
        foreach (Card other in cpuCards)
        {
            if (other == card) continue;
            if (ArePair(card, other))
                compatibility++;
        }

        // 0 matches -> discard often
        if (compatibility == 0)
            return Random.value < 0.40f;  // slightly more aggressive

        // 1 match -> occasionally discard
        if (compatibility == 1)
            return Random.value < 0.15f;

        // 2+ matches -> never discard
        return false;
    }

    public void EnterDiscardMode()
    {
        if (!playerTurn)
            return;

        isDiscardMode = true;
        selectedCardToDiscard = null;

        discardMenuPanel.SetActive(true);

        
        foreach (Card c in cards)
            c.GetComponent<Button>().enabled = false;

        
        foreach (Card c in playerCards)
            c.GetComponent<Button>().enabled = true;
    }

    public void SelectCardForDiscard(Card card)
    {
        if (!isDiscardMode)
            return;

        selectedCardToDiscard = card;
        confirmDiscardPanel.SetActive(true);
        confirmDiscardDescription.text = string.Format("Discard the " + card.foodName + "? You will not get this card back...");
    }

    public void ConfirmDiscard()
    {
        if (!isDiscardMode || selectedCardToDiscard == null)
            return;

        
        playerCards.Remove(selectedCardToDiscard);

        discarded.Add(selectedCardToDiscard);
        selectedCardToDiscard.GetComponent<Button>().enabled = false;
        selectedCardToDiscard.transform.SetParent(discardedPanel.transform);
        selectedCardToDiscard.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

        drawHalter.SetActive(false);
        ReorganizePlayerCards();

        ExitDiscardMode();
    }

    private void ReorganizePlayerCards()
    {
        for (int i = 0; i < playerCards.Count; i++)
        {
            playerCards[i].transform.position = playerCardPlacers[i].transform.position;
        }
    }

    public void CancelDiscard()
    {
        ExitDiscardMode();
    }

    private void ExitDiscardMode()
    {
        isDiscardMode = false;
        selectedCardToDiscard = null;

        discardMenuPanel.SetActive(false);
        confirmDiscardPanel.SetActive(false);


        if (playerTurn)
        {
            // Only enable deck buttons again if player is allowed to draw
            if (numPlayerCardsDrawn < 3)
            {
                foreach (Card c in cards)
                {
                    if (!discarded.Contains(c))
                    {
                        c.GetComponent<Button>().enabled = true;
                    }
                }

                if (numPlayerCardsDrawn > 0) stopDrawingButton.SetActive(true);
            }
            else
            {
                // Player maxed drawing; keep deck locked
                foreach (Card c in cards)
                    c.GetComponent<Button>().enabled = false;

                stopDrawingButton.SetActive(false);
            }
        }
    }

    public void EnterMatchMode()
    {
        if (!playerTurn)
            return;

        isMatchMode = true;
        firstMatchCard = null;
        secondMatchCard = null;

        matchMenuPanel.SetActive(true);
        confirmMatchPanel.SetActive(false);
        panelDescription.text = "Select first card for matching...";

        // Disable full deck
        foreach (Card c in cards)
            c.GetComponent<Button>().enabled = false;

        // Enable player's hand
        foreach (Card c in playerCards)
            c.GetComponent<Button>().enabled = true;

        // Store original FRONT colors
        originalCardColors.Clear();
        foreach (Card c in playerCards)
        {
            RawImage ri = c.front.GetComponent<RawImage>();
            Image img = c.front.GetComponent<Image>();

            if (ri != null)
                originalCardColors[c] = ri.color;
            else if (img != null)
                originalCardColors[c] = img.color;
        }
    }

    public void SelectFirstMatchCard(Card card)
    {
        if (!isMatchMode)
            return;

        firstMatchCard = card;
        secondMatchCard = null;

        panelDescription.text = string.Format(firstMatchCard.foodName + " selected! Now select second card...");

        foreach (Card c in playerCards)
        {
            Button btn = c.GetComponent<Button>();
            RawImage ri = c.front.GetComponent<RawImage>();
            Image img = c.front.GetComponent<Image>();

            if (c == card)
            {
                btn.enabled = false;
                continue;
            }

            bool compatible = matchManager.IsMatch(card.foodName, c.foodName);

            if (compatible)
            {
                btn.enabled = true;

                if (originalCardColors.TryGetValue(c, out Color original))
                {
                    if (ri != null) ri.color = original;
                    if (img != null) img.color = original;
                }
            }
            else
            {
                btn.enabled = false;

                Color dark = new Color(0.3f, 0.3f, 0.3f, 1f);
                if (ri != null) ri.color = dark;
                if (img != null) img.color = dark;
            }
        }
    }

    public void SelectSecondMatchCard(Card card)
    {
        if (!isMatchMode || firstMatchCard == null)
            return;

        if (!matchManager.IsMatch(firstMatchCard.foodName, card.foodName))
            return; // shouldn't ever happen — buttons are disabled

        secondMatchCard = card;
        confirmMatchPanel.SetActive(true);
        confirmMatchDescription.text = string.Format("Match Together " + firstMatchCard.foodName + " and " + secondMatchCard.foodName + "?");
    }

    public void ConfirmMatch()
    {
        if (firstMatchCard == null || secondMatchCard == null)
            return;

        // Update score
        playerScore++;

        // Remove from hand
        playerCards.Remove(firstMatchCard);
        playerCards.Remove(secondMatchCard);

        // Hide cards
        discarded.Add(firstMatchCard);
        firstMatchCard.GetComponent<Button>().enabled = false;
        firstMatchCard.transform.SetParent(discardedPanel.transform);
        firstMatchCard.transform.position = discardedPlacers[discarded.Count - 1].transform.position;
        discarded.Add(secondMatchCard);
        secondMatchCard.GetComponent<Button>().enabled = false;
        secondMatchCard.transform.SetParent(discardedPanel.transform);
        secondMatchCard.transform.position = discardedPlacers[discarded.Count - 1].transform.position;

        drawHalter.SetActive(false);
        // Clean up UI colors
        RestoreCardColors();

        ReorganizePlayerCards();
        ExitMatchMode();
    }

    public void CancelMatch()
    {
        confirmMatchPanel.SetActive(false);
        RestoreCardColors();
        ExitMatchMode();
    }

    private void RestoreCardColors()
    {
        foreach (var pair in originalCardColors)
        {
            Card c = pair.Key;
            Color original = pair.Value;

            RawImage ri = c.front.GetComponent<RawImage>();
            Image img = c.front.GetComponent<Image>();

            if (ri != null) ri.color = original;
            if (img != null) img.color = original;
        }
    }

    private void ExitMatchMode()
    {
        isMatchMode = false;
        firstMatchCard = null;
        secondMatchCard = null;

        matchMenuPanel.SetActive(false);
        confirmMatchPanel.SetActive(false);

        if (playerTurn)
        {
            // Only enable deck buttons again if player is allowed to draw
            if (numPlayerCardsDrawn < 3)
            {
                foreach (Card c in cards)
                {
                    if (!discarded.Contains(c))
                    {
                        c.GetComponent<Button>().enabled = true;
                    }
                }

                if (numPlayerCardsDrawn > 0) stopDrawingButton.SetActive(true);
            }
            else
            {
                // Player maxed drawing; keep deck locked
                foreach (Card c in cards)
                    c.GetComponent<Button>().enabled = false;

                stopDrawingButton.SetActive(false);
            }
        }
        RestoreCardColors();
    }


    public void OpenFoodPairs()
    {
        foodPairsPanel.SetActive(true);
    }
    public void CloseFoodPairs()
    {
        foodPairsPanel.SetActive(false);
    }
    public void OpenDiscarded()
    {
        discardedPanel.SetActive(true);
    }
    public void CloseDiscarded()
    {
        discardedPanel.SetActive(false);
    }
    private void EndGame()
    {
        // Compare points
        if (playerScore > cpuScore)
        {
            winPanel.SetActive(true);
        }
        else if (cpuScore > playerScore)
        {
            losePanel.SetActive(true);
        }
        else
        {
            tiePanel.SetActive(true);
        }
    }

    public void OpenInstructions()
    {
        instructionsPanel.SetActive(true);
    }
    public void CloseInstructions()
    {
        instructionsPanel.SetActive(false);
    }

    public void ResetGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void ToTitleScreen()
    {
        SceneManager.LoadScene("TitleScene");
    }
}

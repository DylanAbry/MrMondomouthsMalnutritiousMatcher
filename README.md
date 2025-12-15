Details for the game:

1.) Deterministic Game Rules

[SerializeField] private int maxHandSize = 6;
[SerializeField] private int maxCardsPerTurn = 3;

if (cpuCards.Count >= maxHandSize)
    return false;

if (numCPUCardsDrawn >= maxCardsPerTurn)
    return false;

- Rule-based constraints limiting the AI’s action space. This helps prevent illegal moves and states and simplifies AI logic to not have to consider any moves in an illegal state.

2.) Randomized Deck via Fisher-Yates Shuffle (Used for Slide Puzzle Bonus Game in Alchemist Auction)

for (int i = temp.Count - 1; i > 0; i--)
{
    int j = Random.Range(0, i + 1);
    Card c = temp[i];
    temp[i] = temp[j];
    temp[j] = c;
}

- This ensures meaningful probability estimates in the later parts of the game, ensures no bias in the deck and a guaranteed randomized shuffle of cards at the start of each game.

3.) Heuristic Hand Evaluation (Local Utility Function)

private int EvaluateHand(List<Card> hand)
{
    int count = 0;
    for (int i = 0; i < hand.Count; i++)
        for (int j = i + 1; j < hand.Count; j++)
            if (ArePair(hand[i], hand[j]))
                count++;
    return count;
}

- This is a utility heuristic that counts the number of scoring pairs that exist. This is how the CPU optimizes its current hand to get the most possible matches and replaces expensive algorithms like minimax.

4.) Probabilistic Outcome Evaluation

private float EstimatePairProbability()
{
    int possible = 0;
    int remaining = deckOrder.Count;

    foreach (Card deckCard in deckOrder)
        foreach (Card held in cpuCards)
            if (ArePair(deckCard, held))
            {
                possible++;
                break;
            }

    return (remaining == 0) ? 0f : (float)possible / remaining;
}

- Used for drawing cards to estimate the probability of a match if an extra card is drawn. This is light expected-value reasoning and avoids full combinatorics.

5.) Risk-Adjusted Decision Policy

if (cpuScore < playerScore)
    pPair += 0.25f;

if (cpuPotential < playerPotential)
    pPair += 0.15f;

if (deckOrder.Count == 1)
{
    int scoreDiff = playerScore - cpuScore;
    if (scoreDiff >= 3)
        return true;
    return false;
}

- This adjusts the CPU's approach depending on the game state. It'll play more riskier if behind in score and more conservative if currently winning. This is a form of bounded rationality to make the CPU act more human.

6.) Stochastic Policy (Temperature Controlled Randomness)

[SerializeField] private float cpuTemperature = 0.7f;

return Random.value < pPair;

- This prevents perdictability and ensures the AI can't play perfect. This soft decision policy allows the CPU to be beatable and not a guaranteed loss every game for the player.

7.) Greedy Matching Loop (Local Optimization)

while (matched)
{
    matched = ResolveOneCPUPair();
}

if (ArePair(cpuCards[i], cpuCards[j]))
{
    cpuScore++;
    ...
    return true;
}

- Greedy algorithm that immediately cashes in any pairs for future points.

8.) Heuristic Discard Selection (Value Minimized)

foreach (Card c in cpuCards)
{
    int compatibility = 0;
    foreach (Card other in cpuCards)
        if (ArePair(c, other))
            compatibility++;

    if (compatibility < worstScore)
    {
        worstScore = compatibility;
        worstCard = c;
    }
}

- Cards with lower future payoff are discarded. The deck, current cards in the CPU's hand and the cards in the player's hand and dicarded are all examined to determined which card would be the best card to strategically discard.

 9.) Controlled Mistakes (CPU Discard Randomness)

 [SerializeField] private float cpuDiscardRandomness = 0.25f;

if (Random.value < cpuDiscardRandomness)
{
    int randomIndex = Random.Range(0, cpuCards.Count);
    return cpuCards[randomIndex];
}

- Mimics human error to randomly discard a card. The card discarded is not always the one that's the best strategy for the CPU to win, making it more beatable.

10.) Finite-State Turn System

StartCoroutine(CPUTurn());

- CPU follows a finite-state machine each turn of drawing a card, matching if possible, choosign to discard, choosing to draw again, and ending the turn. (Coroutines are manual sequences that can be implemented in Unity games)

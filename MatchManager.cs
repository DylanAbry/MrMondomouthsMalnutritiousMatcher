using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchManager : MonoBehaviour
{
    public Dictionary<string, List<string>> matchSystem;
    public int numCardsSelected;
    //public Card card1, card2;
    // Start is called before the first frame update
    void Start()
    {
        matchSystem = new Dictionary<string, List<string>>()
        {
            {"Fish", new List<string> {"Ice Cream", "Cereal"}}, {"Onion", new List<string> {"Yogurt", "Ice Cream"}}, {"Egg", new List<string> {"Peanut Butter", "Lemon"}},
            {"Milk", new List<string> {"Ketchup", "Lemon"}}, {"Banana", new List<string> {"Hot Sauce", "BBQ Sauce"}}, {"Ketchup", new List<string> {"Milk", "Yogurt"}},
            {"Peanut Butter", new List<string> {"Egg", "Spaghetti"}}, {"Spaghetti", new List<string> {"Watermelon", "Peanut Butter"}}, {"Cookie", new List<string> {"Garlic", "Broccoli"}},
            {"Ice Cream", new List<string> {"Fish", "Onion"}}, {"Pineapple", new List<string> {"Steak", "Crab"}}, {"Chocolate", new List<string> {"Garlic", "Hot Sauce"}},
            {"Garlic", new List<string> {"Cookie", "Chocolate"}}, {"BBQ Sauce", new List<string> {"Banana", "Carrot"}}, {"Carrot", new List<string> {"Coffee", "BBQ Sauce"}},
            {"Yogurt", new List<string> {"Onion", "Ketchup"}}, {"Broccoli", new List<string> {"Cookie", "Coffee"}}, {"Watermelon", new List<string> {"Steak", "Spaghetti"}},
            {"Lemon", new List<string> {"Milk", "Egg"}}, {"Coffee", new List<string> {"Carrot", "Broccoli"}}, {"Hot Sauce", new List<string> {"Banana", "Chocolate"}},
            {"Cereal", new List<string> {"Fish", "Crab"}}, {"Crab", new List<string> {"Cereal", "Pineapple"}}, {"Steak", new List<string> {"Watermelon", "Pineapple"}}
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsMatch(string food1, string food2)
    {
        
        return matchSystem.ContainsKey(food1) && matchSystem[food1].Contains(food2);
    }
}

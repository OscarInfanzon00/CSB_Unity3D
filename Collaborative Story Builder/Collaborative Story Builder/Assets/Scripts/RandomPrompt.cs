using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomPrompt : MonoBehaviour {
    public Text settingText;
    public Text characterText;
    public Text conflictText;

    private static List<string> settings = new List<string>
    {
        "A dystopian futuristic cyberpunk city.",
        "A haunted mansion in the middle of the woods.",
        "Atop a skyscraper in a moonlit metropolis.",
        "A mysterious alien planet on the outskirts of the Milky Way."

    };
    private static List<string> characters = new List<string>
    {
        "A rogue scientist obsessed with immortality.",
        "A rebellious teenager with a mysterious past.",
        "A time traveler stuck in the wrong era.",
        "A detective who can read minds."
    };

    private static List<string> conflicts = new List<string>
    {
        "A war between two secret societies.",
        "A mysterious letter changes everything.",
        "An experiment goes horribly wrong.",
        "A prophecy that must be fulfilled or prevented."
    };

    public void GeneratePrompt()
    {
        settingText.text = "Setting: " + settings[Random.Range(0, settings.Count)];
        characterText.text = "Character: " + characters[Random.Range(0, characters.Count)];
        conflictText.text = "Conflict: " + conflicts[Random.Range(0, conflicts.Count)];
    }
}
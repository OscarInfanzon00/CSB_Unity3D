using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class WordCounter : MonoBehaviour {
    public TMP_InputField inputField;
    public TMP_Text wordCountText;

    void Start() {
        inputField.onValueChanged.AddListener(UpdateWordCount);
    }

    void UpdateWordCount(string text) {
        int wordCount = CountWords(text);
        wordCountText.text = "Word Count: " + wordCount;
        
        int CountWords(string text) {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            string[] words = Regex.Split(text.Trim(), @"\W+");

            return words.Length;
        }
    }
}

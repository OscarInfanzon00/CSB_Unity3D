using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LanguageDropdownScript : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown inputLanguageDropdown;
    [SerializeField] private TMP_Dropdown outputLanguageDropdown;

    private Dictionary<string, string> languageCodes = new Dictionary<string, string>()
    {
        {"English", "en"},
        {"Spanish", "es"},
        {"French", "fr"},
        {"German", "de"},
        {"Russian", "ru"},
        {"Japanese", "ja"},
        {"Chinese (Simplified)", "zh-CN"},
        {"Chinese (Traditional)", "zh-TW"},
        {"Korean", "ko"},
        {"Portuguese", "pt"},
        {"Italian", "it"},
        {"Arabic", "ar"},
        {"Dutch", "nl"},
        {"Hindi", "hi"},
        {"Turkish", "tr"},
        {"Vietnamese", "vi"},
        {"Polish", "pl"},
        {"Swedish", "sv"},
        {"Danish", "da"},
        {"Norwegian", "no"},
        {"Finnish", "fi"},
        {"Greek", "el"},
        {"Czech", "cs"},
        {"Thai", "th"},
        {"Indonesian", "id"},
        {"Ukrainian", "uk"},
        {"Filipino", "fil"},
        {"Slovak", "sk"},
    };

    void Start()
    {
        PopulateDropdown(inputLanguageDropdown);
        PopulateDropdown(outputLanguageDropdown);

        inputLanguageDropdown.onValueChanged.AddListener(OnInputLanguageSelected);
        outputLanguageDropdown.onValueChanged.AddListener(OnOutputLanguageSelected);

        // Set defaults if needed
        SetDefaultLanguages("English", "Russian");
    }

    private void PopulateDropdown(TMP_Dropdown dropdown)
    {
        dropdown.options.Clear();
        foreach (var language in languageCodes.Keys)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(language));
        }
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void OnInputLanguageSelected(int index)
    {
        string selectedLanguage = inputLanguageDropdown.options[index].text;
        if (languageCodes.TryGetValue(selectedLanguage, out string code))
        {
            Settings.inputLangTranslation = code;
            Debug.Log($"Input language set to: {selectedLanguage} ({code})");
        }
    }

    private void OnOutputLanguageSelected(int index)
    {
        string selectedLanguage = outputLanguageDropdown.options[index].text;
        if (languageCodes.TryGetValue(selectedLanguage, out string code))
        {
            Settings.outputLangTranslation = code;
            Debug.Log($"Output language set to: {selectedLanguage} ({code})");
        }
    }

    public void SetDefaultLanguages(string inputLanguage, string outputLanguage)
    {
        SetDropdownValue(inputLanguageDropdown, inputLanguage);
        SetDropdownValue(outputLanguageDropdown, outputLanguage);

        if (languageCodes.TryGetValue(inputLanguage, out string inputCode))
        {
            Settings.inputLangTranslation = inputCode;
        }

        if (languageCodes.TryGetValue(outputLanguage, out string outputCode))
        {
            Settings.outputLangTranslation = outputCode;
        }
    }

    private void SetDropdownValue(TMP_Dropdown dropdown, string language)
    {
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == language)
            {
                dropdown.value = i;
                dropdown.RefreshShownValue();
                return;
            }
        }
    }
}

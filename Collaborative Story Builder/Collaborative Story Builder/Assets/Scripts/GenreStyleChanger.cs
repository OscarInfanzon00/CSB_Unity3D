using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GenreStyleChanger : MonoBehaviour
{
    [System.Serializable]
    public class GenreTheme
    {
        public string genreName;
        public List<Sprite> backgroundImages = new List<Sprite>(); // Match count with background objects
        public List<Sprite> panelImages = new List<Sprite>(); // Match count with UI panels
        public List<Color> panelColors = new List<Color>(); // Match count with UI panels
        public List<Sprite> buttonImages = new List<Sprite>(); // Match count with UI buttons
        public List<Color> buttonColors = new List<Color>(); // Match count with UI buttons
    }

    public TMP_Dropdown genreDropdown;
    public List<Image> backgroundImages;
    public List<Image> uiPanels;
    public List<Button> uiButtons;
    public List<GenreTheme> genreThemes;

    // Stores default values
    private List<Sprite> defaultBackgrounds = new List<Sprite>();
    private List<Sprite> defaultPanelImages = new List<Sprite>();
    private List<Color> defaultPanelColors = new List<Color>();
    private List<Sprite> defaultButtonImages = new List<Sprite>();
    private List<Color> defaultButtonColors = new List<Color>();

    void Start()
    {
        SaveDefaultTheme(); // Save initial appearance
        genreDropdown.onValueChanged.AddListener(ChangeTheme);
        InitializeDropdown();
    }

    void SaveDefaultTheme()
    {
        for (int i = 0; i < backgroundImages.Count; i++)
            defaultBackgrounds.Add(backgroundImages[i].sprite);

        for (int i = 0; i < uiPanels.Count; i++)
        {
            defaultPanelImages.Add(uiPanels[i].sprite);
            defaultPanelColors.Add(uiPanels[i].color);
        }

        for (int i = 0; i < uiButtons.Count; i++)
        {
            Image buttonImage = uiButtons[i].GetComponent<Image>();
            if (buttonImage != null) defaultButtonImages.Add(buttonImage.sprite);
            defaultButtonColors.Add(uiButtons[i].colors.normalColor);
        }
    }

    void InitializeDropdown()
    {
        genreDropdown.ClearOptions();
        genreDropdown.options.Add(new TMP_Dropdown.OptionData("Default")); // Default option
        foreach (GenreTheme theme in genreThemes)
        {
            genreDropdown.options.Add(new TMP_Dropdown.OptionData(theme.genreName));
        }
        genreDropdown.value = 0;
        ChangeTheme(0);
    }

    void ChangeTheme(int index)
    {
        if (index == 0) // Default setting
        {
            ResetToDefault();
            return;
        }

        int themeIndex = index - 1; // Adjust for "Default" option
        if (themeIndex < 0 || themeIndex >= genreThemes.Count) return;
        GenreTheme selectedTheme = genreThemes[themeIndex];

        // Update Backgrounds
        for (int i = 0; i < backgroundImages.Count; i++)
        {
            if (i < selectedTheme.backgroundImages.Count && selectedTheme.backgroundImages[i] != null)
                backgroundImages[i].sprite = selectedTheme.backgroundImages[i];
        }

        // Update UI Panels
        for (int i = 0; i < uiPanels.Count; i++)
        {
            if (i < selectedTheme.panelImages.Count && selectedTheme.panelImages[i] != null)
                uiPanels[i].sprite = selectedTheme.panelImages[i];

            if (i < selectedTheme.panelColors.Count)
                uiPanels[i].color = selectedTheme.panelColors[i];
        }

        // Update UI Buttons
        for (int i = 0; i < uiButtons.Count; i++)
        {
            Image buttonImage = uiButtons[i].GetComponent<Image>();
            if (buttonImage != null && i < selectedTheme.buttonImages.Count && selectedTheme.buttonImages[i] != null)
                buttonImage.sprite = selectedTheme.buttonImages[i];

            if (i < selectedTheme.buttonColors.Count)
            {
                ColorBlock colors = uiButtons[i].colors;
                colors.normalColor = selectedTheme.buttonColors[i];
                colors.highlightedColor = selectedTheme.buttonColors[i] * 1.2f;
                colors.pressedColor = selectedTheme.buttonColors[i] * 0.8f;
                colors.selectedColor = selectedTheme.buttonColors[i];
                colors.disabledColor = selectedTheme.buttonColors[i] * 0.5f;
                uiButtons[i].colors = colors;
            }
        }
    }

    void ResetToDefault()
    {
        // Restore Backgrounds
        for (int i = 0; i < backgroundImages.Count; i++)
            backgroundImages[i].sprite = defaultBackgrounds[i];

        // Restore UI Panels
        for (int i = 0; i < uiPanels.Count; i++)
        {
            uiPanels[i].sprite = defaultPanelImages[i];
            uiPanels[i].color = defaultPanelColors[i];
        }

        // Restore UI Buttons
        for (int i = 0; i < uiButtons.Count; i++)
        {
            Image buttonImage = uiButtons[i].GetComponent<Image>();
            if (buttonImage != null) buttonImage.sprite = defaultButtonImages[i];

            ColorBlock colors = uiButtons[i].colors;
            colors.normalColor = defaultButtonColors[i];
            colors.highlightedColor = defaultButtonColors[i] * 1.2f;
            colors.pressedColor = defaultButtonColors[i] * 0.8f;
            colors.selectedColor = defaultButtonColors[i];
            colors.disabledColor = defaultButtonColors[i] * 0.5f;
            uiButtons[i].colors = colors;
        }
    }
}



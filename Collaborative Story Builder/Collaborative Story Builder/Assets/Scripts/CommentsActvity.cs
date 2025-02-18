using UnityEngine;
using TMPro;

public class CommentsActvity : MonoBehaviour


{

    public GameObject panel;
    public TextMeshProUGUI authorFromStory;
    public TextMeshProUGUI authorInComment;
    public TextMeshProUGUI dateFromStory;
    public TextMeshProUGUI dateInComment;
    public TextMeshProUGUI StoryFromStory;
    public TextMeshProUGUI storyInComment;



    

    
    //public



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }



    //public GameObject panel; 

    
    public void TogglePanel()
    {
        if (panel != null)
        {
            panel.SetActive(!panel.activeSelf);

            
            authorInComment.text = authorFromStory.text;
            dateInComment.text = dateFromStory.text;
            storyInComment.text = StoryFromStory.text;
            
        }
    }




    // Update is called once per frame
    void Update()
    {
        
    }
}

using System;
using System.Collections.Generic;

[Serializable]
public class Story
{
    public string storyID;
    public List<string> storyTexts;
    public List<string> users;
    public DateTime timestamp;

    public Story(string id, List<string> texts, List<string> usersList, DateTime time)
    {
        storyID = id;
        storyTexts = texts;
        users = usersList;
        timestamp = time;
    }
}

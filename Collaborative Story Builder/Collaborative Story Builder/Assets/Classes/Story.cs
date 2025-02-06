using UnityEngine;
using System.Collections.Generic;

    public class Story
    {
        public string StoryId { get; set; }
        public string Title { get; set; }
        public List<string> Contributors { get; set; }
        public List<string> Content { get; set; }

        // Constructor
        public Story(string storyId, string title, List<string> contributors, List<string> content)
        {
            StoryId = storyId;
            Title = title;
            Contributors = contributors;
            Content = content;
        }
    }

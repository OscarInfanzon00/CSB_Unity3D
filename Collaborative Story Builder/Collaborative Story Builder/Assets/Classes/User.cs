using UnityEngine;

    public class User
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int UserLvL { get; set; }
        public int Words { get; set; }

        // Constructor
        public User(string userId, string username, string email, int userLvL, int words)
        {
            UserId = userId;
            Username = username;
            Email = email;
            UserLvL = userLvL;
            Words = words;
        }
    }

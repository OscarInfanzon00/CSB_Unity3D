using UnityEngine;
    public class Turn
    {
        public string UserId { get; set; }
        public bool IsTurnActive { get; set; }

        // Constructor
        public Turn(string userId, bool isTurnActive)
        {
            UserId = userId;
            IsTurnActive = isTurnActive;
        }
    }

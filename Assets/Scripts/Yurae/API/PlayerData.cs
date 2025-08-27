using System;

namespace Player
{
    [Serializable]
    public class PlayerData
    {
        public int seedTicket;
        public int gold;
        public int sunlight;
        public int seedCount;
        public int level;
    }

    [Serializable]
    public class PlayerResponse
    {
        public int status;
        public string message;
        public PlayerData data;
    }
}

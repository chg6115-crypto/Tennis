using System;

namespace TennisGame
{
    // Pure rules model: server 0 = player, server 1 = AI.
    public sealed class TennisScore
    {
        public readonly int[] Points = new int[2];
        public readonly int[] Games = new int[2];
        public readonly int[] Sets = new int[2];
        public int Server { get; private set; }
        public bool TieBreak { get; private set; }
        public int Winner { get; private set; } = -1;
        public string LastSet { get; private set; } = "";
        public bool ServeRight => (Points[0] + Points[1]) % 2 == 0;
        readonly int gamesToWin, setsToWin;
        int tieBreakFirstServer;

        public TennisScore(int gamesToWin = 6, int setsToWin = 2)
        {
            this.gamesToWin = Math.Max(1, gamesToWin);
            this.setsToWin = Math.Max(1, setsToWin);
        }

        public void Award(int side)
        {
            if (side < 0 || side > 1) throw new ArgumentOutOfRangeException(nameof(side));
            if (Winner >= 0) return;
            int other = 1 - side;
            Points[side]++;
            if (TieBreak)
            {
                if (Points[side] >= 7 && Points[side] - Points[other] >= 2)
                {
                    Games[side]++;
                    Server = 1 - tieBreakFirstServer;
                    WinSet(side);
                }
                else
                {
                    int total = Points[0] + Points[1];
                    Server = ((total + 1) / 2) % 2 == 1 ? 1 - tieBreakFirstServer : tieBreakFirstServer;
                }
                return;
            }
            if (Points[side] < 4 || Points[side] - Points[other] < 2) return;
            Points[0] = Points[1] = 0;
            Games[side]++;
            Server = 1 - Server;
            if (Games[side] >= gamesToWin && Games[side] - Games[other] >= 2)
                WinSet(side);
            else if (Games[0] == gamesToWin && Games[1] == gamesToWin)
            {
                TieBreak = true;
                tieBreakFirstServer = Server;
            }
        }

        void WinSet(int side)
        {
            LastSet = Games[0] + " - " + Games[1];
            Sets[side]++;
            Points[0] = Points[1] = Games[0] = Games[1] = 0;
            TieBreak = false;
            if (Sets[side] >= setsToWin) Winner = side;
        }

        public string PointText(int side)
        {
            if (TieBreak) return Points[side].ToString();
            if (Points[0] >= 3 && Points[1] >= 3)
                return Points[side] > Points[1 - side] ? "AD" : "40";
            return new[] { "0", "15", "30", "40" }[Math.Min(Points[side], 3)];
        }
    }
}


using System;
using TennisPrototype;

static class Program
{
    static int checks;
    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        checks++;
    }
    static void Game(TennisScore score, int side)
    {
        for (int i = 0; i < 4; i++) score.Award(side);
    }
    static void Main()
    {
        var s = new TennisScore();
        Check(s.Server == 0 && s.ServeRight, "Opening server and court");
        s.Award(0);
        Check(s.PointText(0) == "15" && !s.ServeRight, "15 and opposite service court");
        s.Award(0); s.Award(0);
        Check(s.PointText(0) == "40", "30 then 40");
        s.Award(1); s.Award(1); s.Award(1);
        Check(s.PointText(0) == "40" && s.PointText(1) == "40", "Deuce");
        s.Award(0);
        Check(s.PointText(0) == "AD" && s.Games[0] == 0, "Advantage does not win game");
        s.Award(1);
        Check(s.PointText(0) == "40" && s.Games[0] == 0, "Return to deuce");
        s.Award(1); s.Award(1);
        Check(s.Games[1] == 1 && s.Server == 1 && s.Points[0] == 0 && s.ServeRight, "Game won, server changes and points reset");
        s = new TennisScore(6, 2);
        for (int i = 0; i < 5; i++) { Game(s,0); Game(s,1); }
        Game(s,0);
        Check(s.Games[0] == 6 && s.Games[1] == 5 && s.Sets[0] == 0, "6-5 needs two-game lead");
        Game(s,1);
        Check(s.TieBreak && s.Server == 0, "6-6 begins tiebreak");
        s.Award(0);
        Check(s.Server == 1 && !s.ServeRight, "Tiebreak switches after opening point");
        s.Award(1);
        Check(s.Server == 1 && s.ServeRight, "Second server takes two points");
        s.Award(0);
        Check(s.Server == 0, "Tiebreak switches every two points");
        for (int i = 0; i < 4; i++) { s.Award(0); s.Award(1); }
        s.Award(1); // 6-6
        s.Award(0); // 7-6
        Check(s.TieBreak && s.Sets[0] == 0, "7-6 tiebreak is not finished");
        s.Award(0);
        Check(!s.TieBreak && s.Sets[0] == 1 && s.LastSet == "7 - 6" && s.Server == 1, "8-6 wins tiebreak; next set receives first");
        for (int i = 0; i < 6; i++) Game(s,0);
        Check(s.Winner == 0 && s.Sets[0] == 2, "Two sets win match");
        s.Award(1);
        Check(s.Points[1] == 0, "Finished match rejects additional score");
        s = new TennisScore(2, 1); Game(s,1); Game(s,1);
        Check(s.Winner == 1, "Short configurable match and AI victory");
        Console.WriteLine($"PASS: {checks} tennis rules checks");
    }
}

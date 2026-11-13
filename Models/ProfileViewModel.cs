namespace Histador.Models;

public class ProfileViewModel
{
    public bool IsOwnProfile { get; set; }
    public string DisplayName { get; set; } = "";
    public string Initials { get; set; } = "";
    public string? IconKey { get; set; }
    public bool IsAdmin { get; set; }

    public int Level { get; set; }
    public int Xp { get; set; }
    public int WorldDiscoveredPercent { get; set; }
    public MiniMapViewModel MiniMap { get; set; } = new();

    public string? Rank { get; set; }
    public int Mmr { get; set; }

    public int? HintPoints { get; set; }

    public int LessonsPassed { get; set; }
    public int PerfectQuizzes { get; set; }
    public int DuelsWon { get; set; }
    public int DuelsLost { get; set; }
}

public class MiniMapViewModel
{
    public double Width { get; set; }
    public double Height { get; set; }
    public List<MiniMapHex> Hexes { get; set; } = new();
}

public class MiniMapHex
{
    public string Points { get; set; } = "";
    public string State { get; set; } = "";
    public string Name { get; set; } = "";
}

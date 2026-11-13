using System.Globalization;
using Histador.Data;
using Histador.Data.Entities;
using Histador.Models;
using Microsoft.EntityFrameworkCore;

namespace Histador.Services;

public class MapState
{
    public int TotalTerritories { get; set; }
    public HashSet<int> Discovered { get; set; } = new();
    public HashSet<int> Owned { get; set; } = new();
    public HashSet<int> Contested { get; set; } = new();

    public int DiscoveredPercent => TotalTerritories == 0
        ? 0
        : (int)Math.Round(100.0 * Discovered.Count / TotalTerritories);
}

public class MapStateService
{
    private const double HexSize = 14;
    private const double Padding = 4;

    private readonly HistadorDbContext _db;

    public MapStateService(HistadorDbContext db)
    {
        _db = db;
    }

    public MapState GetForPlayer(int playerId)
    {
        var completedLessons = _db.LessonProgresses
            .Where(lp => lp.PlayerId == playerId)
            .Select(lp => lp.LessonId)
            .ToHashSet();

        var held = _db.PlayerTerritories
            .Where(pt => pt.PlayerId == playerId)
            .ToList();

        var territories = _db.Territories.AsNoTracking().ToList();

        var state = new MapState
        {
            TotalTerritories = territories.Count,
            Owned = held.Where(h => h.Status == TerritoryStatus.Owned).Select(h => h.TerritoryId).ToHashSet(),
            Contested = held.Where(h => h.Status == TerritoryStatus.Contested).Select(h => h.TerritoryId).ToHashSet(),
        };

        foreach (var t in territories)
        {
            var unlocked = t.UnlockedByLessonId == null || completedLessons.Contains(t.UnlockedByLessonId.Value);
            if (unlocked || state.Owned.Contains(t.Id) || state.Contested.Contains(t.Id))
                state.Discovered.Add(t.Id);
        }

        return state;
    }

    public MiniMapViewModel BuildMiniMap(MapState state)
    {
        var firstEraId = _db.Eras.OrderBy(e => e.OrderIndex).Select(e => e.Id).FirstOrDefault();
        var territories = _db.Territories.AsNoTracking().Where(t => t.EraId == firstEraId).ToList();

        var width = Math.Sqrt(3) * HexSize;
        var map = new MiniMapViewModel();
        double maxX = 0, maxY = 0;

        foreach (var t in territories)
        {
            var cx = Padding + width / 2 + t.MapQ * width + (t.MapR % 2 == 1 ? width / 2 : 0);
            var cy = Padding + HexSize + t.MapR * 1.5 * HexSize;
            maxX = Math.Max(maxX, cx + width / 2);
            maxY = Math.Max(maxY, cy + HexSize);

            var stateName = state.Owned.Contains(t.Id) ? "owned"
                : state.Contested.Contains(t.Id) ? "contested"
                : state.Discovered.Contains(t.Id) ? "discovered"
                : "fog";

            map.Hexes.Add(new MiniMapHex
            {
                Points = HexPoints(cx, cy),
                State = stateName,
                Name = stateName == "fog" ? "?" : (Localization.Texts.IsEnglish ? t.NameEn : t.NameBg),
            });
        }

        map.Width = Math.Ceiling(maxX + Padding);
        map.Height = Math.Ceiling(maxY + Padding);
        return map;
    }

    private static string HexPoints(double cx, double cy)
    {
        var points = new string[6];
        for (var i = 0; i < 6; i++)
        {
            var angle = Math.PI / 180 * (60 * i - 30);
            var x = cx + HexSize * Math.Cos(angle);
            var y = cy + HexSize * Math.Sin(angle);
            points[i] = x.ToString("0.#", CultureInfo.InvariantCulture) + "," + y.ToString("0.#", CultureInfo.InvariantCulture);
        }
        return string.Join(" ", points);
    }
}

using Histador.Data;
using Histador.Data.Entities;
using Histador.Models;
using Histador.Services;
using Microsoft.AspNetCore.Mvc;

namespace Histador.Controllers;

[Route("Profile")]
public class ProfileController : Controller
{
    private readonly HistadorDbContext _db;
    private readonly CurrentPlayerService _currentPlayer;
    private readonly MapStateService _mapState;

    public ProfileController(HistadorDbContext db, CurrentPlayerService currentPlayer, MapStateService mapState)
    {
        _db = db;
        _currentPlayer = currentPlayer;
        _mapState = mapState;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        var me = _currentPlayer.Get();
        if (me == null) return RedirectToAction("Index", "Home");

        return View("Index", BuildProfile(me, isOwnProfile: true));
    }

    [HttpGet("{displayName}")]
    public IActionResult Show(string displayName)
    {
        var me = _currentPlayer.Get();
        if (me == null) return RedirectToAction("Index", "Home");

        var name = displayName.ToLower();
        var player = _db.Players.FirstOrDefault(p => p.DisplayName.ToLower() == name);
        if (player == null) return NotFound();

        return View("Index", BuildProfile(player, isOwnProfile: player.Id == me.Id));
    }

    private ProfileViewModel BuildProfile(Player player, bool isOwnProfile)
    {
        var lessons = _db.LessonProgresses.Where(lp => lp.PlayerId == player.Id).ToList();

        var finishedDuelWinners = _db.Duels
            .Where(d => d.Status == DuelStatus.Finished && (d.ChallengerId == player.Id || d.OpponentId == player.Id))
            .Select(d => d.WinnerId)
            .ToList();

        var mapState = _mapState.GetForPlayer(player.Id);

        return new ProfileViewModel
        {
            IsOwnProfile = isOwnProfile,
            DisplayName = player.DisplayName,
            Initials = GetInitials(player.DisplayName),
            IconKey = player.IconKey,
            IsAdmin = player.IsAdmin,
            Level = player.Level,
            Xp = player.Xp,
            WorldDiscoveredPercent = mapState.DiscoveredPercent,
            MiniMap = _mapState.BuildMiniMap(mapState),
            Rank = null,
            Mmr = player.Mmr,
            HintPoints = isOwnProfile ? player.HintPoints : null,
            LessonsPassed = lessons.Count,
            PerfectQuizzes = lessons.Count(lp => lp.TotalQuestions > 0 && lp.CorrectAnswers == lp.TotalQuestions),
            DuelsWon = finishedDuelWinners.Count(w => w == player.Id),
            DuelsLost = finishedDuelWinners.Count(w => w != null && w != player.Id),
        };
    }

    private static string GetInitials(string displayName)
    {
        var letters = displayName.Where(char.IsLetter).Take(2).ToArray();
        return letters.Length == 0 ? "?" : new string(letters).ToUpper();
    }
}

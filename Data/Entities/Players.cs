namespace Histador.Data.Entities;

public class Player
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? IconKey { get; set; }
    public bool IsAdmin { get; set; }
    public int HintPoints { get; set; }
    public int Mmr { get; set; } = 1000;
    public string Email { get; set; } = "";
    public string Language { get; set; } = "bg";

    public int Xp { get; set; }
    public int Level { get; set; } = 1;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<LessonProgress> LessonProgress { get; set; } = new();
    public List<PlayerTerritory> Territories { get; set; } = new();
    public List<ExamAttempt> ExamAttempts { get; set; } = new();
}

public class LessonProgress
{
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public class ExamAttempt
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int EraId { get; set; }
    public Era Era { get; set; } = null!;

    public int Score { get; set; }
    public int MaxScore { get; set; }
    public bool Passed { get; set; }
    public DateTime TakenAt { get; set; } = DateTime.UtcNow;
}

public enum TerritoryStatus
{
    Owned = 0,
    Contested = 1
}

public class PlayerTerritory
{
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int TerritoryId { get; set; }
    public Territory Territory { get; set; } = null!;

    public TerritoryStatus Status { get; set; } = TerritoryStatus.Owned;
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
}

public enum DuelStatus
{
    Pending = 0,
    InProgress = 1,
    Finished = 2,
    Expired = 3,
    Declined = 4
}

public class Duel
{
    public int Id { get; set; }

    public int ChallengerId { get; set; }
    public Player Challenger { get; set; } = null!;
    public int OpponentId { get; set; }
    public Player Opponent { get; set; } = null!;
    public int TerritoryId { get; set; }
    public Territory Territory { get; set; } = null!;

    public DuelStatus Status { get; set; } = DuelStatus.Pending;
    public int ChallengerScore { get; set; }
    public int OpponentScore { get; set; }
    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }
    public int XpStake { get; set; } = 120;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    public List<DuelRound> Rounds { get; set; } = new();
}

public class DuelRound
{
    public int Id { get; set; }
    public int DuelId { get; set; }
    public Duel Duel { get; set; } = null!;
    public int RoundNumber { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public int? ChallengerAnswerId { get; set; }
    public AnswerOption? ChallengerAnswer { get; set; }
    public int? ChallengerTimeMs { get; set; }

    public int? OpponentAnswerId { get; set; }
    public AnswerOption? OpponentAnswer { get; set; }
    public int? OpponentTimeMs { get; set; }
}

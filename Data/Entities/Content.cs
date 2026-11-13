namespace Histador.Data.Entities;

public class Era
{
    public int Id { get; set; }
    public string NameBg { get; set; } = "";
    public string NameEn { get; set; } = "";
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public int OrderIndex { get; set; }

    public List<Territory> Territories { get; set; } = new();
    public List<Lesson> Lessons { get; set; } = new();
}

public class Territory
{
    public int Id { get; set; }
    public int EraId { get; set; }
    public Era Era { get; set; } = null!;

    public string NameBg { get; set; } = "";
    public string NameEn { get; set; } = "";
    public int MapQ { get; set; }
    public int MapR { get; set; }
    public bool IsCapital { get; set; }

    public int? UnlockedByLessonId { get; set; }
    public Lesson? UnlockedByLesson { get; set; }
}

public class Lesson
{
    public int Id { get; set; }
    public int EraId { get; set; }
    public Era Era { get; set; } = null!;

    public string TitleBg { get; set; } = "";
    public string TitleEn { get; set; } = "";
    public string BodyBg { get; set; } = "";
    public string BodyEn { get; set; } = "";
    public int Minutes { get; set; } = 4;
    public int OrderIndex { get; set; }
    public int XpReward { get; set; } = 10;

    public List<Question> Questions { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public string TextBg { get; set; } = "";
    public string TextEn { get; set; } = "";
    public string? ExplanationBg { get; set; }
    public string? ExplanationEn { get; set; }
    public int Difficulty { get; set; } = 1;

    public List<AnswerOption> Answers { get; set; } = new();
}

public class AnswerOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string TextBg { get; set; } = "";
    public string TextEn { get; set; } = "";
    public bool IsCorrect { get; set; }
}

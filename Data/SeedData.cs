using Histador.Data.Entities;

namespace Histador.Data;

public static class SeedData
{
    public static void Seed(HistadorDbContext db)
    {
        if (db.Eras.Any()) return;

        var founding = new Lesson
        {
            OrderIndex = 1, Minutes = 4,
            TitleBg = "Основаването на България (681 г.)",
            TitleEn = "The founding of Bulgaria (681)",
            BodyBg = "През 681 г. хан Аспарух сключва мир с Византия и българската държава на Дунав е призната.",
            BodyEn = "In 681 Khan Asparuh made peace with Byzantium and the Bulgarian state on the Danube was recognised.",
            Questions =
            {
                Q("Кой хан основава Дунавска България?", "Which khan founded Danubian Bulgaria?",
                    ("Аспарух", "Asparuh", true), ("Кубрат", "Kubrat", false),
                    ("Тервел", "Tervel", false), ("Крум", "Krum", false)),
                Q("Коя е първата столица на държавата?", "What was the state's first capital?",
                    ("Плиска", "Pliska", true), ("Преслав", "Preslav", false),
                    ("Търново", "Tarnovo", false), ("Охрид", "Ohrid", false)),
            }
        };

        var baptism = new Lesson
        {
            OrderIndex = 2, Minutes = 4,
            TitleBg = "Покръстването (864 г.)",
            TitleEn = "The Christianisation (864)",
            BodyBg = "През 864 г. княз Борис I приема християнството, а през 870 г. Българската църква получава самостоятелност.",
            BodyEn = "In 864 Prince Boris I adopted Christianity, and in 870 the Bulgarian Church gained autonomy.",
            Questions =
            {
                Q("Кой български владетел приема християнството?", "Which Bulgarian ruler adopted Christianity?",
                    ("Борис I", "Boris I", true), ("Хан Крум", "Khan Krum", false),
                    ("Цар Симеон", "Tsar Simeon", false), ("Хан Омуртаг", "Khan Omurtag", false)),
                Q("През коя година е покръстването?", "In which year did the Christianisation happen?",
                    ("864", "864", true), ("681", "681", false), ("893", "893", false), ("927", "927", false)),
            }
        };

        var simeon = new Lesson
        {
            OrderIndex = 3, Minutes = 5,
            TitleBg = "Златният век на цар Симеон",
            TitleEn = "Tsar Simeon's Golden Age",
            BodyBg = "При Симеон (893–927) столицата е преместена в Преслав, а книжовността процъфтява.",
            BodyEn = "Under Simeon (893–927) the capital moved to Preslav and literature flourished.",
            Questions =
            {
                Q("Как се нарича епохата на цар Симеон?", "What is Tsar Simeon's era called?",
                    ("Златен век", "Golden Age", true), ("Сребърен век", "Silver Age", false),
                    ("Възраждане", "Revival", false), ("Тъмни векове", "Dark Ages", false)),
                Q("В коя година е битката при Беласица (Ключ)?", "In which year was the Battle of Kleidion?",
                    ("1014", "1014", true), ("971", "971", false), ("1018", "1018", false), ("927", "927", false),
                    difficulty: 2),
            }
        };

        var era1 = new Era
        {
            OrderIndex = 1, StartYear = 681, EndYear = 1018,
            NameBg = "Първа българска държава", NameEn = "First Bulgarian State",
            Lessons = { founding, baptism, simeon },
            Territories =
            {
                T("Плиска", "Pliska", 2, 1, capital: true),
                T("Мизия", "Moesia", 1, 1),
                T("Онгъл", "Ongal", 2, 0),
                T("Добруджа", "Dobruja", 3, 1, founding),
                T("Преслав", "Preslav", 2, 2, simeon),
                T("Тракия", "Thrace", 3, 2, baptism),
                T("Дакия", "Dacia", 1, 0, baptism),
                T("Македония", "Macedonia", 1, 2, simeon),
            }
        };

        var asen = new Lesson
        {
            OrderIndex = 1, Minutes = 4,
            TitleBg = "Въстанието на Асен и Петър (1185 г.)",
            TitleEn = "The uprising of Asen and Peter (1185)",
            BodyBg = "През 1185 г. братята Асен и Петър вдигат въстание и възстановяват българската държава със столица Търново.",
            BodyEn = "In 1185 the brothers Asen and Peter led an uprising and restored the Bulgarian state, with Tarnovo as capital.",
            Questions =
            {
                Q("Коя е столицата на Втората българска държава?", "What was the capital of the Second Bulgarian State?",
                    ("Търново", "Tarnovo", true), ("Плиска", "Pliska", false),
                    ("София", "Sofia", false), ("Видин", "Vidin", false)),
            }
        };

        var era2 = new Era
        {
            OrderIndex = 2, StartYear = 1185, EndYear = 1396,
            NameBg = "Втора българска държава", NameEn = "Second Bulgarian State",
            Lessons = { asen },
            Territories = { T("Търново", "Tarnovo", 2, 1, capital: true), T("Видин", "Vidin", 0, 1, asen) }
        };

        var paisius = new Lesson
        {
            OrderIndex = 1, Minutes = 3,
            TitleBg = "Паисий Хилендарски",
            TitleEn = "Paisius of Hilendar",
            BodyBg = "През 1762 г. Паисий Хилендарски завършва „История славянобългарска“.",
            BodyEn = "In 1762 Paisius of Hilendar completed the \"Slavonic-Bulgarian History\".",
            Questions =
            {
                Q("Кой написва „История славянобългарска“?", "Who wrote the \"Slavonic-Bulgarian History\"?",
                    ("Паисий Хилендарски", "Paisius of Hilendar", true), ("Софроний Врачански", "Sophronius of Vratsa", false),
                    ("Васил Левски", "Vasil Levski", false), ("Христо Ботев", "Hristo Botev", false)),
            }
        };

        var era3 = new Era
        {
            OrderIndex = 3, StartYear = 1762, EndYear = 1878,
            NameBg = "Българско възраждане", NameEn = "Bulgarian National Revival",
            Lessons = { paisius },
            Territories = { T("Хилендар", "Hilendar", 1, 1, capital: true) }
        };

        db.Eras.AddRange(era1, era2, era3);

        var petar = new Player
        {
            Username = "petar", DisplayName = "Petar", Email = "petar@example.com", Language = "bg",
            HintPoints = 15, Mmr = 1180,
            Xp = 1240, Level = 7, CurrentStreak = 12, LongestStreak = 20, LastActiveDate = DateTime.UtcNow.Date
        };
        var ivana = new Player
        {
            Username = "ivana_k", DisplayName = "Ivana_K", Email = "ivana@example.com", Language = "en",
            HintPoints = 8, Mmr = 1320,
            Xp = 1890, Level = 9, CurrentStreak = 4, LongestStreak = 31, LastActiveDate = DateTime.UtcNow.Date
        };
        var admin = new Player
        {
            Username = "admin", DisplayName = "Admin", Email = "admin@example.com", Language = "bg",
            IsAdmin = true, LastActiveDate = DateTime.UtcNow.Date
        };
        db.Players.AddRange(petar, ivana, admin);

        db.LessonProgresses.AddRange(
            new LessonProgress { Player = petar, Lesson = founding, CorrectAnswers = 2, TotalQuestions = 2 },
            new LessonProgress { Player = petar, Lesson = baptism, CorrectAnswers = 1, TotalQuestions = 2 },
            new LessonProgress { Player = ivana, Lesson = founding, CorrectAnswers = 2, TotalQuestions = 2 },
            new LessonProgress { Player = ivana, Lesson = simeon, CorrectAnswers = 2, TotalQuestions = 2 });

        Territory Era1(string nameEn) => era1.Territories.First(t => t.NameEn == nameEn);

        foreach (var name in new[] { "Pliska", "Moesia", "Ongal", "Dobruja", "Thrace" })
            db.PlayerTerritories.Add(new PlayerTerritory { Player = petar, Territory = Era1(name) });

        db.PlayerTerritories.AddRange(
            new PlayerTerritory { Player = petar, Territory = Era1("Macedonia"), Status = TerritoryStatus.Contested },
            new PlayerTerritory { Player = ivana, Territory = Era1("Macedonia"), Status = TerritoryStatus.Contested },
            new PlayerTerritory { Player = ivana, Territory = Era1("Preslav") });

        var duel = new Duel
        {
            Challenger = ivana, Opponent = petar, Territory = Era1("Macedonia"),
            Status = DuelStatus.Pending, XpStake = 120,
            ExpiresAt = DateTime.UtcNow.AddHours(6)
        };
        var duelQuestions = era1.Lessons.SelectMany(l => l.Questions).Take(5).ToList();
        for (int i = 0; i < duelQuestions.Count; i++)
            duel.Rounds.Add(new DuelRound { RoundNumber = i + 1, Question = duelQuestions[i] });
        db.Duels.Add(duel);

        db.Duels.Add(new Duel
        {
            Challenger = ivana, Opponent = petar, Territory = Era1("Moesia"),
            Status = DuelStatus.Finished, ChallengerScore = 1, OpponentScore = 4, Winner = petar,
            CreatedAt = DateTime.UtcNow.AddDays(-2), ExpiresAt = DateTime.UtcNow.AddDays(-1)
        });

        db.SaveChanges();
    }

    private static Question Q(string textBg, string textEn,
        (string bg, string en, bool correct) a, (string bg, string en, bool correct) b,
        (string bg, string en, bool correct) c, (string bg, string en, bool correct) d,
        int difficulty = 1)
    {
        var q = new Question { TextBg = textBg, TextEn = textEn, Difficulty = difficulty };
        foreach (var (bg, en, correct) in new[] { a, b, c, d })
            q.Answers.Add(new AnswerOption { TextBg = bg, TextEn = en, IsCorrect = correct });
        return q;
    }

    private static Territory T(string nameBg, string nameEn, int q, int r,
        Lesson? unlockedBy = null, bool capital = false)
        => new() { NameBg = nameBg, NameEn = nameEn, MapQ = q, MapR = r, IsCapital = capital, UnlockedByLesson = unlockedBy };
}

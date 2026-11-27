namespace Histador.ViewModels
{
    public class ProfileViewModel
    {
        public bool IsOwnProfile { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public int? SelectedIconId { get; set; }
        public bool IsAdmin { get; set; }

        public int Level { get; set; }
        public double WorldPercent { get; set; }

        public string? Rank { get; set; }
        public int Mmr { get; set; }

        public int? HintPoints { get; set; }

        public int? LessonsPassed { get; set; }
        public int? PerfectQuizzes { get; set; }
        public int? DuelsWon { get; set; }
        public int? DuelsLost { get; set; }
    }
}

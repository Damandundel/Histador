using Histador.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Histador.Data;

public class HistadorDbContext : DbContext
{
    public HistadorDbContext(DbContextOptions<HistadorDbContext> options) : base(options)
    {
    }

    public DbSet<Era> Eras => Set<Era>();
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<AnswerOption> AnswerOptions => Set<AnswerOption>();

    public DbSet<Player> Players => Set<Player>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<PlayerTerritory> PlayerTerritories => Set<PlayerTerritory>();
    public DbSet<Duel> Duels => Set<Duel>();
    public DbSet<DuelRound> DuelRounds => Set<DuelRound>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Player>(e =>
        {
            e.HasIndex(p => p.Username).IsUnique();
            e.HasIndex(p => p.DisplayName).IsUnique();
            e.Property(p => p.DisplayName).HasMaxLength(32);
            e.HasIndex(p => p.Email).IsUnique();
            e.Property(p => p.Username).HasMaxLength(32);
            e.Property(p => p.Language).HasMaxLength(2);
        });

        b.Entity<Territory>()
            .HasOne(t => t.UnlockedByLesson)
            .WithMany()
            .HasForeignKey(t => t.UnlockedByLessonId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<LessonProgress>().HasKey(lp => new { lp.PlayerId, lp.LessonId });
        b.Entity<PlayerTerritory>().HasKey(pt => new { pt.PlayerId, pt.TerritoryId });

        b.Entity<Duel>(e =>
        {
            e.HasOne(d => d.Challenger).WithMany().HasForeignKey(d => d.ChallengerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Opponent).WithMany().HasForeignKey(d => d.OpponentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Winner).WithMany().HasForeignKey(d => d.WinnerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(d => d.Territory).WithMany().HasForeignKey(d => d.TerritoryId).OnDelete(DeleteBehavior.Restrict);
            e.Property(d => d.Status).HasConversion<string>();
        });

        b.Entity<DuelRound>(e =>
        {
            e.HasOne(r => r.Question).WithMany().HasForeignKey(r => r.QuestionId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.ChallengerAnswer).WithMany().HasForeignKey(r => r.ChallengerAnswerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.OpponentAnswer).WithMany().HasForeignKey(r => r.OpponentAnswerId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(r => new { r.DuelId, r.RoundNumber }).IsUnique();
        });

        b.Entity<PlayerTerritory>().Property(pt => pt.Status).HasConversion<string>();
    }
}

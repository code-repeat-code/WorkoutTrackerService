using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerServices.Entities;

public partial class WorkoutContext : DbContext
{
    public WorkoutContext()
    {
    }

    public WorkoutContext(DbContextOptions<WorkoutContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Workout> Workouts { get; set; }

    public virtual DbSet<WorkoutExercise> WorkoutExercises { get; set; }

    public virtual DbSet<WorkoutReport> WorkoutReports { get; set; }

    public virtual DbSet<WorkoutSchedule> WorkoutSchedules { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("Exercise_pkey");

            entity.ToTable("Exercise");

            entity.Property(e => e.Category).HasMaxLength(30);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.ExerciseName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.WorkoutId).HasName("Workout_pkey");

            entity.ToTable("Workout");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.DeletedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastUpdatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.WorkoutName).HasMaxLength(100);
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.HasKey(e => e.WorkoutExerciseId).HasName("WorkoutExercise_pkey");

            entity.ToTable("WorkoutExercise");

            entity.HasOne(d => d.Exercise).WithMany(p => p.WorkoutExercises)
                .HasForeignKey(d => d.ExerciseId)
                .HasConstraintName("WorkoutExercise_ExerciseId_fkey");

            entity.HasOne(d => d.Workout).WithMany(p => p.WorkoutExercises)
                .HasForeignKey(d => d.WorkoutId)
                .HasConstraintName("WorkoutExercise_WorkoutId_fkey");
        });

        modelBuilder.Entity<WorkoutReport>(entity =>
        {
            entity.HasKey(e => e.WorkoutReportId).HasName("WorkoutReport_pkey");

            entity.ToTable("WorkoutReport");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<WorkoutSchedule>(entity =>
        {
            entity.HasKey(e => e.WorkoutScheduleId).HasName("WorkoutSchedule_pkey");

            entity.ToTable("WorkoutSchedule");

            entity.Property(e => e.ScheduledDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.Workout).WithMany(p => p.WorkoutSchedules)
                .HasForeignKey(d => d.WorkoutId)
                .HasConstraintName("WorkoutSchedule_WorkoutId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

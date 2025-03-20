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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=workout;Username=postgres;Password=P@1905inA");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("Exercise_pkey");

            entity.ToTable("Exercise");

            entity.Property(e => e.Category).HasColumnType("character varying");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("Users_pkey");

            entity.HasIndex(e => e.Email, "Users_Email_key").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Email).HasColumnType("character varying");
            entity.Property(e => e.FirstName).HasColumnType("character varying");
            entity.Property(e => e.LastName).HasColumnType("character varying");
            entity.Property(e => e.PasswordHash).HasColumnType("character varying");
            entity.Property(e => e.RefreshToken).HasColumnType("character varying");
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Role).HasColumnType("character varying");
            entity.Property(e => e.Username).HasColumnType("character varying");
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.WorkoutId).HasName("Workout_pkey");

            entity.ToTable("Workout");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasColumnType("character varying");
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

            entity.HasOne(d => d.Workout).WithMany(p => p.WorkoutReports)
                .HasForeignKey(d => d.WorkoutId)
                .HasConstraintName("WorkoutReport_WorkoutId_fkey");
        });

        modelBuilder.Entity<WorkoutSchedule>(entity =>
        {
            entity.HasKey(e => e.WorkoutScheduleId).HasName("WorkoutSchedule_pkey");

            entity.ToTable("WorkoutSchedule");

            entity.Property(e => e.ScheduledDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Status).HasColumnType("character varying");

            entity.HasOne(d => d.Workout).WithMany(p => p.WorkoutSchedules)
                .HasForeignKey(d => d.WorkoutId)
                .HasConstraintName("WorkoutSchedule_WorkoutId_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

using ClinicDataApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Data;

public sealed class ClinicDbContext(DbContextOptions<ClinicDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<SpravkaType> SpravkaTypes => Set<SpravkaType>();
    public DbSet<Spravka> Spravki => Set<Spravka>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>()
            .HasIndex(patient => patient.Email)
            .IsUnique();

        modelBuilder.Entity<Spravka>()
            .HasOne(spravka => spravka.Patient)
            .WithMany(patient => patient.Spravki)
            .HasForeignKey(spravka => spravka.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Spravka>()
            .HasOne(spravka => spravka.SpravkaType)
            .WithMany(type => type.Spravki)
            .HasForeignKey(spravka => spravka.SpravkaTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(appointment => appointment.Patient)
            .WithMany(patient => patient.Appointments)
            .HasForeignKey(appointment => appointment.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

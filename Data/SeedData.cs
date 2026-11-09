using ClinicDataApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicDataApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(ClinicDbContext db)
    {
        if (await db.Patients.AnyAsync())
        {
            return;
        }

        var alice = new Patient
        {
            Name = "Алиса Иванова",
            BirthDate = new DateTime(2000, 5, 15),
            Phone = "+996111222333",
            Email = "alice@example.com",
            BloodType = "O+"
        };
        var bek = new Patient
        {
            Name = "Бек Садыков",
            BirthDate = new DateTime(1998, 9, 3),
            Phone = "+996555123456",
            Email = "bek@example.com",
            BloodType = "A+"
        };

        db.Patients.AddRange(alice, bek);
        db.SpravkaTypes.AddRange(
            new SpravkaType { Name = "Справка 083", Price = 100, ValidMonths = 6, Description = "Медицинская справка 083" },
            new SpravkaType { Name = "Санитарная книжка", Price = 750, ValidMonths = 12, Description = "Медицинская книжка" },
            new SpravkaType { Name = "Справка в бассейн", Price = 200, ValidMonths = 3, Description = "Допуск к занятиям в бассейне" },
            new SpravkaType { Name = "Справка для поступления", Price = 300, ValidMonths = 6, Description = "Медицинская справка для учебного заведения" }
        );
        await db.SaveChangesAsync();

        db.Spravki.AddRange(
            new Spravka { PatientId = alice.Id, SpravkaTypeId = 1, IssuedAt = DateTime.UtcNow.AddDays(-5), Notes = "Первичный осмотр" },
            new Spravka { PatientId = bek.Id, SpravkaTypeId = 2, IssuedAt = DateTime.UtcNow.AddDays(-2), Notes = "" },
            new Spravka { PatientId = alice.Id, SpravkaTypeId = 3, IssuedAt = DateTime.UtcNow.AddDays(-1), Notes = "" }
        );
        db.Appointments.Add(new Appointment
        {
            PatientId = alice.Id,
            StartsAt = DateTime.UtcNow.AddDays(2),
            Reason = "Плановый осмотр",
            Status = "Scheduled"
        });
        await db.SaveChangesAsync();
    }
}

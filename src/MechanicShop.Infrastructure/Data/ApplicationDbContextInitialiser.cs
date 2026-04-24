using MechanicShop.Domain.Customers;
using MechanicShop.Domain.Customers.Vehicles;
using MechanicShop.Domain.Employees;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks;
using MechanicShop.Domain.RepairTasks.Enum;
using MechanicShop.Domain.RepairTasks.Parts;
using MechanicShop.Domain.WorkOrders;
using MechanicShop.Domain.WorkOrders.Enum;
using MechanicShop.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace MechanicShop.Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    AppDbContext context,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration)
{
    private readonly string masterPassword = configuration["SeedSettings:MasterPassword"]!;

    public async Task InitialiseAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // -------------------------------
        // 1️⃣ Seed Roles
        // -------------------------------
        var managerRole = new IdentityRole(nameof(Role.Manager));
        var laborRole = new IdentityRole(nameof(Role.Labor));

        if (!await roleManager.RoleExistsAsync(managerRole.Name!))
            await roleManager.CreateAsync(managerRole);

        if (!await roleManager.RoleExistsAsync(laborRole.Name!))
            await roleManager.CreateAsync(laborRole);

        // -------------------------------
        // 2️⃣ Seed Identity Users
        // -------------------------------
        var users = new[]
        {
            new AppUser
            {
                Id = "19a59129-6c20-417a-834d-11a208d32d96",
                Email = "pm@localhost",
                UserName = "pm@localhost",
                EmailConfirmed = true
            },
            new AppUser
            {
                Id = "b6327240-0aea-46fc-863a-777fc4e42560",
                Email = "john.labor@localhost",
                UserName = "john.labor@localhost",
                EmailConfirmed = true
            },
            new AppUser
            {
                Id = "8104ab20-26c2-4651-b1de-c0baf04dbbd9",
                Email = "peter.labor@localhost",
                UserName = "peter.labor@localhost",
                EmailConfirmed = true
            },
            new AppUser
            {
                Id = "e17c83de-1089-4f19-bf79-5f789133d37f",
                Email = "kevin.labor@localhost",
                UserName = "kevin.labor@localhost",
                EmailConfirmed = true
            },
            new AppUser
            {
                Id = "54cd01ba-b9ae-4c14-bab6-f3df0219ba4c",
                Email = "suzan.labor@localhost",
                UserName = "suzan.labor@localhost",
                EmailConfirmed = true
            }
        };

        foreach (var user in users)
        {
            if (!await userManager.Users.AnyAsync(u => u.Email == user.Email))
            {
                await userManager.CreateAsync(user, masterPassword);

                if (user.Email!.Contains("pm", StringComparison.OrdinalIgnoreCase))
                    await userManager.AddToRoleAsync(user, managerRole.Name!);
                else
                    await userManager.AddToRoleAsync(user, laborRole.Name!);
            }
        }

        // -------------------------------
        // 3️⃣ Seed Employees
        // -------------------------------
        if (!await context.Employees.AnyAsync())
        {
            context.Employees.AddRange(
                Employee.Create("Primary", "Manager", Role.Manager).Value,
                Employee.Create("John", "S.", Role.Labor).Value,
                Employee.Create("Peter", "R.", Role.Labor).Value,
                Employee.Create("Kevin", "M.", Role.Labor).Value,
                Employee.Create("Suzan", "L.", Role.Labor).Value
            );
        }

        // -------------------------------
        // 4️⃣ Seed Customers + Vehicles
        // -------------------------------
        if (!await context.Customers.AnyAsync())
        {
            var vehiclesList01 = new[]
            {
                Vehicle.Create("Toyota", "Camry", 2020, "ABC123").Value,
                Vehicle.Create("Honda", "Civic", 2018, "XYZ456").Value
            };

            var vehiclesList02 = new[]
            {
                Vehicle.Create("Ford", "Focus", 2021, "DEF789").Value,
                Vehicle.Create("Chevrolet", "Malibu", 2019, "GHI012").Value
            };

            var vehiclesList03 = new[]
            {
                Vehicle.Create("Tesla", "Model 3", 2022, "TESLA3").Value
            };

            var vehiclesList04 = new[]
            {
                Vehicle.Create("BMW", "X5", 2017, "BMWX5").Value
            };

            context.Customers.AddRange(
                Customer.Create("Ahmed", "Ali", "ahmed.ali@example.com", "+201012345678", [.. vehiclesList01]).Value,
                Customer.Create("Sarah", "Peter", "sarah.peter@example.com", "+201023456789", [.. vehiclesList02]).Value,
                Customer.Create("Michael", "Smith", "michael.smith@example.com", "+201034567890", [.. vehiclesList03]).Value,
                Customer.Create("Emily", "Johnson", "emily.johnson@example.com", "+201045678901", [.. vehiclesList04]).Value
            );
        }

        // -------------------------------
        // 5️⃣ Seed RepairTasks + Parts
        // -------------------------------
        if (!await context.RepairTasks.AnyAsync())
        {
            context.RepairTasks.AddRange(
                RepairTask.Create("Engine Oil Change", 50m, RepairDurationMinutes.Min60,
                    [
                        Part.Create("Engine Oil", 25m, 1).Value,
                        Part.Create("Oil Filter", 10m, 1).Value
                    ])
                    .Value,

                RepairTask.Create("Brake Replacement", 150m, RepairDurationMinutes.Min90,
                    [
                        Part.Create("Brake Pads", 40m, 2).Value,
                        Part.Create("Brake Fluid", 15m, 1).Value
                    ])
                    .Value,

                RepairTask.Create("Tire Rotation", 30m, RepairDurationMinutes.Min45,
                    [
                        Part.Create("Tire Valve", 5m, 4).Value
                    ])
                    .Value,

                RepairTask.Create("Battery Replacement", 70m, RepairDurationMinutes.Min30,
                    [
                        Part.Create("Car Battery", 120m, 1).Value
                    ])
                    .Value,

                RepairTask.Create("Air Filter Replacement", 40m, RepairDurationMinutes.Min30,
                    [
                        Part.Create("Air Filter", 20m, 1).Value,
                        Part.Create("Cabin Filter", 25m, 1).Value
                    ])
                    .Value
            );
        }

        await context.SaveChangesAsync();

        // -------------------------------
        // 6️⃣ Seed WorkOrders (Completed, InProgress, Scheduled)
        // -------------------------------
        if (!await context.WorkOrders.AnyAsync())
        {
            var vehicles = await context.Vehicles.ToListAsync();
            var repairTasks = await context.RepairTasks.ToListAsync();

            var labors = await context.Employees
                .Where(e => e.Role == Role.Labor)
                .Select(e => e.Id)
                .ToArrayAsync();

            var spots = new[] { Spot.A, Spot.B, Spot.C, Spot.D };
            var nowUtc = DateTimeOffset.UtcNow;
            var random = new Random();

            var workOrders = new List<WorkOrder>();

            // 🔹 Completed WorkOrders (5 Completed)
            for (int i = 0; i < 5; i++)
            {
                var task = repairTasks[i % repairTasks.Count];
                var start = nowUtc.AddDays(-i - 1);
                var end = start.AddMinutes((int)task.EstimatedRepairDurationMinutes);

                var workOrder = WorkOrder.Create(
                    vehicles[i % vehicles.Count].Id,
                    start,
                    end,
                    labors[i % labors.Length],
                    spots[i % spots.Length],
                    [task]
                ).Value;

                workOrder.UpdateStatus(WorkOrderState.Completed, DateTimeOffset.UtcNow);
                workOrders.Add(workOrder);
            }

            // 🔹 InProgress WorkOrders (3 InProgress)
            for (int i = 0; i < 3; i++)
            {
                var vehicle = vehicles[i % vehicles.Count];
                var labor = labors[i % labors.Length];
                var spot = spots[i % spots.Length];
                var tasks = repairTasks.Skip(i).Take(2).ToList();
                var start = nowUtc.AddMinutes(-20 * (i + 1));
                var end = start.AddMinutes(tasks.Sum(t => (int)t.EstimatedRepairDurationMinutes));

                var workOrder = WorkOrder.Create(
                    vehicle.Id,
                    start,
                    end,
                    labor,
                    spot,
                    tasks
                ).Value;

                workOrder.UpdateStatus(WorkOrderState.InProgress, DateTimeOffset.UtcNow);
                workOrders.Add(workOrder);
            }

            // 🔹 Scheduled WorkOrders (5 Scheduled)
            for (int i = 0; i < 5; i++)
            {
                var vehicle = vehicles[i % vehicles.Count];
                var labor = labors[i % labors.Length];
                var spot = spots[i % spots.Length];

                var taskCount = random.Next(1, 3);
                var selectedTasks = repairTasks.OrderBy(_ => random.Next()).Take(taskCount).ToList();

                var startOffset = random.Next(1, 6);
                var start = nowUtc.AddHours(startOffset);
                var duration = selectedTasks.Sum(t => (int)t.EstimatedRepairDurationMinutes);
                var end = start.AddMinutes(duration);

                var workOrder = WorkOrder.Create(
                    vehicle.Id,
                    start,
                    end,
                    labor,
                    spot,
                    selectedTasks
                ).Value;

                workOrder.UpdateStatus(WorkOrderState.Scheduled, DateTimeOffset.UtcNow);
                workOrders.Add(workOrder);
            }

            context.WorkOrders.AddRange(workOrders);
            await context.SaveChangesAsync();
        }
    }
}

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}
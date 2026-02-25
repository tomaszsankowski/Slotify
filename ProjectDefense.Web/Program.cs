using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using ProjectDefense.Core.Dtos;
using ProjectDefense.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/api/rooms", async (ApplicationDbContext context) =>
{
    var rooms = await context.Rooms
        .Select(r => new RoomDto { Id = r.Id, Name = r.Name, RoomNumber = r.RoomNumber })
        .ToListAsync();
    return Results.Ok(rooms);
})
.WithName("GetRooms")
.WithTags("API");

app.MapGet("/api/slots/available", async (ApplicationDbContext context) =>
{
    var slots = await context.Reservations
        .Where(r => r.StudentId == null && !r.IsBlocked && r.StartTime > DateTime.Now)
        .Include(r => r.SupervisorAvailability.Room)
        .Include(r => r.SupervisorAvailability.Supervisor)
        .OrderBy(r => r.StartTime)
        .Select(r => new AvailableSlotDto
        {
            Id = r.Id,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            RoomName = r.SupervisorAvailability.Room.Name,
            SupervisorName = r.SupervisorAvailability.Supervisor.UserName
        })
        .ToListAsync();
    return Results.Ok(slots);
})
.WithName("GetAvailableSlots")
.WithTags("API");

app.MapPost("/api/slots/{id}/book", async (int id, BookSlotRequestDto request, ApplicationDbContext context, UserManager<ApplicationUser> userManager) =>
{
    if (string.IsNullOrEmpty(request.StudentId))
    {
        return Results.BadRequest("StudentId is required.");
    }

    var appUser = await userManager.FindByIdAsync(request.StudentId);
    if (appUser == null)
    {
        return Results.NotFound("Student with the provided ID was not found.");
    }

    if (await userManager.IsLockedOutAsync(appUser))
    {
        return Results.Forbid();
    }

    var hasReservation = await context.Reservations.AnyAsync(r => r.StudentId == appUser.Id);
    if (hasReservation)
    {
        return Results.Conflict("This student already has a reservation.");
    }

    var reservation = await context.Reservations.FindAsync(id);
    if (reservation == null || reservation.StudentId != null || reservation.IsBlocked)
    {
        return Results.NotFound("This time slot is not available.");
    }

    reservation.StudentId = appUser.Id;
    await context.SaveChangesAsync();

    return Results.Ok("Reservation successful.");
})
.WithName("BookSlot")
.WithTags("API");


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    var roles = new[] { "Student", "Supervisor" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create default supervisor
    var supervisorEmail = "Supervisor@test.com";
    if (await userManager.FindByEmailAsync(supervisorEmail) == null)
    {
        var defaultSupervisor = new ApplicationUser { UserName = supervisorEmail, Email = supervisorEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(defaultSupervisor, "Password123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(defaultSupervisor, "Supervisor");
        }
    }

    // Create default student
    var studentEmail = "student@test.com";
    if (await userManager.FindByEmailAsync(studentEmail) == null)
    {
        var defaultStudent = new ApplicationUser { UserName = studentEmail, Email = studentEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(defaultStudent, "Password123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(defaultStudent, "Student");
        }
    }

    // Log all students
    Console.WriteLine("--- Listing all students in the database ---");
    var students = await userManager.GetUsersInRoleAsync("Student");
    if (students.Any())
    {
        foreach (var student in students)
        {
            Console.WriteLine($"Student found: UserName = {student.UserName}, ID = {student.Id}");
        }
    }
    else
    {
        Console.WriteLine("No students found in the database.");
    }
    Console.WriteLine("--- End of student list ---");
}

app.Run();

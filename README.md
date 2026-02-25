# Slotify

A project defense scheduling system built with **ASP.NET Core 8 (Razor Pages)**. Supervisors define their availability and manage lab rooms, while students browse open time slots and book exactly one slot for their project defense.

![Slotify screenshot](docs/screenshot.png)

## Features

| Feature                    | Description                                                                                                                   |
| :------------------------- | :---------------------------------------------------------------------------------------------------------------------------- |
| **Supervisor Module**      | Full CRUD for lab rooms, availability window management (date range, hours, slot duration), automatic slot generation.        |
| **Reservation Management** | Supervisors see all reservations and can cancel any of them (even occupied ones) or reschedule a student to a different slot. |
| **Period Blocking**        | Supervisors can block a time period (e.g. sick leave) — existing reservations are cancelled and new ones prevented.           |
| **Student Banning**        | Supervisors can lock a student's account, preventing them from logging in or making reservations.                             |
| **Student Module**         | Students see only free, future, unblocked slots. They can reserve exactly one slot at a time.                                 |
| **Change / Cancel**        | Students can cancel their reservation or switch to a different slot (cancel old + book new).                                  |
| **Authentication & Roles** | ASP.NET Core Identity with "Student" and "Supervisor" roles; registration with e-mail confirmation via SendGrid.              |
| **Minimal API + Swagger**  | REST endpoints: `GET /api/rooms`, `GET /api/slots/available`, `POST /api/slots/{id}/book` with Swagger documentation.         |
| **Console Client**         | Interactive CLI app for browsing available slots and booking via the API.                                                     |
| **Data Export**            | Export reservation lists for a selected room and date range to `.txt`, `.xlsx` (ClosedXML), or `.pdf` (QuestPDF).             |
| **Validation**             | Form validation, overlapping availability detection, automatic blocking of past reservations.                                 |

## Tech Stack

- **Framework:** ASP.NET Core 8 Razor Pages (.NET 8)
- **Database:** SQL Server (LocalDB) + Entity Framework Core 8
- **Authentication:** ASP.NET Core Identity with roles
- **Email:** SendGrid (`IEmailSender`)
- **API:** Minimal API + Swashbuckle (Swagger)
- **Export:** ClosedXML (`.xlsx`), QuestPDF (`.pdf`)
- **Frontend:** Bootstrap, jQuery, Razor Pages

## Project Structure

```
Slotify/
├── ProjectDefense.sln                    # Solution file
│
├── ProjectDefense.Core/                  # Class library — shared data model
│   ├── Data/
│   │   ├── ApplicationUser.cs            # User entity (inherits from IdentityUser)
│   │   ├── Reservation.cs               # Reservation entity (time slot)
│   │   ├── Room.cs                       # Lab room entity
│   │   └── SupervisorAvailability.cs     # Supervisor availability entity
│   └── Dtos/
│       ├── AvailableSlotDto.cs           # Available slot DTO (API)
│       ├── BookSlotRequestDto.cs         # Booking request DTO (API)
│       └── RoomDto.cs                    # Room DTO (API)
│
├── ProjectDefense.Web/                   # ASP.NET Core app (Razor Pages + Minimal API)
│   ├── Program.cs                        # App config, middleware, API endpoints, data seeding
│   ├── Data/
│   │   ├── ApplicationDbContext.cs       # EF Core DbContext
│   │   └── Migrations/                   # Database migrations
│   ├── Services/
│   │   └── EmailSender.cs               # IEmailSender implementation (SendGrid)
│   ├── Pages/
│   │   ├── Index.cshtml                  # Landing page
│   │   ├── ChooseRole.cshtml             # Role selection after registration
│   │   ├── Rooms/                        # Room CRUD (Index, Create, Edit, Delete, Details)
│   │   ├── Student/
│   │   │   └── Reservations/             # Student module (Index, Book, MyReservation)
│   │   └── Supervisor/
│   │       ├── Availabilities/           # Availability management (Index, Create, Edit, Delete)
│   │       ├── Reservations/             # Reservation management (Index, Cancel, Reschedule)
│   │       ├── BlockPeriod.cshtml        # Block time periods
│   │       ├── UnblockPeriod.cshtml      # Unblock time periods
│   │       ├── Students.cshtml           # Student list (banning)
│   │       └── Export.cshtml             # Data export (txt/xlsx/pdf)
│   ├── Areas/Identity/Pages/             # Login / registration pages (Identity UI)
│   └── wwwroot/                          # Static assets (CSS, JS, libraries)
│
└── ProjectDefense.Cli/                   # Console app — API client
    └── Program.cs                        # Menu: browse slots, rooms, book a slot
```

## Requirements

- **.NET SDK:** 8.0 or higher
- **SQL Server LocalDB** (ships with Visual Studio)
- **SendGrid API key** (for registration confirmation emails)

## How to Run

1. **Clone the repository:**

   ```bash
   git clone https://github.com/<your-username>/Slotify.git
   cd Slotify
   ```

2. **Configure SendGrid (User Secrets):**

   ```bash
   cd ProjectDefense.Web
   dotnet user-secrets set "SendGridKey" "<your-api-key>"
   cd ..
   ```

3. **Apply migrations and start the web app:**

   ```bash
   dotnet restore
   dotnet ef database update --project ProjectDefense.Web
   dotnet run --project ProjectDefense.Web
   ```

4. **Open in browser:**
   Navigate to `https://localhost:7197` (or the URL shown in the console).

5. **Test accounts (seeded on first run):**
   - **Supervisor:** `Supervisor@test.com` / `Password123!`
   - **Student:** `student@test.com` / `Password123!`

6. **Console client (optional):**
   In a separate terminal, with the web app running:
   ```bash
   dotnet run --project ProjectDefense.Cli
   ```

## API

Swagger documentation is available at `/swagger` (Development mode).

| Method | Endpoint               | Description                                  |
| :----- | :--------------------- | :------------------------------------------- |
| `GET`  | `/api/rooms`           | List all rooms                               |
| `GET`  | `/api/slots/available` | List free, future time slots                 |
| `POST` | `/api/slots/{id}/book` | Book a slot (body: `{ "studentId": "..." }`) |

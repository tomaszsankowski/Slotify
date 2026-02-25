using System.Net.Http.Json;
using ProjectDefense.Core.Dtos;

const string apiBaseUrl = "https://localhost:7197";

var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };

while (true)
{
    Console.WriteLine("\nChoose an option:");
    Console.WriteLine("1. View available slots");
    Console.WriteLine("2. View rooms");
    Console.WriteLine("3. Book a slot");
    Console.WriteLine("0. Exit");
    Console.Write("> ");

    var choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await ViewAvailableSlots();
            break;
        case "2":
            await ViewRooms();
            break;
        case "3":
            await BookSlot();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Invalid option. Please try again.");
            break;
    }
}

async Task BookSlot()
{
    Console.Write("Enter your Student ID: ");
    var studentId = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(studentId))
    {
        Console.WriteLine("Student ID cannot be empty.");
        return;
    }

    Console.Write("Enter the ID of the slot you want to book: ");
    if (!int.TryParse(Console.ReadLine(), out var slotId))
    {
        Console.WriteLine("Invalid slot ID.");
        return;
    }

    try
    {
        var request = new BookSlotRequestDto { StudentId = studentId };
        var response = await client.PostAsJsonAsync($"/api/slots/{slotId}/book", request);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Reservation successful!");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed to book slot. Status: {response.StatusCode}. Reason: {error}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error booking slot: {ex.Message}");
    }
}

async Task ViewAvailableSlots()
{
    try
    {
        var slots = await client.GetFromJsonAsync<List<AvailableSlotDto>>("/api/slots/available");
        if (slots != null && slots.Any())
        {
            Console.WriteLine("\n--- Available Slots ---");
            foreach (var slot in slots)
            {
                Console.WriteLine($"ID: {slot.Id} | {slot.StartTime:g} - {slot.EndTime:t} | Room: {slot.RoomName} | Supervisor: {slot.SupervisorName}");
            }
        }
        else
        {
            Console.WriteLine("No available slots found.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching slots: {ex.Message}");
    }
}

async Task ViewRooms()
{
    try
    {
        var rooms = await client.GetFromJsonAsync<List<RoomDto>>("/api/rooms");
        if (rooms != null && rooms.Any())
        {
            Console.WriteLine("\n--- Available Rooms ---");
            foreach (var room in rooms)
            {
                Console.WriteLine($"ID: {room.Id} | Name: {room.Name} | Number: {room.RoomNumber}");
            }
        }
        else
        {
            Console.WriteLine("No rooms found.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching rooms: {ex.Message}");
    }
}
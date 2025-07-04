using Gomotel.Domain.Entities;
using Gomotel.Domain.Enums;
using Gomotel.Domain.ValueObjects;

namespace Gomotel.Tests;

public class DomainTests
{
    public static void RunTests()
    {
        Console.WriteLine("Running Domain Tests...");

        // Test Address Value Object
        TestAddressCreation();

        // Test Money Value Object
        TestMoneyOperations();

        // Test TimeRange Value Object
        TestTimeRangeOperations();

        // Test Motel Entity
        TestMotelCreation();

        // Test Room Entity
        TestRoomCreation();

        // Test Reservation Entity
        TestReservationCreation();

        Console.WriteLine("All Domain Tests Passed!");
    }

    private static void TestAddressCreation()
    {
        Console.WriteLine("Testing Address creation...");

        var address = Address.Create("123 Main St", "Anytown", "CA", "12345", "USA");

        Console.WriteLine($"Address created: {address}");

        try
        {
            Address.Create("", "City", "State", "12345", "USA");
            throw new Exception("Should have thrown exception for empty street");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Address validation working correctly");
        }
    }

    private static void TestMoneyOperations()
    {
        Console.WriteLine("Testing Money operations...");

        var money1 = Money.Create(100.50m, "USD");
        var money2 = Money.Create(50.25m, "USD");

        var sum = money1.Add(money2);
        Console.WriteLine($"${money1.Amount} + ${money2.Amount} = ${sum.Amount}");

        var difference = money1.Subtract(money2);
        Console.WriteLine($"${money1.Amount} - ${money2.Amount} = ${difference.Amount}");

        try
        {
            Money.Create(-10, "USD");
            throw new Exception("Should have thrown exception for negative amount");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("✓ Money validation working correctly");
        }
    }

    private static void TestTimeRangeOperations()
    {
        Console.WriteLine("Testing TimeRange operations...");

        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(2);

        var timeRange = TimeRange.Create(startTime, endTime);
        Console.WriteLine($"TimeRange: {timeRange.StartTime} to {timeRange.EndTime}");
        Console.WriteLine($"Duration: {timeRange.Duration.TotalHours} hours");

        var overlappingRange = TimeRange.Create(startTime.AddHours(1), endTime.AddHours(1));
        Console.WriteLine($"Overlaps: {timeRange.OverlapsWith(overlappingRange)}");
    }

    private static void TestMotelCreation()
    {
        Console.WriteLine("Testing Motel creation...");

        var address = Address.Create("123 Hotel Ave", "Miami", "FL", "33101", "USA");
        var ownerId = Guid.NewGuid();

        var motel = Motel.Create(
            "Sunset Motel",
            "A beautiful motel by the beach",
            address,
            "+1-555-0123",
            "info@sunsetmotel.com",
            ownerId,
            "https://example.com/image.jpg"
        );

        Console.WriteLine($"Motel created: {motel.Name} at {motel.Address}");
        Console.WriteLine($"Owner ID: {motel.OwnerId}");
        Console.WriteLine($"Active: {motel.IsActive}");

        // Test deactivation
        motel.Deactivate();
        Console.WriteLine($"After deactivation - Active: {motel.IsActive}");

        motel.Activate();
        Console.WriteLine($"After reactivation - Active: {motel.IsActive}");
    }

    private static void TestRoomCreation()
    {
        Console.WriteLine("Testing Room creation...");

        var motelId = Guid.NewGuid();
        var pricePerHour = Money.Create(75.00m, "USD");

        var room = Room.Create(
            motelId,
            "101",
            "Deluxe Room",
            "A comfortable room with ocean view",
            RoomType.Deluxe,
            2,
            pricePerHour,
            "https://example.com/room.jpg"
        );

        Console.WriteLine($"Room created: {room.Name} (#{room.RoomNumber})");
        Console.WriteLine($"Type: {room.Type}, Capacity: {room.Capacity}");
        Console.WriteLine($"Price per hour: {room.PricePerHour}");
        Console.WriteLine($"Available: {room.IsAvailable}");

        // Test availability
        room.SetAvailability(false);
        Console.WriteLine($"After setting unavailable: {room.IsAvailable}");
    }

    private static void TestReservationCreation()
    {
        Console.WriteLine("Testing Reservation creation...");

        var motelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var startTime = DateTime.UtcNow.AddHours(1);
        var endTime = startTime.AddHours(3);
        var timeRange = TimeRange.Create(startTime, endTime);

        var totalAmount = Money.Create(225.00m, "USD"); // 3 hours * $75

        var reservation = Reservation.Create(
            motelId,
            roomId,
            userId,
            timeRange,
            totalAmount,
            "Please prepare extra towels"
        );

        Console.WriteLine($"Reservation created: {reservation.Id}");
        Console.WriteLine($"Status: {reservation.Status}");
        Console.WriteLine(
            $"Time: {reservation.TimeRange.StartTime} to {reservation.TimeRange.EndTime}"
        );
        Console.WriteLine($"Total Amount: {reservation.TotalAmount}");
        Console.WriteLine($"Special Requests: {reservation.SpecialRequests}");

        // Test status changes
        reservation.Confirm();
        Console.WriteLine($"After confirmation - Status: {reservation.Status}");

        reservation.CheckIn();
        Console.WriteLine($"After check-in - Status: {reservation.Status}");
        Console.WriteLine($"Check-in time: {reservation.CheckInTime}");

        reservation.CheckOut();
        Console.WriteLine($"After check-out - Status: {reservation.Status}");
        Console.WriteLine($"Check-out time: {reservation.CheckOutTime}");

        Console.WriteLine($"Domain events count: {reservation.DomainEvents.Count}");
        foreach (var domainEvent in reservation.DomainEvents)
        {
            Console.WriteLine($"Event: {domainEvent.GetType().Name} at {domainEvent.OccurredOn}");
        }
    }
}

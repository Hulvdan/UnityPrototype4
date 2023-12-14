namespace BFG.Runtime.Entities {
public enum MapResourceBookingType {
    Construction,
    Processing,
}

public struct MapResourceBooking {
    public MapResourceBookingType type;
    public Building building;
    public int priority;

    public static MapResourceBooking FromResourceToBook(ResourceToBook resourceToBook) {
        return new() {
            building = resourceToBook.building,
            priority = resourceToBook.priority,
            type = resourceToBook.bookingType,
        };
    }
}
}

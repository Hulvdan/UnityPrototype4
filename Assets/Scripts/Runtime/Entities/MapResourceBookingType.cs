namespace BFG.Runtime.Entities {
public enum MapResourceBookingType {
    Construction,
    Processing,
}

public struct MapResourceBooking {
    public MapResourceBookingType Type;
    public Building Building;
    public int Priority;

    public static MapResourceBooking FromResourceToBook(ResourceToBook resourceToBook) {
        return new() {
            Building = resourceToBook.Building,
            Priority = resourceToBook.Priority,
            Type = resourceToBook.BookingType,
        };
    }
}
}

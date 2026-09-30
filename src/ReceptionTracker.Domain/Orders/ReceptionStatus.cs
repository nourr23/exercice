namespace ReceptionTracker.Domain.Orders;

public enum ReceptionStatus
{
    Pending,             // nothing received
    PartiallyReceived,   // some received → the "indeterminate" checkbox
    Received             // everything received
}
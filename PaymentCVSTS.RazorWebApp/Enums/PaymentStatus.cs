namespace PaymentCVSTS.RazorWebApp.Enums
{
    public enum PaymentStatus : byte
    {
        Pending = 0,  // Payment is pending
        Completed = 1, // Payment is completed
        Failed = 2  // Payment failed
    }
}
namespace Notification.Service.Models
{
    public class ProductEvent
    {
        public int Id { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public DateTime ReceivedAt { get; set; }
    }
}

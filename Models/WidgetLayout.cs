namespace NIA_CRM.Models
{
    public class WidgetLayout
    {
        public int Id { get; set; }  // Primary Key
        public int UserId { get; set; }  // Tracks which user the layout belongs to
        public int WidgetId { get; set; }  // ID of the widget
        public int PositionX { get; set; }  // X position of the widget
        public int PositionY { get; set; }  // Y position of the widget
        public int Width { get; set; }  // Width of the widget
        public int Height { get; set; }  // Height of the widget
        public bool IsOnDashboard { get; set; }  // Flag to indicate if the widget is on the dashboard
        public DateTime CreatedAt { get; set; } = DateTime.Now;  // Created timestamp
        public DateTime UpdatedAt { get; set; } = DateTime.Now;  // Last updated timestamp
    }
}

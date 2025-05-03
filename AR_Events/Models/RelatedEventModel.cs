public class RelatedEventModel
{
    public int Id { get; set; }
    public string Event_Name { get; set; }
    public DateTime Event_StartDate { get; set; }
    public DateTime Event_EndDate { get; set; }
    public decimal EventCost_PerPerson { get; set; }
    public decimal GST { get; set; }
    public string Event_Organizer { get; set; }
}

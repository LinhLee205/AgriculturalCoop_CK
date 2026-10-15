namespace AgriculturalCoop.CoreBusiness.Entities;

public class DealerNotification
{
    public int DealerNotificationId { get; set; }
    public int DealerId { get; set; }
    public int HarvestBatchId { get; set; }
    public int PreOrderLineId { get; set; }
    public int Kind { get; set; } // 0: allocation result; 1: reviewed supplement
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsAiGenerated { get; set; }
    public bool IsSuperseded { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public DateTime? ReadAt { get; set; }
}

namespace AgriculturalCoop.CoreBusiness.Entities;

public sealed record AllocationNoticeSnapshot(
    int BatchId, string BatchCode, string ProductName, int PreOrderLineId, int PreOrderId,
    int DealerId, string DealerName, string RecipientEmail,
    decimal ExpectedQuantity, decimal ActualQuantity, decimal RequestedQuantity,
    decimal AllocatedQuantity, decimal UnitPrice, string Unit);

public sealed class AllocationNoticeDraft
{
    public required AllocationNoticeSnapshot Snapshot { get; init; }
    public required string Fingerprint { get; init; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    public bool IsAiGenerated { get; set; }
}

public sealed record AllocationNoticeFacts(string BatchCode, string ProductName,
    decimal ExpectedQuantity, decimal ActualQuantity, decimal RequestedQuantity,
    decimal AllocatedQuantity, decimal UnitPrice, string Unit, string Reason);
public sealed record AllocationNoticeWording(string Introduction, string Closing);

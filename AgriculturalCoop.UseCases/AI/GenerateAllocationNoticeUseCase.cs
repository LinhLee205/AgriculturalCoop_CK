using System;
using System.Text;
using System.Threading.Tasks;
using AgriculturalCoop.CoreBusiness.Entities;
using AgriculturalCoop.CoreBusiness.Services;

namespace AgriculturalCoop.UseCases.AI
{
    public interface IGenerateAllocationNoticeUseCase
    {
        Task<string> ExecuteAsync(
            HarvestBatch batch, 
            AllocationResultItem allocationItem, 
            string? weatherFactor = "Mưa lũ đầu vụ ảnh hưởng nhẹ đến tỷ lệ đậu quả");
    }

    public class GenerateAllocationNoticeUseCase : IGenerateAllocationNoticeUseCase
    {
        public Task<string> ExecuteAsync(
            HarvestBatch batch, 
            AllocationResultItem allocationItem, 
            string? weatherFactor = "Mưa lũ đầu vụ ảnh hưởng nhẹ đến tỷ lệ đậu quả")
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Kính gửi Quý Đại lý: {allocationItem.DealerName},");
            sb.AppendLine();
            sb.AppendLine($"Ban Quản trị Hợp tác xã Nông nghiệp xin thông báo kết quả thu hoạch và phân bổ sản lượng mùa vụ đối với lô [{batch.BatchCode}] - {batch.Product?.ProductName}:");
            sb.AppendLine();
            sb.AppendLine($"• Sản lượng dự kiến toàn lô: {batch.ExpectedQuantity:N0} kg");
            sb.AppendLine($"• Sản lượng thực tế thu hoạch: {batch.ActualQuantity:N0} kg");
            sb.AppendLine($"• Nguyên nhân biến động: {weatherFactor}.");
            sb.AppendLine();
            sb.AppendLine($"--- THÔNG TIN PHÂN BỔ ĐƠN ĐẶT TRƯỚC #{allocationItem.PreOrderId} ---");
            sb.AppendLine($"• Khối lượng đại lý đặt trước: {allocationItem.RequestedQuantity:N0} kg");
            sb.AppendLine($"• Khối lượng được phân bổ thực tế: {allocationItem.AllocatedQuantity:N0} kg ({allocationItem.AllocationRatioPercent}% so với nhu cầu)");
            sb.AppendLine($"• Đơn giá chốt bao tiêu: {batch.UnitPrice:N0} VNĐ/kg");
            sb.AppendLine($"• Thành tiền thực nhận: {(allocationItem.AllocatedQuantity * batch.UnitPrice):N0} VNĐ");
            sb.AppendLine();
            sb.AppendLine("Hợp tác xã cam kết chất lượng nông sản đạt chuẩn VietGAP và sẽ bắt đầu điều phối giao hàng từ sáng mai.");
            sb.AppendLine("Trân trọng cảm ơn sự đồng hành và thấu hiểu của Quý Đại lý!");
            sb.AppendLine("--- BAN QUẢN TRỊ HỢP TÁC XÃ ---");

            return Task.FromResult(sb.ToString());
        }
    }
}

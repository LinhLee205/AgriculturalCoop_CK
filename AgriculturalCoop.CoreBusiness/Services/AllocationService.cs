using System;
using System.Collections.Generic;
using System.Linq;
using AgriculturalCoop.CoreBusiness.Entities;

namespace AgriculturalCoop.CoreBusiness.Services
{
    public class AllocationResultItem
    {
        public int PreOrderLineId { get; set; }
        public int PreOrderId { get; set; }
        public int DealerId { get; set; }
        public string DealerName { get; set; } = string.Empty;
        public decimal RequestedQuantity { get; set; }
        public decimal AllocatedQuantity { get; set; }
        public decimal AllocationRatioPercent { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public class AllocationService
    {
        /// <summary>
        /// Thuật toán phân bổ sản lượng thực tế cho các đơn đặt trước (Luật nghiệp vụ 2).
        /// Áp dụng giải thuật Tỷ lệ + Largest Remainder (Hamilton-Hare) để xử lý triệt để phần dư,
        /// bảo đảm: Tổng phân bổ = Sản lượng thực tế (100% khớp, không chênh lệch, không vượt quá).
        /// </summary>
        public List<AllocationResultItem> CalculateAllocation(
            HarvestBatch batch, 
            List<PreOrderLine> preOrderLines, 
            decimal actualHarvestQuantity)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));
            if (preOrderLines == null || !preOrderLines.Any()) return new List<AllocationResultItem>();

            if (actualHarvestQuantity < 0)
                throw new ArgumentException("Sản lượng thực tế không được âm.", nameof(actualHarvestQuantity));

            decimal totalRequested = preOrderLines.Sum(l => l.Quantity);
            var results = new List<AllocationResultItem>();

            // Trường hợp 1: Sản lượng thực tế đủ hoặc dư so với tổng đặt trước
            if (actualHarvestQuantity >= totalRequested)
            {
                foreach (var line in preOrderLines)
                {
                    results.Add(new AllocationResultItem
                    {
                        PreOrderLineId = line.PreOrderLineId,
                        PreOrderId = line.PreOrderId,
                        DealerId = line.PreOrder?.DealerId ?? 0,
                        DealerName = line.PreOrder?.Dealer?.DealerName ?? $"Đại lý #{line.PreOrder?.DealerId}",
                        RequestedQuantity = line.Quantity,
                        AllocatedQuantity = line.Quantity,
                        AllocationRatioPercent = 100.0m,
                        Note = actualHarvestQuantity > totalRequested 
                            ? $"Phân bổ đủ 100% (Thu hoạch thặng dư {actualHarvestQuantity - totalRequested:N0} kg)"
                            : "Phân bổ đủ 100% số lượng yêu cầu"
                    });
                }
                return results;
            }

            // Trường hợp 2: Hụt sản lượng (ActualQuantity < TotalRequested)
            // Áp dụng thuật toán phân bổ theo tỷ lệ + Largest Remainder
            if (totalRequested == 0 || actualHarvestQuantity == 0)
            {
                foreach (var line in preOrderLines)
                {
                    results.Add(new AllocationResultItem
                    {
                        PreOrderLineId = line.PreOrderLineId,
                        PreOrderId = line.PreOrderId,
                        DealerId = line.PreOrder?.DealerId ?? 0,
                        DealerName = line.PreOrder?.Dealer?.DealerName ?? $"Đại lý #{line.PreOrder?.DealerId}",
                        RequestedQuantity = line.Quantity,
                        AllocatedQuantity = 0,
                        AllocationRatioPercent = 0,
                        Note = "Sản lượng thực tế = 0 hoặc không có đơn vị phân bổ"
                    });
                }
                return results;
            }

            decimal globalRatio = actualHarvestQuantity / totalRequested;

            var calculationEntries = preOrderLines.Select(line =>
            {
                decimal exactShare = (line.Quantity / totalRequested) * actualHarvestQuantity;
                decimal floorShare = Math.Floor(exactShare);
                decimal remainder = exactShare - floorShare;

                return new
                {
                    Line = line,
                    ExactShare = exactShare,
                    FloorShare = floorShare,
                    Remainder = remainder
                };
            }).ToList();

            decimal totalFloorAllocated = calculationEntries.Sum(e => e.FloorShare);
            decimal missingUnits = actualHarvestQuantity - totalFloorAllocated; // Số đơn vị dư cần bù

            // Sắp xếp ưu tiên bù phần dư: Remainder giảm dần, sau đó theo ID đơn đặt trước
            var sortedEntries = calculationEntries
                .OrderByDescending(e => e.Remainder)
                .ThenBy(e => e.Line.PreOrderId)
                .ToList();

            var allocatedDictionary = new Dictionary<int, decimal>();
            for (int i = 0; i < sortedEntries.Count; i++)
            {
                var entry = sortedEntries[i];
                decimal allocated = entry.FloorShare;
                if (i < missingUnits)
                {
                    allocated += 1.0m; // Cộng thêm 1 đơn vị từ phần dư
                }
                // Đảm bảo không vượt quá số lượng đại lý yêu cầu ban đầu
                if (allocated > entry.Line.Quantity)
                {
                    allocated = entry.Line.Quantity;
                }
                allocatedDictionary[entry.Line.PreOrderLineId] = allocated;
            }

            // Điều chỉnh tinh chỉnh nếu tổng chưa khớp do cận biên
            decimal currentSum = allocatedDictionary.Values.Sum();
            if (currentSum < actualHarvestQuantity)
            {
                decimal diff = actualHarvestQuantity - currentSum;
                foreach (var entry in sortedEntries)
                {
                    if (diff <= 0) break;
                    if (allocatedDictionary[entry.Line.PreOrderLineId] < entry.Line.Quantity)
                    {
                        allocatedDictionary[entry.Line.PreOrderLineId] += 1;
                        diff -= 1;
                    }
                }
            }
            else if (currentSum > actualHarvestQuantity)
            {
                decimal diff = currentSum - actualHarvestQuantity;
                foreach (var entry in sortedEntries.OrderBy(e => e.Remainder))
                {
                    if (diff <= 0) break;
                    if (allocatedDictionary[entry.Line.PreOrderLineId] > 0)
                    {
                        allocatedDictionary[entry.Line.PreOrderLineId] -= 1;
                        diff -= 1;
                    }
                }
            }

            // Tạo danh sách kết quả cuối cùng
            foreach (var line in preOrderLines)
            {
                decimal allocated = allocatedDictionary[line.PreOrderLineId];
                decimal ratioPercent = line.Quantity > 0 
                    ? Math.Round((allocated / line.Quantity) * 100m, 2) 
                    : 0;

                results.Add(new AllocationResultItem
                {
                    PreOrderLineId = line.PreOrderLineId,
                    PreOrderId = line.PreOrderId,
                    DealerId = line.PreOrder?.DealerId ?? 0,
                    DealerName = line.PreOrder?.Dealer?.DealerName ?? $"Đại lý #{line.PreOrder?.DealerId}",
                    RequestedQuantity = line.Quantity,
                    AllocatedQuantity = allocated,
                    AllocationRatioPercent = ratioPercent,
                    Note = $"Phân bổ theo tỷ lệ {Math.Round(globalRatio * 100m, 1)}% (Nhận {allocated:N0}/{line.Quantity:N0} kg)"
                });
            }

            return results;
        }
    }
}

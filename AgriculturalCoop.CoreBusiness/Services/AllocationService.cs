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

            if (actualHarvestQuantity != decimal.Round(actualHarvestQuantity, 2) ||
                preOrderLines.Any(l =>
                    l.Quantity <= 0 ||
                    l.Quantity != decimal.Round(l.Quantity, 2)))
            {
                throw new ArgumentException(
                    "Khối lượng phải hợp lệ và có tối đa 2 chữ số thập phân.");
            }


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

            var entries = preOrderLines
                .Select(line =>
                {
                    var exactUnits =
                        line.Quantity / totalRequested * actualHarvestQuantity * 100m;

                    return new
                    {
                        Line = line,
                        Units = decimal.Floor(exactUnits),
                        Remainder = exactUnits - decimal.Floor(exactUnits)
                    };
                })
                .OrderByDescending(e => e.Remainder)
                .ThenBy(e => e.Line.PreOrderId)
                .ThenBy(e => e.Line.PreOrderLineId)
                .ToList();

            var missingUnits = decimal.ToInt32(
                actualHarvestQuantity * 100m - entries.Sum(e => e.Units));

            var allocatedDictionary = entries
                .Select((entry, index) => new
                {
                    entry.Line.PreOrderLineId,
                    Quantity =
                        (entry.Units + (index < missingUnits ? 1m : 0m)) / 100m
                })
                .ToDictionary(x => x.PreOrderLineId, x => x.Quantity);

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

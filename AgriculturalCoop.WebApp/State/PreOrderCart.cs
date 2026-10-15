using System;
using System.Collections.Generic;
using System.Linq;
using AgriculturalCoop.CoreBusiness.Entities;

namespace AgriculturalCoop.WebApp.State
{
    public class PreOrderCartItem
    {
        public int HarvestBatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Unit { get; set; } = "kg";
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal ExpectedQuantity { get; set; }
        public decimal CurrentOrderedQuantity { get; set; }
        public decimal AvailableQuantity => Math.Max(0, ExpectedQuantity - CurrentOrderedQuantity);
        public string ImageUrl { get; set; } = string.Empty;
        public decimal LineTotal => Quantity * UnitPrice;
    }

    public interface IPreOrderCart
    {
        IReadOnlyList<PreOrderCartItem> Items { get; }
        int TotalItemCount { get; }
        decimal TotalAmount { get; }
        event Action? OnChange;

        void AddItem(HarvestBatch batch, decimal quantity);
        void UpdateQuantity(int batchId, decimal quantity);
        void RemoveItem(int batchId);
        void Clear();
    }

    /// <summary>
    /// Scoped Service lưu trữ giỏ đặt trước tạm thời (K1.4 - 4 điểm).
    /// Ánh xạ từ ShoppingCart của eShop.
    /// </summary>
    public class PreOrderCart : IPreOrderCart
    {
        private readonly List<PreOrderCartItem> _items = new();

        public IReadOnlyList<PreOrderCartItem> Items => _items.AsReadOnly();
        public int TotalItemCount => _items.Count;
        public decimal TotalAmount => _items.Sum(i => i.LineTotal);

        public event Action? OnChange;

        public void AddItem(HarvestBatch batch, decimal quantity)
        {
            if (batch == null || quantity <= 0) return;

            var existing = _items.FirstOrDefault(i => i.HarvestBatchId == batch.HarvestBatchId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _items.Add(new PreOrderCartItem
                {
                    HarvestBatchId = batch.HarvestBatchId,
                    BatchCode = batch.BatchCode,
                    ProductName = batch.Product?.ProductName ?? $"Lô #{batch.HarvestBatchId}",
                    Category = batch.Product?.Category ?? "Nông sản",
                    Unit = batch.Product?.Unit ?? "kg",
                    UnitPrice = batch.UnitPrice,
                    Quantity = quantity,
                    ExpectedQuantity = batch.ExpectedQuantity,
                    CurrentOrderedQuantity = batch.TotalOrderedQuantity,
                    ImageUrl = batch.Product?.ImageUrl ?? "/images/products/placeholder.svg"
                });
            }

            NotifyStateChanged();
        }

        public void UpdateQuantity(int batchId, decimal quantity)
        {
            var item = _items.FirstOrDefault(i => i.HarvestBatchId == batchId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                NotifyStateChanged();
            }
        }

        public void RemoveItem(int batchId)
        {
            var item = _items.FirstOrDefault(i => i.HarvestBatchId == batchId);
            if (item != null)
            {
                _items.Remove(item);
                NotifyStateChanged();
            }
        }

        public void Clear()
        {
            _items.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}

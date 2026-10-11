using System;

namespace AgriculturalCoop.CoreBusiness.Exceptions
{
    public class PreOrderDomainException : Exception
    {
        public PreOrderDomainException(string message) : base(message)
        {
        }
    }

    public class ExceededExpectedYieldException : PreOrderDomainException
    {
        public int BatchId { get; }
        public decimal ExpectedYield { get; }
        public decimal CurrentOrdered { get; }
        public decimal AttemptedQuantity { get; }

        public ExceededExpectedYieldException(int batchId, decimal expectedYield, decimal currentOrdered, decimal attemptedQuantity)
            : base($"Số lượng đặt trước ({attemptedQuantity:N0} kg) vượt quá sản lượng dự kiến còn lại của lô (Dự kiến: {expectedYield:N0} kg, Đã đặt: {currentOrdered:N0} kg, Còn lại: {(expectedYield - currentOrdered):N0} kg).")
        {
            BatchId = batchId;
            ExpectedYield = expectedYield;
            CurrentOrdered = currentOrdered;
            AttemptedQuantity = attemptedQuantity;
        }
    }
}

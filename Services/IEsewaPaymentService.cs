namespace ECommerceApp.Services
{
    // A tiny DTO carrying everything the auto-submitting HTML form needs to POST to eSewa.
    public class EsewaFormData
    {
        public string PaymentFormUrl { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string TaxAmount { get; set; } = "0";
        public string TotalAmount { get; set; } = string.Empty;
        public string TransactionUuid { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string ProductServiceCharge { get; set; } = "0";
        public string ProductDeliveryCharge { get; set; } = "0";
        public string SuccessUrl { get; set; } = string.Empty;
        public string FailureUrl { get; set; } = string.Empty;
        public string SignedFieldNames { get; set; } = "total_amount,transaction_uuid,product_code";
        public string Signature { get; set; } = string.Empty;
    }

    public interface IEsewaPaymentService
    {
        // Builds the signed form fields needed to redirect the browser to eSewa's payment page.
        EsewaFormData BuildPaymentForm(decimal totalAmount, string transactionUuid);

        // eSewa's response after payment is a base64 encoded JSON blob in the query string.
        // Returns (isSuccess, decodedTransactionUuid, decodedTransactionCode) after verifying the signature.
        (bool IsValidSignature, string TransactionUuid, string? TransactionCode, string Status) DecodeAndVerify(string base64EncodedData);
    }
}

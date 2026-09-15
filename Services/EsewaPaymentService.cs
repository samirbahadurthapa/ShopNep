using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECommerceApp.Services
{
    // Implements eSewa's ePay v2 integration:
    //   1. We build a signed form and the browser auto-POSTs it to eSewa.
    //   2. eSewa redirects back to our success/failure URL with a base64 "data" query param.
    //   3. We decode it and re-verify the signature before trusting it.
    //
    // NOTE: eSewa's exact field names/endpoints have changed over the years and may change
    // again — always cross-check against eSewa's current official developer documentation
    // before going live, and test thoroughly in their sandbox (rc-epay.esewa.com.np) first.
    public class EsewaPaymentService : IEsewaPaymentService
    {
        private readonly IConfiguration _config;

        public EsewaPaymentService(IConfiguration config)
        {
            _config = config;
        }

        public EsewaFormData BuildPaymentForm(decimal totalAmount, string transactionUuid)
        {
            var merchantCode = _config["Esewa:MerchantCode"] ?? "EPAYTEST";
            var secretKey = _config["Esewa:SecretKey"] ?? "";
            var paymentUrl = _config["Esewa:PaymentFormUrl"] ?? "";
            var baseUrl = _config["AppSettings:BaseUrl"] ?? "";

            var amountStr = totalAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            var signedFieldNames = "total_amount,transaction_uuid,product_code";
            var message = $"total_amount={amountStr},transaction_uuid={transactionUuid},product_code={merchantCode}";
            var signature = ComputeHmacSha256Base64(message, secretKey);

            return new EsewaFormData
            {
                PaymentFormUrl = paymentUrl,
                Amount = amountStr,
                TaxAmount = "0",
                TotalAmount = amountStr,
                TransactionUuid = transactionUuid,
                ProductCode = merchantCode,
                ProductServiceCharge = "0",
                ProductDeliveryCharge = "0",
                SuccessUrl = $"{baseUrl}/Payment/EsewaSuccess",
                FailureUrl = $"{baseUrl}/Payment/EsewaFailure",
                SignedFieldNames = signedFieldNames,
                Signature = signature
            };
        }

        public (bool IsValidSignature, string TransactionUuid, string? TransactionCode, string Status) DecodeAndVerify(string base64EncodedData)
        {
            var secretKey = _config["Esewa:SecretKey"] ?? "";

            string json;
            try
            {
                var bytes = Convert.FromBase64String(base64EncodedData);
                json = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return (false, string.Empty, null, "Failed");
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            string GetStr(string name) => root.TryGetProperty(name, out var v) ? v.GetString() ?? "" : "";

            var transactionUuid = GetStr("transaction_uuid");
            var status = GetStr("status");
            var totalAmount = GetStr("total_amount");
            var productCode = GetStr("product_code");
            var transactionCode = GetStr("transaction_code");
            var signedFieldNames = GetStr("signed_field_names");
            var receivedSignature = GetStr("signature");

            // Rebuild the exact message using the field order eSewa told us it signed,
            // then compare our own signature against the one they sent.
            var fieldNames = signedFieldNames.Split(',');
            var messageParts = new List<string>();
            foreach (var field in fieldNames)
            {
                var value = field.Trim() switch
                {
                    "total_amount" => totalAmount,
                    "transaction_uuid" => transactionUuid,
                    "product_code" => productCode,
                    _ => GetStr(field.Trim())
                };
                messageParts.Add($"{field.Trim()}={value}");
            }
            var message = string.Join(",", messageParts);
            var expectedSignature = ComputeHmacSha256Base64(message, secretKey);

            var isValid = expectedSignature == receivedSignature &&
                          string.Equals(status, "COMPLETE", StringComparison.OrdinalIgnoreCase);

            return (isValid, transactionUuid, transactionCode, status);
        }

        private static string ComputeHmacSha256Base64(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return Convert.ToBase64String(hash);
        }
    }
}

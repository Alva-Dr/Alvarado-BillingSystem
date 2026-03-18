namespace SarEquipEnterprise.Models
{
    public class PayMongoCheckoutResponse
    {
        public PayMongoCheckoutData? data { get; set; }
    }

    public class PayMongoCheckoutData
    {
        public string? id { get; set; }
        public PayMongoCheckoutAttributes? attributes { get; set; }
    }

    public class PayMongoCheckoutAttributes
    {
        public string? checkout_url { get; set; }
        public string? payment_intent_id { get; set; }
    }

    public class PayMongoWebhookData
    {
        public PayMongoWebhookEvent? data { get; set; }
    }

    public class PayMongoWebhookEvent
    {
        public string? id { get; set; }
        public string? type { get; set; }
        public PayMongoWebhookEventAttributes? attributes { get; set; }
    }

    public class PayMongoWebhookEventAttributes
    {
        public string? type { get; set; }
        public PayMongoWebhookEventData? data { get; set; }
    }

    public class PayMongoWebhookEventData
    {
        public string? id { get; set; }
        public PayMongoWebhookEventDataAttributes? attributes { get; set; }
    }

    public class PayMongoWebhookEventDataAttributes
    {
        public string? reference_number { get; set; }
    }
}

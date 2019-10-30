namespace uber_net.Models
{
    public class Price
    {
        public string product_id { get; set; }
        public string currency_code { get; set; }
        public string display_name { get; set; }
        public string estimate { get; set; }
        public int? low_estimate { get; set; }
        public int? high_estimate { get; set; }
        public double surge_multiplier { get; set; }
    }
}

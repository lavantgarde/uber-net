namespace uber_net.Models
{
    public class ResponseHeader
    {
        public string RateLimitRemaining { get; set; }
        public string Etag { get; set; }
        public string RateLimitReset { get; set; }
        public string RateLimitLimit { get; set; }
        public string UberApp { get; set; }
    }
}

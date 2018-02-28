namespace ApiTypes
{
    public class IdsConfig
    {
        public string IdsAuthority { get; set; }
        public string[] CorsOrigins { get; set; }
        public string DelegationClientId { get; set; }
        public string DelegationSecret { get; set; }
        public bool? DiscoveryClientRequiresHttps { get; set; }
    }
}

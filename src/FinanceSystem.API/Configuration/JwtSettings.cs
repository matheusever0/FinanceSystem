﻿namespace FinanceSystem.API.Configuration
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ExpiryHours { get; set; }
    }
}

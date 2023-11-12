using Microsoft.AspNetCore.RateLimiting;

namespace ChatAPI.Policies.RateLimitting
{
    public static class SendMessagePolicy
    {
        public const string NAME = nameof(SendMessagePolicy);

        public static RateLimiterOptions AddSendMessagePolicy(this RateLimiterOptions options, IConfiguration configuration)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IConfigurationSection configSection = configuration.GetRateLimitingSection(NAME);

            options.AddFixedWindowLimiter(NAME, opt =>
            {
                opt.Window = TimeSpan.FromMinutes(2);
                opt.PermitLimit = configSection.GetValue<int>("PermitLimit");

            });

            return options;
        }
    }
}

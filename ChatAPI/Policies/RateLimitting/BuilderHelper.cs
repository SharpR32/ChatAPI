using System.Diagnostics.CodeAnalysis;

namespace ChatAPI.Policies.RateLimitting
{
    public static class BuilderHelper
    {
        public static IConfigurationSection GetRateLimitingSection(this IConfiguration configuration, [NotNull] string policyName)
        {
            if (policyName == null)
                throw new ArgumentNullException(nameof(policyName));

            return configuration.GetSection($"RateLimiting:{policyName}");
        }
    }
}

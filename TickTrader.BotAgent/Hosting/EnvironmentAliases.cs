using System.Linq;

namespace TickTrader.BotAgent.Hosting
{
    public static class EnvironmentAliases
    {
        public const string DevelopmentEnvironmentName = "Development";
        public const string StagingEnvironmentName = "Staging";
        public const string ProductionEnvironmentName = "Production";


        public static readonly string[] DevelopmentAliases = new[] { "Dev", "Develop", DevelopmentEnvironmentName };
        public static readonly string[] StagingAliases = new[] { "Stg", "Stage", StagingEnvironmentName };
        public static readonly string[] ProductionAliases = new[] { "Prod", ProductionEnvironmentName };


        public static bool IsDevelopment(string environment)
        {
            return DevelopmentAliases.Any(a => string.Compare(a, environment, true) == 0);
        }

        public static bool IsStaging(string environment)
        {
            return StagingAliases.Any(a => string.Compare(a, environment, true) == 0);
        }

        public static bool IsProduction(string environment)
        {
            return ProductionAliases.Any(a => string.Compare(a, environment, true) == 0);
        }

        public static string ResolveEnvironemntAlias(string environment)
        {
            if (IsDevelopment(environment))
                return DevelopmentEnvironmentName;
            if (IsStaging(environment))
                return StagingEnvironmentName;
            if (IsProduction(environment))
                return ProductionEnvironmentName;

            return string.IsNullOrEmpty(environment) ? ProductionEnvironmentName : environment;
        }
    }
}

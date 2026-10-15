namespace KeyBasedAuthenticationTemplate_Automation.Configuration
{
    public enum RunMode
    {
        // Generate the reports only.
        Reports,

        // Refresh the SQL Server staging table first, then generate the reports.
        StageAndReports
    }

    public static class RunModeParser
    {
        public const string ReportsArg = "reports";
        public const string StageAndReportsArg = "stage-and-reports";

        public const string Usage =
            "Usage: KeyBasedAuthenticationTemplate_Automation [reports | stage-and-reports]\n" +
            "  reports            generate the reports only (default when no argument is given)\n" +
            "  stage-and-reports  load the staging table, then generate the reports";

        // No argument means "reports", so existing schedules keep working unchanged.
        public static bool TryParse(string[] args, out RunMode mode)
        {
            mode = RunMode.Reports;
            if (args.Length == 0)
            {
                return true;
            }

            if (args.Length > 1)
            {
                return false;
            }

            if (args[0].Equals(ReportsArg, StringComparison.OrdinalIgnoreCase))
            {
                mode = RunMode.Reports;
                return true;
            }

            if (args[0].Equals(StageAndReportsArg, StringComparison.OrdinalIgnoreCase))
            {
                mode = RunMode.StageAndReports;
                return true;
            }

            return false;
        }
    }
}

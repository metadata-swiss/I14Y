namespace Bfs.Iop.Infrastructure.Security.Helpers;

public static class IopClaimsHelper
{
    public static class ClaimTypes
    {
        public const string AgenciesClaimType = "dcat_agents";
        public const string EmailClaimType = "email";
        public const string FirstNameClaimType = "given_name";
        public const string LastNameClaimType = "family_name";
        public const string I14YClientTypeClaimType = "i14y_client_type";
        public const string RoleClaimType = "role";
    }

    public static class ClaimValues
    {
        public const string I14YClientTypeClaimTechnicalValue = "technical";
    }

    public static class Roles
    {
        public static class General
        {
            public const string Allow = "BFS-i14y.ALLOW";
        }

        public static class BusinessRoles
        {
            public const string InteroperabilityService = "BFS-i14y.interoperabilityservice";
            public const string LocalDataSteward = "BFS-i14y.localdatasteward";
            public const string Submitter = "BFS-i14y.submitter";
            public const string StewardshipOrganizationViewer = "BFS-i14y.stewardshiporganizationviewer";
            public const string SwissDataSteward = "BFS-i14y.swissdatasteward";
        }
    }
}

using System;

namespace PharmacyApp.Common.Repositories
{
    internal static class SQLUtility
    {
        public static string GetConnectionString()
        {
            return "Data Source=" + Environment.MachineName + "; Initial Catalog=Pharmacy;Integrated Security=true;TrustServerCertificate=true;";
        }
    }
}

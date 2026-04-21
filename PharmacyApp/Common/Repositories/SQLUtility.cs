using System;

namespace PharmacyApp.Common.Repositories
{
    internal static class SQLUtility
    {
        public static string GetConnectionString()
        {
            //if it doesn't work, replace the Environment.MachineName with the name from SqlServer
            return "Data Source=" + Environment.MachineName + "; Initial Catalog=Pharmacy;Integrated Security=true;TrustServerCertificate=true;";
        }
    }
}

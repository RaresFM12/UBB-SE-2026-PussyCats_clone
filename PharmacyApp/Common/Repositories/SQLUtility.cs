using System;

namespace PharmacyApp.Common.Repositories
{
    internal static class SQLUtility
    {
        public static string GetConnectionString()
        {
            return "Data Source=" + "DESKTOP-C5LH746\\SQLEXPRESS;" + "Initial Catalog=Pharmacy;Integrated Security=true;TrustServerCertificate=true;";
        }
    }
}

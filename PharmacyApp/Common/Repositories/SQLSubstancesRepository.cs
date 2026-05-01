using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Repositories
{
    public class SQLSubstancesRepository : ISubstancesRepository
    {
        public void AddSubstance(string name, float lethalDose, string description)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string insertSubstanceCommandString =
                $"INSERT INTO Substances VALUES ('{name}', {lethalDose}, '{description}')";

            using SqlConnection connection = new (connectionString);

            SqlCommand insertSubstanceCommand = new (insertSubstanceCommandString, connection);

            connection.Open();
            insertSubstanceCommand.ExecuteNonQuery();
        }

        public Substance GetSubstanceByName(string name)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectSubstanceQueryString = $"SELECT * FROM Substances WHERE name='{name}'";

            using SqlConnection connection = new (connectionString);

            SqlDataAdapter selectSubstanceAdapter = new (selectSubstanceQueryString, connection);

            DataSet substanceDataFromDB = new ();
            connection.Open();
            selectSubstanceAdapter.Fill(substanceDataFromDB, "Substances");

            if (substanceDataFromDB.Tables["Substances"].Rows.Count == 0)
            {
                return null;
            }

            DataRow substanceDataRow = substanceDataFromDB.Tables["Substances"].Rows[0];
            return new Substance((string)substanceDataRow["name"], (float)(decimal)substanceDataRow["lethalDose"],
                (string)substanceDataRow["description"]);
        }

        public List<Substance> GetAllSubstances()
        {
            List<Substance> allSubstances = new ();
            string connectionString = SQLUtility.GetConnectionString();
            string selectAllSubstancesQueryString = $"SELECT * FROM Substances";

            using SqlConnection connection = new (connectionString);
            SqlDataAdapter selectSubstancesAdapter = new (selectAllSubstancesQueryString, connection);
            DataSet substanceDataFromDB = new ();

            connection.Open();
            selectSubstancesAdapter.Fill(substanceDataFromDB, "Substances");

            foreach (DataRow substanceDataRow in substanceDataFromDB.Tables["Substances"].Rows)
            {
                Substance newSubstance = new (
                    (string)substanceDataRow["name"],
                    (float)(decimal)substanceDataRow["lethalDose"],
                    (string)substanceDataRow["description"]);

                allSubstances.Add(newSubstance);
            }

            return allSubstances;
        }

        public void RemoveSubstanceByName(string name)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string deleteSubstanceCommandString = $"DELETE FROM Substances WHERE name='{name}'";
            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE name='{name}'";
            using SqlConnection connection = new (connectionString);

            connection.Open();
            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, connection);
            deleteActiveSubstancesCommand.ExecuteNonQuery();
            SqlCommand deleteSubstanceCommand = new SqlCommand(deleteSubstanceCommandString, connection);
            deleteSubstanceCommand.ExecuteNonQuery();
        }

        public void UpdateSubstanceByName(Substance substance)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string updateSubstanceCommandString = $"UPDATE Substances " +
                                                  $"SET lethalDose = {substance.LethalDose}, " +
                                                  $"description = '{substance.Description}' " +
                                                  $"WHERE name = '{substance.Name}'";

            using SqlConnection connection = new (connectionString);

            SqlCommand updateSubstanceCommand = new (updateSubstanceCommandString, connection);
            connection.Open();
            updateSubstanceCommand.ExecuteNonQuery();
        }

        public bool SubstanceExists(string name)
        {
            string connectionString = SQLUtility.GetConnectionString();
            string selectSubstanceQueryString = $"SELECT * FROM Substances WHERE name='{name}'";

            using SqlConnection connection = new (connectionString);

            SqlDataAdapter selectSubstanceAdapter = new (selectSubstanceQueryString, connection);

            DataSet substanceDataFromDB = new ();

            connection.Open();
            selectSubstanceAdapter.Fill(substanceDataFromDB, "Substances");

            if (substanceDataFromDB.Tables["Substances"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public Dictionary<string, int> GetTop30Substances()
        {
            Dictionary<string, int> topSubstances = new ();
            string connectionString = SQLUtility.GetConnectionString();
            string selectTopSubstancesQueryString = $"SELECT TOP 30 s.name, COUNT(o.orderId) as nbOrders FROM Substances s INNER JOIN ItemSubstances its ON s.name = its.name INNER JOIN OrderItems oi ON its.itemId = oi.itemId INNER JOIN Orders o ON oi.orderId=o.orderId WHERE o.pickUpDate >= DATEADD(MONTH, -1, GETDATE()) GROUP BY s.name ORDER BY COUNT(o.orderId) DESC";
            using SqlConnection connection = new (connectionString);
            SqlDataAdapter selectTopSubstancesAdapter = new (selectTopSubstancesQueryString, connection);
            DataSet topSubstancesDataFromDB = new ();
            connection.Open();
            selectTopSubstancesAdapter.Fill(topSubstancesDataFromDB, "Substances");
            foreach (DataRow substanceDataRow in topSubstancesDataFromDB.Tables["Substances"].Rows)
            {
                string substanceName = (string)substanceDataRow["name"];
                int itemCount = (int)substanceDataRow["nbOrders"];
                topSubstances[substanceName] = itemCount;
            }
            return topSubstances;
        }
    }
}

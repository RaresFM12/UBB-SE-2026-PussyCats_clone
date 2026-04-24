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
            if (SubstanceExists(name))
            {
                throw new ArgumentException("Substance " + name + " exists already.");
            }

            string connString = SQLUtility.GetConnectionString();
            string insertSubstanceCommandString =
                $"INSERT INTO Substances VALUES ('{name}', {lethalDose}, '{description}')";

            using SqlConnection conn = new (connString);

            SqlCommand insertSubstanceCommand = new (insertSubstanceCommandString, conn);

            conn.Open();
            insertSubstanceCommand.ExecuteNonQuery();
        }

        public Substance GetSubstanceByName(string name)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectSubstanceQueryString = $"SELECT * FROM Substances WHERE name='{name}'";

            using SqlConnection conn = new (connString);

            SqlDataAdapter selectSubstanceAdapter = new (selectSubstanceQueryString, conn);

            DataSet substanceDataFromDB = new ();
            conn.Open();
            selectSubstanceAdapter.Fill(substanceDataFromDB, "Substances");

            if (substanceDataFromDB.Tables["Substances"].Rows.Count == 0)
            {
                throw new ArgumentException("Substance " + name + " does NOT exist.");
            }

            DataRow substanceDataRow = substanceDataFromDB.Tables["Substances"].Rows[0];
            return new Substance((string)substanceDataRow["name"], (float)(decimal)substanceDataRow["lethalDose"],
                (string)substanceDataRow["description"]);
        }

        public List<Substance> GetAllSubstances()
        {
            List<Substance> allSubstances = new ();
            string connString = SQLUtility.GetConnectionString();
            string selectAllSubstancesQueryString = $"SELECT * FROM Substances";

            using SqlConnection conn = new (connString);
            SqlDataAdapter selectSubstancesAdapter = new (selectAllSubstancesQueryString, conn);
            DataSet substanceDataFromDB = new ();

            conn.Open();
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
            if (!SubstanceExists(name))
            {
                throw new ArgumentException("Substance " + name + " does NOT exist.");
            }

            string connString = SQLUtility.GetConnectionString();
            string deleteSubstanceCommandString = $"DELETE FROM Substances WHERE name='{name}'";
            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE name='{name}'";
            using SqlConnection conn = new (connString);

            conn.Open();
            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, conn);
            deleteActiveSubstancesCommand.ExecuteNonQuery();
            SqlCommand deleteSubstanceCommand = new SqlCommand(deleteSubstanceCommandString, conn);
            deleteSubstanceCommand.ExecuteNonQuery();
        }

        public void UpdateSubstanceByName(Substance substance)
        {
            if (!SubstanceExists(substance.Name))
            {
                throw new ArgumentException("Substance " + substance.Name + "does NOT exist.");
            }

            string connString = SQLUtility.GetConnectionString();
            string updateSubstanceCommandString = $"UPDATE Substances " +
                                                  $"SET lethalDose = {substance.LethalDose}, " +
                                                  $"description = '{substance.Description}' " +
                                                  $"WHERE name = '{substance.Name}'";

            using SqlConnection conn = new (connString);

            SqlCommand updateSubstanceCommand = new (updateSubstanceCommandString, conn);
            conn.Open();
            updateSubstanceCommand.ExecuteNonQuery();
        }

        public bool SubstanceExists(string name)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectSubstanceQueryString = $"SELECT * FROM Substances WHERE name='{name}'";

            using SqlConnection conn = new (connString);

            SqlDataAdapter selectSubstanceAdapter = new (selectSubstanceQueryString, conn);

            DataSet substanceDataFromDB = new ();

            conn.Open();
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
            string connString = SQLUtility.GetConnectionString();
            string selectTopSubstancesQueryString = $"SELECT TOP 30 s.name, COUNT(o.orderId) as nbOrders FROM Substances s INNER JOIN ItemSubstances its ON s.name = its.name INNER JOIN OrderItems oi ON its.itemId = oi.itemId INNER JOIN Orders o ON oi.orderId=o.orderId WHERE o.pickUpDate >= DATEADD(MONTH, -1, GETDATE()) GROUP BY s.name ORDER BY COUNT(o.orderId) DESC";
            using SqlConnection conn = new (connString);
            SqlDataAdapter selectTopSubstancesAdapter = new (selectTopSubstancesQueryString, conn);
            DataSet topSubstancesDataFromDB = new ();
            conn.Open();
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

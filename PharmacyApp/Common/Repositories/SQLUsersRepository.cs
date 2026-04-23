using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Repositories
{
    public class SQLUsersRepository : IUsersRepository
    {
        public SQLUsersRepository()
        {
        }

        public void AddUser(string email, string phoneNumber, string passwordHash, string username,
            bool discountNotifications, bool isDisabled = false, bool isAdmin = false, int loyaltyPoints = 0)
        {
            if (UserExists(email))
            {
                throw new ArgumentException("User with E-Mail " + email + " exists already.");
            }

            string connString = SQLUtility.GetConnectionString();
            string insertNewUserString =
                "INSERT INTO Users VALUES " +
                $"('{email}', '{phoneNumber}', '{passwordHash}', '{isDisabled}', '{isAdmin}', '{username}', '{discountNotifications}', {loyaltyPoints})";

            using SqlConnection conn = new (connString);

            SqlCommand insertNewUserCommand = new (insertNewUserString, conn);

            conn.Open();
            insertNewUserCommand.ExecuteNonQuery();
        }
        public List<User> GetAllUsers()
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUsersString = $"SELECT * FROM Users";
            using SqlConnection conn = new (connString);

            SqlDataAdapter selectUsersAdapter = new (selectUsersString, conn);
            DataSet usersDataFromDB = new ();
            conn.Open();
            selectUsersAdapter.Fill(usersDataFromDB, "Users");

            if (usersDataFromDB.Tables["Users"].Rows.Count == 0)
            {
                return new List<User>();
            }

            List<User> users = new ();
            foreach (DataRow userRow in usersDataFromDB.Tables["Users"].Rows)
            {
                User resultUser = new ((int)userRow["userId"], (string)userRow["email"],
                    (string)userRow["phoneNumber"],
                    (string)userRow["passwordHash"], (bool)userRow["isAdmin"], (bool)userRow["isDisabled"],
                    (string)userRow["username"], (bool)userRow["discountNotifications"],
                    (int)userRow["loyaltyPoints"]);

                int userID = (int)userRow["userId"];
                DataSet userDataFromDB = new ();

                string selectPeriodTrackersString = $"SELECT * FROM PeriodTrackers WHERE userId={userID}";
                SqlDataAdapter selectPeriodTrackersAdapter = new (selectPeriodTrackersString, conn);
                selectPeriodTrackersAdapter.Fill(userDataFromDB, "PeriodTrackers");

                if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
                {
                    DataRow userPeriodTrackerRow =
                        userDataFromDB.Tables["PeriodTrackers"].Rows[0];

                    resultUser.SetPeriodTracker(
                        DateOnly.FromDateTime((DateTime)userPeriodTrackerRow["startPeriodDate"]),
                        (int)userPeriodTrackerRow["cycleDays"],
                        (int)userPeriodTrackerRow["periodLasts"], (int)userPeriodTrackerRow["PremenstrualSyndromeOption"]);
                }

                string selectUserNotificationsString = $"SELECT * FROM UserNotifications WHERE userId={userID}";
                string selectUserDiscountsString = $"SELECT * FROM UserDiscounts WHERE userId={userID}";
                string selectPeriodNotesString = $"SELECT * FROM PeriodNotes WHERE userId={userID}";

                SqlDataAdapter selectUserNotificationsAdapter = new (selectUserNotificationsString, conn);
                SqlDataAdapter selectUserDiscountsAdapter = new (selectUserDiscountsString, conn);
                SqlDataAdapter selectPeriodNotesAdapter = new (selectPeriodNotesString, conn);

                selectUserNotificationsAdapter.Fill(userDataFromDB, "UserNotifications");
                selectUserDiscountsAdapter.Fill(userDataFromDB, "UserDiscounts");
                selectPeriodNotesAdapter.Fill(userDataFromDB, "PeriodNotes");

                foreach (DataRow notificationsRow in userDataFromDB.Tables["UserNotifications"].Rows)
                {
                    if ((bool)notificationsRow["favouriteItem"])
                    {
                        resultUser.AddItemToFavoriteItems((int)notificationsRow["itemId"]);
                    }

                    if ((bool)notificationsRow["stockAlert"])
                    {
                        resultUser.AddStockAlertToUser((int)notificationsRow["itemId"]);
                    }
                }

                foreach (DataRow discountRow in userDataFromDB.Tables["UserDiscounts"].Rows)
                {
                    resultUser.AddUserDiscount((int)discountRow["itemId"],
                        (float)(decimal)discountRow["itemDiscountPercentage"]);
                }

                foreach (DataRow periodNoteRow in userDataFromDB.Tables["PeriodNotes"].Rows)
                {
                    resultUser.AddPeriodNoteToUser((int)periodNoteRow["noteId"], (string)periodNoteRow["noteBody"],
                        (bool)periodNoteRow["isDone"]);
                }

                users.Add(resultUser);
            }

            return users;
        }

        public User GetUserByEmail(string email)
        {
            string connectionString = SQLUtility.GetConnectionString();
            const string query = "SELECT * FROM Users WHERE email = @Email";

            using SqlConnection connection = new (connectionString);
            using SqlCommand command = new (query, connection);

            command.Parameters.AddWithValue("@Email", email);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                throw new ArgumentException("User with E-Mail " + email + " does NOT exist.");
            }

            User user = new (
                reader.GetInt32(reader.GetOrdinal("userId")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("phoneNumber")),
                reader.GetString(reader.GetOrdinal("passwordHash")),
                reader.GetBoolean(reader.GetOrdinal("isAdmin")),
                reader.GetBoolean(reader.GetOrdinal("isDisabled")),
                reader.GetString(reader.GetOrdinal("username")),
                reader.GetBoolean(reader.GetOrdinal("discountNotifications")),
                reader.GetInt32(reader.GetOrdinal("loyaltyPoints")));

            return user;
        }

        public User GetUserById(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";
            string selectPeriodTrackerString = $"SELECT * FROM PeriodTrackers WHERE userId={id}";
            string selectUserNotificationsString = $"SELECT * FROM UserNotifications WHERE userId={id}";
            string selectUserDiscountsString = $"SELECT * FROM UserDiscounts WHERE userId={id}";
            string selectPeriodNotesString = $"SELECT * FROM PeriodNotes WHERE userId={id}";

            using SqlConnection conn = new (connString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, conn);
            SqlDataAdapter selectPeriodTrackerAdapter = new (selectPeriodTrackerString, conn);
            SqlDataAdapter selectUserNotificationsAdapter = new (selectUserNotificationsString, conn);
            SqlDataAdapter selectUserDiscountsAdapter = new (selectUserDiscountsString, conn);
            SqlDataAdapter selectPeriodNotesAdapter = new (selectPeriodNotesString, conn);

            DataSet userDataFromDB = new ();

            conn.Open();

            selectUserAdapter.Fill(userDataFromDB, "Users");
            selectPeriodTrackerAdapter.Fill(userDataFromDB, "PeriodTrackers");
            selectUserNotificationsAdapter.Fill(userDataFromDB, "UserNotifications");
            selectUserDiscountsAdapter.Fill(userDataFromDB, "UserDiscounts");
            selectPeriodNotesAdapter.Fill(userDataFromDB, "PeriodNotes");

            if (userDataFromDB.Tables["Users"].Rows.Count == 0)
            {
                throw new ArgumentException("User with ID " + id + " does NOT exist.");
            }

            DataRow userRow = userDataFromDB.Tables["Users"].Rows[0];

            User resultUser = new ((int)userRow["userId"], (string)userRow["email"],
                (string)userRow["phoneNumber"],
                (string)userRow["passwordHash"], (bool)userRow["isAdmin"], (bool)userRow["isDisabled"],
                (string)userRow["username"], (bool)userRow["discountNotifications"],
                (int)userRow["loyaltyPoints"]);

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
            {
                DataRow userPeriodTrackerRow = userDataFromDB.Tables["PeriodTrackers"].Rows[0];

                resultUser.SetPeriodTracker(DateOnly.FromDateTime((DateTime)userPeriodTrackerRow["startPeriodDate"]),
                    (int)userPeriodTrackerRow["cycleDays"],
                    (int)userPeriodTrackerRow["periodLasts"], (int)userPeriodTrackerRow["PMSOption"]);
            }

            foreach (DataRow notificationsRow in userDataFromDB.Tables["UserNotifications"].Rows)
            {
                if ((bool)notificationsRow["favouriteItem"])
                {
                    resultUser.AddItemToFavoriteItems((int)notificationsRow["itemId"]);
                }

                if ((bool)notificationsRow["stockAlert"])
                {
                    resultUser.AddStockAlertToUser((int)notificationsRow["itemId"]);
                }
            }

            foreach (DataRow discountRow in userDataFromDB.Tables["UserDiscounts"].Rows)
            {
                resultUser.AddUserDiscount((int)discountRow["itemId"],
                    (float)(decimal)discountRow["itemDiscountPercentage"]);
            }

            foreach (DataRow periodNoteRow in userDataFromDB.Tables["PeriodNotes"].Rows)
            {
                resultUser.AddPeriodNoteToUser((int)periodNoteRow["noteId"], (string)periodNoteRow["noteBody"],
                    (bool)periodNoteRow["isDone"]);
            }

            return resultUser;
        }

        public void UpdateUser(User newUser)
        {
            string connString = SQLUtility.GetConnectionString();
            string updateUserString = $"UPDATE Users " +
                                      $"SET email = '{newUser.Email}', " +
                                      $"phoneNumber = '{newUser.PhoneNumber}', " +
                                      $"passwordHash = '{newUser.PasswordHash}', " +
                                      $"isDisabled = '{newUser.IsDisabled}', " +
                                      $"isAdmin = '{newUser.IsAdmin}', " +
                                      $"username = '{newUser.Username}', " +
                                      $"discountNotifications = '{newUser.DiscountNotifications}', " +
                                      $"loyaltyPoints = {newUser.LoyaltyPoints} " +
                                      $"WHERE userId={newUser.Id}";

            using SqlConnection conn = new (connString);
            conn.Open();
            SqlCommand updateUserCommand = new (updateUserString, conn);
            updateUserCommand.ExecuteNonQuery();

            DataSet userDataFromDB = new ();
            string selectPeriodTrackersString = $"SELECT * FROM PeriodTrackers WHERE userId={newUser.Id}";
            SqlDataAdapter selectPeriodTrackersAdapter = new (selectPeriodTrackersString, conn);
            selectPeriodTrackersAdapter.Fill(userDataFromDB, "PeriodTrackers");

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
            {
                string deletePeriodTrackerString = $"DELETE FROM PeriodTrackers WHERE userId = {newUser.Id}";
                SqlCommand deletePeriodTrackerCommand = new (deletePeriodTrackerString, conn);
                deletePeriodTrackerCommand.ExecuteNonQuery();
            }

            string periodDate =
                $"{newUser.StartPeriodDate.Year}-{newUser.StartPeriodDate.Month}-{newUser.StartPeriodDate.Day}";
            string insertPeriodTrackerString =
                $"INSERT INTO PeriodTrackers " +
                $"VALUES ({newUser.Id}, '{periodDate}',{newUser.CycleDays},{newUser.PeriodLasts},{newUser.PremenstrualSyndromeOption})";
            SqlCommand insertPeriodTrackerCommand = new (insertPeriodTrackerString, conn);
            insertPeriodTrackerCommand.ExecuteNonQuery();

            string deleteUserNotificationsString = $"DELETE FROM UserNotifications WHERE userId = {newUser.Id}";
            SqlCommand deleteUserNotificationsCommand = new (deleteUserNotificationsString, conn);
            deleteUserNotificationsCommand.ExecuteNonQuery();

            string insertUserNotificationsString;
            foreach (int itemID in newUser.FavoriteItems)
            {
                if (newUser.StockAlerts.Contains(itemID))
                {
                    insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'True', 'True')";
                }
                else
                {
                    insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'True', 'False')";
                }

                SqlCommand insertUserNotificationsCommand = new (insertUserNotificationsString, conn);
                insertUserNotificationsCommand.ExecuteNonQuery();
            }
            foreach (int itemID in newUser.StockAlerts)
            {
                if (newUser.FavoriteItems.Contains(itemID))
                {
                    continue;
                }

                insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'False', 'True')";
                SqlCommand insertUserNotificationsCommand = new (insertUserNotificationsString, conn);
                insertUserNotificationsCommand.ExecuteNonQuery();
            }

            string deleteUserDiscountsString = $"DELETE FROM UserDiscounts WHERE userId = {newUser.Id}";
            SqlCommand deleteUserDiscountsCommand = new (deleteUserDiscountsString, conn);
            deleteUserDiscountsCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, float> userDiscount in newUser.UserDiscounts)
            {
                string insertUserDiscountString =
                    $"INSERT INTO UserDiscounts VALUES ({newUser.Id},{userDiscount.Key}, {userDiscount.Value})";
                SqlCommand insertUserDiscountsCommand = new (insertUserDiscountString, conn);
                insertUserDiscountsCommand.ExecuteNonQuery();
            }

            string deletePeriodNotesString = $"DELETE FROM PeriodNotes WHERE userId = {newUser.Id}";
            SqlCommand deletePeriodNotesCommand = new (deletePeriodNotesString, conn);
            deletePeriodNotesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, Tuple<string, bool>> periodNote in newUser.PeriodNotes)
            {
                string insertPeriodNoteString =
                    $"INSERT INTO PeriodNotes VALUES ({newUser.Id},{periodNote.Key}, '{periodNote.Value.Item1}', '{periodNote.Value.Item2}')";
                SqlCommand insertPeriodNoteCommand = new (insertPeriodNoteString, conn);
                insertPeriodNoteCommand.ExecuteNonQuery();
            }
        }

        public bool UserExists(string email)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE email='{email}'";

            using SqlConnection conn = new (connString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, conn);

            DataSet userDataFromDB = new ();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool UserExists(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";

            using SqlConnection conn = new (connString);
            SqlDataAdapter selectUserAdapter = new (selectUserString, conn);
            DataSet userDataFromDB = new ();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool UserHasPeriodTracker(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM PeriodTrackers WHERE userId={id}";

            using SqlConnection conn = new (connString);

            SqlDataAdapter selectUserAdapter = new (selectUserString, conn);

            DataSet userDataFromDB = new ();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "PeriodTrackers");

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}
using Microsoft.Data.SqlClient;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

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
                throw new ArgumentException("User with E-Mail " + email + " exists already.");

            string connString = SQLUtility.GetConnectionString();
            string insertNewUserString =
                "INSERT INTO Users VALUES " +
                $"('{email}', '{phoneNumber}', '{passwordHash}', '{isDisabled}', '{isAdmin}', '{username}', '{discountNotifications}', {loyaltyPoints})";

            using SqlConnection conn = new SqlConnection(connString);

            SqlCommand insertNewUserCommand = new SqlCommand(insertNewUserString, conn);

            conn.Open();
            insertNewUserCommand.ExecuteNonQuery();
        }


        public List<User> GetAllUsers()
        {
            string connString = SQLUtility.GetConnectionString();

            string selectUsersString = $"SELECT * FROM Users";


            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUsersAdapter = new SqlDataAdapter(selectUsersString, conn);

            DataSet usersDataFromDB = new DataSet(); // all the tables will be put in this DataSet

            conn.Open();

            selectUsersAdapter.Fill(usersDataFromDB,
                "Users"); // this uses the connection, fills the data from the select query into a table called "Users"

            if (usersDataFromDB.Tables["Users"].Rows.Count == 0)
                return new List<User>(); // empty list


            List<User> users = new List<User>(); // this list will contain all users 

            // iterate each user and find their data
            foreach (DataRow userRow in usersDataFromDB.Tables["Users"].Rows)
            {
                User resultUser = new User((int)userRow["userId"], (string)userRow["email"],
                    (string)userRow["phoneNumber"],
                    (string)userRow["passwordHash"], (bool)userRow["isAdmin"], (bool)userRow["isDisabled"],
                    (string)userRow["username"], (bool)userRow["discountNotifications"],
                    (int)userRow["loyaltyPoints"]); // default values for period tracker

                int userID = (int)userRow["userId"];


                //NOTE: userData, not usersData...
                DataSet
                    userDataFromDB = new DataSet(); // all the tables relevant to THIS specific user will be put here

                // query their period tracker
                string selectPeriodTrackersString = $"SELECT * FROM PeriodTrackers WHERE userId={userID}";
                SqlDataAdapter selectPeriodTrackersAdapter = new SqlDataAdapter(selectPeriodTrackersString, conn);
                selectPeriodTrackersAdapter.Fill(userDataFromDB, "PeriodTrackers");


                if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count >
                    0) // they have period tracker data, otherwise leave default values
                {
                    DataRow userPeriodTrackerRow =
                        userDataFromDB.Tables["PeriodTrackers"].Rows[0]; // their period tracker, only one

                    resultUser.SetPeriodTracker(
                        DateOnly.FromDateTime((DateTime)userPeriodTrackerRow["startPeriodDate"]),
                        (int)userPeriodTrackerRow["cycleDays"],
                        (int)userPeriodTrackerRow["periodLasts"], (int)userPeriodTrackerRow["PremenstrualSyndromeOption"]);
                }


                // now find, using the same open connection, their notifications, discounts and period notes
                string selectUserNotificationsString = $"SELECT * FROM UserNotifications WHERE userId={userID}";
                string selectUserDiscountsString = $"SELECT * FROM UserDiscounts WHERE userId={userID}";
                string selectPeriodNotesString = $"SELECT * FROM PeriodNotes WHERE userId={userID}";

                SqlDataAdapter selectUserNotificationsAdapter = new SqlDataAdapter(selectUserNotificationsString, conn);
                SqlDataAdapter selectUserDiscountsAdapter = new SqlDataAdapter(selectUserDiscountsString, conn);
                SqlDataAdapter selectPeriodNotesAdapter = new SqlDataAdapter(selectPeriodNotesString, conn);

                selectUserNotificationsAdapter.Fill(userDataFromDB, "UserNotifications");
                selectUserDiscountsAdapter.Fill(userDataFromDB, "UserDiscounts");
                selectPeriodNotesAdapter.Fill(userDataFromDB, "PeriodNotes");

                // now  give them their notifications and discounts
                foreach (DataRow notificationsRow in userDataFromDB.Tables["UserNotifications"].Rows)
                {
                    if ((bool)notificationsRow[
                            "favouriteItem"]) // add that item to the user's favorite items if they checked it
                        resultUser.AddItemToFavoriteItems((int)notificationsRow["itemId"]);
                    if ((bool)notificationsRow["stockAlert"]) // same principle
                        resultUser.AddStockAlertToUser((int)notificationsRow["itemId"]);
                    // if both are false, the item is NOT in the database at all
                }

                foreach (DataRow discountRow in userDataFromDB.Tables["UserDiscounts"].Rows)
                {
                    // a row is only present here if the user has personal discounts on some items
                    resultUser.AddUserDiscount((int)discountRow["itemId"],
                        (float)(decimal)discountRow["itemDiscountPercentage"]);
                }

                // now add the user's period notes
                foreach (DataRow periodNoteRow in userDataFromDB.Tables["PeriodNotes"].Rows)
                {
                    resultUser.AddPeriodNoteToUser((int)periodNoteRow["noteId"], (string)periodNoteRow["noteBody"],
                        (bool)periodNoteRow["isDone"]);
                }


                users.Add(resultUser); // finally add the user to the users list
            }

            return users;
        }

        public User GetUserByEmail(string email)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE email='{email}'";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, conn);

            DataSet userDataFromDB = new DataSet();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count == 0)
                throw new ArgumentException("User with E-Mail " + email + " does NOT exist.");

            DataRow userRow = userDataFromDB.Tables["Users"].Rows[0];


            return
                GetUserById(
                    (int)userRow["userId"]); // please ignore the inefficiency of this, better than line duplications
        }

        public User GetUserById(int id)
        {
            string connString = SQLUtility.GetConnectionString();

            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";
            string selectPeriodTrackerString = $"SELECT * FROM PeriodTrackers WHERE userId={id}";
            string selectUserNotificationsString = $"SELECT * FROM UserNotifications WHERE userId={id}";
            string selectUserDiscountsString = $"SELECT * FROM UserDiscounts WHERE userId={id}";
            string selectPeriodNotesString = $"SELECT * FROM PeriodNotes WHERE userId={id}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, conn);
            SqlDataAdapter selectPeriodTrackerAdapter = new SqlDataAdapter(selectPeriodTrackerString, conn);
            SqlDataAdapter selectUserNotificationsAdapter = new SqlDataAdapter(selectUserNotificationsString, conn);
            SqlDataAdapter selectUserDiscountsAdapter = new SqlDataAdapter(selectUserDiscountsString, conn);
            SqlDataAdapter selectPeriodNotesAdapter = new SqlDataAdapter(selectPeriodNotesString, conn);

            DataSet userDataFromDB = new DataSet(); // all the tables will be put in this DataSet

            conn.Open();

            selectUserAdapter.Fill(userDataFromDB,
                "Users"); // this uses the connection, fills the data from the select query into a table called "Users"
            selectPeriodTrackerAdapter.Fill(userDataFromDB, "PeriodTrackers");
            selectUserNotificationsAdapter.Fill(userDataFromDB, "UserNotifications");
            selectUserDiscountsAdapter.Fill(userDataFromDB, "UserDiscounts");
            selectPeriodNotesAdapter.Fill(userDataFromDB, "PeriodNotes");

            if (userDataFromDB.Tables["Users"].Rows.Count == 0)
                throw new ArgumentException("User with ID " + id + " does NOT exist.");


            // create the user from the unique user and period tracker rows
            DataRow userRow = userDataFromDB.Tables["Users"].Rows[0]; // the user with this id

            User resultUser = new User((int)userRow["userId"], (string)userRow["email"],
                (string)userRow["phoneNumber"],
                (string)userRow["passwordHash"], (bool)userRow["isAdmin"], (bool)userRow["isDisabled"],
                (string)userRow["username"], (bool)userRow["discountNotifications"],
                (int)userRow["loyaltyPoints"]);

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count >
                0) // they have period tracker data, otherwise leave default values
            {
                DataRow userPeriodTrackerRow =
                    userDataFromDB.Tables["PeriodTrackers"].Rows[0]; // their period tracker, only one

                resultUser.SetPeriodTracker(DateOnly.FromDateTime((DateTime)userPeriodTrackerRow["startPeriodDate"]),
                    (int)userPeriodTrackerRow["cycleDays"],
                    (int)userPeriodTrackerRow["periodLasts"], (int)userPeriodTrackerRow["PremenstrualSyndromeOption"]);
            }

            // now  give them their notifications and discounts
            foreach (DataRow notificationsRow in userDataFromDB.Tables["UserNotifications"].Rows)
            {
                if ((bool)notificationsRow[
                        "favouriteItem"]) // add that item to the user's favorite items if they checked it
                    resultUser.AddItemToFavoriteItems((int)notificationsRow["itemId"]);
                if ((bool)notificationsRow["stockAlert"]) // same principle
                    resultUser.AddStockAlertToUser((int)notificationsRow["itemId"]);
                // if both are false, the item is NOT in the database at all
            }

            foreach (DataRow discountRow in userDataFromDB.Tables["UserDiscounts"].Rows)
            {
                // a row is only present here if the user has personal discounts on some items
                resultUser.AddUserDiscount((int)discountRow["itemId"],
                    (float)(decimal)discountRow["itemDiscountPercentage"]);
            }

            // now add the user's period notes
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

            using SqlConnection conn = new SqlConnection(connString);

            // this command updates ONLY the Users table

            conn.Open();
            SqlCommand updateUserCommand = new SqlCommand(updateUserString, conn);
            updateUserCommand.ExecuteNonQuery(); // non-query command


            // this user's period tracker, notes, notifications and discounts have to be updated :(
            // how? Delete the old entries and insert the new ones, most logical way to do it... hopefully


            // first for the period tracker, delete + insert, IF it exists
            // in some cases, it just doesn't exist (for example the user is updated when they click Calculate the first time in the period tracker)

            DataSet userDataFromDB = new DataSet();

            // query their period tracker
            string selectPeriodTrackersString = $"SELECT * FROM PeriodTrackers WHERE userId={newUser.Id}";
            SqlDataAdapter selectPeriodTrackersAdapter = new SqlDataAdapter(selectPeriodTrackersString, conn);
            selectPeriodTrackersAdapter.Fill(userDataFromDB, "PeriodTrackers");


            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0) // only delete if it exists
            {
                string deletePeriodTrackerString = $"DELETE FROM PeriodTrackers WHERE userId = {newUser.Id}";
                SqlCommand deletePeriodTrackerCommand = new SqlCommand(deletePeriodTrackerString, conn);
                deletePeriodTrackerCommand.ExecuteNonQuery();
            }

            string periodDate =
                $"{newUser.StartPeriodDate.Year}-{newUser.StartPeriodDate.Month}-{newUser.StartPeriodDate.Day}";
            string insertPeriodTrackerString =
                $"INSERT INTO PeriodTrackers " +
                $"VALUES ({newUser.Id}, '{periodDate}',{newUser.CycleDays},{newUser.PeriodLasts},{newUser.PremenstrualSyndromeOption})";
            SqlCommand insertPeriodTrackerCommand = new SqlCommand(insertPeriodTrackerString, conn);
            insertPeriodTrackerCommand.ExecuteNonQuery();

            // for the User Notifications, delete + insert
            string deleteUserNotificationsString = $"DELETE FROM UserNotifications WHERE userId = {newUser.Id}";
            SqlCommand deleteUserNotificationsCommand = new SqlCommand(deleteUserNotificationsString, conn);
            deleteUserNotificationsCommand.ExecuteNonQuery();


            string insertUserNotificationsString;
            foreach (int itemID in newUser.FavoriteItems)
            {
                // for each item in favorites, see if it also exists in stock alerts
                // if it does NOT, then add it as false, otherwise add it as true.
                // after that, when iterating through stock alerts, we know we already have either true-true pairs, or true-false fav-stock
                // so we need false-true pairs => check only for stock alert items that are NOT in favorite items
                if (newUser.StockAlerts.Contains(itemID))
                    insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'True', 'True')"; // has notifications for both

                else
                    insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'True', 'False')"; // has notifications only for favorite

                SqlCommand insertUserNotificationsCommand = new SqlCommand(insertUserNotificationsString, conn);
                insertUserNotificationsCommand.ExecuteNonQuery();
            }
            foreach (int itemID in newUser.StockAlerts)
            {
                if (newUser.FavoriteItems.Contains(itemID))
                    continue; // ignore user, already has entry

                insertUserNotificationsString =
                        $"INSERT INTO UserNotifications " +
                        $"VALUES ({newUser.Id}, {itemID}, 'False', 'True')"; // has notifications only for stock alerts


                SqlCommand insertUserNotificationsCommand = new SqlCommand(insertUserNotificationsString, conn);
                insertUserNotificationsCommand.ExecuteNonQuery();
            }

            // do the same for user discounts delete + insert
            string deleteUserDiscountsString = $"DELETE FROM UserDiscounts WHERE userId = {newUser.Id}";
            SqlCommand deleteUserDiscountsCommand = new SqlCommand(deleteUserDiscountsString, conn);
            deleteUserDiscountsCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, float> userDiscount in newUser.UserDiscounts)
            {
                string insertUserDiscountString =
                    $"INSERT INTO UserDiscounts VALUES ({newUser.Id},{userDiscount.Key}, {userDiscount.Value})";
                SqlCommand insertUserDiscountsCommand = new SqlCommand(insertUserDiscountString, conn);
                insertUserDiscountsCommand.ExecuteNonQuery();
            }

            // finally, do the same for period notes
            string deletePeriodNotesString = $"DELETE FROM PeriodNotes WHERE userId = {newUser.Id}";
            SqlCommand deletePeriodNotesCommand = new SqlCommand(deletePeriodNotesString, conn);
            deletePeriodNotesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, Tuple<string, bool>> periodNote in newUser.PeriodNotes)
            {
                string insertPeriodNoteString =
                    $"INSERT INTO PeriodNotes VALUES ({newUser.Id},{periodNote.Key}, '{periodNote.Value.Item1}', '{periodNote.Value.Item2}')";
                SqlCommand insertPeriodNoteCommand = new SqlCommand(insertPeriodNoteString, conn);
                insertPeriodNoteCommand.ExecuteNonQuery();
            }

        }

        public bool UserExists(string email)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE email='{email}'";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, conn);

            DataSet userDataFromDB = new DataSet();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
                return true;
            return false;
        }

        public bool UserExists(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM Users WHERE userId={id}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, conn);

            DataSet userDataFromDB = new DataSet();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "Users");

            if (userDataFromDB.Tables["Users"].Rows.Count > 0)
                return true;
            return false;
        }

        public bool UserHasPeriodTracker(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectUserString = $"SELECT * FROM PeriodTrackers WHERE userId={id}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter selectUserAdapter = new SqlDataAdapter(selectUserString, conn);

            DataSet userDataFromDB = new DataSet();

            conn.Open();
            selectUserAdapter.Fill(userDataFromDB, "PeriodTrackers");

            if (userDataFromDB.Tables["PeriodTrackers"].Rows.Count > 0)
                return true;
            return false;
        }
    }
}
﻿using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class UsersDBCrud
    {
        private readonly string _connectionString;
        // Create an instance of the MySqlDataAccess class
        private MySqlDataAccess db = new MySqlDataAccess();

        // Constructor
        public UsersDBCrud(string connectionString)
        {
            _connectionString = connectionString;
        }


        // ----------------- CRUD Operations ----------------- //
        // ----------------- Users DB CRUD Operations ----------------- //


        // Create Users table if doesn't exist and create user_table in the users database
        public void CreateUsersDB()
        {
            string sql = @"CREATE SCHEMA IF NOT EXISTS users;
                            USE users;
                            CREATE TABLE IF NOT EXISTS user_table (
                            Id INT NOT NULL AUTO_INCREMENT,
                            Username VARCHAR(200) NOT NULL,
                            MasterPassword VARCHAR(500) NOT NULL,
                            Email VARCHAR(300) NOT NULL,
                            User_Database VARCHAR(300) NOT NULL,
                            PRIMARY KEY (Id),
                            UNIQUE KEY Id_UNIQUE (Id),
                            UNIQUE KEY Username_UNIQUE (Username),
                            UNIQUE KEY Email_UNIQUE (Email));";

            db.SaveData(sql, new { }, _connectionString);
        }


        // Get all users from the users database's user_table
        public List<UsersModel> GetAllUsers()
        {
            string sql = "SELECT * FROM users.user_table";

            try
            {
                return db.LoadData<UsersModel, dynamic>(sql, new { }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add a new user to the users database's user_table
        public void CreateUser(string username, string masterPassword, string email, string userDatabase)
        {
            UsersModel data = new UsersModel
            {
                Username = username,
                MasterPassword = masterPassword,
                Email = email,
                User_Database = userDatabase
            };

            string sql = @"INSERT INTO users.user_table (Username, MasterPassword, Email, User_Database) 
                            VALUES (@Username, @MasterPassword, @Email, @User_Database);";

            db.SaveData(sql, data, _connectionString);

        }

        // Create a new user database
        public void CreateUserDatabase(string userDatabase)
        {
            string sql = $"CREATE SCHEMA IF NOT EXISTS {userDatabase};";

            db.SaveData(sql, new { }, _connectionString);
        }

        // Create Tables for the user in their particular newly created database
        public void CreateTables(string userDatabase)
        {

            // Create Logins Table
            string sql = $"CREATE TABLE {userDatabase}.`logins_table` (`Id` int NOT NULL AUTO_INCREMENT,  " +
                $"`Name` varchar(500) NOT NULL, `Username` varchar(300) NOT NULL," +
                $"`Password` BLOB NOT NULL, `URL` varchar(500), PRIMARY KEY (`Id`)," +
                $" UNIQUE KEY `Id_UNIQUE` (`Id`))";

            db.SaveData(sql, new { }, _connectionString);

            // Create Cards Table
            sql = $"CREATE TABLE {userDatabase}.`cards_table` (`Id` int NOT NULL AUTO_INCREMENT," +
                $"`Name` varchar(300) NOT NULL,  `CardholderName` varchar(300) NOT NULL," +
                $"`CardNumber` BLOB NOT NULL, `Brand` varchar(300) NOT NULL," +
                $"`ExpirationMonth` BLOB NOT NULL,`ExpirationYear` BLOB NOT NULL, " +
                $"`SecurityCode` BLOB NOT NULL,PRIMARY KEY (`Id`), UNIQUE KEY `Id_UNIQUE` (`Id`))";

            db.SaveData(sql, new { }, _connectionString);

            // Create Identities Table
            sql = $"CREATE TABLE {userDatabase}.`identities_table` (`Id` int NOT NULL AUTO_INCREMENT, " +
                $"`Name` varchar(300) NOT NULL, `Title` varchar(300) NOT NULL, `FirstName` varchar(300) NOT NULL," +
                $"`LastName` varchar(300) NOT NULL, `Username` varchar(100) DEFAULT NULL, " +
                $"`Company` varchar(200) DEFAULT NULL,`LicenseNumber` BLOB DEFAULT NULL, " +
                $"`Email` varchar(300) DEFAULT NULL, `Phone` BLOB DEFAULT NULL," +
                $"`Address` BLOB DEFAULT NULL, `Zip` BLOB DEFAULT NULL, " +
                $"`Country` varchar(200), `NidNo` BLOB DEFAULT NULL, `PassportNo` BLOB DEFAULT NULL," +
                $"PRIMARY KEY (`Id`), UNIQUE KEY `Id_UNIQUE` (`Id`))";

            db.SaveData(sql, new { }, _connectionString);


            // Create Secure Notes Table
            sql = $"CREATE TABLE {userDatabase}.`secure_notes_table` (`Id` int NOT NULL AUTO_INCREMENT," +
                $" `Name` varchar(300) NOT NULL,`SecureNote` BLOB NOT NULL, PRIMARY KEY (`Id`)," +
                $" UNIQUE KEY `Id_UNIQUE` (`Id`))";

            db.SaveData(sql, new { }, _connectionString);

            // Create user_cred Table with aes key, iv and sercretKey for 2FA
            sql = $"CREATE TABLE {userDatabase}.`user_cred` (`Id` int NOT NULL AUTO_INCREMENT," +
                $" `aesKey` BLOB DEFAULT NULL, `aesIV` BLOB DEFAULT NULL, `secretKey` BLOB NOT NULL," +
                $" PRIMARY KEY (`Id`), UNIQUE KEY `Id_UNIQUE` (`Id`))";

            db.SaveData(sql, new { }, _connectionString);
        }

        // Delete a user from the users database's user_table and delete the user's database using username and master password

        public void DeleteUser(string username, string masterPassword)
        {
            string sql = "DELETE FROM users.user_table WHERE Username = @Username AND MasterPassword = @MasterPassword";

            db.SaveData(sql, new { Username = username, MasterPassword = masterPassword }, _connectionString);
        }


        // Delete users Database
        public void DeleteDatabase(string userDatabase)
        {
            string sql = $"DROP DATABASE IF EXISTS `{userDatabase}`;";

            db.SaveData(sql, new { }, _connectionString);
        }

        // Get the Master Password from the Users database's user_table by unique Username
        public string GetMasterPassword(string username)
        {
            string sql = "SELECT MasterPassword FROM users.user_table WHERE Username = @Username";

            try
            {
                var data = db.LoadData<UsersModel, dynamic>(sql, new { Username = username }, _connectionString).First();

                return data.MasterPassword;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Update the Master Password in the Users database's user_table by unique Username
        public void UpdateMasterPassword(string username, string masterPassword)
        {
            string sql = "UPDATE users.user_table SET MasterPassword = @MasterPassword WHERE Username = @Username";

            db.SaveData(sql, new { Username = username, MasterPassword = masterPassword }, _connectionString);
        }


        // ----------------- Secure Notes Table Operations ----------------- //

        // Select Name and SecureNote from the secure_notes_table
        public List<SecureNotesModel> GetSecureNotes(string userDatabase)
        {
            string sql = $"SELECT Name, SecureNote FROM {userDatabase}.secure_notes_table";

            try
            {
                return db.LoadData<SecureNotesModel, dynamic>(sql, new { }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add a new Secure Note to the secure_notes_table
        public void AddSecureNote(string name, byte[] secureNote, string userDatabase)
        {
            SecureNotesModel data = new SecureNotesModel
            {
                Name = name,
                SecureNote = secureNote
            };

            string sql = $"INSERT INTO {userDatabase}.secure_notes_table (Name, SecureNote) VALUES (@Name, @SecureNote);";

            db.SaveData(sql, data, _connectionString);
        }

        // Delete a Secure Note from the secure_notes_table
        public void DeleteSecureNote(int id, string userDatabase)
        {
            string sql = $"DELETE FROM {userDatabase}.secure_notes_table WHERE Id = @Id";

            db.SaveData(sql, new { Id = id }, _connectionString);
        }

        // Update Name or SecureNote or Both in the secure_notes_table
        public void UpdateSecureNote(int id, string name, byte[] secureNote, string userDatabase)
        {
            string sql = $"UPDATE {userDatabase}.secure_notes_table SET Name = @Name, SecureNote = @SecureNote WHERE Id = @Id";

            db.SaveData(sql, new { Id = id, Name = name, SecureNote = secureNote }, _connectionString);
        }

        // Get id from secure_notes_table
        public int GetSecureNoteId(string name, string userDatabase)
        {
            string sql = $"SELECT Id FROM {userDatabase}.secure_notes_table WHERE Name = @Name";

            try
            {
                var data = db.LoadData<SecureNotesModel, dynamic>(sql, new { Name = name }, _connectionString).First();

                return data.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Search for a Secure Note in the secure_notes_table
        public List<SecureNotesModel> SearchSecureNotes(string search, string userDatabase)
        {
            string sql = $"SELECT Name, SecureNote FROM {userDatabase}.secure_notes_table WHERE Name LIKE @Search";

            try
            {
                return db.LoadData<SecureNotesModel, dynamic>(sql, new { Search = $"%{search}%" }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // ----------------- Logins Table Operations ----------------- //
        
        // Add items to logins_table
        public void AddLogin(string name, string username, byte[] password, string url, string userDatabase)
        {
            LoginsModel data = new LoginsModel
            {
                Name = name,
                Username = username,
                Password = password,
                URL = url
            };

            string sql = $"INSERT INTO {userDatabase}.logins_table (Name, Username, Password, URL) " +
                $"VALUES (@Name, @Username, @Password, @URL);";

            db.SaveData(sql, data, _connectionString);
        }

        // Get all items from logins_table
        public List<LoginsModel> GetLogins(string userDatabase)
        {
            string sql = $"SELECT Name, Username, Password, URL FROM {userDatabase}.logins_table";

            try
            {
                return db.LoadData<LoginsModel, dynamic>(sql, new { }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Delete an item from logins_table
        public void DeleteLogin(int id, string userDatabase)
        {
            string sql = $"DELETE FROM {userDatabase}.logins_table WHERE Id = @Id";

            db.SaveData(sql, new { Id = id }, _connectionString);
        }

        // Update an item in logins_table
        public void UpdateLogin(int id, string name, string username, byte[] password, string url, string userDatabase)
        {
            string sql = $"UPDATE {userDatabase}.logins_table SET Name = @Name, " +
                $"Username = @Username, Password = @Password, URL = @URL WHERE Id = @Id";

            db.SaveData(sql, new { Id = id, Name = name, Username = username, Password = password, URL = url }, _connectionString);
        }

        // Get id from logins_table by Name and username
        public int GetLoginId(string name, string username, string userDatabase)
        {
            string sql = $"SELECT Id FROM {userDatabase}.logins_table WHERE Name = @Name AND Username = @Username";

            try
            {
                var data = db.LoadData<LoginsModel, dynamic>(sql, new { Name = name, Username = username }, _connectionString).First();

                return data.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // search for a login in the logins_table
        public List<LoginsModel> SearchLogins(string search, string userDatabase)
        {
            string sql = $"SELECT Name, Username, Password, URL FROM {userDatabase}.logins_table WHERE Name LIKE @Search OR Username LIKE @Search OR URL LIKE @Search";

            try
            {
                return db.LoadData<LoginsModel, dynamic>(sql, new { Search = $"%{search}%" }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }


        // ----------------- Cards Table Operations ----------------- //

        // Add items to cards_table
        public void AddCard(string name, string cardholderName, byte[] cardNumber, string brand, 
            byte[] expirationMonth, byte[] expirationYear, byte[] securityCode, string userDatabase)
        {
            CardsModel data = new CardsModel
            {
                Name = name,
                CardholderName = cardholderName,
                CardNumber = cardNumber,
                Brand = brand,
                ExpirationMonth = expirationMonth,
                ExpirationYear = expirationYear,
                SecurityCode = securityCode
            };

            string sql = $"INSERT INTO {userDatabase}.cards_table (Name, CardholderName, CardNumber, Brand, " +
                $"ExpirationMonth, ExpirationYear, SecurityCode) " +
                $"VALUES (@Name, @CardholderName, @CardNumber, @Brand, @ExpirationMonth, @ExpirationYear, " +
                $"@SecurityCode);";

            db.SaveData(sql, data, _connectionString);
        }

        // Get all items from cards_table
        public List<CardsModel> GetCards(string userDatabase)
        {
            string sql = $"SELECT Name, CardholderName, CardNumber, Brand, ExpirationMonth, ExpirationYear, " +
                $"SecurityCode FROM {userDatabase}.cards_table";

            try
            {
                return db.LoadData<CardsModel, dynamic>(sql, new { }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Delete an item from cards_table
        public void DeleteCard(int id, string userDatabase)
        {
            string sql = $"DELETE FROM {userDatabase}.cards_table WHERE Id = @Id";

            db.SaveData(sql, new { Id = id }, _connectionString);
        }

        // Update an item in cards_table
        public void UpdateCard(int id, string name, string cardholderName, byte[] cardNumber, 
            string brand, byte[] expirationMonth, byte[] expirationYear, byte[] securityCode, 
            string userDatabase)
        {
            string sql = $"UPDATE {userDatabase}.cards_table SET Name = @Name, CardholderName = @CardholderName, " +
                $"CardNumber = @CardNumber, Brand = @Brand, ExpirationMonth = @ExpirationMonth, ExpirationYear = @ExpirationYear, " +
                $"SecurityCode = @SecurityCode WHERE Id = @Id";

            db.SaveData(sql, new { Id = id, Name = name, CardholderName = cardholderName, CardNumber = cardNumber, Brand = brand, ExpirationMonth = expirationMonth, ExpirationYear = expirationYear, SecurityCode = securityCode }, _connectionString);
        }

        // Get id from cards_table by Name and Brand
        public int GetCardId(string name, string brand, string userDatabase)
        {
            string sql = $"SELECT Id FROM {userDatabase}.cards_table WHERE Name = @Name AND Brand = @Brand";

            try
            {
                var data = db.LoadData<CardsModel, dynamic>(sql, new { Name = name, Brand = brand }, _connectionString).First();

                return data.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Search for a card in the cards_table
        public List<CardsModel> SearchCards(string search, string userDatabase)
        {
            string sql = $"SELECT Name, CardholderName, CardNumber, Brand, ExpirationMonth, ExpirationYear, " +
                $"SecurityCode FROM {userDatabase}.cards_table WHERE Name LIKE @Search OR CardholderName LIKE @Search OR Brand LIKE @Search";

            try
            {
                return db.LoadData<CardsModel, dynamic>(sql, new { Search = $"%{search}%" }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }


        // ----------------- Identities Table Operations ----------------- //

        // Add items to identities_table

        public void AddIdentity(string name, string title, string firstName, string lastName, string username, 
            string company, byte[] licenseNumber, string email, byte[] phone, byte[] address, byte[] zip, 
            string country, byte[] nidNo, byte[] passportNo, string userDatabase)
        {
            IdentitiesModel data = new IdentitiesModel
            {
                Name = name,
                Title = title,
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                Company = company,
                LicenseNumber = licenseNumber,
                Email = email,
                Phone = phone,
                Address = address,
                Zip = zip,
                Country = country,
                NidNo = nidNo,
                PassportNo = passportNo
            };

            string sql = $"INSERT INTO {userDatabase}.identities_table (Name, Title, FirstName, LastName, " +
                $"Username, Company, LicenseNumber, Email, Phone, Address, Zip, Country, NidNo, PassportNo) " +
                $"VALUES (@Name, @Title, @FirstName, @LastName, @Username, @Company, @LicenseNumber, @Email, " +
                $"@Phone, @Address, @Zip, @Country, @NidNo, @PassportNo);";

            db.SaveData(sql, data, _connectionString);
        }

        // Get all items from identities_table
        public List<IdentitiesModel> GetIdentities(string userDatabase)
        {
            string sql = $"SELECT Name, Title, FirstName, LastName, Username, Company, LicenseNumber, Email, Phone, " +
                $"Address, Zip, Country, NidNo, PassportNo FROM {userDatabase}.identities_table";

            try
            {
                return db.LoadData<IdentitiesModel, dynamic>(sql, new { }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Delete an item from identities_table
        public void DeleteIdentity(int id, string userDatabase)
        {
            string sql = $"DELETE FROM {userDatabase}.identities_table WHERE Id = @Id";

            db.SaveData(sql, new { Id = id }, _connectionString);
        }

        // Update an item in identities_table
        public void UpdateIdentity(int id, string name, string title, string firstName, string lastName, 
            string username, string company, byte[] licenseNumber, string email, byte[] phone, byte[] address, 
            byte[] zip, string country, byte[] nidNo, byte[] passportNo, string userDatabase)
        {
            string sql = $"UPDATE {userDatabase}.identities_table SET Name = @Name, Title = @Title, FirstName = @FirstName, " +
                $"LastName = @LastName, Username = @Username, Company = @Company, LicenseNumber = @LicenseNumber, " +
                $"Email = @Email, " +
                $"Phone = @Phone, Address = @Address, Zip = @Zip, Country = @Country, NidNo = @NidNo, " +
                $"PassportNo = @PassportNo WHERE Id = @Id";

            db.SaveData(sql, new { Id = id, Name = name, Title = title, FirstName = firstName, LastName = lastName, 
                Username = username, Company = company, LicenseNumber = licenseNumber, Email = email, 
                Phone = phone, Address = address, Zip = zip, Country = country, NidNo = nidNo, 
                PassportNo = passportNo }, _connectionString);
        }

        // Get id from identities_table by Name
        public int GetIdentityId(string name, string userDatabase)
        {
            string sql = $"SELECT Id FROM {userDatabase}.identities_table WHERE Name = @Name";

            try
            {
                var data = db.LoadData<IdentitiesModel, dynamic>(sql, new { Name = name }, _connectionString).First();

                return data.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        // Search for an identity in the identities_table
        public List<IdentitiesModel> SearchIdentities(string search, string userDatabase)
        {
            string sql = $"SELECT Name, Title, FirstName, LastName, Username, Company, LicenseNumber, Email, Phone, " +
                $"Address, Zip, Country, NidNo, PassportNo FROM {userDatabase}.identities_table WHERE Name LIKE @Search OR Username LIKE @Search OR Email LIKE @Search";

            try
            {
                return db.LoadData<IdentitiesModel, dynamic>(sql, new { Search = $"%{search}%" }, _connectionString);
            }
            catch (Exception)
            {
                return null;
            }
        }


        // ----------------- User Cred Table Operations ----------------- //

        // Add secretKey to the user_cred table
        public void AddSecretKey(string userDatabase, byte[] secretKey)
        {
            UserCredModel data = new UserCredModel
            {
                SecretKey = secretKey
            };

            string sql = $"INSERT INTO {userDatabase}.user_cred (secretKey) VALUES (@SecretKey);";

            db.SaveData(sql, data, _connectionString);
        }


        // Get secretKey from user_cred table
        public byte[] GetSecretKey(string userDatabase)
        {
            string sql = $"SELECT SecretKey FROM {userDatabase}.user_cred";

            try
            {
                var data = db.LoadData<UserCredModel, dynamic>(sql, new { }, _connectionString).First();

                return data.SecretKey;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

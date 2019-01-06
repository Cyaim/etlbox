﻿using ALE.ETLBox;
using ALE.ETLBox.ConnectionManager;
using ALE.ETLBox.ControlFlow;
using ALE.ETLBox.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ALE.ETLBoxTest {
    [TestClass]
    public class TestSqlConnectionManager {
        public TestContext TestContext { get; set; }
        public string ConnectionStringParameter => TestContext?.Properties["connectionString"].ToString();
        public string DBNameParameter => TestContext?.Properties["dbName"].ToString();

        [ClassInitialize]
        public static void TestInit(TestContext testContext) {
            TestHelper.RecreateDatabase(testContext);
        }
      
        [TestMethod]
        public void TestOpeningCloseConnection() {
            /*User calls Open()
 This first call creates a SqlConnection() object
 It then checks to ensure that it's not open and attempts to open a connection
 User calls Open() again
 This second call creates a new SqlConnection() object
 It then checks to ensure that it's not open, which it is not because its a new one...
 */
            SqlConnectionManager con = new SqlConnectionManager(new ConnectionString(ConnectionStringParameter));
            
            AssertOpenConnectionCount(0);
            con.Open();
            AssertOpenConnectionCount(1);
            con.Close(); //won't close any connection - ado.net will keep the connection open in it's pool in case it's needed again
            AssertOpenConnectionCount(1);
            SqlConnection.ClearAllPools();
            AssertOpenConnectionCount(0);



        }

        [TestMethod]
        public void TestOpeningConnectionTwice() {
            SqlConnectionManager con = new SqlConnectionManager(new ConnectionString(ConnectionStringParameter));
            AssertOpenConnectionCount(0);
            con.Open();
            con.Open();
            AssertOpenConnectionCount(1);
            con.Close();
            AssertOpenConnectionCount(1);
            SqlConnection.ClearAllPools();
            AssertOpenConnectionCount(0);
        }

        private void AssertOpenConnectionCount(int allowedOpenConnections) {
            ConnectionString conString = new ConnectionString(ConnectionStringParameter);
            SqlConnectionManager master = new SqlConnectionManager(conString.GetMasterConnection());
            string dbName = conString.SqlConnectionString.InitialCatalog;
            int? openConnections = 
                new SqlTask("Count open connections", 
                $@"SELECT COUNT(dbid) as NumberOfConnections FROM sys.sysprocesses
                    WHERE dbid > 0 and DB_NAME(dbid) = '{dbName}'") 
                { ConnectionManager = master, DisableLogging = true }
                .ExecuteScalar<int>()
                .Value;
            Assert.AreEqual(allowedOpenConnections, openConnections);
        }
    }
}
using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.Exceptions;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using System;
using System.Collections.Generic;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class DbSourceWithSqlTests
    {
        public static IEnumerable<object[]> Connections => Config.AllSqlConnections("DataFlow");

        public static SqlConnectionManager SqlConnection => Config.SqlConnection.ConnectionManager("DataFlow");
        public static IEnumerable<object[]> OdbcConnections => Config.AllOdbcConnectionsExceptAccess("DataFlow");

        public DbSourceWithSqlTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        public class MySimpleRow
        {
            public long Col1 { get; set; }
            public string Col2 { get; set; }
        }

        [Theory, MemberData(nameof(Connections)),
            MemberData(nameof(OdbcConnections))]
        public void SqlWithSelectStar(IConnectionManager connection)
        {
            //Arrange
            TwoColumnsTableFixture s2c = new TwoColumnsTableFixture(connection, "SourceSelectStar");
            s2c.InsertTestData();
            TwoColumnsTableFixture d2c = new TwoColumnsTableFixture(connection, "DestinationSelectStar");

            //Act
            DbSource<MySimpleRow> source = new DbSource<MySimpleRow>()
            {
                Sql = $@"SELECT * FROM {s2c.QB}SourceSelectStar{s2c.QE}",
                ConnectionManager = connection
            };
            DbDestination<MySimpleRow> dest = new DbDestination<MySimpleRow>(connection, "DestinationSelectStar");
            source.LinkTo(dest);
            source.Execute();
            dest.Wait();
            d2c.AssertTestData();
            //Assert

        }

        [Theory, MemberData(nameof(Connections)),
            MemberData(nameof(OdbcConnections))]
        public void SqlWithNamedColumns(IConnectionManager connection)
        {
            //Arrange
            TwoColumnsTableFixture s2c = new TwoColumnsTableFixture(connection, "SourceSql");
            s2c.InsertTestData();
            TwoColumnsTableFixture d2c = new TwoColumnsTableFixture(connection, "DestinationSql");

            //Act
            DbSource<MySimpleRow> source = new DbSource<MySimpleRow>()
            {
                Sql = $@"SELECT CASE WHEN {s2c.QB}Col1{s2c.QE} IS NOT NULL THEN {s2c.QB}Col1{s2c.QE} ELSE {s2c.QB}Col1{s2c.QE} END AS {s2c.QB}Col1{s2c.QE}, 
{s2c.QB}Col2{s2c.QE} 
FROM {s2c.QB}SourceSql{s2c.QE}",
                ConnectionManager = connection
            };
            DbDestination<MySimpleRow> dest = new DbDestination<MySimpleRow>(connection, "DestinationSql");
            source.LinkTo(dest);
            source.Execute();
            dest.Wait();

            //Assert
            d2c.AssertTestData();
        }

        [Fact]
        public void SqlWithSelectStarAndDynamicObject()
        {
            //Arrange
            TwoColumnsTableFixture s2c = new TwoColumnsTableFixture(SqlConnection, "SourceSelectStarDynamic");
            s2c.InsertTestData();
            //Act
            DbSource source = new DbSource()
            {
                Sql = $@"SELECT * FROM {s2c.QB}SourceSelectStarDynamic{s2c.QE}",
                ConnectionManager = SqlConnection
            };
            MemoryDestination dest = new MemoryDestination();
            source.LinkTo(dest);

            Assert.Throws<ETLBoxException>(() =>
            {
                try
                {
                    source.Execute();
                    dest.Wait();
                }
                catch (Exception e)
                {
                    throw e.InnerException;
                }
            });
        }
    }
}

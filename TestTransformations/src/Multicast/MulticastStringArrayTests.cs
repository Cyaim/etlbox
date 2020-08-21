using ETLBox.Connection;
using ETLBox.DataFlow.Connectors;
using ETLBox.DataFlow.Transformations;
using ETLBoxTests.Fixtures;
using ETLBoxTests.Helper;
using Xunit;

namespace ETLBoxTests.DataFlowTests
{
    [Collection("DataFlow")]
    public class MulticastStringArrayTests
    {
        public SqlConnectionManager Connection => Config.SqlConnection.ConnectionManager("DataFlow");
        public MulticastStringArrayTests(DataFlowDatabaseFixture dbFixture)
        {
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public void SplitInto2Tables(int maxBufferSize)
        {
            //Arrange
            TwoColumnsTableFixture sourceTable = new TwoColumnsTableFixture("Source");
            sourceTable.InsertTestData();
            TwoColumnsTableFixture dest1Table = new TwoColumnsTableFixture("Destination1");
            TwoColumnsTableFixture dest2Table = new TwoColumnsTableFixture("Destination2");

            DbSource<string[]> source = new DbSource<string[]>(Connection, "Source");
            DbDestination<string[]> dest1 = new DbDestination<string[]>(Connection, "Destination1");
            DbDestination<string[]> dest2 = new DbDestination<string[]>(Connection, "Destination2");
            Multicast<string[]> multicast = new Multicast<string[]>();

            if (maxBufferSize > 0)
            {
                dest1.MaxBufferSize = maxBufferSize;
                dest1.BatchSize = maxBufferSize;
            }
            //Act


            source.LinkTo(multicast);
            multicast.LinkTo(dest1);
            multicast.LinkTo(dest2);
            source.Execute();
            dest1.Wait();
            dest2.Wait();

            //Assert
            dest1Table.AssertTestData();
            dest2Table.AssertTestData();
        }

    }
}

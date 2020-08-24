﻿using ETLBox.Connection;

namespace ETLBox.ControlFlow.Tasks
{
    /// <summary>
    /// Drops a table. Use DropIfExists to drop a table only if it exists.
    /// </summary>
    public class DropTableTask : DropTask<IfTableOrViewExistsTask>, ILoggableTask
    {
        internal override string GetSql()
        {
            return $@"DROP TABLE {ON.QuotatedFullName}";
        }

        public DropTableTask()
        {
        }

        public DropTableTask(string tableName) : this()
        {
            ObjectName = tableName;
        }

        public static void Drop(string tableName)
            => new DropTableTask(tableName).Drop();
        public static void Drop(IConnectionManager connectionManager, string tableName)
            => new DropTableTask(tableName) { ConnectionManager = connectionManager }.Drop();
        public static void DropIfExists(string tableName)
            => new DropTableTask(tableName).DropIfExists();
        public static void DropIfExists(IConnectionManager connectionManager, string tableName)
            => new DropTableTask(tableName) { ConnectionManager = connectionManager }.DropIfExists();

    }


}

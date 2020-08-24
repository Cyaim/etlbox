﻿using ETLBox.Connection;
using ETLBox.Exceptions;

namespace ETLBox.ControlFlow.Tasks
{
    /// <summary>
    /// Drops a schema. Use DropIfExists to drop a schema only if it exists. For MySql, use the DropDatabase task instead.
    /// </summary>
    public class DropSchemaTask : DropTask<IfSchemaExistsTask>, ILoggableTask
    {
        internal override string GetSql()
        {
            if (!DbConnectionManager.SupportSchemas)
                throw new ETLBoxNotSupportedException("This task is not supported!");

            string sql = $@"DROP SCHEMA {ON.QuotatedFullName}";
            return sql;
        }

        public DropSchemaTask()
        {
        }

        public DropSchemaTask(string schemaName) : this()
        {
            ObjectName = schemaName;
        }

        public static void Drop(string schemaName)
            => new DropSchemaTask(schemaName).Drop();
        public static void Drop(IConnectionManager connectionManager, string schemaName)
            => new DropSchemaTask(schemaName) { ConnectionManager = connectionManager }.Drop();
        public static void DropIfExists(string schemaName)
            => new DropSchemaTask(schemaName).DropIfExists();
        public static void DropIfExists(IConnectionManager connectionManager, string schemaName)
            => new DropSchemaTask(schemaName) { ConnectionManager = connectionManager }.DropIfExists();
    }


}

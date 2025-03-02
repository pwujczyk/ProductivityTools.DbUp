using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityTools
{
    public class DBUp
    {
        private string JorunalSchema;
        private string JorunalTableName;

        public DBUp() : this("dbo", "dbUp") { }
        public DBUp(string schema) : this(schema, "dbUp") { }
        public DBUp(string schema, string dbupJournalTableName)
        {
            this.JorunalSchema = schema;
            this.JorunalTableName = dbupJournalTableName;
        }

        private static void InvokeQuery(string connectionString, string query)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void CreateSQLDB(string databaseName, string dataSourceConnectionString)
        {
            string query = string.Format(@"IF NOT EXISTS ( SELECT [Name] FROM sys.databases WHERE [name] = '{0}' )
                            BEGIN
                                CREATE DATABASE {0}
                            END
                            ", databaseName);

            InvokeQuery(dataSourceConnectionString, query);
        }

        private void CreateAdminSchema(string initialCatalogConnectionString)
        {
            var query = string.Format(@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
                            BEGIN
                            EXEC('CREATE SCHEMA {0}')
                            END", JorunalSchema);
            InvokeQuery(initialCatalogConnectionString, query);
        }

        public void PerformUpdate(string dataSource, string databaseName, Assembly assembly,bool LeaveConsoleOpen = false, bool? trustServerCertificate = null)
        {
            var dataSourceCS = ConnectionStringLight.GetSqlDataSourceConnectionString(dataSource, trustServerCertificate);
            CreateSQLDB(databaseName, dataSourceCS);

            string connectionString = ConnectionStringLight.GetSqlServerConnectionString(dataSource, databaseName, trustServerCertificate);
            CreateAdminSchema(connectionString);
            UpdateDatabase(assembly, connectionString, LeaveConsoleOpen);
        }

        private void UpdateDatabase(Assembly assembly, string updateDBConnectionString, bool LeaveConsoleOpen)
        {
            var upgrader = DbUp.DeployChanges.To
              .SqlDatabase(updateDBConnectionString)
              .WithScriptsEmbeddedInAssembly(assembly)
              .JournalToSqlTable(JorunalSchema, JorunalTableName)
              .LogToConsole()
              .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
            }

            if (LeaveConsoleOpen)
            {
                Console.WriteLine("You are set LeaveConsoleOpen parameter, press enter to close console");
                Console.ReadLine();
            }
        }
    }
}

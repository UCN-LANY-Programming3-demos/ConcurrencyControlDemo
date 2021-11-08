﻿using DbUp;
using System.Reflection;

namespace OrderService.Database
{
    public static class DataBaseVersion
    {
        public static bool Upgrade(string connectionString)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            return result.Successful;
        }

        public static void Drop(string connectionString)
        {
            DropDatabase.For.SqlDatabase(connectionString);
        }
    }
}

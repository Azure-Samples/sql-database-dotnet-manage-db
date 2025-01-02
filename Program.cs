// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Samples.Common;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using System.Threading.Tasks;
using System;
using System.Data;

namespace ManageSqlDatabase
{
    public class Program
    {
        private static ResourceIdentifier? _resourceGroupId = null;

        /**
         * Azure Storage sample for managing SQL Database -
         *  - Create a SQL Server along with 2 firewalls.
         *  - Create a database in SQL server
         *  - Change performance level (SKU) of SQL Database
         *  - List and delete firewalls.
         *  - Create another firewall in the SQlServer
         *  - Delete database, firewall and SQL Server
         */
        public static async Task RunSample(ArmClient client)
        {
            try
            {
                //Get default subscription
                SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();

                //Create a resource group in the EastUS region
                string rgName = Utilities.CreateRandomName("rgSQLServer");
                Utilities.Log("Creating resource group...");
                var rgLro = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(AzureLocation.EastUS));
                ResourceGroupResource resourceGroup = rgLro.Value;
                _resourceGroupId = resourceGroup.Id;
                Utilities.Log($"Created a resource group with name: {resourceGroup.Data.Name} ");

                // ============================================================
                // Create a SQL Server, with 2 firewall rules.
                string sqlServerName = Utilities.CreateRandomName("sqlserver");
                Utilities.Log("Creating SQL Server...");
                SqlServerData sqlData = new SqlServerData(AzureLocation.EastUS)
                {
                    AdministratorLogin = "sqladmin" + sqlServerName,
                    AdministratorLoginPassword = Utilities.CreatePassword()
                };
                var sqlServerLro = await resourceGroup.GetSqlServers().CreateOrUpdateAsync(WaitUntil.Completed, sqlServerName, sqlData);
                SqlServerResource sqlServer = sqlServerLro.Value;
                Utilities.Log($"Created a SQL Server with name: {sqlServer.Data.Name} " );

                string firewallRuleName1 = Utilities.CreateRandomName("firewallrule1-");
                Utilities.Log("Creating 2 firewall rules...");
                SqlFirewallRuleData firewallData1 = new SqlFirewallRuleData()
                {
                    StartIPAddress = "10.0.0.1",
                    EndIPAddress = "10.0.0.10"
                };
                var firewallRuleLro1 = await sqlServer.GetSqlFirewallRules().CreateOrUpdateAsync(WaitUntil.Completed, firewallRuleName1, firewallData1);
                SqlFirewallRuleResource firewallRule1 = firewallRuleLro1.Value;
                Utilities.Log($"Created first firewall rule with name {firewallRule1.Data.Name}");

                string firewallRuleName2 = Utilities.CreateRandomName("firewallrule2-");
                SqlFirewallRuleData firwallData2 = new SqlFirewallRuleData()
                {
                    StartIPAddress = "10.2.0.1",
                    EndIPAddress = "10.2.0.10"
                };
                var firewallRuleLro2 = await sqlServer.GetSqlFirewallRules().CreateOrUpdateAsync(WaitUntil.Completed, firewallRuleName2, firwallData2);
                SqlFirewallRuleResource firewallRule2 = firewallRuleLro2.Value;
                Utilities.Log($"Created Second firewall rule with name {firewallRule2.Data.Name}");

                // ============================================================
                // Create a Database in SQL server created above.
                Utilities.Log("Creating a database...");
                string DBName = Utilities.CreateRandomName("sql-database");
                SqlDatabaseData DBDate = new SqlDatabaseData(AzureLocation.EastUS) { };
                var sqlDBLro = await sqlServer.GetSqlDatabases().CreateOrUpdateAsync(WaitUntil.Completed, DBName, DBDate);
                SqlDatabaseResource sqlDB = sqlDBLro.Value;
                Utilities.Log($"Created database with name: {sqlDB.Data.Name} ");

                // ============================================================
                // Update the edition of database.
                Utilities.Log("Updating a database...");
                SqlDatabasePatch sqlDatabasePatch = new SqlDatabasePatch()
                {
                    Sku = new SqlSku("HS_Gen5_2"),
                    MaxSizeBytes = 268435456000L,
                    LicenseType = DatabaseLicenseType.LicenseIncluded
                };
                sqlDB = (await sqlDB.UpdateAsync(WaitUntil.Completed, sqlDatabasePatch)).Value;
                Utilities.Log($"Updated a datebase ");

                // ============================================================
                // List and delete all firewall rules.
                Utilities.Log("Listing all firewall rules");
                var firewallRules =await sqlServer.GetSqlFirewallRules().GetAllAsync().ToEnumerableAsync();
                foreach (var firewallRule in firewallRules)
                {
                    // Print information of the firewall rule.
                    Utilities.Log($"Listing a firewall rule with name: {firewallRule.Data.Name}");

                    // Delete the firewall rule.
                    Utilities.Log($"Deleting a firewall rule ...");
                    firewallRule.Delete(WaitUntil.Completed);
                    Utilities.Log($"Deleted a firewall rule with name: {firewallRule.Data.Name}");
                }

                // ============================================================
                // Add new firewall rules.
                Utilities.Log("Creating a new firewall rule for SQL Server...");
                string newfirewallName = Utilities.CreateRandomName("newfirewallrule");
                SqlFirewallRuleData newfirewallData = new SqlFirewallRuleData()
                {
                    StartIPAddress = "10.10.10.10",
                    EndIPAddress = "10.10.10.10"
                };
                var newfirewallLro = await sqlServer.GetSqlFirewallRules().CreateOrUpdateAsync(WaitUntil.Completed, newfirewallName, newfirewallData);
                SqlFirewallRuleResource newfirewallRule = newfirewallLro.Value;
                Utilities.Log($"Created a new firewall rule for SQL Server with name: {newfirewallRule.Data.Name}");

                // Delete the database.
                Utilities.Log("Deleting a database...");
                await sqlDB.DeleteAsync(WaitUntil.Completed);

                // Delete the SQL Server.
                Utilities.Log("Deleting a Sql Server...");
                await sqlServer.DeleteAsync(WaitUntil.Completed);
            }
            finally
            {
                try
                {
                    if (_resourceGroupId is not null)
                    {
                        Utilities.Log($"Deleting Resource Group...");
                        await client.GetResourceGroupResource(_resourceGroupId).DeleteAsync(WaitUntil.Completed);
                        Utilities.Log($"Deleted Resource Group: {_resourceGroupId.Name}");
                    }
                }
                catch(Exception e)
                {
                    Utilities.Log(e);
                }
            }
        }
        public static async Task Main(string[] args)
        {
            try
            {
                //=================================================================
                // Authenticate

                var clientId = Environment.GetEnvironmentVariable("CLIENT_ID");
                var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
                var tenantId = Environment.GetEnvironmentVariable("TENANT_ID");
                var subscription = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
                ClientSecretCredential credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                ArmClient client = new ArmClient(credential, subscription);

                await RunSample(client);
            }
            catch (Exception e)
            {
                Utilities.Log(e);
            }
        }
    }
}
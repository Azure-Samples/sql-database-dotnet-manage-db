---
services: Sql
platforms: .Net
author: anudeepsharma
---

#Getting Started with Sql - Manage Sql Database - in .Net #

          Azure Storage sample for managing SQL Database -
           - Create a SQL Server along with 2 firewalls.
           - Create a database in SQL server
           - Change performance level (SKU) of SQL Database
           - List and delete firewalls.
           - Create another firewall in the SQlServer
           - Delete database, firewall and SQL Server


## Running this Sample ##

To run this sample:

Set the environment variable `AZURE_AUTH_LOCATION` with the full path for an auth file. See [how to create an auth file](https://github.com/Azure/azure-sdk-for-net/blob/Fluent/AUTH.md).

    git clone https://github.com/Azure-Samples/sql-database-dotnet-manage-db.git

    cd sql-database-dotnet-manage-db

    dotnet restore

    dotnet run

## More information ##

[Azure Management Libraries for C#](https://github.com/Azure/azure-sdk-for-net/tree/Fluent)
[Azure .Net Developer Center](https://azure.microsoft.com/en-us/develop/net/)
If you don't have a Microsoft Azure subscription you can get a FREE trial account [here](http://go.microsoft.com/fwlink/?LinkId=330212)

---

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
1. Add IdentityServer4 nuget, add AddIdentityServer() and add UseIdentityServer()
2. Add IdentityServer4.EntityFramework and Pomelo.EntityFrameworkCore.MySql, add AddConfigurationStore and AddOperationalStore.
3. Add Microsoft.EntityFrameworkCore.Tools/Microsoft.EntityFrameworkCore.Design, and add migration.
    Add-Migration initPersistedGrantDb -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
    Add-Migration initConfigurationDb -c ConfigurationDbContext -o Migrations/ConfigurationDb
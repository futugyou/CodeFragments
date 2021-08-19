1. Add IdentityServer4 nuget, add AddIdentityServer() and add UseIdentityServer()
2. Add IdentityServer4.EntityFramework and Pomelo.EntityFrameworkCore.MySql, add AddConfigurationStore and AddOperationalStore.
3. Add Microsoft.EntityFrameworkCore.Tools/Microsoft.EntityFrameworkCore.Design, and add migration.
    Add-Migration initPersistedGrantDb -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
    Add-Migration initConfigurationDb -c ConfigurationDbContext -o Migrations/ConfigurationDb
4. create X.509 certificates.
5. visit well-known endopint https://localhost:5001/.well-known/openid-configuration, and test.
```
curl --location --request POST 'https://localhost:5001/connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'client_id=client' \
--data-urlencode 'client_secret=secret' \
--data-urlencode 'scope=api1' \
--data-urlencode 'grant_type=client_credentials'
```
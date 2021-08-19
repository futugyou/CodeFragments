1. Add IdentityServer4 package, add AddIdentityServer() and add UseIdentityServer()
2. Add IdentityServer4.EntityFramework and Pomelo.EntityFrameworkCore.MySql package, add AddConfigurationStore and AddOperationalStore.
3. Add Microsoft.EntityFrameworkCore.Tools/Microsoft.EntityFrameworkCore.Design package, and add migration.
    Add-Migration InitPersistedGrantDb -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
    Add-Migration InitConfigurationDb -c ConfigurationDbContext -o Migrations/ConfigurationDb
4. create X.509 certificates.
5. visit well-known endopint https://localhost:5001/.well-known/openid-configuration, and add jwtapi test project.
```
curl --location --request POST 'https://localhost:5001/connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'client_id=client' \
--data-urlencode 'client_secret=secret' \
--data-urlencode 'scope=api1' \
--data-urlencode 'grant_type=client_credentials'
```

6. Add Microsoft.AspNetCore.Identity.EntityFrameworkCore package, add ApplicationUser and ApplicationDbContext.
7. AddDbContext and AddIdentity, then add migration.
    Add-Migration InitIdentityDb -c ApplicationDbContext -o Migrations/IdentityDb
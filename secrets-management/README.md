# Managing user secrets in ASP.NET Core 
## User secrets for development
Steps:
```python3
dotnet user-secrets init
# single secret
dotnet user-secrets set API_KEY "someverylongapikey"
# connection strings
dotnet user-secrets set ConnectionStrings:MySQL "somedifficult connection string"
dotnet user-secrets set ConnectionStrings:MSSQL "MSSQL somedifficult connection string"
# section (falattened)
dotnet user-secrets set user:name "Kiryl"
dotnet user-secrets set user:lastname "Volkau"
```
Then add:
```c#
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

    if (env.IsDevelopment())
    {
        // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
        builder.AddUserSecrets();
    }

    builder.AddEnvironmentVariables();
    Configuration = builder.Build();
}
```
To see all: `dotnet user-secrets list`. <br/>
To reset them: `dotnet user-secrets clear`. <br/>
To display all of them in the browser:
```c#
 var user = Configuration.GetSection("user").Get<User>();
 var api = $"<p>API key : {Configuration["API_KEY"]} </p>";
 var mysql = $"<p>MYSQL : {Configuration.GetConnectionString("MYSQL")}</p>";
 var mssql = $"<p>MSSQL : {Configuration.GetConnectionString("MSSQL")}</p>";
 var userp = $"<p>User : {user.Name} {user.LastName}";
 await context.Response.WriteAsync(api+mssql+mysql+userp);
```
Additional link: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1 .
## Azure key vault (production)
Three options:
* OAuth - still need to manage one secret.
* Certificates - good only for self-hosting.
* Managed identity - good for hosting in Azure.

Process for the third:
1. Create Key Vault resource in Azure
2. Add NuGet package `Microsoft.Extensions.Configuration.AzureKeyVault`.
3. To `CreateHostBuilder` pipeline add:
```c#
.ConfigureAppConfiguration((context, config) => 
{
    config.AddAzureKeyVault("https://azure-keyvault-name-uri/");
})
```
Then: 
* Azure Service Token Provider will try several identification processes, as VS, Azure CLI, and, finally, Azure Managed Identities.
* Locally 1st 2 will work, in the cloud use 3rd.
* In web application enable managed identities.
* In key vault add new policy -> select your app as a principal.
* For just getting secrets grant `GET` and `LIST` (we need to add all) permissions.
* When setting secrets use `--` instead of `:`.
* Accessing secrets works the same.

More on the managed identities: https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview .
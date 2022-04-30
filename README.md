# SimpleIdentity
A simple bearer access and refresh token api
(Note: This readme is a work in progress itself)

## Configuring for your solution
Copy the connection strings, token settings into your main startup project (usually the main app's api project). Update the keys and issuer/audience. Really best to keep
the key in the environment secrets.

###### Step 1

        {
          "Logging": {
            "LogLevel": {
              "Default": "Information",
              "Microsoft": "Warning",
              "Microsoft.Hosting.Lifetime": "Information"
            }
          },
          "ConnectionStrings": {
            "Identity": "Server=(localdb)\\MSSQLLocalDB;Database=IdentityAppDb;Trusted_Connection=true; MultipleActiveResultSets=true"
          },
          "TokenSettings": {
            "SecurityKey": "rlyaKithd1sxz56787aq1x5rYVl6Z80ODU350mdz", // <-- UPDATE THIS
            "ValidIssuer": "IdentityApp", // <-- UPDATE THIS
            "ValidAudience": "IdentityApp_User", // <-- UPDATE THIS
            "SkewTime": "0",
            "DefaultTimeout": "20"
          },
          "AllowedOrigins": "*",
          "AllowedHosts": "*"

###### Step 2
Make sure to include this project as startup.
1. In VS right click on the solution.
2. Select properties
3. Left side -> Under Common Properties > Startup Project -> Select single (if only hosting this) or multiple if it isn't selected already. 
4. Find the project name 'SimpleIdentity' 
5. Under action select your choice. (Start allows for debugging breakpoints)

*To change the 'url' the api is hosted at you can find a quick guide by searching for Project Settings (But to get you in the direction, look under properties
when you right click the project itself)

###### Step 3
At some point you want to build the identity databases. These use microsoft identity as defaults (user, etc), it also includes a token table for tracking
current tokens and refresh times. Using entityframeworkcore.tools package allows for code first migrations, seeding, db creationg and updates.
Included is the basic migrations in the /migrations folder.
Using PackageManager Console type in:
        Update-Database -Context IdentityContext -StartupProject NameOfStartUpProject
(Be sure to have installed the entityframeworkcore.design nuget package to your startup project)

#### Tips
You can use the user model from SimpleIdentity or you can grab the identity user's id, store it in your own user's table as an 'externalId' or 'identityId'. This
allows you to pull information from identity api for a particular user.

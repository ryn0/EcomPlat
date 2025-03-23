Add-Migration MIGRATIONNAME -Project EcomPlat.Data 

dotnet ef database update --context ApplicationDbContext --project EcomPlat.Data\EcomPlat.Data.csproj

---

session cache

dotnet tool install --global dotnet-sql-cache
dotnet sql-cache create "<connection-string>" dbo SessionCache

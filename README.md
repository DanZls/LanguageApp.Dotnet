# Local container run

docker compose up --build   # start
docker compose down         # stop & remove


# Database update

dotnet ef migrations add <migration_name>
dotnet ef database update
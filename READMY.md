# запустить докер файла локально
```
cd /Users/skripach.cpp/RiderProjects/NewsFeedMonorepo/NewsFeedApi
docker build -f Web/Dockerfile -t newsfeed-api .

docker run -d \
  -p 8080:8080 \
  -p 8081:8081 \
  -e ASPNETCORE_URLS="http://+:8080" \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Port=5432;Database=NewsFeed;Username=postgres;Password=password;" \
  --name newsfeed-api \
  newsfeed-api
```

# создать миграцию
dotnet ef migrations add <ИмяМиграции> --project Infrastructure --startup-project Web

# откатить 1 миграцию назад
dotnet ef database update InitialCreate --project Infrastructure --startup-project Web

# автоисправление ошибок
dotnet format    
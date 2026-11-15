FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

COPY CodeWithMixx.API/CodeWithMixx.API.csproj CodeWithMixx.API/
RUN dotnet restore CodeWithMixx.API/CodeWithMixx.API.csproj

COPY . .
RUN dotnet publish CodeWithMixx.API/CodeWithMixx.API.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build --chown=app:app /out .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER app

CMD [ "dotnet", "CodeWithMixx.API.dll" ]

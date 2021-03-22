#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["./samples/VBoLu.AspNetCore.Authentication/VBoLu.AspNetCore.Authentication.csproj", "samples/VBoLu.AspNetCore.Authentication/"]
COPY ["./src/VBoLu.AspNetCore.Authentication.DingDing/VBoLu.AspNetCore.Authentication.DingDing.csproj", "src/VBoLu.AspNetCore.Authentication.DingDing/"]
RUN dotnet restore "samples/VBoLu.AspNetCore.Authentication/VBoLu.AspNetCore.Authentication.csproj"
COPY . .
WORKDIR "/src/samples/VBoLu.AspNetCore.Authentication"
RUN dotnet build "VBoLu.AspNetCore.Authentication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VBoLu.AspNetCore.Authentication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VBoLu.AspNetCore.Authentication.dll"]
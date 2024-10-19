# استخدام الصورة الأساسية لـ ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# استخدام صورة تطوير .NET
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["MyApiProject/MyApiProject.csproj", "MyApiProject/"]
RUN dotnet restore "MyApiProject/MyApiProject.csproj"
COPY . .
WORKDIR "/src/MyApiProject"
RUN dotnet build "MyApiProject.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApiProject.csproj" -c Release -o /app/publish

# نسخة التشغيل
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyApiProject.dll"]

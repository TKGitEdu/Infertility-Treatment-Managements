# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Đường dẫn này đã đúng, giữ nguyên
COPY ["Infertility-Treatment-Managements/Infertility-Treatment-Managements.csproj", "Infertility-Treatment-Managements/"]
RUN dotnet restore "./Infertility-Treatment-Managements/Infertility-Treatment-Managements.csproj"
COPY . .
WORKDIR "/src/Infertility-Treatment-Managements"
RUN dotnet build "./Infertility-Treatment-Managements.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Giai đoạn publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Infertility-Treatment-Managements.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Giai đoạn runtime - điều chỉnh cho Render
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Thiết lập biến môi trường cần thiết cho Render
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Mở cổng mà Render sẽ sử dụng
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Infertility-Treatment-Managements.dll"]
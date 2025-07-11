# Giai đoạn build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Sửa đường dẫn csproj để phù hợp với cấu trúc thư mục thực tế
COPY ["Infertility-Treatment-Managements.csproj", "./"]
RUN dotnet restore "./Infertility-Treatment-Managements.csproj"

# Sao chép toàn bộ mã nguồn
COPY . .
RUN dotnet build "Infertility-Treatment-Managements.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Giai đoạn publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Infertility-Treatment-Managements.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Giai đoạn runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Thiết lập biến môi trường
ENV ASPNETCORE_ENVIRONMENT=Production

# Mở cổng
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Infertility-Treatment-Managements.dll"]
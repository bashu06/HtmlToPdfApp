FROM mcr.microsoft.com/dotnet/aspnet:3.1-focal AS base
WORKDIR /app
EXPOSE 80

# Install dependencies for wkhtmltopdf
RUN apt-get update && apt-get install -y \
    libjpeg8 \
    libpng16-16 \
    libxrender1 \
    libfontconfig1 \
    libxext6 \
    libx11-6 \
    fontconfig \
    xfonts-base \
    xfonts-75dpi \
    libfreetype6 \
    libssl1.1 \
    && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:3.1-focal AS build
WORKDIR /src
COPY ["HtmlToPdfApp.csproj", "./"]
RUN dotnet restore "./HtmlToPdfApp.csproj"
COPY . .
RUN dotnet build "HtmlToPdfApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HtmlToPdfApp.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY libwkhtmltox.so .
RUN chmod +x libwkhtmltox.so
ENTRYPOINT ["dotnet", "HtmlToPdfApp.dll"]
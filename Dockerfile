# Use the standard Microsoft .NET Core container
FROM microsoft/dotnet
 
# Copy our code from the "/src/MyWebApi" folder to the "/app" folder in our container
WORKDIR /app
COPY . .

 
# Expose port 80 for the Web API traffic
ENV ASPNETCORE_URLS http://*:5000
EXPOSE 5000
 
# Restore the necessary packages
RUN dotnet restore WebAPIApplication.csproj
 
# Build and run the dotnet application from within the container
ENTRYPOINT ["dotnet", "run"]
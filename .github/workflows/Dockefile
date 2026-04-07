FROM ://microsoft.com AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o out

FROM ://microsoft.com
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "ScheduleBotik.dll"]

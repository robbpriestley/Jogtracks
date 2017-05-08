FROM microsoft/dotnet:latest

# This technique speeds up the build by decoupling the restore from any basic code changes.
# http://blogs.msmvps.com/theproblemsolver/2016/03/01/turbocharging-docker-build/

COPY ./SPA_Template.csproj ./SPA_Template/
WORKDIR ./SPA_Template/
RUN dotnet restore
COPY . /SPA_Template/
RUN dotnet build

EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "run"]
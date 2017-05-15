FROM microsoft/dotnet:latest

# This technique speeds up the build by decoupling the restore from any basic code changes.
# http://blogs.msmvps.com/theproblemsolver/2016/03/01/turbocharging-docker-build/

COPY ./Jogtracks.csproj ./Jogtracks/
WORKDIR ./Jogtracks/
RUN dotnet restore
COPY . /Jogtracks/
RUN dotnet build

EXPOSE 5001/tcp
ENTRYPOINT ["dotnet", "run"]
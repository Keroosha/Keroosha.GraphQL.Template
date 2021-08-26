FROM mcr.microsoft.com/dotnet/sdk:5.0 as apits

WORKDIR /opt/app
COPY ./app /opt/app
RUN dotnet run --GenerateTsApi

FROM node:15-alpine as BUILD_FRONT

WORKDIR /opt/webapp
COPY ./app/webapp /opt/webapp
COPY --from=apits /opt/app/webapp/src/api.ts /opt/webapp/src/api.ts
RUN npm i && npm run dist

FROM apits as BUILD_WEB

COPY --from=BUILD_FRONT /opt/webapp/build /opt/app/webapp/build
RUN dotnet publish -r linux-x64 -c Release -p:IncludeWeb=true -o out app.csproj

FROM mcr.microsoft.com/dotnet/aspnet:3.1-bionic as FINAL_WEB

RUN mkdir -v /var/blob /var/shared &&\
    apt update && apt install -y wget &&\
     wget -O wkhtml.deb\
      https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6-1/wkhtmltox_0.12.6-1.bionic_amd64.deb &&\
      apt install -y ./wkhtml.deb && rm wkhtml.deb && apt-get clean

COPY --from=BUILD_WEB /opt/app/out /opt/app

WORKDIR /opt/app

VOLUME /data
VOLUME /var/blob
VOLUME /var/shared

EXPOSE 12321

ENTRYPOINT ./Keroosha.GraphQL.Web
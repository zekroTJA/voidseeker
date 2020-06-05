# voidseeker

Self-hosted image storage, management, indexing and searching utility. Built with ASP.NET Core and React on top of Elasticsearch and Minio.

> This project is currently in a `work-in-progerss` state, so there is a lot of unfinished stuff. ;)

## Techniques

voidseeker is split into two main components: a **back end** and a **front end** server.

### Back End

The back end, created with ASP.NET Core, exposes a REST API which is accessed by the front end client to get information about images and users and commands to manage them. Therefore, the backend connects to a [Elasticsearch](https://www.elastic.co) database instance, which can also be hostet on a different machine. This database holds the information about users and images and provides a fast endpoints for an efficient and complex search over image meta data. Also, the backend acts like a gateway to [Minio](https://min.io), which is a self-hosted object storage. The minio client also allows to connect to more common object storages like Amazon S3 or Google Cloud Storage.

### Front End

The front end is a React SPA *(Single Page Application)*, which connects to the REST API of the back end. The compiled front end files are served using Nginx and are not served directly by the back end to prevent putting the load of providing static front end files to the back end.

## Setup & Hosting

## Docker

You can use the provided [**docker-compose.yml**](docker-compose.yml) to set all all nessecary parts to rune voidsearch on one single server. This contains the following instances:
- **nginx**  
  *Reverse proxy which provides one single endpoint for the backend and frontend.*
- **elasticsearch**  
  *Elasticsearch database instance sotring data of users, images and settings.*
- **minio**  
  *Minio object storage instance for storing images and thumbnails.*
- **voidsearch-backend**  
  *voidsearch backend providing the REST API.*
- **voidsearch-frontend**  
  *voidsearch frontend static file host.*

The following image tags are available over [docker hub](https://hub.docker.com) and automatically compiled by GitHub Actions:
- [zekro/voidseeker-backend:latest](https://hub.docker.com/r/zekro/voidseeker-backend/tags)
- [zekro/voidseeker-frontend:latest](https://hub.docker.com/r/zekro/voidseeker-frontend/tags)

## Self-Compile

Clone the repository using git.
```
$ git clone https://github.com/zekroTJA/voidseeker.git .
```

### Back End

Use the [.NET Core SDK](https://dotnet.microsoft.com/download) to compile the RESTAPI project. `<RID>` is the runtime identifier for the target platform you want to build the executable. [Here](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog) you can find a list of valid RIDs.
```
$ dotnet publish RESTAPI -c Release -r <RID>
```
The executable and libraries are located at `voidseeker/RESTAPI/bin/Release/netcoreapp3.1/win10-x64/publish/`.

### Front End

To build the front end, you need to have [Node.js](https://nodejs.org/en/download/) and [npm](https://docs.npmjs.com/downloading-and-installing-node-js-and-npm) installed.

First of all, change directory to `WebApp`.
```
$ cd WebApp
```

After that, install the required node dependencies.
```
$ npm install
```

Then, use the provided `build` script to build the production assets.
```
$ npm run build
```
After that, compiled files are located at `voidseeker/WebApp/build/` and can now be uploaded to a web server which serves the SPA files.

---

© 2020 Ringo Hoffmann (zekro Development)  
Covered by the MIT Licence.

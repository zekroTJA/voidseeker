# voidseeker

Self-hosted image storage, management, indexing and searching utility. Built with ASP.NET and Reaact on top of Elasticsearch and Minio.

> This project is currently in a `work-in-progerss` state, so there is a lot of unfinished stuff. ;)

## Techniques

voidseeker is split into two main components: a **back end** and **front end** server.

### Back End

The back end, created with ASP.NET Core, exposes a REST API which is accessed by the front end client to get information about images and users and commands to manage them. Therefore, the backend connects to a [Elasticsearch](https://www.elastic.co) database instance, which can also be hostet on a different machine. This database holds the information about users and images and provides fast endpoints for an efficient and complex search over image meta data. Also, the backend acts like a gateway to [Minio](https://min.io), which is a self-hosted object storage. The minio client also allows to connect to more common object storages like Amazon S3 or Google Cloud Storage.

### Front End

The front end is a React SPA *(Single Page Application)*, which connects to the REST API of the back end. The compiled front end files are served using Nginx and are not served directly by the back end to prevent putting the load of providing static front end files to the back end.

## Setup & Hosting

> *soon*

---

© 2020 Ringo Hoffmann (zekro Development)  
Covered by the MIT Licence.

### Overview
**Jogtracks** is a web application that tracks jogging times for users.

This project a **Single Page Application** (SPA). Specifically, it is an **ASP.NET Core MVC** application that can serve a **REST API** as well as a single web page view.

### Permission Levels
There are three permission levels: user, manager, and administrator. User and manager are directly associated with **Jogger** and **Coach** users respectively.

##### Jogger
A jogger has **user permissions** and can...

##### Coach
A coach has **manager permissions** and can...

##### Administrator
An administrator can... 

### Appendix 1: Technology
**Technologies Used**

* [Visual Studio Code](https://code.visualstudio.com/)
* [.NET Core](https://www.microsoft.com/net/core)
* [TypeScript](https://www.typescriptlang.org/)
* [Handlebars](http://handlebarsjs.com/)
* [jQuery](https://jquery.com/)
* [jQuery Validation Plugin](https://jqueryvalidation.org/)
* [Docker](https://www.docker.com/)
* [Amazon Web Services](https://aws.amazon.com/)
* [Microsoft SQL Server (MSSQL)](https://www.microsoft.com/en-us/sql-server/sql-server-2016)
* [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

**Server Technology**

The project is compatible with cloud infrastructure and uses **Docker** to encapsulate the application in a container which is then hosted on an **Amazon Web Services** (AWS) **Ubuntu Linux** server. The **MSSQL** database is hosted on **Amazon RDS**.

**HTTPS Requirement**

Please keep in mind that in a true production environment, the web application would to be served via HTTPS. Otherwise usernames, passwords, and other data would be transmitted from the client to the server in plaintext. As this project is currently for demonstration purposes only, the application is not served via HTTPS.

**Project Origins**

This project evolved from several earlier projects I developed and certain tutorials I completed. The project incorporates numerous proven component elements derived from these earlier projects. The result is a reasonably standardized format suitable for use as a template.

**References**

* Password hashing in ASP.NET Core: [https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing)

**Attribution**

* Certain basic SPA aspects of the project were originally derived from an excellent tutorial on **tutorialzine**: [http://tutorialzine.com/2015/02/single-page-app-without-a-framework/](http://tutorialzine.com/2015/02/single-page-app-without-a-framework/).
* Form styling courtesy of **Sanwebe**: [https://www.sanwebe.com/2014/08/css-html-forms-designs](https://www.sanwebe.com/2014/08/css-html-forms-designs)

### Appendix 2: Test Cases

**Sign Up**

Sign up with both **jogger** and **coach** account types.

* Browser **localSettings** should contain token, accountType, and coach.
* **localSettings** coach should be null.
* **Settings** mode should show an empty coach select control as no coach has been selected yet.

**Sign In**

* Browser **localSettings** should contain token, accountType, and coach.
* **localSettings** coach should be null, or set to specified value.
* **Settings** mode should show an appropriate coach select control depending on whether a coach is already selected or not.
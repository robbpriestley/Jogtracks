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
* [jQuery Select2](https://select2.github.io/)
* [jQuery Datepicker](https://jqueryui.com/datepicker/)
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

### Appendix 2: Testing Notes

**Sign Up**

* Sign up with both **jogger** and **coach** account types. Only one account type can be selected at a time during a sign up scenario.
* Test **form validation** is working, and ensure the form is cleared when switching between authentication modes. There should be no residual form validation messages. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for username.
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
  * Password and confirm password must match.
* After successful sign up:
  * A "Welcome, user!" message should appear in the header. 
  * The **Sign Up** and **Sign In** buttons should be replaced with **Settings** and **Sign Out** buttons.
  * The **jogs** page should be displayed.
  * The browser **localSettings** should contain token, accountType, coach, token, and userName fields.

**Sign In**

* Test **form validation** is working, and ensure the form is cleared when switching between authentication modes. There should be no residual form validation messages. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for username.
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
* After successful sign in:
  * A "Welcome, user!" message should appear in the header. 
  * The **Sign Up** and **Sign In** buttons should be replaced with **Settings** and **Sign Out** buttons.
  * The **jogs** page should be displayed.
  * The browser **localSettings** should contain token, accountType, coach, token, and userName fields.

**Settings**

* Account Type should be determined by user.
* **Coach** select control group:
  * The control group should be hidden for **COACH** and **ADMIN** user account types as it is irrelevant.
  * The control group should appear and be properly populated with any previously-selected coach if the user is a **JOGGER** user account type.
  * If the user is a **JOGGER**, the coach select control should populate on activation via AJAX call to the server with a list of valid coaches. The user should then be able to select a coach and persist the selection using the **Save Changes** button. Clicking the button causes a confirmation message to be displayed. Message fades out after 3 seconds.
* **Password reset** control group should have good form validation and should function to change the password, with a confirmation message being displayed on **Submit** button click. Message fades out after 3 seconds. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
  * Password and confirm password must match.
* **Done** button should cause **jogs** page to be displayed regardless of whether any changes took place or not.

**Jogs**

* The jogs list should load, ordered first by date descending, then by username.

**Date Filters**

* On the jogs page, the filter date controls should display datepickers that appear on click in the field.
* The **Refresh** button should parse the dates obtained from the datepickers and place correctly-formatted dates into a JSON argument in the URL. The URL will also be changed to **#filter**, which forces a page load. The page that is loaded is the same as the **Jogs** page, but includes only filtered jog results.
* Error control:
  * The user should not be able to defeat the date filter by direct manipulation of the datepickers or by direct manipulation of the JSON filter argument in the URL.
  * If incorrect dates, date ranges, or date filter formats are detected, the user will be directed to the **Error** page, and an appropriate error message will be presented.
* The browser should retain a **history** of the previous date filters, allowing use of the browser's back and forward buttons to navigate through the history.

**Sign Out**

* All form fields should be cleared and all form validation messages removed.
* All **localSettings** values should be cleared.

**Error**

* The error page should show an appropriate error message and a **Back** button.
* If there is no actual error (the user directly navigated to the page using the URL), no error message will be shown, but the Back button still will be shown.
* If the Back button is clicked, the **Jogs** page will be loaded, and the error message will be cleared.
### Overview
**Jogtracks** is a web application that tracks jogging times for users. It allows users to CRUD (create, read, update, and delete) jog data records. Each jog record consists of date, distance, time (duration), and average speed.

Jog records are displayed to the user in a view and can be filtered by date range. A **report** subsection of the view details average distance, time, and speed for all records currently displayed by the filter.

Ordinary **jogger** users can be managed by **coach** users. A coach user has the ability to CRUD user records. Also, special **admin** users can CRUD all jog records in the system as well as user records.

### Functional Demo

* Visit the functional demo application at [http://www.digitalwizardry.ca:5001](http://www.digitalwizardry.ca:5001)
* Log in with admin user **Juan** and password **qqqqqqqq**

### Permission Levels
There are three permission levels: user, manager, and administrator.

**Jogger**: 

* Has **user permissions** 
* Can view and CRUD their own jog records

**Coach**:

* Has **manager permissions**
* Can view all jog records
* Can CRUD all user records

**Administrator**: 

* Has **superuser permissions**
* Can CRUD all jog records
* Can CRUD all user records

### Technical Overview

This project a **Single Page Application** (SPA) and does not perform postbacks to the server. Although the source code may refer to "pages" being loaded, these are more like "views" within the single actual page that comprises the entire web application.

Specifically, the project an **ASP.NET Core MVC** application written in **C#** that serves both a single web page view loaded with template HTML, as well as a **REST API**. In addition to the ASP.NET Core element of the project is a TypeScript file that "transpiles" to the working JavaScript powering the in-browser interactive elements and AJAX. Heavy reliance on **jQuery** will be noted, as well as the support of several supplemental jQuery libraries, and **Handlebars** templating.

* Web page view: `/Views/Index/Index.cshtml`
* REST API: `/Controllers/ApiController.cs`
* TypeScript file: `/wwwroot/js/script.ts`

The application is responsive to a degree and will re-position elements dynamically as the browser window changes size. It doesn't look terrible on extremely small screens such as with smartphones, however additional responsive work would be warranted. The optimal window size is up to approximately 1600 x 1000 pixels.

In server terms, the project is compatible with cloud infrastructure and uses **Docker** to encapsulate the application in a container which is then hosted on an **Amazon Web Services** (AWS) **Ubuntu Linux** server. The use of .NET Core allows this sort of cross-platform compatibility.

The **Microsoft SQL Server Express** database is hosted on **Amazon RDS**. The ASP.NET application code exclusively uses code-first **Entity Framework Core** to communicate with the database. The data model is simple and corresponds to only 3 tables in the database:

* **Accounts** (stores user information)
* **Jogs** (stores jog records)
* **ServiceLog** (keeps a running log of activity in response to REST calls)

### REST API

The user interface web page view performs actions that interact with the server by calling **REST** endpoints in the **API**. As such, functional tests and other automations can be performed by scripting commands that call the REST endpoints directly. Additionally, a REST client like [Postman](https://www.getpostman.com/) can be used to call endpoints in the REST API.

The REST endpoints are as follows:

* GET /api/jog
* GET /api/jogs
* GET /api/jogs/filter
* GET /api/jogs/total
* POST /api/jog
* PUT /api/jog
* DELETE /api/jog
* GET /api/accounts
* GET /api/coaches
* PATCH /api/account
* DELETE /api/account
* POST /api/auth/signup
* POST /api/auth/signin
* POST /api/auth/changepassword

Here is a example REST call using cURL. Note the use of **basic authentication** and the query string containing a valid **token**. For more information on how this works, see the [Security Model](#security-model) section below. In order for this to work, a currently valid token would need to be supplied in the token parameter position.

`curl -s -u g9CZRkDEC5x8vfr96HMvkR3oiEiPLW:ECepRGahbgUCnwH5rCC7Xk3fdkBCKu --request GET http://www.digitalwizardry.ca:5001/api/jogs?token=8239B176-1F3D-4172-A8BD-AA6CF293A753`

### Test Data

A test data set was created involving some manual setup and a volume of randomly generated jog record data that is intended to simulate real-world data closely. The data is input using a shell script that invokes cURL to call the REST API. That script is named **REST_INPUT.sh** and is included with the source code.

There are 7 user accounts, each of which has been associated with between 10 to 50 randomized jog records. As well, each has the same password: **qqqqqqqq**

* **Juan**: Admin
* **Ivan**: Coach
* **Lisa**: Coach
* **Emily**: Jogger
* **James**: Jogger
* **Robb**: Jogger
* **Zoe**: Jogger

Of course, it is also possible to use the application to create your own test data as desired.

### Security Model

Although this application doesn't exactly have industrial-strength security and authentication as we would expect from a true production system, it does have a number of security features that would make it difficult for an attacker to perform unwanted actions.

When a user authenticates, they are issued a token, which is saved to the browser's local settings. It is this token, which would be very difficult for an attacker to guess, that is used to identify the user to the server. Tokens also have a limited life span and are discarded from one session to the next.

Also, the REST API is protected by basic authentication, which would prevent a casual attack on the API endpoints. As well the credentials for the database and other sensitive items of data are stored in a file, secrets.json, which is not stored in version control and is thus not openly readable.

Additionally, account passwords are not stored in the database, rather salts and hashes are. Native .NET cryptography modules handle the hash generation.

Please keep in mind that in a true production environment, the web application would be served via HTTPS. Otherwise usernames, passwords, and other data would be transmitted from the client to the server in plaintext. As this project is currently for demonstration purposes only, the application is not served via HTTPS.

A full security audit would be warranted if this application were to become a full-fledged production system that aimed to protect user data and prevent unauthorized access. This application has at least one vulnerability that would be fairly easy for an attacker to exploit (jog update requires a numeric integer ID value which could be guessed).

In addition to an audit, reasonable next steps to improve security could include implementing [JWT](https://jwt.io/), although in the area of web security there are many considerations and options including third-party services such as [auth0](https://auth0.com/).

### Project Origins and Next Steps

This project evolved from numerous earlier projects I developed and certain tutorials I completed. The project incorporates numerous proven component elements derived from these earlier projects. The result is a reasonably standardized format suitable for use as a template.

This project was completed in only a few days and as such there are numerous ways it could be improved. These include styling for increased responsive rendering on small devices, sorting the jogs list, improved logging, hiding the TypeScript source code, and minifying the JavaScript output code.

### Appendix 1: Inventory of Technologies Used

* [Visual Studio Code](https://code.visualstudio.com/)
* [C# .NET Core](https://www.microsoft.com/net/core)
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

### Appendix 2: References and Attribution

* Password hashing in ASP.NET Core: [https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing)
* Certain basic SPA aspects of the project were originally derived from an excellent tutorial on **tutorialzine**: [http://tutorialzine.com/2015/02/single-page-app-without-a-framework/](http://tutorialzine.com/2015/02/single-page-app-without-a-framework/).
* Form styling courtesy of **Sanwebe**: [https://www.sanwebe.com/2014/08/css-html-forms-designs](https://www.sanwebe.com/2014/08/css-html-forms-designs)

### Appendix 3: Testing Notes

**Sign Up**

* Sign up with both **jogger** and **coach** account types. Only one account type can be selected at a time during a sign up scenario.
* Test **form validation** is working, and ensure the form is cleared when switching between authentication modes. There should be no residual form validation messages. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for username.
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
  * Password and confirm password must match.
  * If the user chooses a username that has already been taken, the validation message "Sorry, that username is already taken" is displayed.
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

**Accounts**

* Accounts button in header menu is only visible to **coach** and **admin** account types.
* The **Add Account** control should function more-or-less identically to the **Sign Up** control (see above) with the exception that on successful add, the page does not change and a success message is shown.
* The **Update Account Type** control should cause the account type selection determined by the radio buttons to update the user account record. A user must be selected. Success message fades out after 3 seconds.
* The **Change Password** control should have good form validation and should function to change the password, with a confirmation message being displayed on **Submit** button click. A user must be selected. Message fades out after 3 seconds. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
  * Password and confirm password must match.
* The **Delete Account** control should allow a user and all the user's associated jog records to be removed from the database. A user must be selected.

**Settings**

* **Account Type** should be determined by user.
* **Coach** select control group:
  * The control group should be hidden for **coach** and **admin** user account types as it is irrelevant.
  * The control group should appear and be properly populated with any previously-selected coach if the user is a **jogger** user account type.
  * If the user is a **jogger**, the coach select control should populate on activation via AJAX call to the server with a list of valid coaches. The user should then be able to select a coach and persist the selection using the **Save Changes** button. Clicking the button causes a confirmation message to be displayed. Message fades out after 3 seconds.
* **Change Password** control group should have good form validation and should function to change the password, with a confirmation message being displayed on **Submit** button click. Message fades out after 3 seconds. Form validation criteria are as follows:
  * Only letters, numbers, and underscores for password.
  * Minimum 8 characters for password.
  * Password and confirm password must match.
* **Done** button should cause **jogs** page to be displayed regardless of whether any changes took place or not.

**Jogs List Page**

* On this page, any applicable jog records should load, ordered first by date descending, then by username.
* If in the case of a coach or admin, more than one user's jogs are listed at once.
* Each of the jog records is clickable so the jog record can be viewed, edited, or deleted.
* At the top of the page, the message "Showing X of Y jogs" should display valid values.

**Jogs List Page: Date Filters**

* A date must be entered for the **From Date** and the **To Date** using either the datepicker control or entered manually in YYYY-MM-DD format.
* The **Refresh** button should parse the dates and place them into a JSON argument in the URL. The URL will also be changed to **#filter**, which forces a page load. The loaded page looks the same as the **Jogs** page, but includes only filtered jog results in the jogs list.
* The **Clear** button removes any currently-active date filter.
* Error control:
  * The user should not be able to defeat the date filter by direct manipulation of the datepickers or by direct manipulation of the JSON filter argument in the URL.
  * If incorrect dates, date ranges, or date filter formats are detected, the user will be directed to the **Error** page, and an appropriate error message will be presented.

**Jogs List Page: Report Section**

* The **Report** section on the jogs list page is context-sensitive and will be updated any time the jogs list changes. It shows, for the set of jogs in the list, the **Average Distance**, **Average Time**, and **Average Speed**.

**Jog Create**

* On the jogs list page, the **New Jog** button, when clicked, causes the jog record editor to be displayed.
* The user can enter new jog data and click **Create** to create a new jog record, or click **Cancel** to return to the previous page.
* The new jog record will display in the jog list unless the date filters are active and the date of the new jog is outside the date filter range.

**Jog Update**

* On the jogs list page, when the user clicks on a jog record, the jog record editor is displayed containing the jog record data.
* The user can enter new jog data and click **Update** to update the jog, or click **Delete** to remove the jog, or click **Cancel** to return to the previous page.
* An updated jog record will be displayed in the jog list. A deleted jog record will be removed from the jog list.
 
**Jog Record Editor: User Select**

* The user select control is hidden for **jogger** account types and displayed for **coach** and **admin** account types, as the latter account types are capable of creating and editing jogs records for users other than themselves.
* The user select control on the jog record editor dynamically loads a list of valid usernames determined for the current context (by operating user and account type).

**Jog Record Editor: Validations**

* If the operating user is not a **jogger** account type, the user must select a username in the **User** select control.
* A **Date** must be selected from the datepicker or entered manually in YYYY-MM-DD format.
* A numeric **Distance** must be entered. The distance can either be an integer or a decimal number, but cannot include text or special characters.
* A **Time** must be entered in HH:MM:SS format. The maximum time in a given day is 23:59:59. Each component of the time must be zero or a positive integer.

**Sign Out**

* All form fields should be cleared and all form validation messages removed.
* All **localSettings** values should be cleared.
* The user will be directed to the front page.

**Error**

* The error page should show an appropriate error message and a **Back** button.
* If there is no actual error (the user directly navigated to the page using the URL), no error message will be shown, but the Back button still will be shown.
* If the Back button is clicked, the **Jogs** page will be loaded, and the error message will be cleared.

**Browser History**

* The browser should retain a **history** of the previous pages, allowing use of the browser's back and forward buttons to navigate through the history. None of these should postback to the server.
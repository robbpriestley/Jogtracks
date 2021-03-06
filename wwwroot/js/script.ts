"use strict";

declare var Handlebars: any;

// *** BEGIN INTERFACES ***

interface IDictionary 
{
	[key: string]: string;
};

interface JQueryStatic
{
	validator: any;
};

interface IJogData
{
	UserName: string;
	UserColor: string;
	Date: string;
	Year: number;
	Month: number;
	Day: number;
	Week: number;
	Distance: number;
	Time: number;
	TimeString: string;
	AverageSpeed: number;
}

// *** BEGIN INTERFACES ***
// *** BEGIN CLASS DEFINITIONS ***

class JogData implements IJogData 
{
	Id: number;
	UserName: string;
	UserColor: string;
	Date: string;
	Year: number;
	Month: number;
	Day: number;
	Week: number;
	Distance: number;
	Time: number;
	TimeString: string;
	AverageSpeed: number;

	constructor(id: number, userName: string, userColor: string, date: string, year: number, month: number, day: number, week: number, distance: number, time: number, timeString: string, averageSpeed: number)
	{
		this.Id = id;
		this.UserName = userName;
		this.UserColor = userColor;
		this.Date = date;
		this.Year = year;
		this.Month = month;
		this.Day = day;
		this.Week = week;
		this.Distance = distance;
		this.Time = time;
		this.TimeString = timeString;
		this.AverageSpeed = averageSpeed;
	}
}

// *** END CLASS DEFINITIONS ***

$(document).ready(function() 
{
	// *** BEGIN SPA ***
	// *** GLOBAL VARIABLES ***

	let Jogs: Array<JogData> = [];
	let Filter: IDictionary = {};
	let BasicAuth: object = {"Authorization": "Basic " + btoa("g9CZRkDEC5x8vfr96HMvkR3oiEiPLW" + ":" + "ECepRGahbgUCnwH5rCC7Xk3fdkBCKu")};

	// *** BEGIN EVENT HANDLERS ***

	// Event handler calls the render function on every hashchange.
	$(window).on("hashchange", function()
	{
		Render(decodeURI(window.location.hash));
	});

	$("#signup").click(function(e) 
	{
		SignOut();
		$("#confirm").show();
		$("#authMessage").text("Create an account");
		window.location.hash = "#signup";
		return false;
	});
	
	$("#signin").click(function(e) 
	{
		SignOut();
		$("#confirm").hide();
		$("#authMessage").text("Sign in to your account");
		window.location.hash = "#signin";
		return false;
	});
	
	$("#settings").click(function(e) 
	{
		window.scrollTo(0, 0);
		Jogs = [];
		Settings();
		return false;
	});

	$("#settingsDone").click(function(e) 
	{
		window.scrollTo(0, 0);
		window.location.hash = "#jogs";
		return false;
	});

	$("#accounts").click(function(e) 
	{
		window.scrollTo(0, 0);
		Accounts();
		return false;
	});

	$("#accountsDone").click(function(e) 
	{
		window.scrollTo(0, 0);
		window.location.hash = "#jogs";
		return false;
	});
	
	$("#signout").click(function(e) 
	{
		window.scrollTo(0, 0);
		Jogs = [];
		SignOut();
		window.location.hash = "#";  // Show welcome page.
		return false;
	});

	$("#username").on("change keyup keydown click paste", function()
	{
		$("#username-taken").css("display", "none");
	});

	$("#aausername").on("change keyup keydown click paste", function()
	{
		$("#aausername-taken").css("display", "none");
	});

	InitializeSelectControls();

	$("#userSelectControl").on("change", (function(e) 
	{
		$("#userSelectLabel").css("display", "none");
	}));

	$("#updateAccountSelectControl").on("change", (function(e) 
	{
		$("#updateAccount-select").css("display", "none");
	}));

	$("#cpAccountSelectControl").on("change", (function(e) 
	{
		$("#cpAccount-select").css("display", "none");
	}));

	$("#deleteAccountSelectControl").on("change", (function(e) 
	{
		$("#deleteAccount-select").css("display", "none");
	}));

	$("#fromDate").datepicker
	({
		dateFormat: "yy-mm-dd",
		changeMonth: true,
		changeYear: true,
		minDate: "-100Y",
		maxDate: 0,
		yearRange: "-100:+nn"
	});

	$("#toDate").datepicker
	({
		dateFormat: "yy-mm-dd",
		changeMonth: true,
		changeYear: true,
		minDate: "-100Y",
		maxDate: 0,
		yearRange: "-100:+nn"
	});

	$("#updateDate").datepicker
	({
		dateFormat: "yy-mm-dd",
		changeMonth: true,
		changeYear: true,
		minDate: "-100Y",
		maxDate: 0,
		yearRange: "-100:+nn"
	});

	$("#filterRefresh").click(function(e) 
	{
		let fromString: string = $("#fromDate").val();
		let toString: string = $("#toDate").val();

		try
		{
			if (fromString == "" || toString == "")
			{
				throw "Filter error: please select both <strong>From Date</strong> and <strong>To Date</strong>.";
			}
		
			let fromDate = ParseDate(fromString);
			let toDate = ParseDate(toString);

			if (fromDate > toDate)
			{
				throw "Filter error: the <strong>From Date</strong> must be the same as, or before, the <strong>From Date</strong>.";
			}

			Filter["FromDate"] = fromDate.toISOString();
			Filter["ToDate"] = toDate.toISOString();

			window.location.hash = "#filter/" + JSON.stringify(Filter);
		}
		catch (error) 
		{
			// Not valid dates or date format in datepicker(s), show error.
			sessionStorage.setItem("errorMessage", error);
			window.location.hash = "#error";
			return;
		}
	});

	$("#filterClear").click(function(e) 
	{
		$("#fromDate").val("");
		$("#toDate").val("");
		Filter = {};
		window.location.hash = "#jogs";
	});

	$("#singleDelete").on("click", function(e) 
	{
		let url: string = decodeURI(window.location.hash);
		let jogId: number = Number(url.split("#jog/")[1].trim());
		DeleteJog(jogId);
	});

	$("#singleCancel").on("click", function(e) 
	{
		LoadJogsPageOrFilter(Filter);
	});

	$("#errorBack").click(function(e) 
	{
		sessionStorage.removeItem("errorMessage");
		window.scrollTo(0, 0);
		window.location.hash = "#";
		return false;
	});

	$("#newJog").click(function(e) 
	{
		window.scrollTo(0, 0);
		window.location.hash = "#jog/0";  // The Id argument of 0 represents a new jog.
		return false;
	});

	// *** END EVENT HANDLERS ***
	// *** BEGIN PAGE RENDERING ***
	
	function Render(url: string): void
	{
		let keyword: string = url.split("/")[0];                 // Get the keyword from the URL.
		$(".main-content .page").css("pointer-events", "none");  // Prevent pages from intercepting focus.
		$(".main-content .page").removeClass("visible");         // Hide the page that"s currently shown.
		ShowHeaderComponents(false);

		switch (keyword)
		{
			case "":
				RenderWelcomePage();
				break;
			
			case "#signup":
			case "#signin":
				RenderAuthPage();
				break;

			case "#accounts":
				RenderAccountsPage();
				break;

			case "#settings":
				RenderSettingsPage();
				break;
			
			case "#jogs":
				JogsPageHelper();
				break;

			case "#filter":
				FilterPageHelper(url);
				break;

			case "#jog":
				RenderSingleJogPage(url, Jogs);
				break;
			
			case "#error":
				RenderErrorPage();
				break;

			default:
				sessionStorage.setItem("errorMessage", "Unknown page or query string.");
				RenderErrorPage();  // Unknown keyword.
				break;
		}
	}

	function JogsPageHelper(): void
	{
		$("#fromDate").val("");
		$("#toDate").val("");
		Filter = {};
		sessionStorage.removeItem("errorMessage");

		$("#report").hide();

		if (localStorage.getItem("accountType") != "COACH")
		{
			$("#newJog").show();
		}
		else
		{
			$("#newJog").hide();
		}
		
		LoadJogs();
		RenderJogsPage("null", "null");
	}

	function FilterPageHelper(url: string): void
	{
		$("#report").hide();
		
		url = url.split("#filter/")[1].trim();

		let fromDate: string;
		let toDate: string;

		try
		{
			Filter = JSON.parse(url);  // Parse filter from query string.

			for (var key in Filter)
			{
				if (key != "FromDate" && key != "ToDate")
				{
					delete Filter[key];
				}
			}

			fromDate = Filter["FromDate"].split("T")[0];
			toDate = Filter["ToDate"].split("T")[0];

			$("#fromDate").val(fromDate);
			$("#toDate").val(toDate);

			ParseDate(fromDate);
			ParseDate(toDate);

			if (Object.keys(Filter).length != 2)
			{
				throw "";  // There needs to be a FromDate and a ToDate in the Filter data.
			}
		}
		catch (error) 
		{
			sessionStorage.setItem("errorMessage", "Filter error: bad filter data in URL.");
			window.location.hash = "#error";
			return;
		}

		LoadJogsWithFilter(fromDate, toDate);
		RenderJogsPage(fromDate, toDate);
	}

	function RenderWelcomePage(): void
	{
		var page = $(".welcome");
		page.addClass("visible");
		ShowHeaderComponents(true);
	}

	function RenderAuthPage(): void
	{
		var page = $(".auth");
		page.addClass("visible");
		page.css("pointer-events", "auto");
		ShowHeaderComponents(true);
	}

	function RenderAccountsPage(): void
	{
		var page = $(".accounts");
		page.addClass("visible");
		page.css("pointer-events", "auto");
		
		ShowHeaderComponents(true);
	}

	function RenderSettingsPage(): void
	{
		var page = $(".settings");
		page.addClass("visible");
		page.css("pointer-events", "auto");

		let accountType: any = localStorage.getItem("accountType");

		if (accountType == "JOGGER")
		{
			$("#accountTypeMessage").html("You are a <strong>Jogger</strong> with <strong>user-level</strong> permissions.");
		}
		else if (accountType == "COACH")
		{
			$("#accountTypeMessage").html("You are a <strong>Coach</strong> with <strong>manager-level</strong> permissions.");
		}
		else
		{
			$("#accountTypeMessage").html("You are an <strong>Administrator</strong> with <strong>superuser-level</strong> permissions.");
		}

		ShowHeaderComponents(true);
	}
	
	function RenderErrorPage(): void
	{
		let errorMessage: string | null = sessionStorage.getItem("errorMessage");
		$("#errorMessage").html(errorMessage == null ? "" : errorMessage);

		var page = $(".errorMessage");
		page.addClass("visible");
		page.css("pointer-events", "auto");
		ShowHeaderComponents(true);
	}

	function ShowHeaderComponents(show: boolean): void
	{
		if (show)
		{
			$('#logo').addClass("visible");
			$('#user').addClass("visible");
			$('#headercontrols').addClass("visible");
		}
		else
		{
			$('#logo').removeClass("visible");
			$('#user').removeClass("visible");
			$('#headercontrols').removeClass("visible");
		}
	}

	// Fill jog list using handlebars template.
	function GenerateJogsHTML(jogs: Array<JogData>): void
	{
		$("#jogsMessage1").text("Showing " + jogs.length.toString() + " of ");
		$("#jogsMessage2").show();

		if (jogs.length > 0)
		{
			CalculateAverages(jogs);
		}
		
		let jogList: JQuery = $(".all-jogs .jogs-list");
		jogList.html("");
		let template: string = $("#jogs-template").html();
		let compiledTemplate: any = Handlebars.compile(template);
		jogList.append(compiledTemplate(jogs));

		// Each jog has data index attribute. On click change the URL hash to open up a preview for
		// the jog. Every hashchange triggers the render function.
		jogList.find("li").on("click", function(e)
		{
			e.preventDefault();
			let jogId: string = $(this).data("index");
			window.location.hash = "#jog/" + jogId;
		})
	}

	function CalculateAverages(jogs: Array<JogData>): void
	{
		let totalTime: number = 0;
		let totalSpeed: number = 0;
		let totalDistance: number = 0;
		
		for (var i: number = 0; i < jogs.length; i++)
		{
			totalTime += jogs[i].Time;
			totalSpeed += jogs[i].AverageSpeed;
			totalDistance += jogs[i].Distance;
		}

		let averageTime: number = totalTime / jogs.length;
		let averageSpeed: number = totalSpeed / jogs.length;
		let averageDistance: number = totalDistance / jogs.length;
		
		$("#averageTime").text(TimeFormat(averageTime));
		$("#averageSpeed").text(averageSpeed.toFixed(2) + " km/h");
		$("#averageDistance").text(averageDistance.toFixed(2) + " km");

		$("#report").show();
	}

	function RenderJogsPage(fromDate: string, toDate: string): void
	{
		$("#dateFrom").val("");
		$("#dateTo").val("");
		
		LoadJogsTotal();

		let page: JQuery = $(".all-jogs");
		page.addClass("visible");
		ShowHeaderComponents(true);
	}

	// Opens preview page for one of the jogs. Parameters index from the hash and the jogs object.
	function RenderSingleJogPage(url: string, jogs: Array<JogData>): void
	{	
		let jogId: number = Number(url.split("#jog/")[1].trim());
		$("#updateId").val(jogId);
		
		$(".single-jog").css("pointer-events", "auto");;

		let accountType: string | null = localStorage.getItem("accountType");
		
		if (jogId == 0)  // 0 indicates a new jog should be created.
		{
			$("#updateHeading").text("Create Jog");
			$("#singleDelete").hide();
			$("#singleSubmit").val("Create");
			$("#userSelectControl").empty();
			$("#updateDate").val("");
			$("#updateDistance").val("");
			$("#updateTime").val("");
		}
		else if (jogs.length > 0)  // Otherwise, an existing jog will be updated.
		{
			$("#updateHeading").text("Update Jog");
			$("#singleSubmit").val("Update");
			$("#singleDelete").show();

			// Find the jog by iterating through the data object and searching for the chosen index.
			jogs.forEach(function(jog)
			{
				if (jog.Id == jogId)
				{
					UpdateSelectControl("userSelectControl", jog.UserName);
					$("#updateDate").val(jog.Date);
					$("#updateDistance").val(jog.Distance);
					$("#updateTime").val(jog.TimeString);
				}
			});
		}

		if (accountType == "JOGGER")
		{
			$("#userSelect").hide();
			$("#singleSubmit").show();
			SingleJogDisabled(false);
		}
		else if (accountType == "COACH")
		{
			$("#userSelect").hide();
			$("#singleSubmit").hide();
			$("#singleDelete").hide();
			SingleJogDisabled(true);
			$("#updateHeading").text("Jog View");
		}
		else  // accountType == "ADMIN"
		{
			$("#userSelect").show();
			$("#singleSubmit").show();
			SingleJogDisabled(false);
		}

		$(".single-jog").addClass("visible");  // Show the page.
	}

	function SingleJogDisabled(readOnly: boolean): void
	{
		if (readOnly)
		{
			$("#updateDate").prop("disabled", true);
			$("#updateDistance").prop("disabled", true);
			$("#updateTime").prop("disabled", true);
		}
		else
		{
			$("#updateDate").prop("disabled", false);
			$("#updateDistance").prop("disabled", false);
			$("#updateTime").prop("disabled", false);
		}
	}

	// *** END PAGE RENDERING ***
	// *** BEGIN FORM VALIDATION ***
	
	$("form[name=authForm]").validate(
	{
		rules: 
		{
			username:
			{
				required: true,
				alphanumeric:true
			},
			password:
			{
				minlength: 8,
				required: true,
				alphanumeric:true
			},
			cpassword:
			{
				required: true,
				equalTo: "#password"
			}
		},
		submitHandler: function(form: any)
		{
			let keyword: string = decodeURI(window.location.hash).split("/")[0];  // Get the keyword from the URL.
			
			let username: string = $("#username").val();
			let password: string = $("#password").val();
			
			if (keyword == "#signup")
			{
				SignUp(username, password);
			}
			else if (keyword == "#signin")
			{
				SignIn(username, password);
			}

			return false;
		}
	});

	$("form[name=addAccountForm]").validate(
	{
		rules: 
		{
			aausername:
			{
				required: true,
				alphanumeric:true
			},
			aapassword:
			{
				minlength: 8,
				required: true,
				alphanumeric:true
			},
			aacpassword:
			{
				required: true,
				equalTo: "#aapassword"
			}
		},
		submitHandler: function(form: any)
		{
			let aausername: string = $("#aausername").val();
			let aapassword: string = $("#aapassword").val();
			AddAccount(aausername, aapassword);
			return false;
		}
	});

	$("form[name=updateAccountForm]").validate(
	{
		submitHandler: function(form: any)
		{
			if ($("#updateAccountSelectControl").val() == null)
			{
				$("#updateAccount-select").text("Please select a user from the list.");
				$("#updateAccount-select").css("display", "inherit");
				return;
			}
			
			$("#updateAccount-select").css("display", "none");
			let accountType: string = $("input[name=accountType]:checked").val();
			UpdateAccountType($("#updateAccountSelectControl").val(), accountType);
			return false;
		}
	});

	$("form[name=cpAccountForm]").validate(
	{
		rules: 
		{
			aChangePassword:
			{
				minlength: 8,
				required: true,
				alphanumeric:true
			},
			acChangePassword:
			{
				required: true,
				equalTo: "#aChangePassword"
			}
		},
		submitHandler: function(form: any)
		{
			if ($("#cpAccountSelectControl").val() == null)
			{
				$("#cpAccount-select").text("Please select a user from the list.");
				$("#cpAccount-select").css("display", "inherit");
				return;
			}
			
			$("#cpAccount-select").css("display", "none");
			let password: string = $("#aChangePassword").val();
			AccountChangePassword(password, $("#cpAccountSelectControl").val());
			return false;
		}
	});

	$("form[name=deleteAccountForm]").validate(
	{
		submitHandler: function(form: any)
		{
			if ($("#deleteAccountSelectControl").val() == null)
			{
				$("#deleteAccount-select").text("Please select a user from the list.");
				$("#deleteAccount-select").css("display", "inherit");
				return;
			}
			
			$("#deleteAccount-select").css("display", "none");
			
			let account: string = $("#deleteAccountSelectControl").val();
			DeleteAccount(account);
			return false;
		}
	});

	$("form[name=changePasswordForm]").validate(
	{
		rules: 
		{
			changePassword:
			{
				minlength: 8,
				required: true,
				alphanumeric:true
			},
			changecPassword:
			{
				required: true,
				equalTo: "#changePassword"
			}
		},
		submitHandler: function(form: any)
		{
			let password: string = $("#changePassword").val();
			ChangePassword(password, localStorage.getItem("userName"));
			return false;
		}
	});

	$("form[name=jogEditForm]").validate(
	{
		rules: 
		{
			updateDate:
			{
				required: true,
				dateFormatCheck: true,
				dateYearCheck: true,
				dateMonthCheck: true,
				dateDayCheck: true,
				dateParseCheck: true
			},
			updateDistance:
			{
				required: true,
				number: true,
				min: 0,
				max: 1000
			},
			updateTime:
			{
				required: true,
				timeFormatCheck: true,
				timeNumberCheck: true,
				timeNumberIntegerCheck: true,
				timeNumberZeroCheck: true,
				timeNumberValueCheck: true
			}
		},
		submitHandler: function(form: any)
		{
			AddOrUpdateJog();
			return false;
		}
	});

	jQuery.validator.addMethod("dateFormatCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split("-");

		if (parts.length != 3)
		{
			return false;
		}
		else if (parts[0] == "" || parts[1] == "" || parts[2] == "")
		{
			return false;
		}

		return true;

	}, "Please use the date format YYYY-MM-DD.");

	jQuery.validator.addMethod("dateYearCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split("-");

		try
		{
			let yyyy: number;
			
			yyyy = Number(parts[0]);

			if (yyyy < 0 || yyyy > 3000)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}

		return true;

	}, "Year should be in the range 0 to 3000.");

	jQuery.validator.addMethod("dateMonthCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split("-");

		try
		{
			let mm: number;
			
			mm = Number(parts[1]);

			if (mm < 1 || mm > 12)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}

		return true;

	}, "Month should be in the range 01 to 12.");

	jQuery.validator.addMethod("dateDayCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split("-");

		try
		{
			let dd: number;
			
			dd = Number(parts[2]);

			if (dd < 1 || dd > 31)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}

		return true;

	}, "Day should be in the range 01 to 31.");

	jQuery.validator.addMethod("dateParseCheck", function(value: any, element: any) 
	{
		try
		{
			let parts: Array<string> = value.split("-");

			let yyyy: number, mm: number, dd: number;
			
			yyyy = Number(parts[0]);
			mm = Number(parts[1]);
			dd = Number(parts[2]);

    		let date: Date = new Date(yyyy, mm - 1, dd);

			if (date.getFullYear() != yyyy || date.getMonth() != mm - 1 || date.getDate() != dd)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}

		return true;

	}, "Please enter a valid date.");

	jQuery.validator.addMethod("timeFormatCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split(":");

		if (parts.length != 3)
		{
			return false;
		}
		else if (parts[0] == "" || parts[1] == "" || parts[2] == "")
		{
			return false;
		}
		
		return true;

	}, "Please use the time format HH:MM:SS.");

	jQuery.validator.addMethod("timeNumberCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split(":");

		if (parts.length != 3)
		{
			return false;
		}
		else if (parts[0] == "" || parts[1] == "" || parts[2] == "")
		{
			return false;
		}

		try
		{
			let hh: number, mm: number, ss: number;
			
			hh = Number(parts[0]);
			mm = Number(parts[1]);
			ss = Number(parts[2]);

			if (isNaN(hh) || isNaN(mm) || isNaN(ss))
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}
		
		return true;

	}, "Time elements must be numbers.");

	jQuery.validator.addMethod("timeNumberIntegerCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split(":");

		try
		{
			let hh: number, mm: number, ss: number;
			
			hh = Number(parts[0]);
			mm = Number(parts[1]);
			ss = Number(parts[2]);

			if 
			(
				hh !== parseInt(parts[0], 10) ||
				mm !== parseInt(parts[1], 10) ||
				ss !== parseInt(parts[2], 10)
			)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}
		
		return true;

	}, "Time elements must be whole numbers.");
	
	jQuery.validator.addMethod("timeNumberZeroCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split(":");

		try
		{
			let hh: number, mm: number, ss: number;
			
			hh = Number(parts[0]);
			mm = Number(parts[1]);
			ss = Number(parts[2]);

			if (hh < 0 || mm < 0 || ss < 0)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}
		
		return true;

	}, "Time elements must be numbers greater than zero.");

	jQuery.validator.addMethod("timeNumberValueCheck", function(value: any, element: any) 
	{
		let parts: Array<string> = value.split(":");

		try
		{
			let hh: number, mm: number, ss: number;
			
			hh = Number(parts[0]);
			mm = Number(parts[1]);
			ss = Number(parts[2]);

			if (hh > 23 || mm > 59 || ss > 59)
			{
				return false;
			}
		}
		catch (error)
		{
			return false;
		}
		
		return true;

	}, "The maximums are 23:59:59.");

	// *** END FORM VALIDATION ***
	// *** BEGIN REST AUTHENTICATION ***

	function AuthCheck(): void
	{
		let token: string | null = localStorage.getItem("token");

		if (token != null)
		{
			CheckToken(token);
			window.location.hash = "#";
			$(window).trigger("hashchange");
		}
		else
		{
			SignOut();
			window.location.hash = "#";
			$(window).trigger("hashchange");
		}
	}

	function CheckToken(token: string): void
	{
		$.ajax
		({
			url: "/api/auth",
			type: "GET",
			contentType: "application/json",
			data: { token: token },
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == "true")
				{
					Authenticated();
				}
				else
				{
					SignOut();
					window.location.hash = "#";
				}
			}
		});
	}

	function Authenticated(): void
	{		
		$("#user").text("Welcome, " + localStorage.getItem("userName") + "!");
		$("#user").show();
		$("#signup").hide();
		$("#signin").hide();
		$("#signout").show();
		$("#settings").show();

		if (localStorage.getItem("accountType") == "JOGGER")
		{
			$("#accounts").hide();
		}
		else
		{
			$("#accounts").show();
		}
	}
	
	function ServerAuthenticated(username: string, authOutput: any): void
	{
		$("#user").text("Welcome, " + username + "!");
		$("#user").show();
		$("#signup").hide();
		$("#signin").hide();
		$("#signout").show();
		$("#settings").show();

		if (authOutput.AccountType == "JOGGER")
		{
			$("#accounts").hide();
		}
		else
		{
			$("#accounts").show();
		}

		localStorage.setItem("token", authOutput.Token);
		localStorage.setItem("coach", authOutput.Coach);
		localStorage.setItem("userName", authOutput.UserName);
		localStorage.setItem("accountType", authOutput.AccountType);
		window.location.hash = "#jogs";
	}

	function SignUp(username: string, password: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/auth/signup",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify({ UserName: username, Password: password }),
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == undefined)
				{
					spinner.stop();
					$("#username-taken").text("Sorry, that username is already taken");
					$("#username-taken").css("display", "inherit");
				}
				else if (result.ValidationMessage != null)
				{
					// Server-side form validation failed.
					spinner.stop();
					sessionStorage.setItem("errorMessage", result.ValidationMessage);
					window.location.hash = "#error";				
				}
				else
				{
					spinner.stop();
					ServerAuthenticated(username, result);
				}
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function SignIn(username: string, password: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/auth/signin",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify({ UserName: username, Password: password }),
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == undefined)
				{
					spinner.stop();
					$("#username-taken").text("Bad username or password");
					$("#username-taken").css("display", "inherit");
				}
				else if (result.ValidationMessage != null)
				{
					// Server-side form validation failed.
					spinner.stop();
					sessionStorage.setItem("errorMessage", result.ValidationMessage);
					window.location.hash = "#error";				
				}
				else
				{
					spinner.stop();
					ServerAuthenticated(username, result);
				}
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function SignOut(): void
	{
		// Clear the jogs list.
		let jogList: JQuery = $(".all-jogs .jogs-list");
		jogList.html("");

		$("#user").hide();
		$("#signup").show();
		$("#signin").show();
		$("#signout").hide();
		$("#settings").hide();
		$("#accounts").hide();

		// *** BEGIN RESET FORMS ***
		
		$("#jogger").prop("checked", true);
		$("#username-taken").css("display", "none");
		$("#aausername-taken").css("display", "none");
		
		$("form[name=authForm]").validate().resetForm();
		$("form[name=jogEditForm]").validate().resetForm();
		$("form[name=changePasswordForm]").validate().resetForm();
		
		$("#username").val("");
		$("#password").val("");
		$("#cpassword").val("");
		$("#aausername").val("");
		$("#aapassword").val("");
		$("#aacpassword").val("");
		$("#changePassword").val("");
		$("#changecPassword").val("");
		
		// *** END RESET FORMS ***

		InitializeSelectControls();
		ClearStorage();

		window.location.hash = "#";
	}

	function AddAccount(username: string, password: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/auth/signup",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify({ UserName: username, Password: password }),
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == undefined)
				{
					spinner.stop();
					$("#aausername-taken").text("Sorry, that username is already taken");
					$("#aausername-taken").css("display", "inherit");
				}
				else if (result.ValidationMessage != null)
				{
					// Server-side form validation failed.
					spinner.stop();
					sessionStorage.setItem("errorMessage", result.ValidationMessage);
					window.location.hash = "#error";				
				}
				else
				{
					spinner.stop();
					$("#aausername").val("");
					$("#aapassword").val("");
					$("#aacpassword").val("");
					$("#addAccountMessage").text("Added account " + username + "!");
					$("#addAccountMessage").show();
					setTimeout(function() { $("#addAccountMessage").fadeOut(); }, 3000);					
				}
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function DeleteAccount(username: string): void
	{
		$.ajax
		({
			url: "/api/account",
			type: "DELETE",
			contentType: "application/json",
			data: JSON.stringify({ UserName: username, Token: localStorage.getItem("token") }),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				InitializeSelectControls();
				$("#deleteAccountMessage").text("Deleted account " + username + "!");
				$("#deleteAccountMessage").show();
				setTimeout(function() { $("#deleteAccountMessage").fadeOut(); }, 3000);					
			}
		});
	}

	function InitializeSelectControls(): void
	{
		$("#updateAccountSelectControl").empty();
		$("#cpAccountSelectControl").empty();
		$("#deleteAccountSelectControl").empty();
		$("#userSelectControl").empty();

		// Annoyingly, the only way I could seem to empty the select controls was to re-initialize them.
		
		$("#updateAccountSelectControl").select2({
			width: "400px",
			placeholder: "Select a user",
			dropdownCssClass : "no-search",
			ajax: 
			{
				url: "/api/accounts",
				type: "GET",
				headers: BasicAuth,
				dataType: "json",
				data: function(params: any) 
				{
					var queryParameters = { token: localStorage.getItem("token"), includeSelf: false }
					return queryParameters;
				},
				processResults: function(data: any) 
				{
					return {
						results: $.map(data, function(user) 
						{
							return {
								text: user.UserName,
								id: user.UserName
							}
						})
					};
				}
			}
		});

		$("#cpAccountSelectControl").select2({
			width: "400px",
			placeholder: "Select a user",
			dropdownCssClass : "no-search",
			ajax: 
			{
				url: "/api/accounts",
				type: "GET",
				headers: BasicAuth,
				dataType: "json",
				data: function(params: any) 
				{
					var queryParameters = { token: localStorage.getItem("token"), includeSelf: false }
					return queryParameters;
				},
				processResults: function(data: any) 
				{
					return {
						results: $.map(data, function(user) 
						{
							return {
								text: user.UserName,
								id: user.UserName
							}
						})
					};
				}
			}
		});

		$("#deleteAccountSelectControl").select2({
			width: "400px",
			placeholder: "Select a user",
			dropdownCssClass : "no-search",
			ajax: 
			{
				url: "/api/accounts",
				type: "GET",
				headers: BasicAuth,
				dataType: "json",
				data: function(params: any) 
				{
					var queryParameters = { token: localStorage.getItem("token"), includeSelf: false }
					return queryParameters;
				},
				processResults: function(data: any) 
				{
					return {
						results: $.map(data, function(user) 
						{
							return {
								text: user.UserName,
								id: user.UserName
							}
						})
					};
				}
			}
		});

		$("#userSelectControl").select2({
			width: "400px",
			placeholder: "Select a user",
			dropdownCssClass : "no-search",
			ajax: 
			{
				url: "/api/accounts",
				type: "GET",
				headers: BasicAuth,
				dataType: "json",
				data: function(params: any) 
				{
					var queryParameters = { token: localStorage.getItem("token"), includeSelf: true }
					return queryParameters;
				},
				processResults: function(data: any) 
				{
					$("#userSelectLabel").css("display", "none");
					
					return {
						results: $.map(data, function(user) 
						{
							return {
								text: user.UserName,
								id: user.UserName
							}
						})
					};
				}
			}
		});
	}

	function ClearStorage(): void
	{
		localStorage.removeItem("token");
		localStorage.removeItem("coach");
		localStorage.removeItem("accountType");
		sessionStorage.removeItem("errorMessage");
	}

	function ChangePassword(password: string, username: any): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);

		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			url: "/api/auth/changepassword",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify({ Token: token, Password: password, UserName: username }),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				$("#changePassword").val("");
				$("#changecPassword").val("");
				$("#changePasswordMessage").show();
				setTimeout(function() { $("#changePasswordMessage").fadeOut(); }, 3000);
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function AccountChangePassword(password: string, username: any): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);

		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			url: "/api/auth/changepassword",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify({ Token: token, Password: password, UserName: username }),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				$("#aChangePassword").val("");
				$("#acChangePassword").val("");
				$("#cpAccountMessage").show();
				$("#cpAccountMessage").text("Password changed for user " + username + "!");
				setTimeout(function() { $("#cpAccountMessage").fadeOut(); }, 3000);
				InitializeSelectControls();
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function UpdateAccountType(username: string, accountType: string): void
	{
		let spinner: Spinner = SpinnerSetup();	 
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/account",
			type: "PATCH",
			contentType: "application/json",
			data: JSON.stringify({ Token: localStorage.getItem("token"), UserName: username, AccountType: accountType }),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				$("#updateAccountMessage").text("Updated account " + username + " to " + accountType + "!");
				$("#updateAccountMessage").show();
				setTimeout(function() { $("#updateAccountMessage").fadeOut(); }, 3000);
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	// *** END REST AUTHENTICATION ***
	// *** BEGIN ACCOUNTS ***

	function Accounts(): void
	{
		// Clear the jogs list.
		let jogList: JQuery = $(".all-jogs .jogs-list");
		jogList.html("");

		window.location.hash = "#accounts";
	}

	// *** END ACCOUNTS ***
	// *** BEGIN SETTINGS ***

	function Settings(): void
	{
		// Clear the jogs list.
		let jogList: JQuery = $(".all-jogs .jogs-list");
		jogList.html("");

		window.location.hash = "#settings";
	}

	// *** END SETTINGS ***
	// *** BEGIN JOGS ***

	function LoadJogs(): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);

		$("#jogsMessage1").text("Loading...");
		$("#jogsMessage2").hide();

		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			url: "/api/jogs",
			type: "GET",
			contentType: "application/json",
			data: { token: token },
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				Jogs = result;
				GenerateJogsHTML(Jogs);
				spinner.stop();
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function LoadJogsWithFilter(fromDate: string, toDate: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);

		$("#jogsMessage1").text("Loading...");
		$("#jogsMessage2").hide();

		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			url: "/api/jogs/filter",
			type: "GET",
			contentType: "application/json",
			data: { fromDate: fromDate, toDate: toDate, token: token },
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				Jogs = result;
				GenerateJogsHTML(Jogs);
				spinner.stop();
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function LoadJogsTotal(): void
	{
		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			url: "/api/jogs/total",
			type: "GET",
			contentType: "application/json",
			data: { Token: token },
			dataType: "json",
			headers: BasicAuth,
			success: function(result) 
			{
				$("#jogsMessage2").text(result + " jogs.");
			}
		});
	}

	function AddOrUpdateJog(): void
	{
		let userName: string | null;
		
		if (localStorage.getItem("accountType") == "JOGGER")
		{
			// Jogger users do not see a user select control, and the userName is always their own.
			userName = localStorage.getItem("userName");
		}
		else if ($("#userSelectControl").val() == null)
		{
			// Coach and admin users are forced to select a user from the user select control.
			$("#userSelectLabel").text("Please select a user from the list.");
			$("#userSelectLabel").css("display", "inherit");
			return;
		}
		else
		{
			// For coach and admin users, obtain the userName from the user select control.
			userName = $("#userSelectControl").val();
		}

		let timeParts: Array<string> = $("#updateTime").val().split(":");

		let hh: number, mm: number, ss: number;
			
		hh = Number(timeParts[0]);
		mm = Number(timeParts[1]);
		ss = Number(timeParts[2]);

		let seconds = hh * 60 * 60 + mm * 60 + ss;
		
		var input = 
		{
			"Token": localStorage.getItem("token"),
			"Id": $("#updateId").val(),
			"UserName": userName,
			"Date": $("#updateDate").val(),
			"Distance": $("#updateDistance").val(),
			"Time": seconds
		}

		if ($("#updateId").val() == "0")
		{
			AddJog(input);
		}
		else
		{
			UpdateJog(input);
		}
	}

	function AddJog(input: any): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/jog",
			type: "POST",
			contentType: "application/json",
			data: JSON.stringify(input),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == "SUCCESS")
				{
					spinner.stop();
					LoadJogsPageOrFilter(Filter);
				}
				else
				{
					// Server-side form validation failed.
					spinner.stop();
					sessionStorage.setItem("errorMessage", result);
					window.location.hash = "#error";
				}
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function UpdateJog(input: any): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/jog",
			type: "PUT",
			contentType: "application/json",
			data: JSON.stringify(input),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == "SUCCESS")
				{
					spinner.stop();
					LoadJogsPageOrFilter(Filter);
				}
				else
				{
					// Server-side form validation failed.
					spinner.stop();
					sessionStorage.setItem("errorMessage", result);
					window.location.hash = "#error";
				}
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	function DeleteJog(jogId: number): void
	{
		let token: string | null = localStorage.getItem("token");
		
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			url: "/api/jog",
			type: "DELETE",
			contentType: "application/json",
			data: JSON.stringify({ Token: token, Id: jogId }),
			dataType: "text",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				LoadJogsPageOrFilter(Filter);
			},
			error: function()
			{
				spinner.stop();
				window.location.hash = "#";
			}
		});
	}

	// *** END JOGS ***
	// *** BEGIN BOOTSTRAP ***

	AuthCheck();

	// *** END BOOTSTRAP ***
});

// *** BEGIN UTILITY ***

function SpinnerSetup() : Spinner
{
	var opts = 
	{
		lines: 9,
		length: 0,
		width: 13,
		radius: 21,
		color: '#000',
		opacity: 0.25,
		rotate: 33
	}

	return new Spinner(opts);
}

function Today(): string
{
	let today: Date = new Date();

	let m: number = (today.getMonth() + 1); // January is 0!
	let d: number = today.getDate();
	let ys: string = today.getFullYear().toString();
	let ms: string = m < 10 ? "0" + m.toString() : m.toString();
	let ds: string = d < 10 ? "0" + d.toString() : d.toString();

	return ys + "-" + ms + "-" + ds;
}

function Reload(): boolean
{
	window.location.hash = "#";
	return false;
}

function ParseDate(input: string): Date
{
	let date: Date | null = null;
	let dateParts: Array<string> = input.split('-');

	let rangeError: boolean = false;

	try
	{
		let year: number = Number(dateParts[0]);
		let month: number = Number(dateParts[1]);
		let day: number = Number(dateParts[2]);

		date = new Date(year, month - 1, day); // Note: months are 0-based

		if (date.toString() == "Invalid Date")
		{
			throw "";
		}
	}
	catch (error)
	{
		throw "Filter error: date format is incorrect.";
	}

	return date; 
}

function TimeFormat(totalSeconds: number): string
{
	totalSeconds = Number(totalSeconds.toFixed());

	let h: number = Math.floor(totalSeconds / 3600);
	totalSeconds = totalSeconds - h * 3600;
	let m: number = Math.floor(totalSeconds / 60);
	totalSeconds = totalSeconds - m * 60;
	let s: number = Math.floor(totalSeconds);

	let hs: string = h < 10 ? "0" + h.toString() : h.toString();
	let ms: string = m < 10 ? "0" + m.toString() : m.toString();
	let ss: string = s < 10 ? "0" + s.toString() : s.toString();

	return hs + ":" + ms + ":" + ss;
}

function UpdateSelectControl(selectControlName: string, value: string): void
{
	let selectControl: JQuery = $("#"+ selectControlName);
	let option: JQuery = $("<option selected></option>");
	// Assuming that the id and the value are the same.
	option.val(value);   // Set id.
	option.text(value);  // Set text.
	selectControl.append(option);     // Add coach to the list of selections.
	selectControl.trigger("change");  // Tell Select2 to update.
}

function LoadJogsPageOrFilter(filter: IDictionary): void
{
	if (!$.isEmptyObject(filter))
	{
		window.location.hash = "#filter/" + JSON.stringify(filter);
	}
	else
	{
		window.location.hash = "#jogs";
	}
}

// *** END UTILITY ***
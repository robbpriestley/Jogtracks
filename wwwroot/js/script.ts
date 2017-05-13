"use strict";

declare var Handlebars:any;

// *** BEGIN INTERFACES ***

interface IDictionary 
{
	[key: string]: string;
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
		$("#accountType").show();
		$("#authMessage").text("Create an account");
		window.location.hash = "#signup";
		return false;
	});
	
	$("#signin").click(function(e) 
	{
		SignOut();
		$("#confirm").hide();
		$("#accountType").hide();
		$("#authMessage").text("Sign in to your account");
		window.location.hash = "#signin";
		return false;
	});
	
	$("#settings").click(function(e) 
	{
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
	
	$("#signout").click(function(e) 
	{
		Jogs = [];
		SignOut();
		window.location.hash = "#";  // Show welcome page.
		return false;
	});

	$("#username").on("change keyup keydown click paste", function()
	{
		$("#").css("display", "none");
	});

	$("input[name=coach]").on("change", (function(e) 
	{
		let value: string = $("input[name=coach]:checked").val();
		
		if (value == "noCoach")
		{
			$("#coachSelect").hide();
			$("#coach-select").css("display", "none");
		}
		else
		{
			$("#coachSelect").show();
			$("#coach-select").css("display", "none");
		}
	}));

	$("#coachSelectControl").select2({
		width: "400px",
		placeholder: "Select a coach",
		dropdownCssClass : "no-search",
		ajax: 
		{
			url: "/api/coaches",
			dataType: "json",
			type: "GET",
			headers: BasicAuth,
			data: function(params: any) 
			{
				var queryParameters = { token: localStorage.getItem("token") }
				return queryParameters;
			},
			processResults: function(data: any) 
			{
				$("#coach-select").css("display", "none");
				
				return {
					results: $.map(data, function(jog) 
					{
						return {
							text: jog.UserName,
							id: jog.UserName
						}
					})
				};
			}
		}
	});

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

	$("#filterRefresh").click(function(e) 
	{
		let fromString: string = $("#fromDate").val();
		let toString: string = $("#toDate").val();

		try
		{
			if (fromString == "" || toString == "")
			{
				throw "Filter error: please select both a from date and a to date.";
			}
		
			let fromDate = ParseDate(fromString);
			let toDate = ParseDate(toString);

			if (fromDate > toDate)
			{
				throw "Filter error: the from date must be the same as, or before, the to date.";
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

	// Single jog page buttons.
	let singleJogPage: JQuery = $(".single-jog");
	singleJogPage.on("click", function(e) 
	{
		if (singleJogPage.hasClass("visible")) 
		{
			let clicked: JQuery = $(e.target);

			// If the close button or the background are clicked go to the jogs page.
			if (clicked.hasClass("close") || clicked.hasClass("overlay"))
			{
				if (!$.isEmptyObject(Filter))
				{
					window.location.hash = "#filter/" + JSON.stringify(Filter);
				}
				else
				{
					window.location.hash = "#jogs";
				}
			}
		}
	});

	$("#errorBack").click(function(e) 
	{
		sessionStorage.removeItem("errorMessage");
		window.scrollTo(0, 0);
		window.location.hash = "#jogs";
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
				DefaultHandler();
				break;
			
			case "#signup":
			case "#signin":
				RenderAuthPage();
				break;

			case "#settings":
				RenderSettingsPage();
				break;
			
			case "#jogs":
				JogsPageHandler();
				break;

			case "#filter":
				FilterPageHandler(url);
				break;

			case "#jog":
				RenderSingleJogPage(url, Jogs);
				break;

			default:
				RenderErrorPage();  // Unknown keyword.
				break;
		}
	}

	function DefaultHandler(): void
	{
		if (localStorage.getItem("token") == null)
		{
			SignOut();
			RenderWelcomePage();
		}
		else
		{
			window.location.hash = "#jogs";
		}
	}

	function JogsPageHandler(): void
	{
		$("#fromDate").val("");
		$("#toDate").val("");
		Filter = {};
		sessionStorage.removeItem("errorMessage");

		$("#report").hide();
		
		LoadJogs();
		RenderJogsPage("null", "null");
	}

	function FilterPageHandler(url: string): void
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

	function RenderSettingsPage(): void
	{
		var page = $(".settings");
		page.addClass("visible");
		page.css("pointer-events", "auto");

		let accountType: any = localStorage.getItem("accountType");

		if (accountType == "JOGGER")
		{
			$("#accountTypeMessage").html("You are a <strong>Jogger</strong> with <strong>user-level</strong> permissions.");
			$("#coachGroup").show();
		}
		else if (accountType == "COACH")
		{
			$("#accountTypeMessage").html("You are a <strong>Coach</strong> with <strong>manager-level</strong> permissions.");
			$("#coachGroup").hide();
		}
		else
		{
			$("#accountTypeMessage").html("You are an <strong>Administrator</strong> with <strong>superuser-level</strong> permissions.");
			$("#coachGroup").hide();
		}

		SetCoachSelectControl();
		ShowHeaderComponents(true);
	}
	
	function RenderErrorPage(): void
	{
		let errorMessage: string | null = sessionStorage.getItem("errorMessage");
		$("#errorMessage").text(errorMessage == null ? "" : errorMessage);

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
			let jogIndex: string = $(this).data("index");
			window.location.hash = "jog/" + jogIndex;
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
		
		LoadJogsTotal(fromDate, toDate);

		let page: JQuery = $(".all-jogs");
		page.addClass("visible");
		ShowHeaderComponents(true);
	}

	// Opens preview page for one of the jogs. Parameters index from the hash and the jogs object.
	function RenderSingleJogPage(url: string, jogs: Array<JogData>): void
	{	
		let jogIndex: number = Number(url.split("#jog/")[1].trim());
		
		let page: JQuery = $(".single-jog");
		page.css("pointer-events", "auto");
		let container: JQuery = $(".preview-large");

		// Find the jog by iterating through the data object and searching for the chosen index.
		if (jogs.length)
		{
			jogs.forEach(function(jog)
			{
				if (jog.Id == jogIndex)
				{
					// Populate ".preview-large" with the chosen jog data.
					container.find("h3").text(jog.UserName);
				}
			});
		}

		page.addClass("visible");  // Show the page.
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
				let accountType: string = $("input[name=accountType]:checked").val();
				SignUp(username, password, accountType);
			}
			else if (keyword == "#signin")
			{
				SignIn(username, password);
			}

			return false;
		}
	});

	$("form[name=coachForm]").validate(
	{
		submitHandler: function(form: any)
		{
			CoachPatch();
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
			ChangePassword(password);
			return false;
		}
	});

	// *** END FORM VALIDATION ***
	// *** BEGIN REST AUTHENTICATION ***

	function AuthCheck(): void
	{
		let token: string | null = localStorage.getItem("token");

		if (token != null)
		{
			Authenticated();
		}
		else
		{
			window.location.hash = "#";  // Show welcome page.
		}
	}

	function Authenticated(): void
	{
		let token: string | null = localStorage.getItem("token");
		let userName: string | null = localStorage.getItem("userName");
		
		$("#user").text("Welcome, " + userName + "!");
		$("#user").show();
		$("#signup").hide();
		$("#signin").hide();
		$("#signout").show();
		$("#settings").show();
	}
	
	function ServerAuthenticated(username: string, authOutput: any): void
	{
		$("#user").text("Welcome, " + username + "!");
		$("#user").show();
		$("#signup").hide();
		$("#signin").hide();
		$("#signout").show();
		$("#settings").show();

		localStorage.setItem("token", authOutput.Token);
		localStorage.setItem("coach", authOutput.Coach);
		localStorage.setItem("userName", authOutput.UserName);
		localStorage.setItem("accountType", authOutput.AccountType);
		window.location.hash = "#jogs";
	}

	function SignUp(username: string, password: string, accountType: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			type: "POST",
			dataType: "json",
			data: JSON.stringify({ UserName: username, Password: password, AccountType: accountType }),
			contentType: "application/json",
			url: "/api/auth/signup",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == undefined)
				{
					spinner.stop();
					$("#username-taken").text("Sorry, that username is already taken");
					$("#username-taken").css("display", "inherit");
				}
				else
				{
					spinner.stop();
					ServerAuthenticated(username, result);
				}
			}
		});
	}

	function SignIn(username: string, password: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			type: "POST",
			dataType: "json",
			data: JSON.stringify({ UserName: username, Password: password }),
			contentType: "application/json",
			url: "/api/auth/signin",
			headers: BasicAuth,
			success: function(result) 
			{
				if (result == undefined)
				{
					spinner.stop();
					$("#username-taken").text("Bad username or password");
					$("#username-taken").css("display", "inherit");
				}
				else
				{
					spinner.stop();
					ServerAuthenticated(username, result);
				}
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

		// *** BEGIN RESET FORMS ***
		
		$("#jogger").prop("checked", true);
		$("#username-taken").css("display", "none");
		
		$("form[name=authForm]").validate().resetForm();
		$("form[name=coachForm]").validate().resetForm();
		$("form[name=changePasswordForm]").validate().resetForm();
		
		$("#username").val("");
		$("#password").val("");
		$("#cpassword").val("");
		$("#changePassword").val("");
		$("#changecPassword").val("");
		
		// *** END RESET FORMS ***

		//Reset other stuff.
		$("#noCoach").prop("checked", true);
		$("#coachSelectControl").empty();
		ClearStorage();
	}

	function ClearStorage(): void
	{
		localStorage.removeItem("token");
		localStorage.removeItem("coach");
		localStorage.removeItem("accountType");
		sessionStorage.removeItem("errorMessage");
	}

	function ChangePassword(password: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);

		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			type: "POST",
			data: JSON.stringify({ Token: token, Password: password }),
			contentType: "application/json",
			url: "/api/auth/changepassword",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				$("#changePassword").val("");
				$("#changecPassword").val("");
				$("#changePasswordMessage").show();
				setTimeout(function() { $("#changePasswordMessage").fadeOut(); }, 3000);
			}
		});
	}

	// *** END REST AUTHENTICATION ***
	// *** BEGIN SETTINGS ***

	function Settings(): void
	{
		// Clear the jogs list.
		let jogList: JQuery = $(".all-jogs .jogs-list");
		jogList.html("");

		window.location.hash = "#settings";
	}

	function SetCoachSelectControl(): void
	{
		let coach: string | null = localStorage.getItem("coach");

		if (coach != null && coach != "null")  // A coach has previously been selected.
		{			
			$("#coachSelect").show();
			$("#useCoach").prop("checked", true);

			let coachSelectControl: JQuery = $("#coachSelectControl");
			let option: JQuery = $("<option selected></option>");
			option.val(coach);   // Set id.
			option.text(coach);  // Set text.
			coachSelectControl.append(option);     // Add coach to the list of selections.
			coachSelectControl.trigger("change");  // Tell Select2 to update.
		}
		else
		{
			$("#coachSelect").hide();
			$("#noCoach").prop("checked", true);
		}
	}

	function CoachPatch(): void
	{
		let coach: string;
		let useCoach: string = $("input[name=coach]:checked").val();
		
		if (useCoach == "useCoach")
		{
			coach = $("#coachSelectControl").val();

			if (coach == null)
			{
				$("#coach-select").text("Please select a coach from the list");
				$("#coach-select").css("display", "inherit");
				return;
			}
		}
		else
		{
			coach = "null";
		}
		
		localStorage.setItem("coach", coach);
		
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			type: "PATCH",
			data: JSON.stringify({ Token: localStorage.getItem("token"), Coach: coach }),
			contentType: "application/json",
			url: "/api/coachpatch",
			headers: BasicAuth,
			success: function(result) 
			{
				spinner.stop();
				$("#changeCoachMessage").show();
				setTimeout(function() { $("#changeCoachMessage").fadeOut(); }, 3000);
			}
		});
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
			type: "GET",
			dataType: "json",
			data: { token: token },
			contentType: "application/json",
			url: "/api/jogs",
			headers: BasicAuth,
			success: function(result) 
			{
				Jogs = result;
				GenerateJogsHTML(Jogs);
				spinner.stop();
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
			type: "GET",
			dataType: "json",
			data: { token: token, fromDate: fromDate, toDate: toDate },
			contentType: "application/json",
			url: "/api/jogsfilter",
			headers: BasicAuth,
			success: function(result) 
			{
				Jogs = result;
				GenerateJogsHTML(Jogs);
				spinner.stop();
			}
		});
	}

	function LoadJogsTotal(fromDate: string, toDate: string): void
	{
		let token: string | null = localStorage.getItem("token");
		
		$.ajax
		({
			type: "GET",
			dataType: "json",
			data: { token: token, fromDate: fromDate, toDate: toDate  },
			contentType: "application/json",
			url: "/api/jogstotal",
			headers: BasicAuth,
			success: function(result) 
			{
				$("#jogsMessage2").text(result + " jogs.");
			}
		});
	}

	// *** END JOGS ***
	// *** BEGIN UTILITY ***

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

	// *** END UTILITY ***
	// *** BEGIN BOOTSTRAP ***

	AuthCheck();
	$(window).trigger("hashchange");  // Trigger a hash change to start the app.

	// *** END BOOTSTRAP ***
});

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
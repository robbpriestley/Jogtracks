"use strict";

declare var Handlebars:any;

// *** BEGIN INTERFACES ***

interface IDictionary 
{
	[key: string]: Array<string>;
};

interface IItemData
{
	Id: number;
	Name: string;
	Description: string;
}

// *** BEGIN INTERFACES ***
// *** BEGIN CLASS DEFINITIONS ***

class ItemData implements IItemData 
{
	Id: number;
	Name: string;
	Rating: number;
	Description: string;

	constructor(id: number, name: string, rating: number, description: string) 
	{
		this.Id = id;
		this.Name = name;
		this.Rating = rating;
		this.Description = description;
	}
}

// *** END CLASS DEFINITIONS ***

$(document).ready(function() 
{
	// *** BEGIN SPA ***
	// *** GLOBAL VARIABLES ***

	let Items: Array<ItemData> = [];
	let BasicAuth: object = {"Authorization": "Basic " + btoa("g9CZRkDEC5x8vfr96HMvkR3oiEiPLW" + ":" + "ECepRGahbgUCnwH5rCC7Xk3fdkBCKu")};

	function sortOn(property: string)
	{
		return function(a: any, b: any)
		{
			if (a[property] < b[property])
			{
				return -1;
			}
			else if(a[property] > b[property])
			{
				return 1;
			}
			else
			{
				return 0;   
			}
		}
	}

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
		window.location.hash = "#signup/";
		return false;
	});
	
	$("#signin").click(function(e) 
	{
		SignOut();
		$("#confirm").hide();
		$("#accountType").hide();
		$("#authMessage").text("Sign in to your account");
		window.location.hash = "#signin/";
		return false;
	});
	
	$("#settings").click(function(e) 
	{
		Items = [];
		Settings();
		return false;
	});
	
	$("#signout").click(function(e) 
	{
		Items = [];
		SignOut();
		window.location.hash = "#";  // Show welcome page.
		return false;
	});
	
	$("#newitem").click(function(e) 
	{
		return false;
	});

	$("#username").on("change keyup keydown click paste", function()
	{
		$("#username-taken").css("display", "none");
	});

	$("input[name=sort]").on("change", (function(e) 
	{
		let value: string = $("input[name=sort]:checked").val();
		Items.sort(sortOn(value));
		GenerateItemsHTML(Items);
		RenderItemsPage(Items);
	}));

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
					results: $.map(data, function(item) 
					{
						return {
							text: item.UserName,
							id: item.UserName
						}
					})
				};
			}
		}
	});

	// Single item page buttons.
	let singleItemPage: JQuery = $(".single-item");
	singleItemPage.on("click", function(e) 
	{
		if (singleItemPage.hasClass("visible")) 
		{
			let clicked: JQuery = $(e.target);

			// If the close button or the background are clicked go to the items page.
			if (clicked.hasClass("close") || clicked.hasClass("overlay"))
			{
				window.location.hash = "#items/";
			}
		}
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
				if (localStorage.getItem("token") == null)
				{
					SignOut();
					RenderWelcomePage();
				}
				else
				{
					window.location.hash = "#items/";
				}
				break;
			
			case "#signup":
			case "#signin":
				RenderAuthPage();
				break;

			case "#settings":
				RenderSettingsPage();
				break;
			
			case "#items":
				RenderItemsPage(Items);
				break;

			case "#item":
				let itemIndex: number = Number(url.split("#item/")[1].trim());
				RenderSingleItemPage(itemIndex, Items);
				break;

			default:
				RenderErrorPage();  // Unknown keyword.
				break;
		}
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

		// Update settings message. Convert account type from all caps to leading cap only.
		let accountType: any = localStorage.getItem("accountType");
		accountType = accountType.toString().toLowerCase();
		accountType = accountType.charAt(0).toUpperCase() + accountType.slice(1);
		$("#settingsMessage").text(accountType + " Settings");

		SetCoachSelectControl();

		ShowHeaderComponents(true);
	}
	
	function RenderErrorPage(): void
	{
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

	// Fill item list using handlebars template. Data argument is obtained from JSON file.
	function GenerateItemsHTML(items: Array<ItemData>): void
	{
		let itemList: JQuery = $(".all-items .items-list");
		itemList.html("");
		let template: string = $("#items-template").html();
		let compiledTemplate: any = Handlebars.compile(template);
		itemList.append(compiledTemplate(items));

		// Each item has data index attribute. On click change the URL hash to open up a preview for
		// the item. Every hashchange triggers the render function.
		itemList.find("li").on("click", function(e)
		{
			e.preventDefault();
			let itemIndex: string = $(this).data("index");
			window.location.hash = "item/" + itemIndex;
		})
	}

	function RenderItemsPage(items: Array<ItemData>): void
	{
		let page: JQuery = $(".all-items");
		let allItems: JQuery = $(".all-items .items-list > li");

		// Hide all items in the items list.
		allItems.addClass("hidden");

		// Iterate over the items. If item ID is in the data object, remove hidden class to reveal.
		allItems.each(function()
		{
			let instance: JQuery = $(this);

			items.forEach(function(item) 
			{
				if (instance.data("index") == item.Id)
				{
					instance.removeClass("hidden");
				}
			});
		});

		// Show the page. Render function hides all pages so we need to show the one we want.
		page.addClass("visible");
		ShowHeaderComponents(true);
	}

	// Opens preview page for one of the items. Parameters index from the hash and the items object.
	function RenderSingleItemPage(index: number, items: Array<ItemData>): void
	{	
		let page: JQuery = $(".single-item");
		let container: JQuery = $(".preview-large");

		// Find the item by iterating through the data object and searching for the chosen index.
		if (items.length)
		{
			items.forEach(function(item)
			{
				if (item.Id == index)
				{
					// Populate ".preview-large" with the chosen item data.
					container.find("h3").text(item.Name);
					container.find("p").text(item.Description);
				}
			});
		}

		page.addClass("visible");  // Show the page.
	}

	// *** END PAGE RENDERING ***
	// *** BEGIN FORM VALIDATION ***
	
	$("form[name='authForm']").validate(
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

	$("form[name='coachForm']").validate(
	{
		submitHandler: function(form: any)
		{
			CoachPatch();
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
		LoadItems(authOutput.Token);
		window.location.hash = "items/";
	}

	function SignOut(): void
	{
		// Clear the items list.
		let itemList: JQuery = $(".all-items .items-list");
		itemList.html("");

		$("#user").hide();
		$("#signup").show();
		$("#signin").show();
		$("#signout").hide();
		$("#settings").hide();

		// Reset the authentication form.
		$("#username").val("");
		$("#password").val("");
		$("#cpassword").val("");
		$("#jogger").prop("checked", true);

		//Reset other stuff.
		$("#noCoach").prop("checked", true);
		$("#coachSelectControl").empty();
		ClearLocalStorage();
	}

	function ClearLocalStorage(): void
	{
		localStorage.removeItem("token");
		localStorage.removeItem("accountType");
		localStorage.removeItem("coach");
	}

	// *** END REST AUTHENTICATION ***
	// *** BEGIN SETTINGS ***

	function Settings(): void
	{
		// Clear the items list.
		let itemList: JQuery = $(".all-items .items-list");
		itemList.html("");

		window.location.hash = "settings/";
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
			}
		});
	}

	// *** END SETTINGS ***
	// *** BEGIN ITEMS ***

	function LoadItems(token: string): void
	{
		let spinner: Spinner = SpinnerSetup();
		spinner.spin($("#main")[0]);
		
		$.ajax
		({
			type: "GET",
			dataType: "json",
			data: { token: token },
			contentType: "application/json",
			url: "/api/items",
			headers: BasicAuth,
			success: function(result) 
			{
				Items = result;
				GenerateItemsHTML(Items);
				spinner.stop();
				window.location.hash = "items/";
			}
		});
	}

	// *** END ITEMS ***
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
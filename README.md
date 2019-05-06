# Registration Page

(*This repo is based on a post for [dev.to](https://dev.to/ianknighton). Instead of writing it twice, I just wrote the post in this readme.*)

I run a [Vintage Volkswagen campout in Lava Hot Springs, Idaho](https://vwsatthesprings.com/). Over the course of the last 6 years, it has gone from being an excuse for a couple of my friends to come together to being one of the bigger gatherings in the state. Because of the year over year growth, we've started running pre-registration for the event to give us a headcount and make sure we have the space.

# The Problem

I need a simple web form that can take a few inputs, calculate a price, handle the payment processing, and then store the data somewhere that I can get to it. In the past, we used a combination of Google Forms and PayPal invoices. It worked, but it was incredibly clunky and took a lot of manual entry on every end. It also required going through PayPal's system which isn't always the easiest for people who don't already have an account.

I had heard of [Stripe](https://stripe.com/), knew they had a .Net library, and figured I could probably make the rest work.

# The Solution

I started with an empty .Net Core 2.2 web app.

```console
dotnet new web --name RegApp
```

This creates an empty application. I really prefer the MVC model, but I hate all the extra crud that comes with the default MVC project, so I add the library and necessary configuration afterwords.

```console
dotnet add package Microsoft.AspNetCore.Mvc
```

And then edit `Startup.cs` to use the Default MVC route.

```csharp
namespace RegApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
```

*Shouts to [I am Bacon](https://www.iambacon.co.uk/blog/create-a-minimal-asp-net-core-2-0-mvc-web-application) for the guide on how to do this. I've referenced this more often than I care to admit.*

Now that the app knows we're going to use MVC routes, we can add the `Models`, `Views`, and `Controllers` folders. Under the `Views` folder, create a folder called `Home` and inside of that create a file called `Index.cshtml`. Inside of the `Controllers` folder, create a file called `HomeController.cs`.

Minus the necessary code bits, we're now on the architecture for an MVC app.

## Adding Stripe

At this point, you'll need to have a Stripe account setup. That's where you'll get your API keys from. You get both a test and live key, which is nice because you can run a bunch of iterations and make sure everything works.

For the time being, we just need to add the library to our project and add the configuration to the `Startup.cs` file.

```console
dotnet add package Stripe.Net
```

And then update the `Startup.cs` file to include the `using` statement and configuration.

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace RegApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            StripeConfiguration.SetApiKey("<YOUR TEST KEY>");

            app.UseMvcWithDefaultRoute();
        }
    }
}
```

## Building Out

### The Models

For my case, I need to know the number of adults and children that will be attending each night and the number of vehicles their group will be bringing. The campground charges us per-adult, but the rest of the information is important so we can make sure we've organized the space correctly.

We'll create a model called `RegistrationModel.cs` to handle this data and allow is to safely pass it around. This same model will take advantage of the built in `DataAnnotations` to give us some validation.

```csharp
using System.ComponentModel.DataAnnotations;

namespace RegApp.Models
{
    public class RegistrationModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Range(0, 15)]
        public int FridayAdults { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int FridayChildren { get; set; }

        [Required]
        [Range(1, 15, ErrorMessage = "Please enter a valid number!")]
        public int Vehicles { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int SaturdayAdults { get; set; }

        [Required]
        [Range(0, 15, ErrorMessage = "Please enter a valid number!")]
        public int SaturdayChildren { get; set; }

        public int PerAdultCost { get; set; }
    }
}
```

In order to Stripe to process the payment, we also need to be able to pass an ID back and forth (more on that later). To handle this, I also created a model called `PaymentModel.cs` that only has one property. Yes, this could have been handled different ways, but having this gives me the ability to move things around if I need to.

```csharp
namespace RegApp.Models
{
    public class PaymentModel
    {
        public string ChargeID { get; set; }
    }
}
```

### The Views

Now that we have the models done, we can wire up some simple views to display and modify the data.

First, we'll take care of the `Index.cshtml` since it will be where the work is done. I like to rely on Razor/C# as much as possible to make this cleaner.

```html
@model RegApp.Models.RegistrationModel

<h2>Welcome to the Registration Page.</h2>
<h4>Please use this page to pre-register for this years event.</h4>
<p>The cost is $7 per adult, per night.</p>

@using (Html.BeginForm("Index", "Home", FormMethod.Post))
{
    <div>
        <label>Name</label>
        @Html.TextBoxFor(m => m.Name)
        @Html.ValidationMessageFor(m => m.Name)
    </div>
    <div>
        <label>Email Address</label>
        @Html.TextBoxFor(m => m.Email)
        @Html.ValidationMessageFor(m => m.Email)
    </div>
    <div>
        <label>Friday Night Adults</label>
        @Html.TextBoxFor(m => m.FridayAdults)
        @Html.ValidationMessageFor(m => m.FridayAdults)
    </div>
    <div>
        <label>Friday Night Children</label>
        @Html.TextBoxFor(m => m.FridayChildren)
        @Html.ValidationMessageFor(m => m.FridayChildren)
    </div>
    <div>
        <label>Saturday Night Adults</label>
        @Html.TextBoxFor(m => m.SaturdayAdults)
        @Html.ValidationMessageFor(m => m.SaturdayAdults)
    </div>
    <div>
        <label>Saturday Night Children</label>
        @Html.TextBoxFor(m => m.SaturdayChildren)
        @Html.ValidationMessageFor(m => m.SaturdayChildren)
    </div>
    <div>
        <label>Total Vehicles</label>
        @Html.TextBoxFor(m => m.Vehicles)
        @Html.ValidationMessageFor(m => m.Vehicles)
    </div>
    <div>
        <input type="submit" value="Submit" />
    </div>
}
```

We'll also add a view called `Payment.cshtml` to handle the redirect to stripe. If everything goes to plan, this page should only show briefly so we won't invest too much into it.

```html
@model RegApp.Models.PaymentModel

<script src="https://js.stripe.com/v3/"></script>

Redirecting to Stripe...

<script>
    var stripe = Stripe('<YOUR_STRIPE_PK_KEY>');
    stripe.redirectToCheckout({
        sessionId: '@Model.ChargeID',
    }).then(function (result) {
        result.error.message = "Oops! Looks like something went wrong. Please try again later."
    });
</script>
```

Basically this page just uses JavaScript to pass through, I mostly lifted this from the Stripe Quick-Start guide.

### The Controller

This is where all of the heavy lifting will happen. The controller handles all of the proper business logic to figure out costs and pass data around. I'm going to break the `HomeController.cs` file up, but if you want to see the whole thing it can be found in the [Git Repo](https://github.com/IanKnighton/RegistrationPage).

The first thing the controller needs to do is be able to handle a the initial landing page load. 

```csharp
public IActionResult Index()
{
    RegistrationModel model = new RegistrationModel();
    return View(model);
}
```

Now that it has an "empty" model and as shown the user our index page, the user can fill out the form and hit submit. 

This controller action handles the submit.

```csharp
[HttpPost]
public IActionResult Index(RegistrationModel model)
{
    if (ModelState.IsValid)
    {
        List<SessionLineItemOptions> items = new List<SessionLineItemOptions>();
        if (model.FridayAdults > 0)
        {
            items.Add(new SessionLineItemOptions
            {
                Name = "Friday Night Adults",
                Description = "The amount of adults that will be camping with us on Friday Night.",
                Amount = 700,
                Currency = "usd",
                Quantity = model.FridayAdults
            });
        }
        if (model.FridayChildren > 0)
        {
            items.Add(new SessionLineItemOptions
            {
                Name = "Friday Night Children",
                Description = "The amount of children that will be camping with us on Friday Night.",
                Amount = 1,
                Currency = "usd",
                Quantity = model.FridayChildren
            });
        }
        if (model.SaturdayAdults > 0)
        {
            items.Add(new SessionLineItemOptions
            {
                Name = "Saturday Night Adults",
                Description = "The amount of adults that will be camping with us on Saturday Night.",
                Amount = 700,
                Currency = "usd",
                Quantity = model.SaturdayAdults
            });
        }
        if (model.SaturdayChildren > 0)
        {
            items.Add(new SessionLineItemOptions
            {
                Name = "Saturday Night Children",
                Description = "The amount of children that will be camping with us on Saturday Night.",
                Amount = 1,
                Currency = "usd",
                Quantity = model.FridayChildren
            });
        }
        if (model.Vehicles > 0)
        {
            items.Add(new SessionLineItemOptions
            {
                Name = "Vehicles",
                Description = "The amount of Vehicles you plan on bringing",
                Amount = 1,
                Currency = "usd",
                Quantity = model.Vehicles
            });
        }

        var options = new SessionCreateOptions
        {
            CustomerEmail = model.Email,
            PaymentMethodTypes = new List<string>
            {
                "card"
            },
            LineItems = items,
            SuccessUrl = "<YOUR SUCCESS PAGE>",
            CancelUrl = "<YOUR CANCEL PAGE>"
        };

        SessionService service = new SessionService();
        Session session = service.Create(options);

        PaymentModel paymodel = new PaymentModel();
        paymodel.ChargeID = session.Id;

        return View("Payment", paymodel);
    }

    return View(model);
}
```

There's a lot going on here, so I'll break it down.

The first thing to do is make sure the model is valid. If it's not, we return the index page with the validation errors displayed to the user to correct them. This saves us from processing (or attempting to process) a request that is missing data.

Next, we go through the model and setup a list of `SessionLineItemOptions`. These are essentially line items that are displayed on the invoice to the user and to you when checkout is completed. One thing to note here is that Stripe does require everything to have a minimum value of $.01 to be passed through. In our case, this wasn't a concern. I wrote a blog post explaining it and so far, no one has had an issue with the couple pennies missing. You could just use the logic to subtract from the adult cost if you wanted it to even out.

Finally, we create and submit the session to Stripe. Assuming this all goes well, we'll recieve a response with an `Id` from the session. That will be used to process the payment. Once we have that, we return the Payment view, which (as we saw earlier) just redirects them to stripe to take the payment.

From here, it's out of our hands. The user goes to Stripe to process their payment and if all goes well the money and information will show up in your Stripe account.

I hope this helps! I know when I was looking for a "quick" walk-through I couldn't find anything close.
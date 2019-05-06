using Microsoft.AspNetCore.Mvc;
using RegApp.Models;
using Stripe.Checkout;
using System;
using System.Collections.Generic;

namespace RegApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            RegistrationModel model = new RegistrationModel();
            return View(model);
        }

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
                    SuccessUrl = "http://localhost:5000/",
                    CancelUrl = "http://localhost:5000/"
                };

                SessionService service = new SessionService();
                Session session = service.Create(options);

                PaymentModel paymodel = new PaymentModel();
                paymodel.ChargeID = session.Id;

                return View("Payment", paymodel);
            }

            return View(model);
        }

        public IActionResult Payment(PaymentModel model)
        {
            return View();
        }
    }
}
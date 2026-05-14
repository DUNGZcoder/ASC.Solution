using ASC.Model.BaseTypes;
using ASC.Utilities;
using ASC.Web.Areas.Account.Models;
using ASC.Web.Controllers;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.Account.Controllers
{
    [Authorize]
    [Area("Account")]
    public class AccountController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // =====================================================================
        // SERVICE ENGINEERS
        // =====================================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers =
                await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());

            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);

            return View(new ServiceEngineerViewModel
            {
                ServiceEngineers = serviceEngineers == null
                    ? null
                    : serviceEngineers.ToList(),

                Registration = new ServiceEngineerRegistrationViewModel
                {
                    IsEdit = false,
                    IsActive = false
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(
            ServiceEngineerViewModel serviceEngineer)
        {
            serviceEngineer.ServiceEngineers =
                HttpContext.Session.GetSession<List<IdentityUser>>("ServiceEngineers");

            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }

            if (serviceEngineer.Registration.IsEdit)
            {
                var user = await _userManager.FindByEmailAsync(
                    serviceEngineer.Registration.Email);

                user.UserName = serviceEngineer.Registration.UserName;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(serviceEngineer);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResult =
                    await _userManager.ResetPasswordAsync(
                        user,
                        token,
                        serviceEngineer.Registration.Password);

                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(serviceEngineer);
                }

                var identity = await _userManager.GetClaimsAsync(user);

                var isActiveClaim =
                    identity.SingleOrDefault(p => p.Type == "IsActive");

                if (isActiveClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, isActiveClaim);
                }

                await _userManager.AddClaimAsync(
                    user,
                    new System.Security.Claims.Claim(
                        "IsActive",
                        serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                IdentityUser user = new IdentityUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };

                IdentityResult result =
                    await _userManager.CreateAsync(
                        user,
                        serviceEngineer.Registration.Password);

                await _userManager.AddClaimAsync(
                    user,
                    new System.Security.Claims.Claim(
                        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
                        serviceEngineer.Registration.Email));

                await _userManager.AddClaimAsync(
                    user,
                    new System.Security.Claims.Claim(
                        "IsActive",
                        serviceEngineer.Registration.IsActive.ToString()));

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(serviceEngineer);
                }

                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        Roles.Engineer.ToString());

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(serviceEngineer);
                }
            }

            if (serviceEngineer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(
                    serviceEngineer.Registration.Email,
                    "Account Created/Modified",
                    $"Email: {serviceEngineer.Registration.Email} Password: {serviceEngineer.Registration.Password}");
            }
            else
            {
                await _emailSender.SendEmailAsync(
                    serviceEngineer.Registration.Email,
                    "Account Deactivated",
                    "Your account has been deactivated.");
            }

            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }

        // =====================================================================
        // CUSTOMERS
        // =====================================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers =
                await _userManager.GetUsersInRoleAsync(Roles.User.ToString());

            HttpContext.Session.SetSession("Customers", customers);

            return View(new CustomerViewModel
            {
                Customers = customers == null ? null : customers.ToList(),
                Registration = new CustomerRegistrationViewModel { IsEdit = false }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            customer.Customers =
                HttpContext.Session.GetSession<List<IdentityUser>>("Customers");

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            if (customer.Registration.IsEdit)
            {
                var user = await _userManager.FindByEmailAsync(
                    customer.Registration.Email);

                var identity = await _userManager.GetClaimsAsync(user);

                var isActiveClaim =
                    identity.SingleOrDefault(p => p.Type == "IsActive");

                var removeClaimResult = await _userManager.RemoveClaimAsync(
                    user,
                    new System.Security.Claims.Claim(
                        isActiveClaim.Type,
                        isActiveClaim.Value));

                var addClaimResult = await _userManager.AddClaimAsync(
                    user,
                    new System.Security.Claims.Claim(
                        isActiveClaim.Type,
                        customer.Registration.IsActive.ToString()));
            }

            if (customer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(
                    customer.Registration.Email,
                    "Account Modified",
                    $"Your account has been activated, Email : {customer.Registration.Email}");
            }
            else
            {
                await _emailSender.SendEmailAsync(
                    customer.Registration.Email,
                    "Account Deactivated",
                    $"Your account has been deactivated.");
            }

            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAdvert.Web.Models.Accounts;
using Amazon.AspNetCore.Identity.Cognito;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAdvert.Web.Controllers
{
    public class AccountsController : Controller
    {
        private readonly SignInManager<CognitoUser> _signInManager;
        private readonly UserManager<CognitoUser> _userManager;
        private readonly CognitoUserPool _pool;

        public AccountsController(SignInManager<CognitoUser> signInManager, UserManager<CognitoUser> userManager, CognitoUserPool pool)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _pool = pool;
        }

        public async Task<IActionResult> Signup()
        {
            var model = new SignupModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Signup(SignupModel signupModel)
        {
            if (ModelState.IsValid)
            {
            //   RedirectToAction("Confirm", "Accounts");
                var user = _pool.GetUser(signupModel.Email);

                if (user.Status != null)
                {
                    ModelState.AddModelError("UserExist", "User with this email already exist");
                    return View(signupModel);
                }
                Dictionary<string, string> validationData = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        { CognitoAttribute.Email.AttributeName , signupModel.Email},
                        { CognitoAttribute.Name.AttributeName , signupModel.Email},
                    };

                user.Attributes.Add(CognitoAttribute.Name.AttributeName, signupModel.Email);
                var createdUser = await ((CognitoUserManager<CognitoUser>)_userManager).CreateAsync(user, signupModel.Password, validationData);
             //  
               // var createdUser = await _userManager.CreateAsync(user, signupModel.Password);

                if (createdUser.Succeeded)
                {
                   return  RedirectToAction("Confirm","Accounts");
                }
            }



            return View();
        }

       
       
        public ActionResult Confirm(ConfirmModel confirmMode)
        {
           
            return View(confirmMode);
        }

        [HttpGet]
        public ActionResult Login()
        {
            LoginModel loginModel = new LoginModel();
            return View(loginModel);
        }

        [HttpPost]
        [ActionName("Login")]
        public async Task<IActionResult> Login_Post(LoginModel loginModel)
        {
          if(ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, false).ConfigureAwait(false);

                if(result.Succeeded)
                {
                    return RedirectToAction("index", "Home");
                }
                else
                {
                    ModelState.AddModelError("loginError", "Email And Password do not match");
                }
            }
            return View("Login", loginModel);
        }


        [HttpPost]

        public async Task<IActionResult> Confirm_Post(ConfirmModel confirmModel)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(confirmModel.Email);
                if (user==null)
                {
                    ModelState.AddModelError("NotFound", "A User with Given Email Not found");
                }
                var result = await (_userManager as CognitoUserManager<CognitoUser>).ConfirmSignUpAsync(user, confirmModel.Code, true).ConfigureAwait(false);
               // var result = await _userManager.ConfirmEmailAsync(user, confirmModel.Code);
                if(result.Succeeded)
                {
                    return RedirectToAction("index","Home");
                }
                else
                {
                    foreach(var item in result.Errors)
                    {
                        ModelState.AddModelError(item.Code, item.Description);
                    }
                    return RedirectToAction("Confirm", new { Email = confirmModel.Email, Code = confirmModel.Code });
                  //  return View(confirmModel);
                }
               
            }

            return RedirectToAction("Confirm",new {Email = confirmModel.Email,Code=confirmModel.Code });


        }
    

    }
}

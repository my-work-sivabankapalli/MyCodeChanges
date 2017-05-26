using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CurriculumTool.UserLogin.Models;
using CurriculumTool.Utility;
using CurriculumTool.Web.ServiceInterface.Interfaces;
using CurriculumTool.Web.ServiceInterface.Controllers;
using CurriculumTool.Models.Account;
using AutoMapper;
using CurriculumTool.Controllers.Shared;
using CurriculumTool.Dtos.Models;
using CurriculumTool.Dtos.Models.User;
using CurriculumTool.Web.ServiceInterface.Models;

namespace CurriculumTool.Controllers
{

    [Authorize]
    public class AccountController : AccountBaseController
    {
        private readonly IUserServiceController _userController;
        private readonly IColServiceController _colServiceController;

        public AccountController()
        {
            _userController = new UserServiceController();
            _colServiceController = new ColServiceController();
        }

        public AccountController(IUserServiceController controller)
        {
            _userController = controller;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ShowFeedback = false;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await UserAccountController.Login(Mapper.Instance.Map<LoginDetailModel>(model));

            switch (result.Status)
            {
                case SignInStatus.Success:
                    if (result.LastLogin == null) return RedirectToAction("UserSetup", "Account");
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                    //return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError(string.Empty, Resources.Account.Login.Error_InvalidLogin);
                    return View(model);
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }


        public async Task<UserProfileViewModel> GetUserProfileViewModel()
        {
            var userid = HttpContext.User.Identity.GetUserId();
            var result  = await _userController.GetUserProfile(userid);
            if (result == null || !result.IsSuccessful())
            {
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            }
            return TransformUserProfileResponse(result);
        }

        public Task<UserProfileResponseDto> UpdateUserProfileViewModel(UserProfileViewModel model)
        {
            var dto = Mapper.Map<UserProfileViewModel, UserProfileRequestDto>(model);
            var list = new List<ContactDataDto>();
            dto.UserId = HttpContext.User.Identity.GetUserId();
            if (!string.IsNullOrWhiteSpace(model.MobileNumber))
            {
                list.Add(new ContactDataDto()
                {
                    Id = !string.IsNullOrWhiteSpace(model.MobileNumberId) ? model.MobileNumberId : null,
                    ContactType = "Mobile",
                    Contact = model.MobileNumber
                });
            }
            if (!string.IsNullOrWhiteSpace(model.WorkNumber))
            {
                list.Add(new ContactDataDto()
                {
                    Id = !string.IsNullOrWhiteSpace(model.WorkNumberId) ? model.WorkNumberId : null,
                    ContactType = "Work",
                    Contact = model.WorkNumber
                });
            }
            dto.ContactDetails = list;
            return _userController.UpdateUserProfile(dto);
        }

        public UserProfileViewModel TransformUserProfileResponse(UserProfileResponseDto result)
        {
            var viewModel = Mapper.Map<UserProfileResponseDto, UserProfileViewModel>(result);
            if (result.ContactDetails != null)
            {
                var mobile = result.ContactDetails.FirstOrDefault(x => x.ContactType.Equals("Mobile", StringComparison.InvariantCultureIgnoreCase));
                var work = result.ContactDetails.FirstOrDefault(x => x.ContactType.Equals("Work", StringComparison.InvariantCultureIgnoreCase));
                viewModel.MobileNumberId = mobile != null ? mobile.Id : null;
                viewModel.MobileNumber = mobile != null ? mobile.Contact : null;
                viewModel.WorkNumberId = work != null ? work.Id : null;
                viewModel.WorkNumber = work != null ? work.Contact : null;
            }
            return viewModel;
        }

        //
        // GET: /Account/UserSetup
        [Authorize]
        public async Task<ActionResult> UserSetup()
        {
            var model = await GetUserProfileViewModel();
            return View(model);
        }

        //
        // Post: /Account/UserSetup
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserSetup(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var response = await UpdateUserProfileViewModel(model);
                if (response != null && response.IsSuccessful())
                {
                    return RedirectToAction("Index", "Col");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex);
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            }
            return View(model);

        }

        //
        // GET: /Account/Details
        [Authorize]
        public async Task<ActionResult> UserDetails()
        {
            if (TempData["UserDetailsSuccess"] != null)
            {
                return View(TempData["UserDetailsSuccess"]);
            }

            var model = await GetUserProfileViewModel();
            return View(model);
        }


        //
        // Post: /Account/UserDetails
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserDetails(UserProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                var response = await UpdateUserProfileViewModel(model);
                if (response != null && response.IsSuccessful())
                {
                    TempData["UserDetailsSuccess"] = TransformUserProfileResponse(response);
                    return RedirectToAction("UserDetails");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.WriteException(ex);
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            }
            return View(model);

        }
        public ActionResult Secured()
        {
            ViewBag.Message = "A secured page.";

            return View();
        }
        public ActionResult Signoff()
        {
            Response.Redirect("/Handlers/signoff.ashx");
            return View();
        }
        public ActionResult Metadata()
        {
            Response.Redirect("/Handlers/metadata.ashx");
            return View();
        }
        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            ViewBag.ShowFeedback = false;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var urlScheme = Request != null && Request.Url != null ? Request.Url.Scheme : null;
            var result = await _userController.RequestForgottenPasswordLink(new ForgotPasswordRequestDto() {
                Email =  model.Email,
                CallbackUrl = Url.Action("ResetPassword", "Account", new {} , urlScheme)
            });
            var error = result.GetFirstError();
            // only show error if it failed to connect to backend
            if (error != null && error.ErrorCode == (int)ErrorCodes.WebConnectionError)
            {
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
                return View(model);
            }
            return RedirectToAction("ForgotPasswordConfirmation");
        }


        [HttpPost]
        public async Task<ActionResult> Feedback(FeedbackFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string body;
                using (var sr = new StreamReader(Server.MapPath("~/App_Data/Templates/FeedbackEmail.txt")))
                
                 {
                    body = sr.ReadToEnd();
                 }

            string messageBody = string.Format(body, model.Name, model.Email, model.Comment);

            var result = await _userController.RequestFeedback(new FeedbackRequestDto()
                {
                    MessageBody = messageBody
                });

                var error = result.GetFirstError();

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, error.ErrorMessage);
                    return PartialView("_FeedbackForm", model);
                }
                return Json(new { success = true, responseText = "Your comment sent successfully!" });
            }
            return PartialView("_FeedbackForm", model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var request = Mapper.Map<ResetPasswordViewModel, ResetPasswordRequestDto>(model);
            var result = await _userController.ResetPassword(request);
            if (result != null)
            {
                var error = result.GetFirstError();
                if (result.IsSuccessful() || (error != null && error.ErrorCode == (int)ErrorCodes.NotFound))
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
            }
            ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var request = Mapper.Map<ChangePasswordViewModel, ChangePasswordRequestDto>(model);
            request.Id = User.Identity.GetUserId();
            var result = await _userController.ChangePassword(request);
            if (result != null)
            {
                var error = result.GetFirstError();
                if (result.IsSuccessful() || (error != null && error.ErrorCode == (int)ErrorCodes.NotFound))
                {
                    return RedirectToAction("ChangePasswordConfirmation", "Account");
                }
                if (error != null && error.ErrorCode == (int) ErrorCodes.ApplicationDataException)
                    ModelState.AddModelError(string.Empty, Resources.Global.Error_IncorrectPassword);
            }
            else
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            return View(model);
        }

        
        // GET: /Account/ChangePasswordConfirmation
        public ActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        //[ColAuthorize(ColRole.Any)]
        public async Task<ActionResult> ListUsers(int colid)
        {
            AddIsAdminAndAboveToViewBag();
            var model = await GetListUsersViewModel(colid);
            var colDetail = await _colServiceController.GetCol(colid);
            if (colDetail != null && colDetail.IsSuccessful())
                model.ColName = colDetail.Name;
            else
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic); 
            return View(model);
        }

        public async Task<ListUsersViewModel> GetListUsersViewModel(int colid)
        {
            var listUsersViewModel = new ListUsersViewModel();
            listUsersViewModel.ColId = colid;

            var usersModel = await _userController.GetUsers(new GetUsersQueryParameters()
            {
                ColId = colid,
                Permissions = new[] { ColRole.ColAdministrator, ColRole.ColLeader, ColRole.ColMember, ColRole.TeacherAcrossSchool, ColRole.TeacherInSchool }
            });

            if (usersModel == null || usersModel.Users == null)
            {
                ModelState.AddModelError(string.Empty, Resources.Global.Error_Generic);
            }
            else
            {
                var listUsersModel = Mapper.Map<UsersOverviewDto, ListUsersViewModel>(
                    usersModel);
                listUsersViewModel.Users = listUsersModel.Users;
                listUsersViewModel.ColAdministratorUsers =
                   listUsersModel.Users.Where(x => x.ColRole == (int)ColRole.ColAdministrator).OrderBy(x => x.GetLastThenFirst()).ToList();

                listUsersViewModel.ColLeaderUsers =
                    listUsersModel.Users.Where(y => y.ColRole == (int)ColRole.ColLeader).OrderBy(y => y.GetLastThenFirst()).ToList();
                listUsersViewModel.ColMemberUsers =
                   listUsersModel.Users.Where(y => y.ColRole == (int)ColRole.ColMember).OrderBy(y => y.GetLastThenFirst()).ToList();
                listUsersViewModel.TeacherAcrossSchoolUsers =
                    listUsersModel.Users.Where(y => y.ColRole == (int)ColRole.TeacherAcrossSchool).OrderBy(y => y.GetLastThenFirst()).ToList();
                listUsersViewModel.TeacherInSchoolUsers =
                    listUsersModel.Users.Where(y => y.ColRole == (int)ColRole.TeacherInSchool).OrderBy(y => y.GetLastThenFirst()).ToList();
            }

            return listUsersViewModel;
        }




        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        /*
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        */
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Col");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
using System.Web.Mvc;
using System.Web.Security;
using Legend_Špedicija.Models.EntityManager;
using Legend_Špedicija.Models.ViewModel;
using MVC5RealWorld.Security;
using Legend_Špedicija.Models.DB;
using Legend_Špedicija.Security;

using System;

namespace Legend_Špedicija.Controllers {
	public class AccountController : Controller {

		//******************************************************
		//**SignUp new user, admin only
		//-----------------------------------------------------
		[HttpPost] [AuthorizeRoles("Admin")]
		public JsonResult SignUp(UserSignUpView USUV1) {
			
			string message = "";
			string Error1 ="";
			string DefURL = "/Home/UserList";
			UserManager UM = new UserManager();

			//check all fields of the USUV model
			bool AllFieldsOK = UM.CheckObjectFields(USUV1);
			if (!AllFieldsOK) {
				message = "Required field is missing";
				return Json(new { url = DefURL, message = message });
			}
			//check if loginName is taken
			if (UM.IsLoginNameExist(USUV1.LoginName)) {
				message = "User Name already taken.";
				return Json(new { url = DefURL, message = message });
			}
			//check email format
			if (!UM.CheckEmailFormat(USUV1.email)) {
				message = "Email Address not valid.";
				return Json(new { url = DefURL, message = message });
			}

			//check if email is unique
			if (!UM.IsEmailUnique(USUV1.email)){
				message = "Email Address already taken.";
				return Json(new { url = DefURL, message = message });
			}

			//add user to db
			int loggedUserId = UM.GetUserIdFromName(User.Identity.Name);
			string PassGenerated =UM.AddUserAccount(USUV1, loggedUserId);//add user with logged admin's Id
			
			//send mail to inform user about registration
			string CompanyUrl = "<a href = '"
				+ string.Format("{0}://{1}/Home/Index", Request.Url.Scheme, Request.Url.Authority)
				+ "'>Legend Šped.</a>";
			bool SendMail = UM.SendRegistrationMail(USUV1, CompanyUrl, PassGenerated, ref Error1);
			if (SendMail) {//get the bool component
				message = "New User was registered. Registration info sent at user's Email.";
			}
			else
				message = "New User was registered but regitration email to User was not sent. " + Error1;

			return Json(new { url = DefURL, message = message });//url which js script will redirect to. message -diagnostic message
		}//end SignUp

		//******************************************************
		//**SignOut
		//-----------------------------------------------------
		[Authorize]
		public ActionResult SignOut() {
			FormsAuthentication.SignOut( );
			return RedirectToAction("Index", "Home");
		}

		//******************************************************
		//** LogIN
		//-----------------------------------------------------
		[HttpPost]
		public JsonResult LogIn (UserLoginView ULV) {
			string error1="";
			int error3 = 0;//0: no error, 1: no role defined for the user, 2: non-existant user error			
			Session["UserRole"] = 0;//default role, 0: nobody
			Session[ "UserId" ] = 0;//0: nobody

			ULV.LoginName = ULV.LoginName.Trim();
			ULV.Password = ULV.Password.Trim();

			//check if any of fields is empty
			if (String.IsNullOrEmpty(ULV.LoginName) || String.IsNullOrEmpty(ULV.Password)) {
				return Json(new {success = true, error = error1});
			}
			UserManager UM = new UserManager( );
			int UserId = UM.GetUserIdFromName(ULV.LoginName);

			//if user is found
			if (UserId != 0){
				//compare provided password with the one that is attached to a user name in db
				bool ArePasswordsSame = UM.ComparePasswords(UserId, ULV.Password);
				if (ArePasswordsSame) {
					FormsAuthentication.SetAuthCookie(ULV.LoginName, false);
					SetSessionUserName(ULV.LoginName);
					Session[ "UserId" ] = UserId;

					int RoleId = UM.GetUserRoleId(UserId, ref error3);
					Session[ "UserRole" ] = RoleId;
					return Json(new {success = true, error = error1});
				}
				else {
					SetSessionUserName("");
					error1 = "Provided password is incorrect.";
				}
			}	
			else {//if user is not found
				error1 = "User Name not found.";
			}
			
			// If we got this far, something failed, return false success
			return Json( new {success = false, error = error1});
		}

		//-----------------------------------------------------
		//-- Set Session["UserName"] to shroten authorized user name or empty string if no one is logged
		//-----------------------------------------------------
		public void SetSessionUserName(string userName) {
			string ShortUserName = "";

			if (!string.IsNullOrEmpty(userName)) {
				ShortUserName = userName;

				if (ShortUserName.Length > 12)
					ShortUserName = ShortUserName.Substring(0, 12);
			}
			Session["UserName"] = ShortUserName;
		}

		//-------------------------------------------------------
		//--GET SMTP paremeters
		//-----------------------------------------------------
		public ActionResult SMTPParams (){
			UserManager Um1 = new UserManager();
			SMTPParamsView param1 = Um1.GetSMTPParams();

			if (param1 != null)   return View(param1);
			else   return View();
		}


		//-------------------------------------------------------
		//--POST SMTP paremeters
		//-----------------------------------------------------
		[HttpPost][AuthorizeRoles("Admin")][ValidateAntiForgeryToken]
		public ActionResult SMTPParams(SMTPParamsView SMTPpView1) {
			if (ModelState.IsValid) {
				UserManager UM = new UserManager();
				int LoggedUserId = UM.GetUserIdFromName(User.Identity.Name);
				bool IsSetParams = UM.SetSMTPParams(SMTPpView1, LoggedUserId);

				if (IsSetParams) 
					ModelState.AddModelError("", "Update Succesfull");
				else 
					ModelState.AddModelError("", "Database Error");
			}
			return View(SMTPpView1);
		}


		//-----------------------------------------------------
		//--GET
		//-----------------------------------------------------
		public ActionResult StoreHash() {
			return View( );
		}

		//-----------------------------------------------------
		//-- POST, from password string and users Id, create salt and hash for password and store them to DB
		//-- temporary function nedeed to sign up the first user
		//-----------------------------------------------------
		[HttpPost][Authorize]
		public ActionResult StoreHash (int UserID, string password1){
			byte[] salt1 = Hmac1.GenerateSalt( );
			string HashPass = Hmac1.GetHashString(password1, salt1);

			using (DemoDBEntities db = new DemoDBEntities( )) {
				SYSUser user1 = db.SYSUsers.Find(UserID);

				if (user1 != null) {
					user1.PasswordEncryptedText = HashPass;
					user1.Salt = salt1;
		
					db.SaveChanges( );
				}
			}
			return View( );
		}		
	}
}
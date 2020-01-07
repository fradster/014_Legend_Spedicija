using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Legend_Špedicija.Models.DB;
using Legend_Špedicija.Models.EntityManager;
using Legend_Špedicija.Models.ViewModel;
using MVC5RealWorld.Security;

namespace Legend_Špedicija.Controllers
{
	public class HomeController: Controller {

		//*********************************************************
		//**GET: Home
		public ActionResult Index() {

			if (!User.Identity.IsAuthenticated) {
				Session[ "UserId" ] = 0;//0: nobody
				Session["UserName"] = "";
				Session["Role"] = 0;// 0- nobody, 1- admin, 2-user
			}
			return View( );
		}

		//*********************************************************
		//**GET UserCount.Get number of users in the list
		//---------------------------------------------------------
		[AuthorizeRoles("Admin")]
		public int UserCount() {
			UserManager UM = new UserManager();
			int NumOfUsers = UM.GetNumOfUsers();
			return NumOfUsers;
		}

		//*********************************************************
		//**GET UserList(). Get User List data for all users
		//---------------------------------------------------------
		[AuthorizeRoles("Admin")]
		public ActionResult UserList(int FirstUserIndex= 1, string Message="") {
			UserManager UM = new UserManager();
			//get currenly logged admin
			int LoggedId = UM.GetUserIdFromName(User.Identity.Name);
			UserDataView UDV = UM.GetUserDataView(FirstUserIndex, LoggedId);
			ViewBag.Message = Message;
			return View(UDV);
		}

		//*********************************************************
		//**POST UserList(), updates user profile
		//---------------------------------------------------------
		[HttpPost] [AuthorizeRoles("Admin")]
		public JsonResult UserList(UserProfileView UPV1) {
			string message = "";
			string DefURL = "/Home/UserList";

			string LoginName = User.Identity.Name;
			UserManager UM = new UserManager();

			//check all fields of the UPV
			bool AllFieldsOK = UM.CheckObjectFields(UPV1);
			if (!AllFieldsOK) {
				message = "Required field is missing";
				return Json(new { url = DefURL, message = message });
			}
			//check if email adress is valid
			if (!UM.CheckEmailFormat(UPV1.email)) {
				message = "Email Address not valid.";
				return Json(new { url = DefURL, message = message });
			}

			//id of logged user is necessarry, because of "modified by" field in db
			int LoginId = UM.GetUserIdFromName(LoginName);
			bool UpdateSucces= UM.UpdateUserAccount(UPV1, LoginId);

			if (!UpdateSucces) {
				message = "Upadating profile not successful";
				return Json(new { url = DefURL, message = message });
			}
			//if update was successful
			int ReqId = UPV1.SYSUserID;//id of the user whose profile should be updated
			bool SameUser = (LoginId == ReqId);//is logged user editing himself
			string NewLoginName = UPV1.LoginName;//login name of submited profle

			//is admin modifying himself and has changed user name
			if (SameUser && !NewLoginName.Equals(LoginName)) {
				//set new authentication cookie
				FormsAuthentication.SetAuthCookie(NewLoginName, false);
				//and set new values for the session object
				Session[ "UserName" ] = NewLoginName;
			}
			message = "Update successful.";
			Session[ "Role" ] = UPV1.RoleName;

			return Json(new {url = DefURL, message = message});//url which js script will redirect to. message -diagnostic message
		}//end UserList


		//*********************************************************
		//**GET Single User profile
		//---------------------------------------------------------
		[Authorize]
		public ActionResult EditProfile() {
			string loginName = User.Identity.Name;
			UserManager UM = new UserManager( );
			UserProfileView UPV = UM.GetUserProfile(UM.GetUserIdFromName(loginName));
			return View(UPV);
		}

		//*********************************************************
		//**POST Single User profile
		[HttpPost]
		[Authorize]
		public ActionResult EditProfile(UserProfileView UPV1) {

			if (ModelState.IsValid) {
				int ReqId = UPV1.SYSUserID;
				string LoginName = User.Identity.Name;
				UserManager UM = new UserManager( );
				int LoginId = UM.GetUserIdFromName(LoginName);//id of logged user
				bool SameUser = (LoginId == ReqId);//is logged user editing himself

				bool UpdateSucces = UM.UpdateUserAccount(UPV1, LoginId);
				if (UpdateSucces && SameUser) {
					//if update was successful and if user was updateing himself
					//set new authentication cookie, but only if user had changed his login Name
					string NewLoginName = UPV1.LoginName;
					if (!NewLoginName.Equals(LoginName)) {
						FormsAuthentication.SetAuthCookie(NewLoginName, false);
						//and set new values for the session object
						Session[ "UserName" ] = NewLoginName;
					}
					ViewBag.Status = "Update Sucessful!";
					Session[ "Role" ] = UPV1.RoleName;
				}
				else if (!UpdateSucces){
					ViewBag.Status = "Update not Sucessful";
				}
				else {//if its not the same user
					ViewBag.Status = "User not Authorized";
				}
			}
			return View(UPV1);
		}


		//---------------------------------------------------
		//-- DeleteUser
		[HttpPost] [AuthorizeRoles("Admin")]
		public string DeleteUser(int userID) {
			string message = "User not permited";

			string LoginName = User.Identity.Name;
			UserManager UM = new UserManager( );
			int LoginId = UM.GetUserIdFromName(LoginName);//id of logged user

			bool SameUser = (LoginId == userID);//is logged user deleting himself

			if (!SameUser) {//admin can not delete himself
				UM.DeleteUser(userID);
				message = "User Deleted";
			}
			return message;
		}

		//----------------------------------------------------------
		//--GET change password
		[Authorize]
		public ActionResult ChangePassword () {
			return View();
		}

		//----------------------------------------------------------
		//--POST change password for the current user
		[HttpPost][Authorize][ValidateAntiForgeryToken]
		public JsonResult ChangePassword(string newPassword) {
			bool success = true;//succesfull update by default
			string report = "Password succesfully updated";//no error by default

			if (string.IsNullOrEmpty(newPassword)){
				success = false;
				report = "Password Invalid";
			}
			else {
				UserManager Um1 = new UserManager();
				string LoginName = User.Identity.Name;
				int loggedUserId = Um1.GetUserIdFromName(LoginName);

				bool error1 = Um1.UpdatePassword(newPassword, loggedUserId);
				
				if (!error1) {
					success = false;
					report = "Ivalid User";
				}
			}
			return Json(new {success, report});
		}

		//----------------------------------------------------------
		//--POST forgot password, InputFiled is either Users Name or email address, Type: 1 for User Name, 2 for Email address
		[HttpPost]
		public JsonResult ForgotPassword(string InputField) {
			string Error1 = "";
			string message = "New password has been generated for you, and sent to your registered Email address";//no error by default
			bool success = true;//operation is succesfull by default
			int UserId;
			
			//check InputField
			if (String.IsNullOrWhiteSpace(InputField)){
				return Json(new {success= false, message="Empty Field"});
			}

			//check if inputField is a valid email address. If it is, find the user
			UserManager UM1 = new UserManager();
			InputField = InputField.Trim();
			if (UM1.CheckEmailFormat(InputField)){
				UserId = UM1.GetUserIdFromEmail(InputField);
			}
			else {//if its not email address, it has to be UserName
				UserId = UM1.GetUserIdFromName(InputField);//get his ID
			}

			if (UserId == 0) {// if no one is found
				success = false;
				message = "User Name or Email adress not found.";
			}
			else {
				//prepare arguments for sending mail to user
				string CompanyUrl = "<a href = '"
					+ string.Format("{0}://{1}/Home/Index", Request.Url.Scheme, Request.Url.Authority)
					+ "'>Legend Šped.</a>";
				//generate new password
				string NewPass = Membership.GeneratePassword(12, 2);
				bool SuccessUpdate = UM1.UpdatePassword(NewPass, UserId);//update user data in db
				
				if (!SuccessUpdate) {
					success = false;
					message = "Password not updated, User Not found";
				}
				else {
					bool SuccessSendmail = UM1.SendChangedPasswordMail(UserId, CompanyUrl, NewPass, ref Error1);
					if (!SuccessSendmail){
						message = Error1;	success = false;
					}	
				}
			}			
			return Json(new {success, message});
		}

		//************************************************************
		// GET ShoutBox partial
		[Authorize]
		public ActionResult ShoutBoxPartial() {
			return PartialView( );
		}

		//************************************************************
		// SendMessage
		[Authorize]
		public JsonResult SendMessage(int userID, string message) {
			UserManager UM = new UserManager( );
			UM.AddMessage(userID, message);
			return Json(new { success = true });
		}

		//************************************************************
		// Get JSON all massages
		[Authorize]
		public JsonResult GetMessages() {
			UserManager UM = new UserManager( );
			return Json(UM.GetAllMessages( ), JsonRequestBehavior.AllowGet);
		}

		//************************************************************
		//put object to file
		internal static void SendEmpToFile(string text1) {
			string path = $"{HttpRuntime.AppDomainAppPath}\\Izlaz_objekat.txt";

			StreamWriter izlaz = new StreamWriter(path);

			izlaz.WriteLine("text= " + text1);
			izlaz.Close( );
		}
	}
}
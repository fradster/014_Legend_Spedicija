using System;
using Legend_Špedicija.Models.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Legend_Špedicija.Models.DB;
using Legend_Špedicija.Security;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using MimeKit.Text;
using System.Web.Security;

namespace Legend_Špedicija.Models.EntityManager {
	public class UserManager {
		
		//******************************************************
		//--Add new user to database from the ViewModel UserSignUpView
		//--return non-hashed password, to be send in info mail to user
		public string AddUserAccount(UserSignUpView USUV1, int loggedUserId) {

			string NewPass, HashPassword;

			using (DemoDBEntities db = new DemoDBEntities( )) {
				SYSUser SU = new SYSUser( );
				SU.LoginName = USUV1.LoginName;

				//generate password and salt
				Tuple <byte[], string, string> GeneratedPass =  GeneratePassword();
				HashPassword= GeneratedPass.Item2;
				NewPass = GeneratedPass.Item3;
				SU.PasswordEncryptedText = HashPassword;
				SU.Salt = GeneratedPass.Item1;

				//user was created by an logged admin
				SU.RowCreatedSYSUserID = loggedUserId;
				SU.RowModifiedSYSUserID = loggedUserId;
				SU.RowCreatedDateTime = DateTime.Now;
				SU.RowModifiedDateTime = DateTime.Now;

				db.SYSUsers.Add(SU);
				//db.SaveChanges( );

				SYSUserProfile SUP = new SYSUserProfile( );
				SUP.SYSUserID = SU.SYSUserID;
				SUP.FirstName = USUV1.FirstName;
				SUP.LastName = USUV1.LastName;
				SUP.email = USUV1.email;
				SUP.RowCreatedSYSUserID = loggedUserId;
				SUP.RowModifiedSYSUserID = loggedUserId;
				SUP.RowCreatedDateTime = DateTime.Now;
				SUP.RowModifiedDateTime = DateTime.Now;
				db.SYSUserProfiles.Add(SUP);
				//db.SaveChanges( );

				SYSUserRole SUR = new SYSUserRole();
				SUR.LOOKUPRoleID = 2;//always set to User
				SUR.SYSUserID = SU.SYSUserID;
				SUR.IsActive = true;
				SUR.RowCreatedSYSUserID = loggedUserId;
				SUR.RowModifiedSYSUserID = loggedUserId;
				SUR.RowCreatedDateTime = DateTime.Now;
				SUR.RowModifiedDateTime = DateTime.Now;
				db.SYSUserRoles.Add(SUR);
				//db.SaveChanges( );

				USER_Preference UP1 = new USER_Preference();
				UP1.SYSUserID = SU.SYSUserID;

				db.SaveChanges();
			}
			return NewPass;
		}

		//-----------------------------------------------------
		//--check that all required fields of the object of the given type are not empty
		//-- ex: CheckObjectFields(USV1);
		//-- USV1 is an instance of the UserSignUpView
		public bool CheckObjectFields(object USV1) {
			if (USV1 == null)
				return false;

			bool AllFieldsNotEmpty = true;
			//get property description of the object
			PropertyDescriptorCollection PDCollection = TypeDescriptor.GetProperties(USV1);
			//loop through the properties
			foreach (PropertyDescriptor Pd1 in PDCollection) {
				bool isRequired = Pd1.Attributes.OfType<RequiredAttribute>().Any();// does property contain the "required" attribute
				if (isRequired) {
					var FieldValue = Pd1.GetValue(USV1);//get value of the Pd1 property of a givnen objext
					if (FieldValue == null || String.IsNullOrEmpty(FieldValue.ToString())){
						AllFieldsNotEmpty = false;
						break;
					}
				}
			}
			return AllFieldsNotEmpty;
		}


		//******************************************************
		//**Cheks if user's name exist
		public bool IsLoginNameExist(string loginName) {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				return db.SYSUsers.Where(o => o.LoginName.Equals(loginName)).Any( );
			}
		}

		//------------------------------------------------------
		//--Check email Adress format
		//------------------------------------------------------
		public bool CheckEmailFormat(string address) {

			//string expression = "/^(([^<>()\\[\\]\\.,;:\\s@\"]+(\\.[^<>()\\[\\]\\.,;:\\s@\"]+)*)|(\".+\"))@(([^<>()[\\]\\.,;:\\s@\"]+\\.)+[^<>()[\\]\\.,;:\\s@\"]{2,})$/i";
			string expression =
				@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
				+ @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
				+ @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
				+ @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

			if (!string.IsNullOrEmpty(address) && Regex.IsMatch(address, expression)) {
				if (Regex.Replace(address, expression, string.Empty).Length == 0) {
					return true;
				}
			}
			return false;
		}

		//------------------------------------------------------
		//--Check if email Adress is unique to given user
		//------------------------------------------------------
		public bool IsEmailUnique(string email1) {
			using (DemoDBEntities db = new DemoDBEntities()){
				var user = db.SYSUserProfiles
					.Where (us => (us.email == email1))
					.FirstOrDefault();

				bool EmailUnique = (user == null);
				return EmailUnique;
			}
		}

		//----------------------------------------------------------
		//--get hash password and salt from UserID, compare hashed password from the db with one delivered as parameter. If user doesnt exist, return empty string
		//------------------------------------------------------
		public bool ComparePasswords (int UserId, string PassToCompare) {

			using (DemoDBEntities db = new DemoDBEntities( )) {
				var user1 = db.SYSUsers.Find(UserId);

				if (user1 != null) {
					string HashPass = user1.PasswordEncryptedText;//get hashed pass string
					byte[] salt1= user1.Salt;//get salt binary
					string HashPassToCompare = Hmac1.GetHashString(PassToCompare, salt1);//hash password to compare
					bool ArePasswordsSame = HashPass.Equals(HashPassToCompare);//compare passwords
					return ArePasswordsSame;
				}
				else
					return false;//no user found
			}
		}

		//---------------------------------------------------------
		//--update user's password from unhashed string and users ID
		//---------------------------------------------------------
		public bool UpdatePassword (string newPassword, int LoggedUserId){
			using (DemoDBEntities db = new DemoDBEntities( )) {
				SYSUser User1 = db.SYSUsers.Find(LoggedUserId);

				if (User1 == null){
					return false;//no user found
				}
				else {
					byte[] salt1 = Hmac1.GenerateSalt();
					string NewHashPass = Hmac1.GetHashString(newPassword, salt1);
					User1.PasswordEncryptedText = NewHashPass;
					User1.Salt = salt1;
					User1.RowModifiedSYSUserID = LoggedUserId;
					User1.RowModifiedDateTime = DateTime.Now;
					db.SaveChanges( );
					return true;
				}
			}
		}

		//-----------------------------------------------------
		//generate new salt & password
		//---------------------------------------------------------
		public Tuple<byte[], string, string> GeneratePassword() {
			byte[] salt1 = Hmac1.GenerateSalt();//new salt
			string newPassword = Membership.GeneratePassword(12, 2);
			string hashNewPass = Hmac1.GetHashString(newPassword, salt1);//hash password
			return new Tuple<byte[], string, string>(salt1, hashNewPass, newPassword);
		}

		//******************************************************
		//**Check if user has a specified role (admin, member)
		//---------------------------------------------------------
		public bool IsUserInRole(int UserID, string roleName1) {
			int error1 = 0;//no error by default
			bool IsInSpecRole;

			int RoleId = GetUserRoleId(UserID, ref error1);

			if (RoleId > 0){
				using (DemoDBEntities db = new DemoDBEntities( )){
					//get role name from the role ID
					string RoleName = GetRoleName(RoleId);
					//is the role of User (by login name) the same as roleName1
					IsInSpecRole = (RoleName == roleName1);
				}
			}
			else {//if role is not "admin" nor "member"
				IsInSpecRole = false;
			}
			return IsInSpecRole;
		}

		//******************************************************
		//**get user's role Id (nobody: 0, admin: 1, member: 2)
		//---------------------------------------------------------
		public int GetUserRoleId (int UserId, ref int error1) {
			//UserRole = 0: nobody; 1: admin; 2: user
			int UserRoleId = 0;// set default
			error1 = 0;//no error by default

			using (DemoDBEntities db = new DemoDBEntities( )){
				SYSUser SU = db.SYSUsers.Find(UserId);//get logged user if exists

				//get collection of roles ID from SU (should be only 1)
				if (SU != null) {//if user exists
					var Roles = (
						from user in db.SYSUserRoles
						where user.SYSUserID.Equals(SU.SYSUserID)
						select user.LOOKUPRoleID
					);

					if (Roles.Any())
						UserRoleId = Roles.FirstOrDefault( );
					else {
						error1 = 1;//no role defined for the user
					}
				}
				else {//if user doesnt exist
					error1 = 2;// non -existant user error
				}
			}
			return UserRoleId;
		}

		//-----------------------------------------------------
		//**get role's name from the roleID
		//---------------------------------------------------------
		public string GetRoleName(int roleID){
			string RoleName = "";

			if (roleID > 0) {
				using (DemoDBEntities db = new DemoDBEntities( )) {
					//get role neam from role ID
					RoleName = (
						from Roles in db.LOOKUPRoles
						where Roles.LOOKUPRoleID == roleID
						select Roles.RoleName
					).FirstOrDefault( );
				}
			}
			return RoleName;
		}

		//******************************************************
		//**get the list of all possible roles, along with role ID, name and description
		//---------------------------------------------------------
		public List<LOOKUPAvailableRole> GetAllRoles() {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				var roles = db.LOOKUPRoles.Select(o => new LOOKUPAvailableRole {
					LOOKUPRoleID = o.LOOKUPRoleID,
					RoleName = o.RoleName,
					RoleDescription = o.RoleDescription
				}).ToList( );
				return roles;
			}
		}

		//******************************************************
		//**get the User's Id from the loginMane, returns 0 if doesnt exist
		//---------------------------------------------------------
		public int GetUserIdFromName(string loginName) {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				SYSUser user = db.SYSUsers
					.Where(o => (o.LoginName.Equals(loginName))).FirstOrDefault();
				if (user != null)
					return user.SYSUserID;
			}
			return 0;//if user is not found
		}

		//--------------------------------------------------------
		//--get user's Id from the email address
		//--------------------------------------------------------
		public int GetUserIdFromEmail(string email) {
			int UserId= 0;//no user found by default
			
			using (DemoDBEntities db = new DemoDBEntities()) {
				var SysUser1 = db.SYSUserProfiles
					.Where(x => x.email == email).FirstOrDefault();
				if (SysUser1 != null){
					UserId = SysUser1.SYSUserID;
				}					
			}
			return UserId;
		}

		//******************************************************
		//**return list of all user profiles-  With all the data from SYSUser and SYSUserProfile tables and his role, beginning from FirstUserNo for the next NoOfUsers (indexes starting from 1, not user ID's)
		//---------------------------------------------------------
		public List<UserProfileView> GetReqUserProfiles(int FirstUserNo, int NoOfUsers) {
			List<UserProfileView> profiles = new List<UserProfileView>( );

			using (DemoDBEntities db = new DemoDBEntities( )) {
				
				profiles = db.SYSUsers
				.OrderBy(x=>x.SYSUserID)
				.Skip(FirstUserNo - 1)
				.Take(NoOfUsers)
				.Select(x => new
					UserProfileView {
						SYSUserID = x.SYSUserID,
						IsRoleActive = x.SYSUserRoles
							.Select(c => c.IsActive).FirstOrDefault(),
						LOOKUPRoleID = x.SYSUserRoles
							.Select(c => c.LOOKUPRoleID).FirstOrDefault(),
						RoleName = x.SYSUserRoles
							.Select(c => c.LOOKUPRole.RoleName).FirstOrDefault(),
						LoginName = x.LoginName,
						FirstName = x.SYSUserProfiles
							.Select(c => c.FirstName).FirstOrDefault(),
						LastName = x.SYSUserProfiles
							.Select(c => c.LastName).FirstOrDefault(),
						email = x.SYSUserProfiles
							.Select(c => c.email).FirstOrDefault(),
					}
				).ToList();
			}
			return profiles;
		}

		//----------------------------------------------------------
		//get the number of users
		//----------------------------------------------------------
		public int GetNumOfUsers() {
			int NumOfUsers;
			using (DemoDBEntities db = new DemoDBEntities()) {
				NumOfUsers = db.SYSUsers.Count();
			}
			return NumOfUsers;
		}

		//******************************************************
		//**Get list of all users profiles and roles, beginning from index (1- based) FirstUserNo, LoggedId is the id of a logged admin
		//---------------------------------------------------------
		public UserDataView GetUserDataView(int FirstUserNo, int LoggedId) {
			int AllUsersNo = GetNumOfUsers();//get the number of all users
			if (FirstUserNo > AllUsersNo) {
				//if first requested users index is bigger than number of users
				return null;
			}

			int NoOfUsers;

			//get number of users to display from admin's User Preferences
			using (DemoDBEntities db = new DemoDBEntities()) {
				var UsrPrf1 = db.USER_Preferences.Where(x => (x.SYSUserID == LoggedId)).FirstOrDefault();
				NoOfUsers = (UsrPrf1 == null) ? 10 : (int) UsrPrf1.NoOfUsersDisplayed;
			}

				if (FirstUserNo + NoOfUsers > AllUsersNo) {
				//if index of the last user is greater than the number of users
				NoOfUsers = AllUsersNo - FirstUserNo+1;
			}
			
			//get user Profile List
			UserDataView UDV = new UserDataView( );
			List<UserProfileView> profiles = GetReqUserProfiles(FirstUserNo, NoOfUsers);
			List<LOOKUPAvailableRole> roles = GetAllRoles( );

			UDV.UserProfile = profiles;
			UDV.UserRoles = new UserRoles {
				SelectedRoleID =0,//any number
				UserRoleList = roles
			};
			return UDV;
		}

		//******************************************************
		//**Get single user profile and fills UserProfileView object
		//---------------------------------------------------------
		public UserProfileView GetUserProfile(int userID) {
			UserProfileView UPV = new UserProfileView( );

			using (DemoDBEntities db = new DemoDBEntities( )) {
				var user = db.SYSUsers.Find(userID);
				if (user != null) {
					UPV.SYSUserID = user.SYSUserID;
					UPV.LoginName = user.LoginName;

					//error, cant do it that way, userId is not a primary key for the SYSUserProfiles
					//var SUP = db.SYSUserProfiles.Find(userID);
					SYSUserProfile SUP = db.SYSUserProfiles
						.Where(x => (x.SYSUserID == userID))
						.FirstOrDefault( );
					if (SUP != null) {
						UPV.FirstName = SUP.FirstName;
						UPV.LastName = SUP.LastName;
						UPV.email = SUP.email;
					}

					//same as above
					//var SUR = db.SYSUserRoles.Find(userID);
					SYSUserRole SUR = db.SYSUserRoles
						.Where(x => (x.SYSUserID == userID))
						.FirstOrDefault( );
					if (SUR != null) {
						UPV.LOOKUPRoleID = SUR.LOOKUPRoleID;
						UPV.RoleName = SUR.LOOKUPRole.RoleName;
						UPV.IsRoleActive = SUR.IsActive;
					}
				}
			}
			return UPV;
		}

		//******************************************************
		//**
		//---------------------------------------------------------
		public bool UpdateUserAccount(UserProfileView UserUpdated, int loggedUserId) {

			bool success = true;

			using (DemoDBEntities db = new DemoDBEntities( )) {
				using (var dbContextTransaction = db.Database.BeginTransaction( )) {
					try {
						SYSUser SU = db.SYSUsers.Find(UserUpdated.SYSUserID);
						SU.LoginName = UserUpdated.LoginName;
						SU.RowModifiedSYSUserID = loggedUserId;
						SU.RowModifiedDateTime = DateTime.Now;
						db.SaveChanges( );

						var userProfile = db.SYSUserProfiles.Where(o => o.SYSUserID == UserUpdated.SYSUserID);
						if (userProfile.Any( )) {
							SYSUserProfile SUP = userProfile.FirstOrDefault( );
							SUP.SYSUserID = SU.SYSUserID;
							SUP.FirstName = UserUpdated.FirstName;
							SUP.LastName = UserUpdated.LastName;
							SUP.email = UserUpdated.email;;
							SUP.RowModifiedSYSUserID = loggedUserId;
							SUP.RowModifiedDateTime = DateTime.Now;
							db.SaveChanges( );
							//Controllers.HomeController.SendEmpToFile(SUP.LastName);
						}

						if (UserUpdated.LOOKUPRoleID > 0) {
							var userRole = db.SYSUserRoles.Where(o => o.SYSUserID == UserUpdated.SYSUserID);
							//SYSUserRole SUR = new SYSUserRole();//	null;

							if (userRole.Any( )) {
								SYSUserRole SUR = userRole.FirstOrDefault( );
								SUR.LOOKUPRoleID = UserUpdated.LOOKUPRoleID;
								SUR.SYSUserID = UserUpdated.SYSUserID;
								SUR.IsActive = true;
								SUR.RowModifiedSYSUserID = loggedUserId;
								SUR.RowModifiedDateTime = DateTime.Now;
							}
							else {
								SYSUserRole SUR = new SYSUserRole( );
								SUR.LOOKUPRoleID = UserUpdated.LOOKUPRoleID;
								SUR.SYSUserID = UserUpdated.SYSUserID;
								SUR.IsActive = true;
								SUR.RowCreatedSYSUserID = loggedUserId;
								SUR.RowModifiedSYSUserID = loggedUserId;
								SUR.RowCreatedDateTime = DateTime.Now;
								SUR.RowModifiedDateTime = DateTime.Now;
								db.SYSUserRoles.Add(SUR);
							}
							db.SaveChanges( );
						}
						dbContextTransaction.Commit( );
					}
					catch {
						dbContextTransaction.Rollback( );
						success = false;
					}
				}
			}
			return success;
		}

		//******************************************************
		//**
		//---------------------------------------------------------
		public void DeleteUser(int userID) {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				using (var dbContextTransaction = db.Database.BeginTransaction( )) {

					try {
						var SUR = db.SYSUserRoles.Where(o => o.SYSUserID == userID);
						if (SUR.Any( )) {
							db.SYSUserRoles.Remove(SUR.FirstOrDefault( ));
							db.SaveChanges( );
						}

						var SUP = db.SYSUserProfiles.Where(o => o.SYSUserID == userID);
						if (SUP.Any( )) {
							db.SYSUserProfiles.Remove(SUP.FirstOrDefault( ));
							db.SaveChanges( );
						}

						var UPr1 = db.USER_Preferences.Where(o => o.SYSUserID == userID);
						if (SUP.Any()) {
							db.USER_Preferences.Remove(UPr1.FirstOrDefault());
							db.SaveChanges();
						}

						var SU = db.SYSUsers.Find(userID);
						if (SU!= null) {
							db.SYSUsers.Remove(SU);
							db.SaveChanges( );
						}
						dbContextTransaction.Commit( );
					}
					catch {dbContextTransaction.Rollback( );}
				}
			}
		}

		//******************************************************
		//**GetAllMessages
		//---------------------------------------------------------
		public List<UserMessage> GetAllMessages() {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				var m = (
					from q in db.SYSUsers
					join q2 in db.Messages
						on q.SYSUserID equals q2.SYSUserID
					join q3 in db.SYSUserProfiles
						on q.SYSUserID equals q3.SYSUserID
					select new UserMessage {
						MessageID = q2.MessageID,
						SYSUserID = q.SYSUserID,
						LoginName = q.LoginName,
						MessageText = q2.MessageText,
						LogDate = q2.DatePosted
					}).OrderBy(o => o.LogDate);

				return m.ToList( );
			}
		}

		//--------------------------------------------------------
		//send mail informing client that he was registered
		//requires UserSignUpView parameter, Company URL (for link in the message body), and ref Error string
		//---------------------------------------------------------
		public bool SendRegistrationMail(UserSignUpView USUV1, string CompanyUrl, string Password1, ref string Error1) {

			string poruka = "Dear " + USUV1.FirstName + ",<br /><br />You have been registered with Legend Šped website. Your login credentials are:" + "<br />User Name:  " + USUV1.LoginName + "<br />Password:  " + Password1 + "<br />Best regards from " + CompanyUrl;//body of the message
			string subject = "LegendŠped - Registration Info";//naslov maila

			bool SuccessSending = SendEmail(USUV1.email, poruka, subject, ref Error1);

			return SuccessSending;
		}

		//--------------------------------------------------------
		//send mail with new password
		//---------------------------------------------------------
		public bool SendChangedPasswordMail(int UserId, string CompanyUrl, string Password1, ref string Error1) {
			bool SuccessSending;

			using (DemoDBEntities db = new DemoDBEntities()) {
				SYSUser SU1 = db.SYSUsers.Find(UserId);

				var UserEmailAndName = SU1.SYSUserProfiles
					//.Where(x => x.SYSUserID == UserId)
					.Select(x => new {
						x.email,
						x.FirstName
					}).FirstOrDefault();

				string poruka = "User " + UserEmailAndName.FirstName  + ",<br /><br />Your password has been changed. Your new login credentials:" + "<br />User Name:  " + SU1.LoginName + "<br />Password:  " + Password1 + "<br />Best regards from " + CompanyUrl;//body of the message
				string subject = "LegendŠped - Password Changed";//naslov maila

				SuccessSending = SendEmail(UserEmailAndName.email, poruka, subject, ref Error1);
			}

			return SuccessSending;
		}


		//--------------------------------------------------------
		//send mail to client general method. EmailAdress to be send to, poruka is HTML content of the message, Subject
		//---------------------------------------------------------
		public bool SendEmail(string EmailAdress, string poruka, string Subject, ref string ErrorMessage) {
			bool success = true;
			ErrorMessage = "";

			//get SMTP parameters from the db
			SMTPParamsView SMTP1 = GetSMTPParams();
			if (SMTP1 == null){
				ErrorMessage = "Server SMTP parameters not found.";
				return false;
			}

			var mes1 = new MimeMessage();
			mes1.From.Add(new MailboxAddress("noreply@mail.com"));//dati nešto bezveze za mail adresu
			mes1.To.Add(new MailboxAddress(EmailAdress));//upisati pravu mail adresu usera (string)
			mes1.Subject = Subject;//naslov maila
			mes1.Body = new TextPart(TextFormat.Html) { Text = poruka };// TextPart je "html" ili plain

			try {
				using (var c1 = new SmtpClient()) {
					c1.Connect(SMTP1.host, 587, SecureSocketOptions.StartTls);
					c1.Authenticate(SMTP1.UserEmail, SMTP1.password);
					c1.Send(mes1);
					c1.Disconnect(true);
				}
			}
			catch (Exception ex1) {
				success = false;
				ErrorMessage = ex1.ToString();
			}

			return success;
		}

		//-------------------------------------------------------
		//--put SMTP Parameters into db
		//---------------------------------------------------------
		public bool SetSMTPParams (SMTPParamsView Param1, int LoggedUserId) {
			bool Success = true;//operation successful by default

			try {
				using (DemoDBEntities db = new DemoDBEntities()) {
					var ParamList = db.Admin_SMTP_parameters;
					string EncyptedPass = Crypt_MD5.encryptPass(Param1.password);//encrypt new password

					if (!ParamList.Any()) {//if there are no parameteres defined
						Admin_SMTP_parameter NewParam1 = new Admin_SMTP_parameter {
							UserEmail = Param1.UserEmail,
							password = EncyptedPass,
							host = Param1.host,
							RowCreatedSYSUserID= LoggedUserId,
							RowCreatedDateTime = DateTime.Now,
							RowModifiedSYSUserID = LoggedUserId,
							RowModifiedDateTime = DateTime.Now
						};
						ParamList.Add(NewParam1);
					}
					else {//otherwise, update existing data
						Admin_SMTP_parameter ExistingParam = ParamList.FirstOrDefault();
						ExistingParam.UserEmail = Param1.UserEmail;
						ExistingParam.password = EncyptedPass;
						ExistingParam.host = Param1.host;
						ExistingParam.RowModifiedSYSUserID = LoggedUserId;
						ExistingParam.RowModifiedDateTime = DateTime.Now;
					}
					db.SaveChanges();
				}
			}
			catch {
				Success = false;
			}
			return Success;
		}

		//-------------------------------------------------------
		//--get SMTP Parameter from db and pack it into SMTPParamsView object
		//---------------------------------------------------------
		public SMTPParamsView GetSMTPParams(){
			try {
				using (DemoDBEntities db = new DemoDBEntities()) {
					var Param1 = db.Admin_SMTP_parameters.FirstOrDefault();//get first set of Admin_SMTP_parameter
					if (Param1 != null) {//if any param element is defined
						SMTPParamsView ParamView1 = new SMTPParamsView();
						ParamView1.UserEmail = Param1.UserEmail;
						string DecyptedPass = Crypt_MD5.decryptPass(Param1.password);//dencrypt new password
						ParamView1.password = DecyptedPass;
						ParamView1.host = Param1.host;
						return ParamView1;
					}
					else {//if there is nothing in the db
						return null;
					}
				}
			}
			catch {//if anything is wrong
				return null;
			}
		}


		//******************************************************
		//** AddMessage
		//---------------------------------------------------------
		public void AddMessage(int userID, string messageText) {
			using (DemoDBEntities db = new DemoDBEntities( )) {
				Message m = new Message( );
				m.MessageText = messageText;
				m.SYSUserID = userID;
				m.DatePosted = DateTime.UtcNow;

				db.Messages.Add(m);
				db.SaveChanges( );
			}
		}
	}
}
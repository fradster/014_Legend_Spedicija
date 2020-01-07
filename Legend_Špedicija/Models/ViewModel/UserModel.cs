using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Legend_Špedicija.Models.ViewModel {

	//------------------------------------------------------------
	//View model for the Add User form
	//------------------------------------------------------------
	public class UserSignUpView {
		[Required(ErrorMessage = "*")] [Display(Name = "Login ID")]
		public string LoginName { get; set; }

		[Required(ErrorMessage = "*")] [Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "*")] [Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Email adress required")]
		[EmailAddress(ErrorMessage = "Email adress format not valid")]
		[Display(Name = "E-mail adress")]
		public string email { get; set; }
	}

	//------------------------------------------------------------
	//View model for the Login form
	//------------------------------------------------------------
	public class UserLoginView {
		//[Key]
		//public int SYSUserID { get; set; }

		[Required(ErrorMessage = "*")] [Display(Name = "Login ID")]
		public string LoginName { get; set; }

		[Required(ErrorMessage = "*")] [DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }
	}

	//*********************************************************
	//** view class for the list of users
	//*********************************************************
	public class UserProfileView {
		[Key]
		public int SYSUserID { get; set; }

		public bool? IsRoleActive { get; set; }

		public int LOOKUPRoleID { get; set; }

		[Display(Name = "Role")]
		public string RoleName { get; set; }

		[Required(ErrorMessage = "User Name is required")]
		[Display(Name = "User Name")]
		public string LoginName { get; set; }

		[Required(ErrorMessage = "*")] [Display(Name = "First Name")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "*")] [Display(Name = "Last Name")]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Email adress required")]
		[EmailAddress(ErrorMessage = "Email adress format not valid")]
		[Display(Name = "E-mail adress")]
		public string email { get; set; }
	}

	//*********************************************************
	//** class represents role id, name and description
	//*********************************************************
	public class LOOKUPAvailableRole {
		[Key]
		public int LOOKUPRoleID { get; set; }
		public string RoleName { get; set; }
		public string RoleDescription { get; set; }
	}

	//*********************************************************
	//**class UserMessage
	//*********************************************************
	public class UserMessage {
		public int MessageID { get; set; }
		public int SYSUserID { get; set; }
		public string LoginName { get; set; }
		public string MessageText { get; set; }
		public DateTime? LogDate { get; set; }
	}

	//*********************************************************
	//** selected Role (id) and list if all roles (Ids, Names, descriptions)
	//*********************************************************
	public class UserRoles {
		public int? SelectedRoleID { get; set; }
		public IEnumerable<LOOKUPAvailableRole> UserRoleList { get; set; }
	}

	//*********************************************************
	//** UserDataView is model to fill-in User list. Contains UserProfile list of each user data, with UserRoles that contains list of all users Roles (Ids, Names, descriptions) and selected User roleId (why?)
	//*********************************************************
	public class UserDataView {
		public IEnumerable<UserProfileView> UserProfile { get; set; }
		public UserRoles UserRoles { get; set; }
	}


	//*********************************************************
	//** Smpt Parameters view
	//*********************************************************
	public class SMTPParamsView {
		[Display (Name = "User Email")] [Required(ErrorMessage="Email address required")]	[DataType(DataType.EmailAddress)]
		public string UserEmail { get; set; }

		[Display(Name = "Password")] [Required(ErrorMessage = "Password required")] [DataType(DataType.Password)]
		public string password { get; set; }

		[Display(Name = "Host Adress")] [Required(ErrorMessage = "Host address required")]
		public string host { get; set; }
	}
}
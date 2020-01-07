using System.Web;
using System.Web.Mvc;
using Legend_Špedicija.Models.DB;
using Legend_Špedicija.Models.EntityManager;

namespace MVC5RealWorld.Security {

	public class AuthorizeRolesAttribute : AuthorizeAttribute {

		//array of all possible roles (admin, member,...)
		private readonly string[] userAssignedRoles;

		//******************************************************
		//**1 param. constructor, fetch the role(s) from the method annotation filter
		public AuthorizeRolesAttribute (params string[] roles) {
			this.userAssignedRoles = roles;
		}

		//******************************************************
		//**
		protected override bool AuthorizeCore (HttpContextBase httpContext) {
			bool authorize = false;

			//using (DemoDBEntities db = new DemoDBEntities()) {
			UserManager UM = new UserManager();
			int UserID = UM.GetUserIdFromName(httpContext.User.Identity.Name);

			//loop through list of roles, checks if user's login name 
			foreach (string role in userAssignedRoles) {
				authorize = UM.IsUserInRole (UserID, role);

				if (authorize)
					break;
			}
			//}
			return authorize;
		}

		//-------------------------------------------------
		//--
		protected override void HandleUnauthorizedRequest (AuthorizationContext filterContext) {
			filterContext.Result = new RedirectResult("~/Home/Index");
		}
	}
}
using System.Web;
using System.Web.Optimization;

namespace Legend_Špedicija {
	public class BundleConfig {
		// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862

		public static void RegisterBundles(BundleCollection bundles1) {

			bundles1.Add(new ScriptBundle("~/bundles/jquery")
				.Include("~/Scripts/jquery-{version}.js")
				.Include ("~/Scripts/jquery-ui-{version}.js")
				.Include("~/Scripts/jquery.easing.{version}.js")
				.Include("~/Scripts/jquery-migrate-{version}.js")
				.Include("~/Scripts/jquery.validate*")
				.Include("~/Scripts/ext.Plugins/jquery.appear.js")
				.Include("~/Scripts/ext.Plugins/jquery.sticky.js")
			);

			bundles1.Add(new ScriptBundle("~/bundles/bootstrap")
				.Include("~/Scripts/bootstrap.js")
				.Include("~/Scripts/respond.js")
			);

			bundles1.Add(new ScriptBundle("~/bundles/Plugins")
				.Include("~/Scripts/owl.carousel.js")
				.Include("~/Scripts/ext.Plugins/stellar.js")
				.Include("~/Scripts/ext.Plugins/wow.min.js")
				.Include("~/Scripts/ext.Plugins/stellarnav.min.js")
				.Include("~/Scripts/ext.Plugins/contact-form.js")
			);

			//main.js doesnt work properly when bundled, (menu not showing when screen is resized for mobile)
			/*bundles1.Add(new ScriptBundle("~/bundles/Main1")
				.Include("~/Scripts/ext.Plugins/main.js")
			);*/

			// Use the development version of Modernizr to develop with and learn from. Then, when you're ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
			bundles1.Add(new ScriptBundle("~/bundles/modernizr")
				.Include("~/Scripts/modernizr-*")
			);

			//styles
			bundles1.Add(new StyleBundle("~/Content/css")
				.Include("~/Content/normalize.css")
				.Include("~/Content/animate.css")
				.Include("~/Content/Added/stellarnav.min.css")
				.Include("~/Content/OwlCarousel/owl.carousel.css")
				.Include("~/Content/bootstrap.css")
				.Include("~/Content/font-awesome.css")
				.Include("~/Content/Added/style.css")
				.Include("~/Content/Added/responsive.css")

				//site.css leaves white stripe at the top of the screen
				//.Include("~/Content/site.css")
				.Include("~/Content/themes/base/all.css")
			);
		}
	}
}

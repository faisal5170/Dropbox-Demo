using Dropbox.Api;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DropboxDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly string appKey;
        private readonly string appSecret;

        public HomeController()
        {
            appKey = "YOUR-APP-KEY";
            appSecret = "YOUR-ACCESS-SECRET";
        }

        private string RedirectUri
        {
            get
            {
                if (Request.Url.Host.ToLowerInvariant() == "localhost")
                {
                    return string.Format("http://{0}:{1}/Home/Auth", this.Request.Url.Host, this.Request.Url.Port);
                }

                var builder = new UriBuilder(
                    Uri.UriSchemeHttps,
                    this.Request.Url.Host)
                {
                    Path = "/Home/Auth"
                };

                return builder.ToString();
            }
        }
        
        // GET: /Home/Auth
        public async Task<ActionResult> Auth(string code, string state)
        {
            try
            {

                var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                    code,
                    appKey,
                    appSecret,
                    this.RedirectUri);

                var dropboxAccessToken = response.AccessToken;
                Session["Token"] = dropboxAccessToken;
            }
            catch (Exception e)
            {
            }
            return RedirectToAction("IndexSuccess");
        }

        public ActionResult IndexSuccess()
        {
            return View("Index");
        }

        public ActionResult Index()
        {
            var state = Guid.NewGuid().ToString("N");

            var redirect = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Code,
                appKey,
                RedirectUri,
                state);

            return Redirect(redirect.ToString());
        }

        public async Task<ActionResult> TestAccessToken()
        {
            try
            {

                using (var dbx = new DropboxClient(Session["Token"].ToString()))
                {
                    var full = await dbx.Users.GetCurrentAccountAsync();
                    var name = full.Name.DisplayName;
                    var email = full.Email;
                }
            }
            catch (Exception e)
            {
            }
            return RedirectToAction("IndexSuccess");
        }
    }
}
using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookingWebClient.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient client = null;
        private string AccountAPiUrl = "";
        private string RoomAPiUrl = "";
        public AccountController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            AccountAPiUrl = "https://localhost:7159/api/Account";
            RoomAPiUrl = "https://localhost:7159/api/Room";
        }
        public async Task<Account> getUser()
        {
            var iduse = HttpContext.Session.GetString("IdUser");
            if (iduse != null)
            {
                HttpResponseMessage response = await client.GetAsync(AccountAPiUrl + "/id?id=" + iduse);
                string strDate = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                Account account = JsonSerializer.Deserialize<Account>(strDate, options);
                return account;
            }
            return null;
        }

        public IActionResult Login()
        {
            return View();
        }
        public async Task<IActionResult> Admin()
        {
            ViewBag.username = await getUser();

            HttpResponseMessage response = await client.GetAsync(RoomAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Room> listRooms = JsonSerializer.Deserialize<List<Room>>(strDate, options);

            return View(listRooms);
        }

        public async Task<IActionResult> customers()
        {
            ViewBag.username = await getUser();

            HttpResponseMessage response = await client.GetAsync(AccountAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Account> listAccounts = JsonSerializer.Deserialize<List<Account>>(strDate, options);
            return View(listAccounts);
        }

        public async Task<IActionResult> InfoUsser()
        {
            var idusr = HttpContext.Session.GetString("IdUser");
            HttpResponseMessage response = await client.GetAsync(AccountAPiUrl + "/id?id=" + idusr);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Account account = JsonSerializer.Deserialize<Account>(strDate, options);

            return View(account);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InfoUsser([Bind("Idacc,Mail,Password,FullName,Phone,St")] Account account)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response1 = await client.PutAsJsonAsync(AccountAPiUrl + "/id?id=" + account.Idacc, account);
                response1.EnsureSuccessStatusCode();
                return RedirectToAction("Index", "Room");
            }

            ViewBag.regis = 1;
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {

            HttpResponseMessage response = await client.GetAsync(AccountAPiUrl + "/" + email + "/" + password);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Account account = JsonSerializer.Deserialize<Account>(strDate, options);

            if (account != null)
            {
                HttpContext.Session.SetString("IdUser", account.Idacc);
                if(account.St==2)
                    return RedirectToAction("Admin");
                else 
                    return RedirectToAction("Index", "Room");


            }
            else
            {
                return View("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Idacc,Mail,Password,FullName,Phone,St")] Account account)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response1 = await client.PostAsJsonAsync(AccountAPiUrl, account);
                response1.EnsureSuccessStatusCode();

                HttpResponseMessage response = await client.GetAsync(AccountAPiUrl + "/" + account.Mail + "/" + account.Password);
                string strDate = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                account = JsonSerializer.Deserialize<Account>(strDate, options);

                HttpContext.Session.SetString("IdUser", account.Idacc);

                return RedirectToAction("Index", "Room");
            }

            ViewBag.regis = 1;
            return View("Login");
        }
    }
}


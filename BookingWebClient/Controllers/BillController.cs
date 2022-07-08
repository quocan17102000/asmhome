using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookingWebClient.Controllers
{
    public class BillController : Controller
    {
        private readonly HttpClient client = null;
        private string BillAPiUrl = "";
        private string AccountAPiUrl = "";


        public BillController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            BillAPiUrl = "https://localhost:7159/api/Bill";
            AccountAPiUrl = "https://localhost:7159/api/Account";

        }
        public async Task<Account> getUser()
        {
            var idusr = HttpContext.Session.GetString("IdUser");
            if (idusr != null)
            {
                HttpResponseMessage response = await client.GetAsync(AccountAPiUrl + "/id?id=" + idusr);
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
        public async Task<IActionResult> Index(string? id)
        {
            ViewBag.username = await getUser();

            HttpResponseMessage response = await client.GetAsync(BillAPiUrl + "/idacc?idacc=" + id);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Bill> listCommets = JsonSerializer.Deserialize<List<Bill>>(strDate, options);

            return View(listCommets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdBill,Rate,Description,Idacc")] Bill Bill)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response1 = await client.PostAsJsonAsync(BillAPiUrl, Bill);
                response1.EnsureSuccessStatusCode();

                return RedirectToAction("Index");
            }

            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("IdBill,Rate,Description,Idacc")] Bill Bill)
        {
            if (id != Bill.Idbill)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    HttpResponseMessage response1 = await client.PutAsJsonAsync(
                     BillAPiUrl + "/id?id=" + id, Bill);
                    response1.EnsureSuccessStatusCode();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }
                return RedirectToAction(nameof(Index));
            }
            return View("Index");
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            HttpResponseMessage response1 = await client.DeleteAsync(
                     BillAPiUrl + "/id?id=" + id);
            response1.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }
    }
}

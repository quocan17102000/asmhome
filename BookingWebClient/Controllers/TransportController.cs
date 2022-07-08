using DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BookingWebClient.Controllers
{
    public class TransportController : Controller
    {
        private readonly HttpClient client = null;
        private string TransportAPiUrl = "";
        private string TypeTransportAPiUrl = "";
        private string BookingTransportDetailAPiUrl = "";
        private string AccountAPiUrl = "";
        private string BillAPiUrl = "";
        public TransportController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            TransportAPiUrl = "https://localhost:7159/api/Transport";
            TypeTransportAPiUrl = "https://localhost:7159/api/TypeTransport";
            AccountAPiUrl = "https://localhost:7159/api/Account";
            BillAPiUrl = "https://localhost:7159/api/Bill";
            BookingTransportDetailAPiUrl = "https://localhost:7159/api/BookingTransportDetail";

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
        public async Task<IActionResult> transport()
        {
            ViewBag.username = await getUser();

            HttpResponseMessage response = await client.GetAsync(TransportAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Transport> listTransports = JsonSerializer.Deserialize<List<Transport>>(strDate, options);

            return View(listTransports);
            return View();
        }

        public async Task<IActionResult> Index()
        {

            HttpResponseMessage response = await client.GetAsync(TransportAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Transport> listTransports = JsonSerializer.Deserialize<List<Transport>>(strDate, options);

            return View(listTransports);

        }
        public async Task<List<Transport>> GetTransportbytype(string type)
        {
            HttpResponseMessage response = await client.GetAsync(TransportAPiUrl + "/type?Type=" + type);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Transport> Transports = JsonSerializer.Deserialize<List<Transport>>(strDate, options);
            return Transports;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addmotobike(int motobike)
        {
            bool icheck = true;
            ViewBag.username = await getUser();
            Account user = await getUser();
            var idbill = HttpContext.Session.GetString("Idbill");

            List<Transport> listMoto = await GetTransportbytype("TT002");
            if (listMoto != null && listMoto.Count >= motobike)
            {
                HttpResponseMessage response = await client.GetAsync(BookingTransportDetailAPiUrl + "/idbill?id=" + idbill);
                string strDate1 = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                List<BookingTransportDetail> listdetaiTransports = JsonSerializer.Deserialize<List<BookingTransportDetail>>(strDate1, options);
                if (listdetaiTransports == null || listdetaiTransports.SingleOrDefault(t => t.Idtransport.Equals("T0002")) == null)
                {

                    BookingTransportDetail detail = new BookingTransportDetail();
                    detail.IdbookingTransportDetail = "BT001";
                    detail.Idbill = idbill;
                    detail.Idtransport = "T0002";
                    detail.Price = listMoto.SingleOrDefault(t => t.Idtransport.Equals("T0002")).Price;
                    detail.St = 0;
                    detail.Quantity = motobike;
                    response = await client.PostAsJsonAsync(BookingTransportDetailAPiUrl, detail);
                    response.EnsureSuccessStatusCode();


                    response = await client.GetAsync(BillAPiUrl + "/id?id=" + idbill);
                    string strDate2 = await response.Content.ReadAsStringAsync();

                    Bill bill = JsonSerializer.Deserialize<Bill>(strDate2, options);

                    bill.Price += (listMoto.SingleOrDefault(t => t.Idtransport.Equals("T0002")).Price * motobike);

                    response = await client.PutAsJsonAsync(BillAPiUrl + "/id?id=" + bill.Idbill, bill);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    BookingTransportDetail detail = listdetaiTransports.SingleOrDefault(t => t.Idtransport.Equals("T0002"));
                    if (detail.Quantity != motobike)
                    {
                        int a = detail.Quantity;
                        detail.Quantity = motobike  ;
                        response = await client.PutAsJsonAsync(BookingTransportDetailAPiUrl + "/id?id=" + detail.IdbookingTransportDetail, detail);
                        response.EnsureSuccessStatusCode();


                        response = await client.GetAsync(BillAPiUrl + "/id?id=" + idbill);
                        string strDate2 = await response.Content.ReadAsStringAsync();

                        Bill bill = JsonSerializer.Deserialize<Bill>(strDate2, options);

                        bill.Price = (bill.Price-(a*detail.Price))+(detail.Price*motobike);

                        response = await client.PutAsJsonAsync(BillAPiUrl + "/id?id=" + bill.Idbill, bill);
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            return RedirectToAction("Index");

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> addbycicle(int bycicle)
        {

            ViewBag.username = await getUser();
            Account user = await getUser();
            var idbill = HttpContext.Session.GetString("Idbill");

            List<Transport> listMoto = await GetTransportbytype("TT001");
            if (listMoto != null && listMoto.Count >= bycicle)
            {
                HttpResponseMessage response = await client.GetAsync(BookingTransportDetailAPiUrl + "/idbill?id=" + idbill);
                string strDate1 = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                List<BookingTransportDetail> listdetaiTransports = JsonSerializer.Deserialize<List<BookingTransportDetail>>(strDate1, options);
                if (listdetaiTransports == null || listdetaiTransports.SingleOrDefault(t => t.Idtransport.Equals("T0001")) == null)
                {

                    BookingTransportDetail detail = new BookingTransportDetail();
                    detail.IdbookingTransportDetail = "BT001";
                    detail.Idbill = idbill;
                    detail.Idtransport = "T0001";
                    detail.Price = listMoto.SingleOrDefault(t => t.Idtransport.Equals("T0001")).Price;
                    detail.St = 0;
                    detail.Quantity = bycicle;
                    response = await client.PostAsJsonAsync(BookingTransportDetailAPiUrl, detail);
                    response.EnsureSuccessStatusCode();


                    response = await client.GetAsync(BillAPiUrl + "/id?id=" + idbill);
                    string strDate2 = await response.Content.ReadAsStringAsync();

                    Bill bill = JsonSerializer.Deserialize<Bill>(strDate2, options);

                    bill.Price += (listMoto.SingleOrDefault(t => t.Idtransport.Equals("T0001")).Price * bycicle);

                    response = await client.PutAsJsonAsync(BillAPiUrl + "/id?id=" + bill.Idbill, bill);
                    response.EnsureSuccessStatusCode();
                }
                else
                {
                    BookingTransportDetail detail = listdetaiTransports.SingleOrDefault(t => t.Idtransport.Equals("T0001"));
                    if (detail.Quantity != bycicle)
                    {
                        int a = detail.Quantity;
                        detail.Quantity = bycicle;
                        response = await client.PutAsJsonAsync(BookingTransportDetailAPiUrl + "/id?id=" + detail.IdbookingTransportDetail, detail);
                        response.EnsureSuccessStatusCode();


                        response = await client.GetAsync(BillAPiUrl + "/id?id=" + idbill);
                        string strDate2 = await response.Content.ReadAsStringAsync();

                        Bill bill = JsonSerializer.Deserialize<Bill>(strDate2, options);

                        bill.Price = (bill.Price - (a * detail.Price)) + (detail.Price * bycicle);

                        response = await client.PutAsJsonAsync(BillAPiUrl + "/id?id=" + bill.Idbill, bill);
                        response.EnsureSuccessStatusCode();
                    }
                }

            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            HttpResponseMessage response = await client.GetAsync(TransportAPiUrl + "/id?id=" + id);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Transport Transport = JsonSerializer.Deserialize<Transport>(strDate, options);
            if (Transport == null)
            {
                return NotFound();
            }

            return View(Transport);
        }

        public async Task<IActionResult> Create()
        {
            HttpResponseMessage response = await client.GetAsync(TypeTransportAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            List<TypeTransport> list = JsonSerializer.Deserialize<List<TypeTransport>>(strDate, options);
            ViewData["TypeTransportID"] = new SelectList(list, "IdtypeTransport", "NameTranspors");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Idtransport,IdtypeTransport,Price,Description")] Transport Transport)
        {
            if (ModelState.IsValid)
            {
                HttpResponseMessage response1 = await client.PostAsJsonAsync(TypeTransportAPiUrl, Transport);
                response1.EnsureSuccessStatusCode();

                return RedirectToAction("Index");
            }


            HttpResponseMessage response = await client.GetAsync(TypeTransportAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            List<TypeTransport> list = JsonSerializer.Deserialize<List<TypeTransport>>(strDate, options);
            ViewData["TypeTransportID"] = new SelectList(list, "IdtypeTransport", "NameTranspors");
            return View(Transport);
        }



        // GET: Transports/Edit/5
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HttpResponseMessage response = await client.GetAsync(TransportAPiUrl + "/id?id=" + id);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Transport Transport = JsonSerializer.Deserialize<Transport>(strDate, options);

            if (Transport == null)
            {
                return NotFound();

            }
            response = await client.GetAsync(TypeTransportAPiUrl);
            strDate = await response.Content.ReadAsStringAsync();


            List<TypeTransport> list = JsonSerializer.Deserialize<List<TypeTransport>>(strDate, options);
            ViewData["TypeTransportID"] = new SelectList(list, "IdtypeTransport", "NameTranspors");

            return View(Transport);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Idtransport,IdtypeTransport,Price,Description")] Transport Transport)
        {
            if (id.Equals(Transport.Idtransport))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    HttpResponseMessage response1 = await client.PutAsJsonAsync(
                     TransportAPiUrl + "/id?id=" + id, Transport);
                    response1.EnsureSuccessStatusCode();
                }
                catch (DbUpdateConcurrencyException)
                {

                    throw;

                }
                return RedirectToAction(nameof(Index));
            }

            HttpResponseMessage response = await client.GetAsync(TypeTransportAPiUrl);
            string strDate = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            List<TypeTransport> list = JsonSerializer.Deserialize<List<TypeTransport>>(strDate, options);
            ViewData["TypeTransportID"] = new SelectList(list, "IdtypeTransport", "NameTranspors");

            return View(Transport);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            HttpResponseMessage response1 = await client.DeleteAsync(
                     TransportAPiUrl + "/id?id=" + id);
            response1.EnsureSuccessStatusCode();

            return RedirectToAction(nameof(Index));
        }
    }
}

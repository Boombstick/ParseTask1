using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;

namespace ParsingTask1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SubscriberController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        public SubscriberController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        // GET: SubscriberController/Create
        [HttpPost("Subscribe")]
        public async Task<ActionResult> Create(string email, string link)
        {
            if (email is null || link is null) return BadRequest();
            var subscriber = await _databaseContext.Subscribers.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (subscriber == null)
            {
                subscriber = new Subscriber()
                {
                    Email = email,
                    AdsLinks = link
                };
                _databaseContext.Subscribers.Add(subscriber);
                await _databaseContext.SaveChangesAsync();
                return Ok();
            }

            subscriber.AdsLinks += $"|{link}";
            await _databaseContext.SaveChangesAsync();
            return Ok();
        }


        // GET: SubscriberController/Delete/5
        [HttpDelete("Unsubscribe")]
        public async Task<ActionResult> Unsubscribe(string email, string link)
        {

            if (email is null || link is null) return BadRequest();
            var subscriber = await _databaseContext.Subscribers.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (subscriber == null) return NotFound();

            var links = subscriber.AdsLinks.Split('|').ToList();

            foreach (var l in links)
            {
                if (l.Equals(link))
                {
                    links.Remove(l);
                    break;
                }
            }
            subscriber.AdsLinks = String.Join("|", links);
            await _databaseContext.SaveChangesAsync();
            return Ok();

        }
        [HttpGet("AllSubscriptions")]
        public async Task<ActionResult> GetAllSubscriptions(string email)
        {
            if (email is null) return BadRequest();
            var subscriber = await _databaseContext.Subscribers.FirstOrDefaultAsync(x => x.Email.Equals(email));
            if (subscriber == null) return NotFound();
            var viewModels = new List<ViewModel>();
            var links = subscriber.AdsLinks.Split('|').ToList();
            foreach (var l in links)
            {
                var model = await GetViewModel(l);
                viewModels.Add(model);
            }
            var response = JsonConvert.SerializeObject(viewModels);

            return Ok(response);
        }

        private async Task<ViewModel> GetViewModel(string link)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = DecompressionMethods.All;

            HttpClient client = new HttpClient(handler);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{link}?ajax=1&similar=1");

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            dynamic oble = JsonConvert.DeserializeObject(responseBody);
            return new ViewModel { Price = (string)oble.price, Link = link };
        }

    }
}

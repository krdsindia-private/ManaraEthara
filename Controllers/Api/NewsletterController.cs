using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Core.Services;

using Umbraco.Cms.Web.Common.Controllers;


namespace ManarEthara.Controllers.Api {

    public class NewletterRequest {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string token { get; set; }
    }

    public class NewsletterController : UmbracoApiController {
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly CaptchaValidator _captchaValidator;

        public NewsletterController(IContentService contentService, ILocalizedTextService localizedTextService, CaptchaValidator captchaValidator) {
            _contentService = contentService;
            _localizedTextService = localizedTextService;
            _captchaValidator = captchaValidator;
        }

        [HttpPost]
        public async Task<IActionResult> newsletter_requests(NewletterRequest model) {
            if (model == null) {
                var errorResponse = new NewsletterResponse {
                    Status = 400,
                    Message = "Invalid data"
                };

                return BadRequest(errorResponse);
            }

            try {

                if (!await _captchaValidator.IsCaptchaValid(model.token)) {
                    var captchaErrorResponse = new EnquiryResponse {
                        Status = 400,
                        Message = "Captcha validation failed"
                    };

                    return BadRequest(captchaErrorResponse);
                }
                var contentService = _contentService;


                var parentItem = contentService.GetById(Guid.Parse("1c3c72df-9fdd-454e-bed4-728ae202ae04"));

                if (parentItem == null) {
                    return NotFound("Parent content item not found");
                }

                long totalRecords;


                var existingNewsletterRequest = contentService.GetPagedDescendants(
                    parentItem.Id,
                    0,
                    int.MaxValue,
                    out totalRecords,
                    null,
                    null
                ).FirstOrDefault(node => node.GetValue<string>("email") == model.email);

                if (existingNewsletterRequest != null) {

                    var errorResponse = new NewsletterResponse {
                        Status = 400,
                        Message = "An entry with the same email address already exists."
                    };
                    return BadRequest(errorResponse);
                }


                var newsletterRequestNode = contentService.CreateContent(
                    model.firstName + " " + model.lastName,
                    parentItem.GetUdi(),
                    "NewsletterRequest");

                newsletterRequestNode.SetValue("firstname", model.firstName);
                newsletterRequestNode.SetValue("lastname", model.lastName);
                newsletterRequestNode.SetValue("email", model.email);




                contentService.SaveAndPublish(newsletterRequestNode);

                var successResponse = new NewsletterResponse {
                    Status = 200,
                    Message = "Newsletter request created and published successfully"
                };

                return Ok(successResponse);

            }
            catch (Exception ex) {

                var errorResponse = new NewsletterResponse {
                    Status = 500,
                    Message = ex.Message
                };

                return StatusCode(500, errorResponse);
            }
        }


        [HttpGet]
        public IActionResult ExportNewsletterRequestsToCsv() {
            try {

                var contentService = _contentService;


                var parentItem = contentService.GetById(Guid.Parse("fff6917a-326f-4146-9474-6ecc8e9ee382"));

                if (parentItem == null) {
                    return NotFound("Parent content item not found");
                }


                var newsletterRequestNodes = contentService.GetPagedChildren(parentItem.Id, 0, int.MaxValue, out _)
               .Where(node => node.Published)
               .ToList();


                if (!newsletterRequestNodes.Any()) {
                    return NotFound("No published newsletter requessts found");
                }


                var newsletterRequests = new List<NewletterRequest>();

                foreach (var node in newsletterRequestNodes) {

                    var firstname = node.GetValue<string>("firstname");
                    var lastname = node.GetValue<string>("lastname");
                    var email = node.GetValue<string>("email");

                    newsletterRequests.Add(new NewletterRequest {
                        firstName = firstname,
                        lastName = lastname,
                        email = email
                    });
                }


                var csvData = ExportToCsv(newsletterRequests);
                var csvFileName = "newsletter_requests.csv";
                var csvFileBytes = System.Text.Encoding.UTF8.GetBytes(csvData);

                return File(csvFileBytes, "text/csv", csvFileName);
            }
            catch (Exception ex) {

                return StatusCode(500, ex.Message);
            }
        }

        private string ExportToCsv(IEnumerable<NewletterRequest> newsletterRequests) {
            var csv = new System.Text.StringBuilder();

            csv.AppendLine("Firstname,Lastname,Email");

            foreach (var request in newsletterRequests) {
                csv.AppendLine($"{request.firstName},{request.lastName},{request.email}");
            }


            return csv.ToString();
        }
    }
}

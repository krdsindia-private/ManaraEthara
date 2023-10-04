
using Microsoft.AspNetCore.Mvc;

using Umbraco.Cms.Core.Services;

using Umbraco.Cms.Web.Common.Controllers;


namespace ManarEthara.Controllers.Api {

    public class EnquiryRequest {
        public string fullName { get; set; }
        public string email { get; set; }
        public string mobile { get; set; }
        public string message { get; set; }
        public string token { get; set; }

    }

    public class EnquiryController : UmbracoApiController {
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly CaptchaValidator _captchaValidator;

        public EnquiryController(IContentService contentService, ILocalizedTextService localizedTextService, CaptchaValidator captchaValidator) {
            _contentService = contentService;
            _localizedTextService = localizedTextService;
            _captchaValidator = captchaValidator;
        }

        public async Task<IActionResult> contactenquiry(EnquiryRequest model) {

            if (model == null) {
                var errorResponse = new EnquiryResponse {
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


                var parentItem = contentService.GetById(Guid.Parse("25993880-8e35-4be3-87d7-1c15e5786a71"));

                if (parentItem == null) {
                    return NotFound("Parent content item not found");
                }


                var enquiryRequestNode = contentService.CreateContent(
                    model.fullName + " " + model.email,
                    parentItem.GetUdi(),
                    "Enquiry");


                enquiryRequestNode.SetValue("enquiryname", model.fullName);
                enquiryRequestNode.SetValue("email", model.email);
                enquiryRequestNode.SetValue("mobilenumber", model.mobile);
                enquiryRequestNode.SetValue("message", model.message);


                contentService.SaveAndPublish(enquiryRequestNode);

                var successResponse = new EnquiryResponse {
                    Status = 200,
                    Message = "Enquiry submitted successfully"
                };

                return Ok(successResponse);
            }
            catch (Exception ex) {

                var errorResponse = new EnquiryResponse {
                    Status = 500,
                    Message = ex.Message
                };

                return StatusCode(500, errorResponse);
            }
        }



    }
}

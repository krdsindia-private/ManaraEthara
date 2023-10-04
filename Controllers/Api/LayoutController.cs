using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace ManarEthara.Controllers.Api {
    public class LayoutController : UmbracoApiController {
        private readonly IContentService _contentService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IConfiguration _configuration;

        public LayoutController(
            IContentService contentService,
            IUmbracoContextAccessor umbracoContextAccessor, IConfiguration configuration) {
            _contentService = contentService;
            _configuration = configuration;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [HttpGet]
        public IActionResult header([FromQuery] string locale) {
            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Layout");

            IPublishedContent headerData = pagesFolder.Descendants().FirstOrDefault(c => c.Name == "Header" && c.Cultures.Keys.Contains(locale));

            if (headerData == null) {
                return NotFound();
            }

            var menuItemsBlockList = headerData.Value<IEnumerable<BlockListItem>>("menuItems", culture: locale)?.Select(x => x.Content);
            var menulist = new List<object>();

            if (menuItemsBlockList != null) {
                foreach (var block in menuItemsBlockList) {
                    var menuRes = new {
                        label = block.Value("label", culture: locale),
                        url = block.Value("url", culture: locale)
                    };
                    menulist.Add(menuRes);
                }
            }

            var copyrightTxt = headerData.Value<string>("copyRightText", culture: locale);

            var socialPlatform = new {
                instagram = headerData.Value<string>("instagram", culture: locale),
                facebook = headerData.Value<string>("facebook", culture: locale),
                twitter = headerData.Value<string>("twitter", culture: locale),
                linkedin = headerData.Value<string>("linkedin", culture: locale),
                tiktok = headerData.Value<string>("tiktok", culture: locale)
            };

            var labels = new {
                copyRightText = copyrightTxt,
            };

            var headerResponse = new {
                menuItems = menulist,
                socialLinks = socialPlatform,
                labels = labels
            };

            return Ok(headerResponse);
        }

        [HttpGet]
        public IActionResult footer([FromQuery] string locale) {
            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Layout");

            IPublishedContent footerData = pagesFolder.Descendants().FirstOrDefault(c => c.Name == "Footer" && c.Cultures.Keys.Contains(locale));

            if (footerData == null) {
                return NotFound();
            }

            var menuItemsBlockList = footerData.Value<IEnumerable<BlockListItem>>("menuItems", culture: locale)?.Select(x => x.Content);
            var menulist = new List<object>();

            if (menuItemsBlockList != null) {
                foreach (var block in menuItemsBlockList) {
                    var menuRes = new {
                        label = block.Value("label", culture: locale),
                        url = block.Value("url", culture: locale)
                    };
                    menulist.Add(menuRes);
                }
            }

            var storeLinksBlockList = footerData.Value<IEnumerable<BlockListItem>>("storeLinks", culture: locale)?.Select(x => x.Content);
            var storelinkslist = new List<object>();

            if (storeLinksBlockList != null) {
                foreach (var storeblock in storeLinksBlockList) {
                    var storeimage = storeblock.Value<IPublishedContent>("image", culture: locale);
                    var storeLinksRes = new {
                        url = storeblock.Value("url", culture: locale),
                        image = storeimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + storeimage.Url(),
                            width = storeimage.Value<int>("UmbracoWidth"),
                            height = storeimage.Value<int>("UmbracoHeight"),
                        } : null,
                    };
                    storelinkslist.Add(storeLinksRes);
                }
            }

            var socialPlatform = new {
                instagram = footerData.Value<string>("instagram", culture: locale),
                facebook = footerData.Value<string>("facebook", culture: locale),
                twitter = footerData.Value<string>("twitter", culture: locale),
                linkedin = footerData.Value<string>("linkedin", culture: locale),
                tiktok = footerData.Value<string>("tiktok", culture: locale)
            };

            var labels = new {
                newsletterTitle = footerData.Value<string>("newsletterTitle", culture: locale),
                menuTitle = footerData.Value<string>("menuTitle", culture: locale),
                downloadAppsTitle = footerData.Value<string>("downloadAppsTitle", culture: locale),
                copyRightText = footerData.Value<string>("copyRightText", culture: locale),
                firstNamePlaceholder = footerData.Value<string>("firstNamePlaceholder", culture: locale),
                lastNamePlaceholder = footerData.Value<string>("lastNamePlaceholder", culture: locale),
                emailPlaceholder = footerData.Value<string>("emailPlaceholder", culture: locale),
                errorFirstNameRequired = footerData.Value<string>("errorFirstNameRequired", culture: locale),
                errorLastNameRequired = footerData.Value<string>("errorLastNameRequired", culture: locale),
                errorEmailRequired = footerData.Value<string>("errorEmailRequired", culture: locale),
                errorInvalidEmail = footerData.Value<string>("errorInvalidEmail, culture: locale"),
                errorMessage = footerData.Value<string>("errorMessage", culture: locale),
                successMessage = footerData.Value<string>("successMessage", culture: locale)
            };

            var footerResponse = new {
                menuItems = menulist,
                storeLinks = storelinkslist,
                labels = labels,
                socialLinks = socialPlatform,
            };

            return Ok(footerResponse);
        }
    }
}

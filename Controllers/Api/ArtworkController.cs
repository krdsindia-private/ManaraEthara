using Html2Markdown;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace ManarEthara.Controllers.Api {
    public class ArtworkController : UmbracoApiController {
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMediaService _mediaService;
        private readonly IConfiguration _configuration;

        public ArtworkController(
            IContentService contentService,
            ILocalizedTextService localizedTextService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IConfiguration configuration, // Updated parameter type
            IMediaService mediaService) {
            _contentService = contentService;
            _localizedTextService = localizedTextService;
            _configuration = configuration; // Updated assignment
            _umbracoContextAccessor = umbracoContextAccessor;
            _mediaService = mediaService;
        }

        [HttpGet]
        public IActionResult artworks([FromQuery] string locale, [FromQuery] string slug) {
            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var contentService = _contentService;

            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Artwork");

            IPublishedContent artworkData = pagesFolder.DescendantsOrSelf().FirstOrDefault(c => c.Value<string>("slug") == slug && c.Value<bool>("isActive") == true && c.Cultures.Keys.Contains(locale) && c.IsPublished());

            IPublishedContent artworkDatacon = pagesFolder.DescendantsOrSelf().FirstOrDefault(c => c.Name == "Artworks" && c.Cultures.Keys.Contains(locale));

            if (artworkData == null) {
                // Artist not found, return a 404 response
                return NotFound();
            }
            if (artworkData == null) {
                return NotFound("Artwork not found");
            }

            var seoimage = artworkData.Value<IPublishedContent>("seoimage", culture: locale);

            // Check for null values and assign null if they are null or empty
            var seoTitle = string.IsNullOrWhiteSpace(artworkData.Value<string>("seotitle", culture: locale)) ? null : artworkData.Value<string>("seotitle", culture: locale);
            var seoDescription = string.IsNullOrWhiteSpace(artworkData.Value<string>("seodescription", culture: locale)) ? null : artworkData.Value<string>("seodescription", culture: locale);
            var seoKeywords = string.IsNullOrWhiteSpace(artworkData.Value<string>("seokeywords", culture: locale)) ? null : artworkData.Value<string>("seokeywords", culture: locale);
            var imageUrl = seoimage != null ? _configuration["ApiConfig:azurebucket"] + seoimage.Url() : null;

            var seoData = new {
                title = seoTitle,
                description = seoDescription,
                keywords = seoKeywords,
                image = seoimage != null ? new {
                    url = imageUrl,
                    width = seoimage.Value<int>("UmbracoWidth"),
                    height = seoimage.Value<int>("UmbracoHeight"),
                } : null,
            };



            IPublishedContent artistiContentPicker = artworkData.Value<IPublishedContent>("location", culture: locale);
            var artworkimages = artworkData.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
            var artworkimglist = new List<object>();

            if (artworkimages != null) {
                foreach (var artworkmediaItem in artworkimages) {
                    var portraitimage = artworkmediaItem.Value<IPublishedContent>("portrait", culture: locale);
                    var artworklandscapeimage = artworkmediaItem.Value<IPublishedContent>("landscape", culture: locale);
                    var artworkcardimage = artworkmediaItem.Value<IPublishedContent>("card", culture: locale);
                    var artworkimgdata = new {
                        portrait = portraitimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + portraitimage.Url(),
                            width = portraitimage.Value<int>("UmbracoWidth"),
                            height = portraitimage.Value<int>("UmbracoHeight"),
                        } : null,
                        landscape = artworklandscapeimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + artworklandscapeimage.Url(),
                            width = artworklandscapeimage.Value<int>("UmbracoWidth"),
                            height = artworklandscapeimage.Value<int>("UmbracoHeight"),
                        } : null,
                        card = artworkcardimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + artworkcardimage.Url(),
                            width = artworkcardimage.Value<int>("UmbracoWidth"),
                            height = artworkcardimage.Value<int>("UmbracoHeight"),
                        } : null,
                    };
                    artworkimglist.Add(artworkimgdata);
                }

            };


            IPublishedContent artistContentPicker = artworkData.Value<IPublishedContent>("artist", culture: locale);
            var artistResponse = new List<object>();

            if (artistContentPicker != null) {
                var socialPlatform = new {
                    instagram = artistContentPicker.Value<string>("instagram", culture: locale),
                    facebook = artistContentPicker.Value<string>("facebook", culture: locale),
                    twitter = artistContentPicker.Value<string>("twitter", culture: locale),
                    linkedin = artistContentPicker.Value<string>("linkedin", culture: locale),
                    tiktok = artistContentPicker.Value<string>("tiktok", culture: locale)
                };

                var locations = new {
                    title = "",
                    slug = ""
                };

                IPublishedContent artistlocation = artistContentPicker.Value<IPublishedContent>("location", culture: locale);
                if (artistlocation != null) {
                    locations = new {
                        title = artistlocation.Value<string>("locationtitle", culture: locale),
                        slug = artistlocation.Value<string>("slug", culture: locale),
                    };

                }

                var artistseoimage = artistContentPicker.Value<IPublishedContent>("seoimage", culture: locale);
                var artistimages = artistContentPicker.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
                var artistimglist = new List<object>();

                if (artistimages != null) {
                    foreach (var artistmediaItem in artistimages) {
                        var artistimage = artistmediaItem.Value<IPublishedContent>("portrait", culture: locale);
                        var artistlandscapeimage = artistmediaItem.Value<IPublishedContent>("landscape", culture: locale);
                        var artistcardimage = artistmediaItem.Value<IPublishedContent>("card", culture: locale);
                        var artistimgdata = new {
                            portrait = artistimage != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + artistimage.Url(),
                                width = artistimage.Value<int>("UmbracoWidth"),
                                height = artistimage.Value<int>("UmbracoHeight"),
                            } : null,
                            landscape = artistlandscapeimage != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + artistlandscapeimage.Url(),
                                width = artistlandscapeimage.Value<int>("UmbracoWidth"),
                                height = artistlandscapeimage.Value<int>("UmbracoHeight"),
                            } : null,
                            card = artistcardimage != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + artistcardimage.Url(),
                                width = artistcardimage.Value<int>("UmbracoWidth"),
                                height = artistcardimage.Value<int>("UmbracoHeight"),
                            } : null,
                        };
                        artistimglist.Add(artistimgdata);
                    }

                };
                var artistseoData = new {
                    title = artistContentPicker.Value<string>("seotitle", culture: locale),
                    description = artistContentPicker.Value<string>("seodescription", culture: locale),
                    keywords = artistContentPicker.Value<string>("seokeywords", culture: locale),
                    image = artistseoimage != null ? new {
                        url = _configuration["ApiConfig:azurebucket"] + artistseoimage.Url(),
                        width = artistseoimage.Value<int>("UmbracoWidth"),
                        height = artistseoimage.Value<int>("UmbracoHeight"),
                    } : null,


                };

                var converter = new Converter();
                var artistResponseData = new {
                    isActive = artistContentPicker.Value<bool>("isActive", culture: locale) ? 1 : 0,
                    variant = artistContentPicker.Value<string>("variant", culture: locale),
                    images = artistimglist?.FirstOrDefault(),
                    location = locations,
                    title = artistContentPicker.Value<string>("artistname", culture: locale),
                    description = artistContentPicker.Value<string>("description", culture: locale),
                    markdown = converter.Convert(artistContentPicker.Value<string>("markdown", culture: locale)),
                    seo = seoData,
                    socialPlatforms = socialPlatform,

                };
                artistResponse.Add(artistResponseData);
            }
            var converter2 = new Converter();
            var locationimages = artistiContentPicker.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
            var locimglist = new List<object>();

            if (locationimages != null) {
                foreach (var locationmediaItem in locationimages) {
                    var locimage = locationmediaItem.Value<IPublishedContent>("portrait", culture: locale);
                    var loclandscapeimage = locationmediaItem.Value<IPublishedContent>("landscape", culture: locale);
                    var loccardimage = locationmediaItem.Value<IPublishedContent>("card", culture: locale);
                    var artistimgdata = new {
                        portrait = locimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + locimage.Url(),
                            width = locimage.Value<int>("UmbracoWidth"),
                            height = locimage.Value<int>("UmbracoHeight"),
                        } : null,
                        landscape = loclandscapeimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + loclandscapeimage.Url(),
                            width = loclandscapeimage.Value<int>("UmbracoWidth"),
                            height = loclandscapeimage.Value<int>("UmbracoHeight"),
                        } : null,
                        card = loccardimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + loccardimage.Url(),
                            width = loccardimage.Value<int>("UmbracoWidth"),
                            height = loccardimage.Value<int>("UmbracoHeight"),
                        } : null,
                    };
                    locimglist.Add(artistimgdata);
                }

            };
            var artworkResponse = new {
                isActive = artworkData.Value<bool>("isActive", culture: locale) ? 1 : 0,
                title = artworkData.Value<string>("artworktitle", culture: locale),
                slug = artworkData.Value<string>("slug", culture: locale),
                images = artworkimglist?.FirstOrDefault(),
                labels = new {
                    locatedOnText = artworkDatacon.Value<string>("locatedOnText", culture: locale),
                },
                artist = artistResponse?.FirstOrDefault(),
                location = new {
                    variant = artistiContentPicker.Value<string>("locationvariant", culture: locale),
                    title = artistiContentPicker.Value<string>("locationtitle", culture: locale),
                    slug = artistiContentPicker.Value<string>("slug", culture: locale),
                    images = locimglist?.FirstOrDefault(),
                },
                markdown = converter2.Convert(artworkData.Value<string>("markdown", culture: locale)),

                seo = seoData,
            };

            return Ok(artworkResponse);
        }
    }
}

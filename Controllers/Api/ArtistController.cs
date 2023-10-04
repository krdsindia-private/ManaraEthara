
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
    public class ArtistController : UmbracoApiController {
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMediaService _mediaService; // Inject IMediaService
        private readonly IConfiguration _configuration;

        public ArtistController(
            IContentService contentService,
            ILocalizedTextService localizedTextService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IConfiguration configuration,
            IMediaService mediaService) { // Add IMediaService to constructor
            _contentService = contentService;
            _configuration = configuration;
            _localizedTextService = localizedTextService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _mediaService = mediaService; // Initialize IMediaService
        }
        [HttpGet]
        public IActionResult artistlist([FromQuery] string locale) {
            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Artist");

            IPublishedContent artistData = pagesFolder.Descendants().FirstOrDefault(c => c.Name == "Artist" && c.Cultures.Keys.Contains(locale));
            if (artistData == null) {
                // Artist not found, return a 404 response
                return NotFound();
            }
            IEnumerable<IPublishedContent> artists = artistData.Children.Where(artist => artist.Value<bool>("isActive") == true && artist.Cultures.Keys.Contains(locale) && artist.IsPublished()).OrderBy(artist => artist.Value<string>("slug", culture: locale));



            var locations = new {
                title = "",
                slug = ""
            };

            var artistlist = new List<object>();

            foreach (var artist in artists) {
                var artistimages = artist.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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
                var artistName = artist.Value<string>("artistName", culture: locale);
                var artistDescription = artist.Value<string>("artistDescription", culture: locale);
                var otherArtistsData = new {
                    isActive = artist.Value<string>("otherisActive", culture: locale),
                    title = artist.Value<string>("othertitle", culture: locale),
                    description = artist.Value<string>("otherdescription", culture: locale),
                };

                IPublishedContent artistlocation = artist.Value<IPublishedContent>("location", culture: locale);
                if (artistlocation != null) {
                    locations = new {
                        title = artistlocation.Value<string>("locationtitle", culture: locale),
                        slug = artistlocation.Value<string>("slug", culture: locale),
                    };

                }
                var artistres = new {

                    variant = artist.Value<string>("variant", culture: locale),
                    images = artistimglist?.FirstOrDefault(),
                    location = locations,
                    title = artist.Value<string>("artistname", culture: locale),
                    description = artist.Value<string>("description", culture: locale),
                    slug = artist.Value<string>("slug", culture: locale),

                    otherArtists = otherArtistsData,

                };
                artistlist.Add(artistres);
            }
            var artistmainimages = artistData.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
            var artistmainimglist = new List<object>();

            if (artistmainimages != null) {
                foreach (var artistmainmediaItem in artistmainimages) {
                    var artistmainimage = artistmainmediaItem.Value<IPublishedContent>("portrait", culture: locale);
                    var artistmainlandscapeimage = artistmainmediaItem.Value<IPublishedContent>("landscape", culture: locale);

                    var artistmainimgdata = new {
                        portrait = artistmainimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + artistmainimage.Url(),
                            width = artistmainimage.Value<int>("UmbracoWidth"),
                            height = artistmainimage.Value<int>("UmbracoHeight"),
                        } : null,
                        landscape = artistmainlandscapeimage != null ? new {
                            url = _configuration["ApiConfig:azurebucket"] + artistmainlandscapeimage.Url(),
                            width = artistmainlandscapeimage.Value<int>("UmbracoWidth"),
                            height = artistmainlandscapeimage.Value<int>("UmbracoHeight"),
                        } : null,

                    };
                    artistmainimglist.Add(artistmainimgdata);
                }
            }
            var artistResponse = new {
                artists = artistlist,
                banner = new {
                    title = artistData.Value<string>("title", culture: locale),
                    images = artistmainimglist?.FirstOrDefault(),
                    labels = new {
                        scrollText = artistData.Value<string>("scrollText", culture: locale)
                    }
                },
                labels = new {
                    artistNamePlaceHolder = artistData.Value<string>("artistNamePlaceHolder", culture: locale),
                    viewMoreBtnText = artistData.Value<string>("viewMoreBtnText", culture: locale),
                    allLocationsText = artistData.Value<string>("allLocationsText", culture: locale),
                    noResultsText = artistData.Value<string>("noResultsText", culture: locale)
                }

            };

            return Ok(artistResponse);
        }
        [HttpGet]
        public IActionResult artists([FromQuery] string locale, [FromQuery] string slug) {

            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Artist");

            IPublishedContent artistData = pagesFolder.DescendantsOrSelf().FirstOrDefault(c => c.Value<string>("slug") == slug && c.Value<bool>("isActive") == true && c.Cultures.Keys.Contains(locale) && c.IsPublished());

            if (artistData == null) {

                return NotFound();
            }
            var contentService = _contentService;

            var artistlist = new List<object>();
            var socialPlatform = new {
                instagram = artistData.Value<string>("instagram", culture: locale),
                facebook = artistData.Value<string>("facebook", culture: locale),
                twitter = artistData.Value<string>("twitter", culture: locale),
                linkedin = artistData.Value<string>("linkedin", culture: locale),
                tiktok = artistData.Value<string>("tiktok", culture: locale)
            };
            var seoimage = artistData.Value<IPublishedContent>("seoimage", culture: locale);

            var locations = new {
                title = "",
                slug = ""
            };

            IPublishedContent artistlocation = artistData.Value<IPublishedContent>("location", culture: locale);
            if (artistlocation != null) {
                locations = new {
                    title = artistlocation.Value<string>("locationtitle", culture: locale),
                    slug = artistlocation.Value<string>("slug", culture: locale),
                };

            }
            var artistimages = artistData.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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

            }

            var seoData = new {
                title = artistData.Value<string>("seotitle", culture: locale),
                description = artistData.Value<string>("seodescription", culture: locale),
                keywords = artistData.Value<string>("seokeywords", culture: locale),
                image = seoimage != null ? new {
                    url = _configuration["ApiConfig:azurebucket"] + seoimage.Url(),
                    width = seoimage.Value<int>("UmbracoWidth"),
                    height = seoimage.Value<int>("UmbracoHeight"),
                } : null,


            };


            var otherArtistsData = new {
                isActive = artistData.Value<string>("otherisActive", culture: locale),
                title = artistData.Value<string>("othertitle", culture: locale),
                description = artistData.Value<string>("otherdescription", culture: locale),
            };
            var converter = new Converter();
            var artistResponse = new {
                isActive = artistData.Value<bool>("isActive", culture: locale) ? 1 : 0,
                variant = artistData.Value<string>("variant", culture: locale),
                images = artistimglist?.FirstOrDefault(),
                title = artistData.Value<string>("artistname", culture: locale),
                slug = artistData.Value<string>("slug", culture: locale),
                description = artistData.Value<string>("description", culture: locale),
                markdown = converter.Convert(artistData.Value<string>("markdown", culture: locale)),
                location = locations,
                otherArtists = otherArtistsData,
                seo = seoData,
                socialPlatforms = socialPlatform,

            };


            return Ok(artistResponse);
        }





    }
}

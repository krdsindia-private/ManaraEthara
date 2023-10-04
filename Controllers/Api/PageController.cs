using System.Diagnostics;
using System.Globalization;
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
    public class PageController : UmbracoApiController {

        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        private readonly IConfiguration _configuration;
        public PageController(

            ILocalizedTextService localizedTextService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IConfiguration configuration) {

            _localizedTextService = localizedTextService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _configuration = configuration;

        }

        [HttpGet]
        public IActionResult pages([FromQuery] string locale, [FromQuery] string slug) {
            try {
                var jsonSettings = new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                    return Problem("Unable to get content");
                }

                IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Page");

                IPublishedContent pageData = pagesFolder.DescendantsOrSelf().FirstOrDefault(c =>
                      c.Value<string>("pageslug") == slug &&
                      c.Value<bool>("pageisActive") == true &&
                      c.Cultures.Keys.Contains(locale) &&
                      c.IsPublished()
                  );



                if (pageData == null) {
                    return NotFound();
                }
                var converter = new Converter();

                var seoimage = pageData.Value<IPublishedContent>("seoimage", culture: locale);

                // Check for null values and assign null if they are null or empty homePage.GetProperty("MetaTitle")?.GetValue(culture: culture)
                var seoTitle = string.IsNullOrWhiteSpace(pageData.Value<string>("seotitle", culture: locale)) ? null : pageData.Value<string>("seotitle", culture: locale);
                var seoDescription = string.IsNullOrWhiteSpace(pageData.Value<string>("seodescription", culture: locale)) ? null : pageData.Value<string>("seodescription", culture: locale);
                var seoKeywords = string.IsNullOrWhiteSpace(pageData.Value<string>("seokeywords", culture: locale)) ? null : pageData.Value<string>("seokeywords", culture: locale);
                var imageUrl = seoimage != null ? _configuration["ApiConfig:azurebucket"] + seoimage.Url() : null;


                var pageComponents = pageData.Value<BlockGridModel>("Components", culture: locale);

                var Components = new List<object>();

                if (pageComponents != null) {


                    foreach (var componentitem in pageComponents) {

                        var content = componentitem.Content;
                        var settings = componentitem.Settings;
                        // get the areas of the block
                        var areas = componentitem.Areas;
                        // get the dimensions of the block
                        var rowSpan = componentitem.RowSpan;
                        var columnSpan = componentitem.ColumnSpan;
                        if (content != null && content.ContentType.Alias == "videoContentBlock") {
                            var video = content.Value<string>("url", culture: locale);
                            /* var videoRootPath = System.IO.Path.GetDirectoryName(video);
                             var rootPath = videoRootPath.Replace("\\", "/");

                             var videoFileName = System.IO.Path.GetFileName(video);
                             var videoRelativePath = System.IO.Path.Combine(rootPath, videoFileName);
                            */
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var VideoContentBlockDate = new {
                                    component = "video-content-block",

                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                    markdown = converter.Convert(content.Value<string>("markdown", culture: locale)),
                                    url = video,
                                    labels = new {
                                        circularText = content.Value<string>("circularText", culture: locale),


                                    }
                                };
                                Components.Add(VideoContentBlockDate);
                            }
                        }
                        if (content != null && content.ContentType.Alias == "banner1") {
                            var bannerimg = content.Value<BlockGridModel>("images", culture: locale);
                            var gridColumns = bannerimg.GridColumns;
                            var bannerimgresponsive = new List<object>();
                            if (bannerimg != null) {
                                foreach (var banneritem in bannerimg) {

                                    var bannercontent = banneritem.Content;

                                    var portraitimageProperty = bannercontent.Value<IPublishedContent>("portrait", culture: locale);
                                    var landscapeimageProperty = bannercontent.Value<IPublishedContent>("landscape", culture: locale);
                                    var imageprops = new {
                                        portrait = portraitimageProperty != null ? new {
                                            url = _configuration["ApiConfig:azurebucket"] + portraitimageProperty.Url(),
                                            width = portraitimageProperty.Value<int>("UmbracoWidth"),
                                            height = portraitimageProperty.Value<int>("UmbracoHeight"),
                                        } : null,

                                        landscape = landscapeimageProperty != null ? new {
                                            url = _configuration["ApiConfig:azurebucket"] + landscapeimageProperty.Url(),
                                            width = landscapeimageProperty.Value<int>("UmbracoWidth"),
                                            height = landscapeimageProperty.Value<int>("UmbracoHeight"),
                                        } : null,

                                    };
                                    bannerimgresponsive.Add(imageprops);

                                }
                            }
                            var label = new {
                                scrollDownText = content.Value<string>("scrollDownText", culture: locale),
                            };
                            if (content.Value<bool>("bannerisActive", culture: locale)) {
                                var bannerData = new {
                                    component = "banner",

                                    isActive = content.Value<bool>("bannerisActive", culture: locale) ? 1 : 0,
                                    title = content.Value<string>("bannertitle", culture: locale),
                                    subTitle = content.Value<string>("subTitle", culture: locale),
                                    description = content.Value<string>("bannerdescription", culture: locale),
                                    images = bannerimgresponsive?.FirstOrDefault(),
                                    labels = label




                                };
                                Components.Add(bannerData);
                            }

                        }
                        if (content != null && content.ContentType.Alias == "contactForm") {
                            var contactformprops = new {
                                component = "contact-form",
                                isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                title = content.Value<string>("title", culture: locale),
                                description = content.Value<string>("description", culture: locale),
                                labels = new {
                                    namePlaceholder = content.Value<string>("namePlaceholder", culture: locale),
                                    emailPlaceholder = content.Value<string>("emailPlaceholder", culture: locale),
                                    mobilePlaceholder = content.Value<string>("mobilePlaceholder", culture: locale),
                                    messagePlaceholder = content.Value<string>("messagePlaceholder", culture: locale),
                                    errorNameRequired = content.Value<string>("errorNameRequired", culture: locale),
                                    errorEmailRequired = content.Value<string>("errorEmailRequired", culture: locale),
                                    errorIsAgreeRequired = content.Value<string>("errorIsAgreeRequired", culture: locale),
                                    communicationText = content.Value<string>("communicationText", culture: locale),
                                    successMsg = content.Value<string>("successMsg", culture: locale),
                                    errorMsg = content.Value<string>("errorMsg", culture: locale),
                                    submitBtnText = content.Value<string>("submitBtnText", culture: locale)
                                }
                            };
                            Components.Add(contactformprops);
                        }
                        if (content != null && content.ContentType.Alias == "gallery") {


                            var galleryimages = content.Value<IEnumerable<BlockListItem>>("galleryimages", culture: locale).Select(x => x.Content);

                            var gallerylist = new List<object>();

                            if (galleryimages != null) {
                                foreach (var gallerymediaItem in galleryimages) {
                                    var thumbnail = gallerymediaItem.Value<IPublishedContent>("thumbnail", culture: locale);
                                    var preview = gallerymediaItem.Value<IPublishedContent>("preview", culture: locale);
                                    var gallerydata = new {
                                        title = gallerymediaItem.Value<string>("title", culture: locale),
                                        thumbnail = thumbnail != null ? new {
                                            url = _configuration["ApiConfig:azurebucket"] + thumbnail.Url(),
                                            width = thumbnail.Value<int>("UmbracoWidth"),
                                            height = thumbnail.Value<int>("UmbracoHeight"),
                                        } : null,
                                        preview = preview != null ? new {
                                            url = _configuration["ApiConfig:azurebucket"] + preview.Url(),
                                            width = preview.Value<int>("UmbracoWidth"),
                                            height = preview.Value<int>("UmbracoHeight"),
                                        } : null,
                                    };
                                    gallerylist.Add(gallerydata);
                                }

                            }
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var galleryResponse = new {
                                    component = "gallery",

                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                    title = content.Value<string>("title", culture: locale),
                                    description = content.Value<string>("description", culture: locale),
                                    images = gallerylist,
                                    labels = new {
                                        viewMoreBtnText = content.Value<string>("viewMoreBtnText", culture: locale)
                                    }

                                };

                                Components.Add(galleryResponse);
                            }
                        }
                        if (content != null && content.ContentType.Alias == "locationCarousel") {


                            var locationList = content.Value<IEnumerable<IPublishedContent>>("locations", culture: locale);

                            var locationitemlist = new List<object>();
                            if (locationList != null) {


                                foreach (var locationitem in locationList) {
                                    var locationimages = locationitem.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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


                                    var locationlistRes = new {


                                        title = locationitem.Value<string>("locationtitle", culture: locale),
                                        slug = locationitem.Value<string>("slug", culture: locale),
                                        variant = locationitem.Value<string>("locationvariant", culture: locale),
                                        images = locimglist?.FirstOrDefault(),


                                    };
                                    locationitemlist.Add(locationlistRes);
                                }
                            }

                            if (content.Value<bool>("isActive", culture: locale)) {
                                var locationResponse = new {
                                    component = "location-carousel",
                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,

                                    titleLine1 = content.Value<string>("titleLine1", culture: locale),
                                    titleLine2 = content.Value<string>("titleLine2", culture: locale),
                                    description = content.Value<string>("description", culture: locale),
                                    locations = locationitemlist,


                                };

                                Components.Add(locationResponse);
                            }

                        }



                        if (content != null && content.ContentType.Alias == "artistCarousel") {

                            var artistList = content.Value<IEnumerable<IPublishedContent>>("artists", culture: locale);
                            if (artistList != null) {
                                artistList = artistList.OrderBy(artist =>
                                    artist.Value<string>("slug", culture: locale),
                                      StringComparer.CurrentCultureIgnoreCase
                                );
                            }
                            var artistitemlist = new List<object>();
                            if (artistList != null) {
                                foreach (var artistitem in artistList) {
                                    var artistimages = artistitem.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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
                                    if (content.Value<bool>("isActive", culture: locale)) {
                                        var artistResponse = new {
                                            isActive = artistitem.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                            variant = artistitem.Value<string>("variant", culture: locale),
                                            title = artistitem.Value<string>("artistname", culture: locale),
                                            description = artistitem.Value<string>("description", culture: locale),
                                            images = artistimglist?.FirstOrDefault(),
                                            slug = artistitem.Value<string>("slug", culture: locale),

                                        };
                                        artistitemlist.Add(artistResponse);
                                    }
                                }
                            }
                            var artistcarouselimage = content.Value<IPublishedContent>("image", culture: locale);
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var ArtistCarouselRes = new {
                                    component = "artist-carousel",

                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                    image = artistcarouselimage != null ? new {
                                        url = _configuration["ApiConfig:azurebucket"] + artistcarouselimage.Url(),
                                        width = artistcarouselimage.Value<int>("UmbracoWidth"),
                                        height = artistcarouselimage.Value<int>("UmbracoHeight"),
                                    } : null,
                                    title = content.Value<string>("artistCarouseltitle", culture: locale),
                                    description = content.Value<string>("artistCarouseldescription", culture: locale),
                                    artists = artistitemlist,



                                };
                                Components.Add(ArtistCarouselRes);
                            }

                        }
                        if (content != null && content.ContentType.Alias == "simpleShapeCarousel") {

                            var slideComponents = content.Value<BlockGridModel>("slides", culture: locale);
                            var slidelist = new List<object>();
                            if (slideComponents != null) {
                                foreach (var slideitem in slideComponents) {

                                    var slidecontent = slideitem.Content;
                                    var slidesettings = slideitem.Settings;
                                    // get the areas of the block
                                    var slideareas = slideitem.Areas;
                                    // get the dimensions of the block
                                    var sliderowSpan = slideitem.RowSpan;
                                    var slidecolumnSpan = slideitem.ColumnSpan;
                                    var slideimg = slidecontent.Value<IPublishedContent>("image", culture: locale);
                                    var slidedata = new {
                                        variant = slidecontent.Value<string>("variant", culture: locale),
                                        title = slidecontent.Value<string>("title", culture: locale),
                                        description = slidecontent.Value<string>("description", culture: locale),
                                        image = slideimg != null ? new {
                                            url = _configuration["ApiConfig:azurebucket"] + slideimg.Url(),
                                            width = slideimg.Value<int>("UmbracoWidth"),
                                            height = slideimg.Value<int>("UmbracoHeight"),
                                        } : null
                                    };
                                    slidelist.Add(slidedata);


                                }
                            }
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var SimpleShapeCarouselData = new {
                                    component = "simple-shape-carousel",

                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                    title = content.Value<string>("title", culture: locale),
                                    description = content.Value<string>("description", culture: locale),
                                    slides = slidelist,



                                };
                                Components.Add(SimpleShapeCarouselData);
                            }

                        }
                        if (content != null && content.ContentType.Alias == "programCarousel") {


                            var programList = content.Value<IEnumerable<IPublishedContent>>("events", culture: locale);


                            var eventlist = new List<object>();
                            if (programList != null) {
                                foreach (var programitem in programList) {

                                    var programimg = programitem.Value<IPublishedContent>("image", culture: locale);
                                    IPublishedContent progamrCatContentPicker = programitem.Value<IPublishedContent>("category", culture: locale);
                                    var programcatimg = progamrCatContentPicker.Value<IPublishedContent>("icon", culture: locale);
                                    var getprogramartist = programitem.Value<IEnumerable<BlockListItem>>("artists", culture: locale).Select(x => x.Content);

                                    var programartistlist = new List<object>();
                                    foreach (var programartistdata in getprogramartist) {
                                        var programArtistRes = new {
                                            name = programartistdata.Value<string>("artname", culture: locale),
                                            role = programartistdata.Value<string>("role", culture: locale)
                                        };
                                        programartistlist.Add(programArtistRes);
                                    }

                                    string formattedStartDate = null;
                                    string formattedEndDate = null;
                                    string formattedStartTime = null;
                                    string formattedEndTime = null;
                                    string formattedStartDateTime = null;
                                    string formattedEndDateTime = null;

                                    var startDateValue = programitem.Value<string>("startDate", culture: locale);
                                    var endDateValue = programitem.Value<string>("endDate", culture: locale);
                                    var startTimeValue = programitem.Value<string>("startTime", culture: locale);
                                    var endTimeValue = programitem.Value<string>("endTime", culture: locale);

                                    // Print the values to debug


                                    // Attempt to parse the date and time values with adjusted formats
                                    if (DateTime.TryParseExact(startDateValue, new[] { "M/d/yyyy", "yyyy-MM-ddTHH:mm:ss", "M/d/yyyy h:mm:ss tt" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startDate) &&
                                        DateTime.TryParseExact(endDateValue, new[] { "M/d/yyyy", "yyyy-MM-ddTHH:mm:ss", "M/d/yyyy h:mm:ss tt" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endDate) &&
                                        DateTime.TryParseExact(startTimeValue, new[] { "M/d/yyyy h:mm:ss tt", "h:mm:ss tt" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime startTime) &&
                                        DateTime.TryParseExact(endTimeValue, new[] { "M/d/yyyy h:mm:ss tt", "h:mm:ss tt" },
                                        CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime endTime)) {
                                        // Format the dates and times as needed

                                        formattedStartDate = startDate.ToString("yyyy-MM-dd"); // Change the format as needed

                                        formattedEndDate = endDate.ToString("yyyy-MM-dd");     // Change the format as needed
                                        formattedStartTime = startTime.ToString("HH:mm:ss");   // Change the format as needed (e.g., "HH:mm:ss")
                                        formattedEndTime = endTime.ToString("HH:mm:ss");       // Change the format as needed (e.g., "HH:mm:ss")

                                        // Combine the date and time into the desired format
                                        formattedStartDateTime = $"{formattedStartDate}T{formattedStartTime}Z";
                                        formattedEndDateTime = $"{formattedEndDate}T{formattedEndTime}Z";

                                    }
                                    else {
                                        Debug.WriteLine("Parsing failed. Check the date and time values for unexpected characters or issues.");
                                    }

                                    //  var progStartDate = programitem.Value<IPublishedContent>("progStartDate");
                                    if (programitem.Value<bool>("isActive", culture: locale)) {
                                        var programRes = new {
                                            isActive = programitem.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                            variant = programitem.Value<string>("variant", culture: locale),
                                            image = programimg != null ? new {
                                                url = _configuration["ApiConfig:azurebucket"] + programimg.Url(),
                                                width = programimg.Value<int>("UmbracoWidth"),
                                                height = programimg.Value<int>("UmbracoHeight"),
                                            } : null,
                                            category = new {
                                                category = progamrCatContentPicker.Value<string>("programcategoryname", culture: locale),
                                                icon = programcatimg != null ? new {
                                                    url = _configuration["ApiConfig:azurebucket"] + programcatimg.Url(),
                                                    width = programcatimg.Value<int>("UmbracoWidth"),
                                                    height = programcatimg.Value<int>("UmbracoHeight"),

                                                } : null,

                                            },
                                            artists = programartistlist,
                                            startDate = formattedStartDate,
                                            endDate = formattedEndDate,
                                            startTime = formattedStartDateTime,
                                            endTime = formattedEndDateTime,
                                            location = programitem.Value<string>("location", culture: locale),

                                        };
                                        eventlist.Add(programRes);
                                    }

                                }
                            }
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var ProgramData = new {
                                    component = "program-carousel",

                                    isActive = content.Value<bool>("isActive", culture: locale) ? 1 : 0,
                                    title = content.Value<string>("programcarotitle", culture: locale),
                                    description = content.Value<string>("programcarodescription", culture: locale),
                                    events = eventlist



                                };

                                Components.Add(ProgramData);
                            }
                        }


                        if (content != null && content.ContentType.Alias == "simpleTitleBlock") {
                            if (content.Value<bool>("isActive", culture: locale)) {
                                var SimpleTitleBlockData = new {
                                    component = "simple-title-block",
                                    title = content.Value<string>("title", culture: locale),
                                    link = new {
                                        label = content.Value<string>("label", culture: locale),
                                        url = content.Value<string>("url", culture: locale)

                                    }


                                };

                                Components.Add(SimpleTitleBlockData);
                            }
                        }

                    }

                }

                var seoData = new {
                    title = seoTitle,
                    description = seoDescription,
                    keywords = seoKeywords,
                    image = seoimage != null ? new {
                        url = _configuration["ApiConfig:azurebucket"] + imageUrl,
                        width = seoimage.Value<int>("UmbracoWidth"),
                        height = seoimage.Value<int>("UmbracoHeight"),

                    } : null
                };





                var homeResponse = new {
                    isActive = pageData.Value<bool>("pageisActive", culture: locale) ? 1 : 0,
                    slug = pageData.Value<string>("pageslug", culture: locale),
                    seo = seoData,
                    components = Components
                };




                return Ok(homeResponse);
            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }





    }
}

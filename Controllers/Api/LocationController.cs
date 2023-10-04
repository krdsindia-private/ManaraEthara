using System.Diagnostics;
using System.Globalization;
using Html2Markdown;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace ManarEthara.Controllers.Api {
    public class LocationController : UmbracoApiController {

        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        private readonly IConfiguration _configuration;
        public LocationController(

            ILocalizedTextService localizedTextService,
            IUmbracoContextAccessor umbracoContextAccessor,
            IConfiguration configuration) {

            _localizedTextService = localizedTextService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _configuration = configuration;

        }
        [HttpGet]
        public IActionResult locationlist([FromQuery] string locale, [FromQuery] string slug) {
            var jsonSettings = new JsonSerializerSettings {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                return Problem("Unable to get content");
            }

            IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Location");

            IPublishedContent locationlistData = pagesFolder.Descendants().FirstOrDefault(c => c.Name == "Locations");



            if (locationlistData == null) {
                // Artist not found, return a 404 response
                return NotFound();
            }
            IEnumerable<IPublishedContent> localtionchildrens = locationlistData.Children.Where(location =>
                    location.Value<bool>("locationisActive") == true
                    && location.Cultures.Keys.Contains(locale) &&
                      location.IsPublished()
                );

            var locationlist = new List<object>();


            if (localtionchildrens != null) {
                foreach (var locationData in localtionchildrens) {

                    var locationimages = locationData.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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




                    var locationResponse = new {
                        isActive = locationData.Value<bool>("locationisActive", culture: locale) ? 1 : 0,
                        title = locationData.Value<string>("locationtitle", culture: locale),
                        slug = locationData.Value<string>("slug", culture: locale),
                        variant = locationData.Value<string>("locationvariant", culture: locale),
                        images = locimglist?.FirstOrDefault(),


                    };


                    locationlist.Add(locationResponse);


                }
            }


            return Ok(locationlist);
        }
        [HttpGet]
        public IActionResult locations([FromQuery] string locale, [FromQuery] string slug) {
            try {
                var jsonSettings = new JsonSerializerSettings {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) || umbracoContext.Content == null) {
                    return Problem("Unable to get content");
                }

                IPublishedContent pagesFolder = umbracoContext.Content.GetAtRoot().FirstOrDefault(c => c.Name == "Location");

                IPublishedContent locationData = pagesFolder.DescendantsOrSelf().FirstOrDefault(c => c.Value<string>("slug") == slug && c.Value<bool>("locationisActive") == true && c.Cultures.Keys.Contains(locale) && c.IsPublished());

                IPublishedContent locDatacon = pagesFolder.DescendantsOrSelf().FirstOrDefault(c => c.Name == "Locations");

                if (locationData == null) {

                    return NotFound();
                }
                var converter = new Converter();
                var seoimage = locationData.Value<IPublishedContent>("seoimage", culture: locale);

                var seoData = new {
                    title = locationData.Value<string>("seotitle", culture: locale),
                    description = locationData.Value<string>("seodescription", culture: locale),
                    keywords = locationData.Value<string>("seokeywords", culture: locale),
                    image = seoimage != null ? new {
                        url = _configuration["ApiConfig:azurebucket"] + seoimage.Url(),
                        width = seoimage.Value<int>("UmbracoWidth"),
                        height = seoimage.Value<int>("UmbracoHeight"),
                    } : null,


                };

                var label = new {
                    scrollText = locDatacon.Value<string>("scrollText", culture: locale),
                    byText = locDatacon.Value<string>("byText", culture: locale),
                };



                Debug.WriteLine("hometestsserdfsdfe");
                var artworklist = new List<object>();
                var artworkMultiNodeTreePicker = locationData.Value<IEnumerable<IPublishedContent>>("list", culture: locale);



                if (artworkMultiNodeTreePicker != null) {

                    foreach (var artworkitem in artworkMultiNodeTreePicker) {
                        var artworkseoimage = artworkitem.Value<IPublishedContent>("seoimage", culture: locale);

                        var artworkseoData = new {
                            title = artworkitem.Value<string>("seotitle", culture: locale),
                            description = artworkitem.Value<string>("seodescription", culture: locale),
                            keywords = artworkitem.Value<string>("seokeywords"),
                            image = artworkseoimage != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + artworkseoimage.Url(),
                                width = artworkseoimage.Value<int>("UmbracoWidth"),
                                height = artworkseoimage.Value<int>("UmbracoHeight"),

                            } : null,

                        };
                        IPublishedContent artistiContentPicker = artworkitem.Value<IPublishedContent>("location", culture: locale);

                        IPublishedContent artistContentPicker = artworkitem.Value<IPublishedContent>("artist", culture: locale);
                        var artistResponse = new List<object>();

                        if (artistContentPicker != null) {
                            var socialPlatform = new {
                                instagram = artistContentPicker.Value<string>("instagram", culture: locale),
                                facebook = artistContentPicker.Value<string>("facebook", culture: locale),
                                twitter = artistContentPicker.Value<string>("twitter", culture: locale),
                                linkedin = artistContentPicker.Value<string>("linkedin", culture: locale),
                                tiktok = artistContentPicker.Value<string>("tiktok", culture: locale)
                            };


                            var artistseoimage = artistContentPicker.Value<IPublishedContent>("seoimage", culture: locale);

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
                        var artworkimages = artworkitem.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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


                        var artworkResponse = new {
                            isActive = artworkitem.Value<bool>("isActive", culture: locale) ? 1 : 0,
                            title = artworkitem.Value<string>("artworktitle", culture: locale),
                            slug = artworkitem.Value<string>("slug", culture: locale),
                            images = artworkimglist?.FirstOrDefault(),
                            artist = artistResponse?.FirstOrDefault(),
                            location = artistiContentPicker?.Name, // Use null-conditional operator
                            markdown = converter.Convert(artworkitem.Value<string>("markdown", culture: locale)),

                            seo = artworkseoData,
                        };

                        artworklist.Add(artworkResponse);
                    }

                }
                var artworkmainimage = locationData.Value<IPublishedContent>("image", culture: locale);
                var artworkData = new {
                    isActive = locationData.Value<bool>("artworkisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("artworktitle", culture: locale),
                    description = locationData.Value<string>("artworkdescription", culture: locale),
                    image = artworkmainimage != null ? new {
                        url = _configuration["ApiConfig:azurebucket"] + artworkmainimage.Url(),
                        width = artworkmainimage.Value<int>("UmbracoWidth"),
                        height = artworkmainimage.Value<int>("UmbracoHeight"),
                    } : null,
                    list = artworklist

                };
                var outletslist = new List<object>();
                var outlets = locationData.Value<IEnumerable<BlockListItem>>("outlets", culture: locale).Select(x => x.Content);
                if (outlets != null) {
                    foreach (var outletdata in outlets) {
                        var outletimage = outletdata.Value<IPublishedContent>("outletsimage", culture: locale);
                        var outletres = new {
                            title = outletdata.Value<string>("outletstitle", culture: locale),
                            description = outletdata.Value<string>("outletsdescription", culture: locale),
                            variant = outletdata.Value<string>("variant", culture: locale),
                            image = outletimage != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + outletimage.Url(),
                                width = outletimage.Value<int>("UmbracoWidth"),
                                height = outletimage.Value<int>("UmbracoHeight"),
                            } : null,
                        };
                        outletslist.Add(outletres);
                    }
                }
                var fnbData = new {
                    isActive = locationData.Value<bool>("fnbisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("fnbtitle", culture: locale),
                    description = locationData.Value<string>("fnbdescription", culture: locale),
                    outlets = outletslist

                };


                var routelist = new List<object>();
                var transportroutes = locationData.Value<IEnumerable<BlockListItem>>("routes", culture: locale).Select(x => x.Content);
                if (transportroutes != null) {
                    foreach (var transportroutedata in transportroutes) {
                        var transportpoints = transportroutedata.Value<IEnumerable<BlockListItem>>("points", culture: locale).Select(x => x.Content);
                        var pointlist = new List<object>();
                        if (transportpoints != null) {
                            foreach (var transportpointsdata in transportpoints) {
                                var pointimage = transportpointsdata.Value<IPublishedContent>("pointsimage", culture: locale);
                                var pointicon = transportpointsdata.Value<IPublishedContent>("pointsicon", culture: locale);
                                var pointObjectpointnode = new {
                                    title = transportpointsdata.Value<string>("pointstitle", culture: locale),
                                    icon = pointicon != null ? new {
                                        url = _configuration["ApiConfig:azurebucket"] + pointicon.Url(),
                                        width = pointicon.Value<int>("UmbracoWidth"),
                                        height = pointicon.Value<int>("UmbracoHeight"),
                                    } : null,
                                    image = pointimage != null ? new {
                                        url = _configuration["ApiConfig:azurebucket"] + pointimage.Url(),
                                        width = pointimage.Value<int>("UmbracoWidth"),
                                        height = pointimage.Value<int>("UmbracoHeight"),
                                    } : null,
                                    shortText = transportpointsdata.Value<string>("pointsshortText")

                                };
                                pointlist.Add(pointObjectpointnode);
                            }
                        }
                        var routeObject = new {
                            title = transportroutedata.Value<string>("routestitle", culture: locale),
                            points = pointlist

                        };
                        routelist.Add(routeObject);
                    }
                }
                var transportData = new {
                    isActive = locationData.Value<bool>("transportisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("transporttitle", culture: locale),
                    description = locationData.Value<string>("transportdesc", culture: locale),
                    routes = routelist
                };
                var amentieslist = new List<object>();
                var amentiesMultiNodeTreePicker = locationData.Value<IEnumerable<IPublishedContent>>("amenitieslist", culture: locale);
                if (amentiesMultiNodeTreePicker != null) {
                    foreach (var amentiesitem in amentiesMultiNodeTreePicker) {
                        var amentiesimg = amentiesitem.Value<IPublishedContent>("icon", culture: locale);
                        var amentieslistObj = new {
                            title = amentiesitem.Value<string>("title", culture: locale),
                            icon = amentiesimg != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + amentiesimg.Url(),
                                width = amentiesimg.Value<int>("UmbracoWidth"),
                                height = amentiesimg.Value<int>("UmbracoHeight"),
                            } : null,

                        };
                        amentieslist.Add(amentieslistObj);
                    }
                }
                var amenitiesData = new {
                    isActive = locationData.Value<bool>("amenitiesisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("amenitiestitle", culture: locale),
                    description = locationData.Value<string>("amenitiesdescription", culture: locale),
                    list = amentieslist
                };

                var infolocimage = locationData.Value<IPublishedContent>("infoimage", culture: locale);
                var informationData = new {
                    isActive = locationData.Value<bool>("infoisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("infotitle", culture: locale),
                    description = locationData.Value<string>("infodescription", culture: locale),
                    image = infolocimage != null ? new {
                        url = _configuration["ApiConfig:azurebucket"] + infolocimage.Url(),
                        width = infolocimage.Value<int>("UmbracoWidth"),
                        height = infolocimage.Value<int>("UmbracoHeight"),
                    } : null,
                    markdown = converter.Convert(locationData.Value<string>("infomarkdown", culture: locale)),
                };
                var otherLocData = new {
                    isActive = locationData.Value<bool>("otherisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("otherloctitle", culture: locale),
                    description = locationData.Value<string>("otherlocdescription", culture: locale),
                };

                var programlist = new List<object>();
                var programMultiNodeTreePicker = locationData.Value<IEnumerable<IPublishedContent>>("programlist", culture: locale);
                if (programMultiNodeTreePicker != null) {
                    foreach (var programitem in programMultiNodeTreePicker) {
                        var programimg = programitem.Value<IPublishedContent>("image", culture: locale);
                        IPublishedContent progamrCatContentPicker = programitem.Value<IPublishedContent>("category", culture: locale);
                        var getprogramartist = programitem.Value<IEnumerable<BlockListItem>>("artists", culture: locale).Select(x => x.Content);

                        var programartistlist = new List<object>();
                        if (getprogramartist != null) {
                            foreach (var programartistdata in getprogramartist) {
                                var programArtistRes = new {
                                    name = programartistdata.Value<string>("artname", culture: locale),
                                    role = programartistdata.Value<string>("role", culture: locale)
                                };
                                programartistlist.Add(programArtistRes);
                            }
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

                            // Now you can use the formatted values as needed
                        }
                        else {
                            Debug.WriteLine("Parsing failed. Check the date and time values for unexpected characters or issues.");
                        }

                        //  var progStartDate = programitem.Value<IPublishedContent>("progStartDate");
                        var programRes = new {
                            isActive = programitem.Value<bool>("isActive", culture: locale) ? 1 : 0,
                            variant = programitem.Value<string>("variant", culture: locale),
                            image = programimg != null ? new {
                                url = _configuration["ApiConfig:azurebucket"] + programimg.Url(),
                                width = programimg.Value<int>("UmbracoWidth"),
                                height = programimg.Value<int>("UmbracoHeight"),
                            } : null,
                            category = progamrCatContentPicker.Value<string>("programcategoryname", culture: locale),
                            artists = programartistlist,
                            startDate = formattedStartDate,
                            endDate = formattedEndDate,
                            startTime = formattedStartDateTime,
                            endTime = formattedEndDateTime,
                            location = programitem.Value<string>("location", culture: locale),

                        };
                        programlist.Add(programRes);
                    }
                }
                var programsData = new {
                    isActive = locationData.Value<bool>("programisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("programtitle", culture: locale),
                    description = locationData.Value<string>("programdescription", culture: locale),
                    list = programlist
                };
                var locationimages = locationData.Value<IEnumerable<BlockListItem>>("images", culture: locale).Select(x => x.Content);
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



                var locationResponse = new {
                    isActive = locationData.Value<bool>("locationisActive", culture: locale) ? 1 : 0,
                    title = locationData.Value<string>("locationtitle", culture: locale),
                    slug = locationData.Value<string>("slug", culture: locale),
                    variant = locationData.Value<string>("locationvariant", culture: locale),
                    images = locimglist?.FirstOrDefault(),
                    seo = seoData,
                    labels = label,
                    artworks = locationData.Value<bool>("artworkisActive", culture: locale) == true ? artworkData : null,
                    fnb = locationData.Value<bool>("fnbisActive", culture: locale) == true ? fnbData : null,
                    transport = locationData.Value<bool>("transportisActive", culture: locale) == true ? transportData : null,
                    amenities = locationData.Value<bool>("amenitiesisActive", culture: locale) == true ? amenitiesData : null,
                    programs = locationData.Value<bool>("programisActive", culture: locale) == true ? programsData : null,
                    information = locationData.Value<bool>("infoisActive", culture: locale) == true ? informationData : null,
                    otherLocations = locationData.Value<bool>("otherisActive", culture: locale) == true ? otherLocData : null,

                };




                return Ok(locationResponse);

                // var response = JsonConvert.SerializeObject(locationResponse, jsonSettings);


            }
            catch (Exception ex) {
                return StatusCode(500, ex.Message);
            }
        }





    }
}

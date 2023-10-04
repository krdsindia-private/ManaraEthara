using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace ManarEthara {
    public static class BackofficeAuthenticationExtensions {
        public static IUmbracoBuilder ConfigureAuthentication(this IUmbracoBuilder builder) {
            builder.AddBackOfficeExternalLogins(logins => {
                const string schema = MicrosoftAccountDefaults.AuthenticationScheme;

                logins.AddBackOfficeLogin(
                    backOfficeAuthenticationBuilder => {
                        backOfficeAuthenticationBuilder.AddMicrosoftAccount(
                            // the scheme must be set with this method to work for the back office
                            backOfficeAuthenticationBuilder.SchemeForBackOffice(schema) ?? string.Empty,
                            options => {
                                //By default this is '/signin-microsoft' but it needs to be changed to this
                                options.CallbackPath = "/umbraco-signin-microsoft/";
                                //Obtained from the AZURE AD B2C WEB APP
                                options.ClientId = "58bb6e58-f058-4a18-b5f9-ea3940bf09ea";
                                //Obtained from the AZURE AD B2C WEB APP

                                options.ClientSecret = "cmx8Q~JrfCEncQZSTJlv2qKzNmcu1bntYSN.daqU";

                                options.TokenEndpoint = $"https://login.microsoftonline.com/8b3ac275-fcdd-4f2f-a2bc-e99673ba2719/oauth2/v2.0/token";

                                options.AuthorizationEndpoint = $"https://login.microsoftonline.com/8b3ac275-fcdd-4f2f-a2bc-e99673ba2719/oauth2/v2.0/authorize";                                    
                            });
                    });
            });
            return builder;
        }
    }
}

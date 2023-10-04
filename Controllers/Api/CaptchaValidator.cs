namespace ManarEthara.Controllers.Api {
    public class CaptchaValidator {
        private readonly string _recaptchaSecretKey;

        public CaptchaValidator(string recaptchaSecretKey) {
            _recaptchaSecretKey = recaptchaSecretKey;
        }

        public async Task<bool> IsCaptchaValid(string token) {
            using (var httpClient = new HttpClient()) {
                var uri = new Uri("https://www.google.com/recaptcha/api/siteverify");

                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("secret",_recaptchaSecretKey),
                new KeyValuePair<string, string>("response", token)
            });

                var response = await httpClient.PostAsync(uri, content);

                if (response.IsSuccessStatusCode) {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize the response from Google reCAPTCHA
                    var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<CaptchaResponse>(responseContent);

                    // Check if the captcha was successful
                    return captchaResponse.Success;
                }

                // If the POST request fails, you might want to log or handle the error.
                return false;
            }
        }

        private class CaptchaResponse {
            public bool Success { get; set; }
        }
    }
}

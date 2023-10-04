/* global angular */
angular.module("umbraco").controller("NewsletterExportController", function ($scope, $http, $window) {



  $scope.formData = {};

  $scope.submitForm = function () {
  

    var apiUrl = "https://manarethara20230915111500.azurewebsites.net/umbraco/api/newsletter/ExportNewsletterRequestsToCsv";
    apiUrl += "?startDate=" + $scope.formData.startDate.toISOString().split('T')[0];
    apiUrl += "&endDate=" + $scope.formData.endDate.toISOString().split('T')[0];

    // Make a GET request to the API to generate the Excel file
    $http.get(apiUrl, { responseType: 'arraybuffer' }).then(function (response) {
      var dataString = new TextDecoder().decode(new Uint8Array(response.data));

      if (dataString.includes("No data found")) {
        // Display an alert in a custom popup
        $window.alert("No data found");
      }
      else {
        var blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        var objectUrl = URL.createObjectURL(blob);

        // Create a temporary anchor element to trigger the download
        var a = document.createElement('a');
        a.href = objectUrl;
        a.download = 'newsletter_export.csv';
        a.style.display = 'none';
        document.body.appendChild(a);
        a.click();

        // Clean up
        document.body.removeChild(a);
        URL.revokeObjectURL(objectUrl);
      }
    }, function (error) {
      // Handle API error
      console.error("API error:", error);
      // You can display an error message to the user here
    });

  
  };
});

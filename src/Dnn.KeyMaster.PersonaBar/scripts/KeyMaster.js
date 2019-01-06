'use strict';
define(
    function () {

        return {
            init: function (wrapper, util, params, callback) {
                $(document).ready(function () {
                    $('#test-success').hide();
                    $('#test-failure').hide();

                    $('#keymaster-test-config').click(function () {
                        $('#test-success').show();
                        $('#test-failure').show();
                        // TODO - add logic to test the keys provided

                        // we need to create a Web API that calls everything internally. We already have the code that does this so it should be pretty simple.
                        //var payload = {
                        //    grant_type: 'client_credentials',
                        //    client_id: $('#keymaster-client-id').val(),
                        //    client_secret: $('#keymaster-client-secret').val(),
                        //    scope: 'https://vault.azure.net/.default'
                        //};

                        //$.ajax({
                        //    url: 'https://login.microsoftonline.com/' + $('#keymaster-directory-id').val() + '/oauth2/v2.0/token',
                        //    contentType: 'application/x-www-form-urlencoded',
                        //    type: 'POST',
                        //    data: $.param(payload),
                        //    beforeSend: function (request) {
                        //        request.setRequestHeader("Access-Control-Allow-Origin", "*");
                        //    },
                        //    success: function (response) {
                        //        if (response != null) {
                        //            alert(response.token_type);
                        //        }
                        //        alert('response null');
                        //    }
                        //});
                    });

                    $('.delete').click(function () {
                        $(this).parent().hide();
                    });
                });
            },

            initMobile: function (wrapper, util, params, callback) {
            },

            load: function (params, callback) {

            },

            loadMobile: function (params, callback) {
            }
        };
    });
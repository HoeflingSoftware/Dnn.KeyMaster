'use strict';
define(
    function () {

        return {
            init: function (wrapper, util, params, callback) {
                $(document).ready(function () {
                    $('#test-success').hide();
                    $('#test-failure').hide();

                    $('#keymaster-test-config').click(function () {
                        $('#test-success').hide();
                        $('#test-failure').hide();
                        $('#test-in-progess').show();

                        var payload = {
                            ClientId: $('#keymaster-client-id').val(),
                            ClientSecret: $('#keymaster-client-secret').val(),
                            SecretName: $('#keymaster-secret-name').val(),
                            DirectoryId: $('#keymaster-directory-id').val(),
                            KeyVaultUrl: $('#keymaster-key-vault-url').val()
                        };

                        $.ajax({
                            contentType: 'application/x-www-form-urlencoded',
                            url: '/desktopmodules/Admin/Dnn.PersonaBar/Modules/KeyMaster/API/Home/TestSecrets',
                            type: 'POST',
                            data: $.param(payload),
                            success: function () {
                                $('#test-in-progess').hide();
                                $('#test-success').show();
                            },
                            error: function () {
                                $('#test-in-progess').hide();
                                $('#test-failure').show();
                            }
                        });
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
﻿'use strict';
define(
    function () {

        return {
            init: function (wrapper, util, params, callback) {
                $(document).ready(function () {
                    $('#test-success').hide();
                    $('#test-failure').hide();
                    $('#save-in-progress').hide();
                    $('#test-in-progess').hide();
                    $('#save-success').hide();

                    var baseRoute = 'Dnn.KeyMaster';
                    var sf = window.dnn.utility.sf;

                    var isKeyMasterOn = {
                        route: baseRoute + '/Home/Status',
                        success: function (response) {
                            if (response.result.isEnabled) {
                                $('#keymaster-live').show();
                                $('#keymaster-start').hide();
                                $('#keymaster-stop').show();
                            } else {
                                $('#keymaster-live').hide();
                                $('#keymaster-start').show();
                                $('#keymaster-stop').hide();
                            }
                        }
                    };

                    sf.get(isKeyMasterOn.route, {}, isKeyMasterOn.success);

                    $.ajax({
                        url: '/API/personabar/' + baseRoute + '/Home/GetSecrets',
                        type: 'GET',
                        success: function (response) {
                            $('#keymaster-client-id').val(response.ClientId);
                            $('#keymaster-client-secret').val(response.ClientSecret);
                            $('#keymaster-secret-name').val(response.SecretName);
                            $('#keymaster-directory-id').val(response.DirectoryId);
                            $('#keymaster-key-vault-url').val(response.KeyVaultUrl);
                        }
                    });

                    $('#keymaster-start').click(function () {
                        $.ajax({
                            url: '/API/personabar/' + baseRoute + '/Home/EnableKeyMaster',
                            type: 'POST',
                            success: function () {
                                location.reload();
                            },
                            error: function () {
                                alert("UNABLE TO START KEY MASTER");
                            }
                        });
                    });

                    $('#keymaster-stop').click(function () {
                        $.ajax({
                            url: '/API/personabar/' + baseRoute + '/Home/DisableKeyMaster',
                            type: 'POST',
                            success: function () {
                                location.reload();
                            },
                            error: function () {
                                alert("UNABLE TO STOP KEY MASTER");
                            }
                        });
                    });

                    $('#keymaster-save-config').click(function () {
                        $('#test-success').hide();
                        $('#save-success').hide();
                        $('#test-failure').hide();
                        $('#test-in-progess').hide();
                        $('#save-in-progress').show();

                        var payload = {
                            ClientId: $('#keymaster-client-id').val(),
                            ClientSecret: $('#keymaster-client-secret').val(),
                            SecretName: $('#keymaster-secret-name').val(),
                            DirectoryId: $('#keymaster-directory-id').val(),
                            KeyVaultUrl: $('#keymaster-key-vault-url').val()
                        };

                        $.ajax({
                            contentType: 'application/x-www-form-urlencoded',
                            url: '/API/personabar/' + baseRoute + '/Home/SaveSecrets',
                            type: 'POST',
                            data: $.param(payload),
                            success: function () {
                                $('#save-in-progress').hide();
                                $('#save-success').show();
                            },
                            error: function () {
                                $('#save-in-progress').hide();
                                $('#test-failure').show();
                            }
                        });
                    });

                    $('#keymaster-test-config').click(function () {
                        $('#test-success').hide();
                        $('#test-failure').hide();
                        $('#save-in-progress').hide();
                        $('#save-success').hide();
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
                            url: '/API/personabar/' + baseRoute + '/Home/TestSecrets',
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
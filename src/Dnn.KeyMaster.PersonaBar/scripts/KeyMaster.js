'use strict';
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

                    var status = {
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

                    sf.get(status.route, {}, status.success);

                    var getSecrets = {
                        route: baseRoute + '/Home/GetSecrets',
                        success: function (response) {
                            $('#keymaster-client-id').val(response.result.ClientId);
                            $('#keymaster-client-secret').val(response.result.ClientSecret);
                            $('#keymaster-secret-name').val(response.result.SecretName);
                            $('#keymaster-directory-id').val(response.result.DirectoryId);
                            $('#keymaster-key-vault-url').val(response.result.KeyVaultUrl);
                        }
                    };

                    sf.get(getSecrets.route, {}, getSecrets.success);

                    $('#keymaster-start').click(function () {
                        var toggle = {
                            route: baseRoute + '/Home/Toggle',
                            payload: { isEnabled: true },
                            success: function (response) {
                                if (response.isSuccessful) {
                                    location.reload();
                                } else {
                                    // todo - display error message somewhere
                                }
                            }
                        };

                        sf.post(toggle.route, toggle.payload, toggle.success);
                    });

                    $('#keymaster-stop').click(function () {
                        var toggle = {
                            route: baseRoute + '/Home/Toggle',
                            payload: { isEnabled: false },
                            success: function (response) {
                                if (response.isSuccessful) {
                                    location.reload();
                                } else {
                                    // todo - display error message somewhere
                                }
                            }
                        };

                        sf.post(toggle.route, toggle.payload, toggle.success);
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

                        var saveConfig = {
                            route: baseRoute + '/Home/SaveSecrets',
                            payload: payload,
                            success: function (response) {
                                if (response.isSuccessful) {
                                    $('#save-in-progress').hide();
                                    $('#save-success').show();
                                } else {
                                    $('#save-in-progress').hide();
                                    $('#test-failure').show();
                                }
                            }
                        };

                        sf.post(saveConfig.route, saveConfig.payload, saveConfig.success);
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

                        var testConfig = {
                            route: baseRoute + '/Home/TestSecrets',
                            payload: payload,
                            success: function (response) {
                                if (response.isSuccessful) {
                                    $('#test-in-progess').hide();
                                    $('#test-success').show();
                                } else {
                                    $('#test-in-progess').hide();
                                    $('#test-failure').show();
                                }
                            }
                        };

                        sf.post(testConfig.route, testConfig.payload, testConfig.success);
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
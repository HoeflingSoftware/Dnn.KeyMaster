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
                    var isEnabled = false;

                    var status = {
                        route: baseRoute + '/Home/Status',
                        success: function (response) {
                            if (response.Success) {
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
                        route: baseRoute + '/Secrets/Get',
                        success: function (response) {
                            if (response.Success) {
                                $('#keymaster-client-id').val(response.Result.ClientId);
                                $('#keymaster-client-secret').val(response.Result.ClientSecret);
                                $('#keymaster-secret-name').val(response.Result.SecretName);
                                $('#keymaster-directory-id').val(response.Result.DirectoryId);
                                $('#keymaster-key-vault-url').val(response.Result.KeyVaultUrl);
                            }
                        }
                    };

                    sf.get(getSecrets.route, {}, getSecrets.success);

                    $('#keymaster-start').click(function () {
                        var toggle = {
                            route: baseRoute + '/Home/Toggle',
                            payload: { isEnabled: true },
                            success: function (response) {
                                if (response.Success) {
                                    location.reload();
                                } else {
                                    window.dnn.utility.notifyError("Something went wrong, check the logs for more information");
                                }
                            }
                        };

                        var modal = {
                            message: 'Start the Key Master, this may take a few seconds and will automatically reload the page',
                            confirm: 'OK',
                            cancel: 'Cancel',
                            confirmCallback: function () {
                                sf.post(toggle.route, toggle.payload, toggle.success);
                            }
                        };

                        window.dnn.utility.confirm(modal.message, modal.confirm, modal.cancel, modal.confirmCallback);                        
                    });

                    $('#keymaster-stop').click(function () {
                        var toggle = {
                            route: baseRoute + '/Home/Toggle',
                            payload: { isEnabled: false },
                            success: function (response) {
                                if (response.Success) {
                                    location.reload();
                                } else {
                                    window.dnn.utility.notifyError("Something went wrong, check the logs for more information");
                                }
                            }
                        };

                        var modal = {
                            message: 'Stop the Key Master, this may take a few seconds and will automatically reload the page',
                            confirm: 'OK',
                            cancel: 'Cancel',
                            confirmCallback: function () {
                                sf.post(toggle.route, toggle.payload, toggle.success);
                            }
                        };   

                        window.dnn.utility.confirm(modal.message, modal.confirm, modal.cancel, modal.confirmCallback); 
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
                            route: baseRoute + '/Secrets/Save',
                            payload: payload,
                            success: function (response) {
                                if (response.Success) {
                                    $('#save-in-progress').hide();
                                    $('#save-success').show();
                                } else {
                                    $('#save-in-progress').hide();
                                    $('#test-failure').show();
                                }
                            }
                        };

                        sf.get(status.route, {}, function (response) {
                            if (response.Success) {
                                var modal = {
                                    message: 'Changing secrets while Key Master is running is dangerous. Your site may no longer work after saving.', 
                                    confirm: 'OK',
                                    cancel: 'Cancel',
                                    confirmCallback: function () {
                                        sf.post(saveConfig.route, saveConfig.payload, saveConfig.success);
                                    },
                                    cancelCallback: function () {
                                        $('#save-in-progress').hide();
                                    }
                                };

                                window.dnn.utility.confirm(modal.message, modal.confirm, modal.cancel, modal.confirmCallback, modal.cancelCallback); 

                            } else {
                                sf.post(saveConfig.route, saveConfig.payload, saveConfig.success);
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

                        var testConfig = {
                            route: baseRoute + '/Secrets/Test',
                            payload: payload,
                            success: function (response) {
                                if (response.Success) {
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
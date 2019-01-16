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
                    $('#keymaster-appsettings').hide();
                                        
                    var baseRoute = 'Dnn.KeyMaster';
                    var sf = window.dnn.utility.sf;
                    var confirm = window.dnn.utility.confirm;

                    var createRow = function (name) {
                        var row = document.createElement('div');
                        row.classList = 'field-body';

                        var createControl = function (value, type, isProtected) {
                            var field = document.createElement('div');
                            field.classList = 'field';

                            var control = document.createElement('p');

                            if (type === 'input') {
                                control.classList = 'control is-expanded';
                            } else {
                                control.classList = 'control';
                            }

                            var contents = {};
                            if (type === 'input') {
                                contents = document.createElement('input');
                                contents.classList = 'input';

                                if (isProtected) {
                                    contents.name = 'value';
                                    contents.type = 'password';
                                    contents.value = '********************';
                                } else {
                                    contents.name = 'key';
                                    contents.type = 'text';
                                    contents.value = value;
                                }
                            } else if (type === 'delete') {
                                contents = document.createElement('button');
                                contents.classList = 'delete';
                            } else if (type === 'view') {
                                contents = document.createElement('span');
                                contents.classList = 'view-secret tag is-warning';
                                contents.innerHTML = value;
                            }

                            control.appendChild(contents);
                            field.appendChild(control);

                            return field;
                        };

                        var nameField = createControl(name, 'input', false);
                        var passwordField = createControl({}, 'input', true);
                        var viewField = createControl('View Secret', 'view');
                        var deleteField = createControl({}, 'delete');

                        row.appendChild(nameField);
                        row.appendChild(passwordField);
                        row.appendChild(viewField);
                        row.appendChild(deleteField);

                        return row;
                    };

                    var buildGetSecret = function (parent, key) {
                        var model = {
                            route: baseRoute + '/AppSettings/Get',
                            parameter: { key: key },
                            success: function (response) {
                                if (response.Success) {
                                    var secret = parent.find("input[name='value']")[0];
                                    secret.type = 'text';
                                    secret.value = response.Result.Value;

                                    openSecret(parent.find('span')[0], 'Update');
                                }
                            }
                        };

                        return model;
                    };

                    var deleteSecretRow = function () {
                        var parent = $(this).parent().parent().parent();
                        var row = this.parentElement.parentElement.parentElement;

                        var key = parent.find("input[name='key']");
                        var value = parent.find("input[name='value']");

                        if (key === '' || value === '') {
                            window.dnn.utility.notifyError("Can't delete empty app setting");
                        } else {

                            var modal = {
                                message: 'Are you sure you want to delete your secret?',
                                confirm: 'Delete',
                                cancel: 'Cancel',
                                confirmCallback: function () {
                                    var key = parent.find("input[name='key']").val();
                                    var deleteSecret = {
                                        route: baseRoute + '/AppSettings/Delete',
                                        payload: {
                                            Key: key
                                        },
                                        success: function (response) {
                                            if (response.Success) {
                                                row.remove();
                                                window.dnn.utility.notify('Deleted secret');
                                            } else {
                                                window.dnn.utility.notifyError('Unable to delete secret!');
                                            }
                                        }
                                    };

                                    sf.post(deleteSecret.route, deleteSecret.payload, deleteSecret.success);
                                }
                            };

                            confirm(modal.message, modal.confirm, modal.cancel, modal.confirmCallback);
                        }
                    };

                    var openSecret = function (element, title) {
                        var button = element;
                        var row = button.parentElement.parentElement.parentElement;
                        button.classList = 'update-secret tag is-danger';
                        button.innerHTML = title;
                        $(button).off('click');
                        $(button).on('click', function () {
                            var parent = $(this)[0].parentElement.parentElement.parentElement;
                            var key = $(row).find("input[name='key']")[0].value;
                            var value = $(row).find("input[name='value']")[0].value;
                            var inputs = $(parent.parentElement).find("input[name='key']");

                            var count = 0;
                            for (var index = 0; index < inputs.length; index++) {
                                if (inputs[index].value === key) {
                                    count++;
                                }
                            }

                            if (count > 1) {
                                window.dnn.utility.notifyError("Can't create duplicate App Setting!");
                            } else if (key === '' || value === '') {
                                window.dnn.utility.notifyError("Can't save empty key/value App Setting!");
                            } else {
                                var modal = {
                                    message: 'Are you sure you want to update your secret?',
                                    confirm: 'Save',
                                    cancel: 'Cancel',
                                    confirmCallback: function () {
                                        var saveSecret = {
                                            route: baseRoute + '/AppSettings/Save',
                                            payload: {
                                                Key: key,
                                                Value: $(row).find("input[name='value']")[0].value
                                            },
                                            success: function (response) {
                                                if (response.Success) {
                                                    var secret = $(parent).find("input[name='value']")[0];
                                                    secret.type = 'password';
                                                    secret.value = '********************';

                                                    button.classList = 'view-secret tag is-warning';
                                                    button.innerHTML = 'View Secret';
                                                    $(button).off('click');
                                                    $(button).on('click', function () {
                                                        var parent = $(this).parent().parent().parent();
                                                        var getSecret = buildGetSecret(parent, key);
                                                        sf.get(getSecret.route, getSecret.parameter, getSecret.success);
                                                    });

                                                    var delButton = $(row).find('.delete');
                                                    delButton.off('click');
                                                    delButton.on('click', deleteSecretRow);

                                                    window.dnn.utility.notify('Successfully updated secret');
                                                } else {
                                                    window.dnn.utility.notifyError('Unable to update secret!');
                                                }
                                            }
                                        };

                                        sf.post(saveSecret.route, saveSecret.payload, saveSecret.success);
                                    }
                                };

                                confirm(modal.message, modal.confirm, modal.cancel, modal.confirmCallback);
                            }
                        });
                    };

                    var isEnvVars = {
                        route: baseRoute + '/Home/IsEnvVars',
                        success: function (response) {
                            if (response.Success) {
                                $('#keymaster-env-vars').show();
                                $('#keymaster-save-config').hide();
                            } else {
                                $('#keymaster-env-vars').hide();
                                $('#keymaster-save-config').show();
                            }
                        }
                    };

                    sf.get(isEnvVars.route, {}, isEnvVars.success);

                    var status = {
                        route: baseRoute + '/Home/Status',
                        success: function (response) {
                            if (response.Success) {
                                $('#keymaster-live').show();
                                $('#keymaster-start').hide();
                                $('#keymaster-stop').show();

                                var appsettings = {
                                    route: baseRoute + '/AppSettings/List',
                                    success: function (response) {
                                        if (response.Success) {
                                            var container = document.getElementById('keymaster-appsettings-container');
                                            for (var index = 0; index < response.Result.length; index++) {
                                                container.append(createRow(response.Result[index].Key));
                                            }

                                            $('.view-secret').on('click', function () {
                                                var parent = $(this).parent().parent().parent();
                                                var key = parent.find("input[name='key']").val();

                                                var getSecret = buildGetSecret(parent, key);
                                                sf.get(getSecret.route, getSecret.parameter, getSecret.success);                                                
                                            });

                                            $($('#keymaster-appsettings-container')[0]).find('.delete').click(deleteSecretRow);
                                        }
                                    }
                                };

                                $('#keymaster-appsettings-add').show();
                                sf.get(appsettings.route, {}, appsettings.success);
                                                                
                                
                            } else {
                                $('#keymaster-appsettings-add').hide();

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

                    $('#keymaster-tabs-appsettings').click(function () {
                        $('#keymaster-tabs-configuration').removeClass('is-active');
                        $('#keymaster-tabs-appsettings').addClass('is-active');
                        $('#keymaster-configuration').hide();
                        $('#keymaster-appsettings').show();
                    });

                    $('#keymaster-tabs-configuration').click(function () {
                        $('#keymaster-tabs-appsettings').removeClass('is-active');
                        $('#keymaster-tabs-configuration').addClass('is-active');
                        $('#keymaster-appsettings').hide();
                        $('#keymaster-configuration').show();
                    });

                    $('#keymaster-add-appsetting').click(function () {
                        var row = createRow('');
                        var secretInput = $(row).find("input[name='value']")[0];
                        secretInput.type = 'text';
                        secretInput.value = '';
                        $(row).find('.delete').click(function () { row.remove(); });
                        openSecret($(row).find('.view-secret')[0], 'Save');
                        $('#keymaster-appsettings-container').append(row);
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
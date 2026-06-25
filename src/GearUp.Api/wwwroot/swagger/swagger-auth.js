(function () {
    var loginPath = '/api/autenticacao/login';

    function aplicarToken(token) {
        if (!token || !window.ui) {
            return;
        }

        window.ui.authActions.authorize({
            Bearer: {
                name: 'Bearer',
                schema: { type: 'http', scheme: 'bearer', bearerFormat: 'JWT' },
                value: token
            }
        });
    }

    var fetchOriginal = window.fetch.bind(window);
    window.fetch = function (input, init) {
        return fetchOriginal(input, init).then(function (response) {
            var url = typeof input === 'string' ? input : (input && input.url) || '';

            if (url.indexOf(loginPath) !== -1 && response.ok) {
                response.clone().json().then(function (data) {
                    aplicarToken(data.accessToken);
                }).catch(function () { });
            }

            return response;
        });
    };
})();

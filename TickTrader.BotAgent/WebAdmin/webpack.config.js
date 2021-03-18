var path = require('path');
var webpack = require('webpack');
const CopyWebpackPlugin = require('copy-webpack-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin')

var config = {
    entry:
    {
        'main': path.join(__dirname, '/Client/boot-client.ts')
    },
    output: {
        filename: '[name].js',
        path: path.join(__dirname, '/wwwroot/dist'),
        publicPath: '/dist/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
    },
    devtool: 'source-map',
    resolve:
    {
        extensions: ['*', '.js', '.ts', '.tsx'],
        fallback: {
            "crypto": false,
            "timers": false
        }
    },
    module: {
        rules: [
            {
                test: require.resolve("jquery"),
                loader: "expose-loader",
                options: {
                    exposes: ["$", "jQuery"],
                },
            },
            {
                test: /\.ts$/,
                include: /Client/,
                use: ['ts-loader'],
                //query: { silent: true }
            },
            {
                test: /\.html$/,
                type: 'asset/source'
            },
            {
                test: /\.css$/,
                use: ["to-string-loader", "css-loader"],
            },
            {
                test: /\.(png|jpg|jpeg|gif|svg)$/,
                type: 'asset',
            }
        ]
    },
    plugins: [
        new webpack.DllReferencePlugin({
            context: __dirname,
            manifest: require('./wwwroot/dist/vendor-manifest.json')
        }),
        new CopyWebpackPlugin({
            patterns: [
                {
                    context: path.join(__dirname, '/Assets'),
                    from: 'img/**',
                    to: path.join(__dirname, '/wwwroot/assets/')
                },
                {
                    context: path.join(__dirname, '/Assets'),
                    from: 'js/**',
                    to: path.join(__dirname, '/wwwroot/assets/')
                }
            ]
        }),
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery"
        })
    ]
};

module.exports = (env, argv) => {
    if (argv.mode === 'production') {
        config.devtool = false;
        config.plugins.push(new UglifyJsPlugin())
    }

    return config;
};

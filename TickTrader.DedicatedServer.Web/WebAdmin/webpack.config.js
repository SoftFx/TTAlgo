var environment = (process.env.NODE_ENV || "development").trim();
var isDevBuild = environment === "development";

var path = require('path');
var webpack = require('webpack');
var nodeExternals = require('webpack-node-externals');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var merge = require('webpack-merge');
var allFilenamesExceptJavaScript = /\.(?!js(\?|$))([^.]+(\?|$))/;

// Configuration in common to both client-side and server-side bundles
var sharedConfig = {
    resolve: { extensions: ['', '.js', '.ts'] },
    output: {
        filename: '[name].js',
        publicPath: '/dist/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
    },
    module: {
        loaders: [
            { test: /\.ts$/, include: /Client/, loader: 'ts', query: { silent: true } },
            { test: /\.html$/, loader: 'raw' },
            { test: /\.css$/, loader: 'css-to-string!css' },
            { test: /\.(png|jpg|jpeg|gif|svg)$/, loader: 'url', query: { limit: 25000 } }
        ]
    }
};

// Configuration for client-side bundle suitable for running in browsers
var clientBundleConfig = merge(sharedConfig, {
    entry: { 'main': path.join(__dirname, '/Client/boot-client.ts') },
    output: { path: path.join(__dirname, '/wwwroot/dist') },
    devtool: isDevBuild ? 'source-map' : null,
    plugins: [
        new webpack.DllReferencePlugin({
            context: __dirname,
            manifest: require('./wwwroot/dist/vendor-manifest.json')
        }),
        new CopyWebpackPlugin([
             { context: path.join(__dirname, '/Assets'), from: 'img/**', to: path.join(__dirname, '/wwwroot/assets/') },
             { context: path.join(__dirname, '/Assets'), from: 'js/**', to: path.join(__dirname, '/wwwroot/assets/') }]),
        new webpack.ProvidePlugin({
            $: "jquery",
            jQuery: "jquery"
        })
    ].concat(isDevBuild ? [] : [
        // Plugins that apply in production builds only
        new webpack.optimize.OccurenceOrderPlugin(),
        new webpack.optimize.UglifyJsPlugin()
    ])
});

module.exports = clientBundleConfig;

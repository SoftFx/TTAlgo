var environment = (process.env.NODE_ENV || "development").trim();
var isDevBuild = environment === "development";

var path = require('path');
var webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin')
//var ExtractTextPlugin = require('extract-text-webpack-plugin');
//var extractCSS = new ExtractTextPlugin('vendor.css');

module.exports = {
    resolve: {
        extensions: ['*', '.js'],
        fallback: {
            "crypto": false
        }
    },
    module: {
        rules: [
            { 
                test: /\.(png|woff|woff2|eot|ttf|svg)(\?|$)/,
                type: 'asset'
            },
            { 
                test: /\.css(\?|$)/, 
                use: [MiniCssExtractPlugin.loader, 'css-loader'], 
            }
        ]
    },
    entry: {
        vendor: [
            '@angular/common',
            '@angular/compiler',
            '@angular/core',
            '@angular/http',
            '@angular/platform-browser',
            '@angular/platform-browser-dynamic',
            '@angular/router',
            '@angular/platform-server',
            'angular2-universal',
            'angular2-universal-polyfills',
            'bootstrap',
            'bootstrap-notify',
            'bootstrap/dist/css/bootstrap.css',
            "angular2-masonry",
            "masonry-layout",
            'es6-shim',
            'jquery',
            'es6-promise',
            'reflect-metadata',
            'zone.js',
            'rxjs',
            'font-awesome/css/font-awesome.css',
            './WebAdmin/assets/css/light-bootstrap.css',
            'string-format',
        ]
    },
    output: {
        path: path.join(__dirname, 'wwwroot', 'dist'),
        filename: '[name].js',
        library: '[name]_[hash]',
    },
    plugins: [
        //extractCSS,
        new MiniCssExtractPlugin({
            filename: 'vendor.css',
        }),
        new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
        // new webpack.optimize.OccurrenceOrderPlugin(),
        new webpack.DllPlugin({
            path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
            name: '[name]_[hash]'
        })
    ].concat(isDevBuild ? [] : [
        new UglifyJsPlugin(),
    ])
};

const Path = require("path");
const TerserPlugin = require("terser-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");
const CopyWebpackPlugin = require("copy-webpack-plugin");

const opts = {
    rootDir: process.cwd(),
    devBuild: process.env.NODE_ENV !== "production"
};

module.exports = {
    entry: {
        app: "./wwwroot/enrollment/src/js/app.js"
    },

    mode: process.env.NODE_ENV === "production" ? "production" : "development",

    devtool:
        process.env.NODE_ENV === "production"
            ? "source-map"
            : "inline-source-map",

    output: {
        path: Path.join(opts.rootDir, "wwwroot/enrollment/static"),
        pathinfo: opts.devBuild,
        filename: "js/[name].js",
        chunkFilename: "js/[name].js"
    },

    performance: { hints: false },

    optimization: {
        minimizer: [
            new TerserPlugin({
                parallel: true,
                terserOptions: {
                    ecma: 5
                }
            }),
            new CssMinimizerPlugin({})
        ],
        runtimeChunk: false
    },

    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/app.css",
            chunkFilename: "css/app.css"
        }),

        new CopyWebpackPlugin({
            patterns: [
                {
                    from: "wwwroot/enrollment/src/fonts",
                    to: "fonts"
                },
                {
                    from: "wwwroot/enrollment/src/img",
                    to: "img"
                }
            ]
        })
    ],

    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /(node_modules)/,
                use: {
                    loader: "babel-loader",
                    options: {
                        cacheDirectory: true
                    }
                }
            },

            {
                test: /\.(sa|sc|c)ss$/,
                use: [
                    MiniCssExtractPlugin.loader,
                    "css-loader",
                    "postcss-loader",
                    "sass-loader"
                ]
            },

            {
                test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
                type: "asset/resource",
                generator: {
                    filename: "fonts/[name][ext]"
                }
            },

            {
                test: /\.(png|jpg|jpeg|gif)(\?v=\d+\.\d+\.\d+)?$/,
                type: "asset/resource",
                generator: {
                    filename: "img/[name][ext]"
                }
            }
        ]
    },

    resolve: {
        extensions: [".js", ".scss"],
        modules: ["node_modules"]
    },

    devServer: {
        static: {
            directory: Path.join(
                __dirname,
                "wwwroot/enrollment/static"
            )
        },
        compress: true,
        port: 8080,
        open: true
    }
};

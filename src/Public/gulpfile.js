/// <binding BeforeBuild='buildStyles, buildScripts' Clean='cleanStyles, cleanScripts' />
"use strict";

const gulp = require("gulp"),
    clean = require("gulp-clean"),
    cssmin = require("gulp-cssmin"),
    rename = require("gulp-rename"),
    sass = require("gulp-sass")(require("sass")),
    terser = require("gulp-terser");

var paths = {
    webroot: "./wwwroot/",
};

paths.cssOutput = paths.webroot + "css/";
paths.scss = paths.webroot + "scss/**/*.scss";
paths.js = paths.webroot + "js/**/*.js";
paths.jsOutput = paths.webroot + "js/";

async function buildStyles() {
    const sassOptions = {
        quietDeps: true,
        loadPaths: paths.webroot + "scss/lib",
    };
    return gulp
        .src(paths.scss)
        .pipe(sass(sassOptions).on("error", sass.logError))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest(paths.cssOutput));
}

async function cleanScripts() {
    return gulp
        .src(paths.jsOutput + "*.min.css", { allowEmpty: true, read: false })
        .pipe(clean());
}

async function cleanStyles() {
    return gulp
        .src(paths.cssOutput + "*.min.css", { allowEmpty: true, read: false })
        .pipe(clean());
}

async function minifyJs() {
    return gulp
        .src([paths.js, "!**/*.min.js"]) // Avoid re-minifying already minified files
        .pipe(terser())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest(paths.jsOutput));
}

exports.buildScripts = minifyJs;
exports.buildStyles = buildStyles;
exports.cleanScripts = cleanScripts;
exports.cleanStyles = cleanStyles;
exports.watch = function () {
    gulp.watch(paths.scss, buildStyles);
};

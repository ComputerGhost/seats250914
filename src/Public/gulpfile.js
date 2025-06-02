/// <binding />
/**
 * My stylesheets need preprocessing, but my script files are simple enough to just include as-is.
 * Therefore, this gulpfile configuration only includes tasks for processing stylesheets.
 */
"use strict";

const gulp = require("gulp"),
    clean = require("gulp-clean"),
    cssmin = require("gulp-cssmin"),
    rename = require("gulp-rename"),
    sass = require("gulp-sass")(require("sass"));

var paths = {
    webroot: "./wwwroot/",
};

paths.scss = paths.webroot + "scss/**/*.scss";
paths.cssOutput = paths.webroot + "css/";

async function buildStyles() {
    return gulp
        .src(paths.scss)
        .pipe(sass().on("error", sass.logError))
        .pipe(cssmin())
        .pipe(rename({ suffix: ".min" }))
        .pipe(gulp.dest(paths.cssOutput));
}

async function cleanStyles() {
    return gulp
        .src(paths.cssOutput + "*.min.css", { allowEmpty: true, read: false })
        .pipe(clean());
}

exports.buildStyles = buildStyles;
exports.cleanStyles = cleanStyles;
exports.watch = function () {
    gulp.watch(paths.scss, buildStyles);
};

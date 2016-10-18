'use strict';

var libQ = require('kew');
var libNet = require('net');
var libFast = require('fast.js');
var fs = require('fs-extra');
var config = new (require('v-conf'))();
var exec = require('child_process').exec;
var qobuzApi = require('./qobuz-api');
var nodetools = require('nodetools');

// Define the ControllerQobuz class
module.exports = ControllerQobuz;

function ControllerQobuz(context) {
    var self = this;

    this.context = context;
    this.commandRouter = this.context.coreCommand;
    this.logger = this.context.logger;
    this.configManager = this.context.configManager;
}

ControllerQobuz.prototype.onVolumioStart = function () {
    var self = this;
    var configFile = this.commandRouter.pluginManager.getConfigurationFile(this.context, 'config.json');
    this.config = new (require('v-conf'))();
    this.config.loadFile(configFile);

    if (self.config.get('bitrate') === true)
        self.samplerate = "320Kbps";
    else self.samplerate = "128Kbps";
};

ControllerQobuz.prototype.getConfigurationFiles = function () {
    return ['config.json'];
};

ControllerQobuz.prototype.addToBrowseSources = function () {
    var data = { name: 'Qobuz', uri: 'qobuz', plugin_type: 'music_service', plugin_name: 'qobuz' };
    this.commandRouter.volumioAddToBrowseSources(data);
};

ControllerQobuz.prototype.handleBrowseUri = function (curUri) {
    var self = this;

    self.commandRouter.pushConsoleMessage('[' + Date.now() + '] ' + 'ControllerQobuz::handleBrowseUri: ' + curUri);

    var response;

    if (curUri.startsWith('qobuz')) {
        if (curUri === 'qobuz') {
            response = self.rootList();
        }
        else if (curUri.startsWith('qobuz/favourites/albums')) {
            response = self.favouriteAlbumsList();
        }
        else if (curUri.startsWith('qobuz/favourites/tracks')) {
            response = self.favouriteTracksList(curUri);
        }
        else if (curUri.startsWith('qobuz/favourites/playlists')) {
            response = self.userPlaylistsList(curUri);
        }
    }

    return response;
};

ControllerQobuz.prototype.listItem = function (type, title, artist, album, icon, uri) {
    return {
        service: 'qobuz',
        type: type,
        title: title,
        artist: artist,
        album: album,
        icon: icon,
        uri: uri
    };
};

ControllerQobuz.prototype.folderListItem = function (title, artist, album, uri) {
    return this.listItem('qobuz-category', title, '', '', 'fa fa-folder-open-o', uri);
};

ControllerQobuz.prototype.trackListItem = function (title, artist, album, uri) {
    return this.listItem('song', title, artist, album, 'fa fa-microphone', uri);
};

ControllerQobuz.prototype.navigationFolderListItem = function (title, uri) {
    var self = this;
    return self.folder(title, "", "", uri);
};

ControllerQobuz.prototype.populatedNavigation = function (views, items, prev) {
    var nav = this.emptyNavigation();
    nav.navigation.lists[0].availableListViews.concat(views);
    nav.navigation.lists[0].items.concat(items);
    nav.navigation.prev.uri = prev;
};

ControllerQobuz.prototype.emptyNavigation = function () {
    return {
        navigation: {
            lists: [
                {
                    "availableListViews": [],
                    "items": []
                }
            ],
            "prev": {
                uri: ''
            }
        }
    };
};

ControllerQobuz.prototype.rootList = function () {
    libQ.resolve(
        this.populatedNavigation(
            ["list"],
            [
                self.navigationFolderListItem("My Albums", "qobuz/favourites/albums"),
                self.navigationFolderListItem("My Tracks", "qobuz/favourites/tracks"),
                self.navigationFolderListItem("My Playlists", "qobuz/favourites/playlists")
            ],
            "/"));
};

ControllerQobuz.prototype.favouriteAlbumsList = function () {
    var self = this;

    var defer = libQ.defer();

    var parseResult = function (result) {
        items = [];
        for (i = 0; i < result.albums.items.length; i++) {
            var qobuzAlbum = result.albums.items[i];
            items.push(this.folderListItem(qobuzAlbum.title, qobuzAlbum.artist.name, qobuzAlbum.title, "qobuz/favourites/album/" + qobuzAlbum.id));
        }
        return items;
    };

    self.api.getFavourites("albums")
        .then(function (result) {
            defer.resolve(self.populatedNavigation(["list", "grid"], parseResult(result), "qobuz"));
        })
        .fail(function (e) { defer.reject(new Error()); });

    return defer.promise;
};

ControllerQobuz.prototype.favouriteTracksList = function () {
    var self = this;

    var defer = libQ.defer();

    var parseResult = function (result) {
        items = [];
        for (i = 0; i < result.tracks.items.length; i++) {
            var qobuzTrack = result.tracks.items[i];
            items.push(this.trackListItem(qobuzTrack.title, qobuzTrack.album.artist.name, qobuzTrack.album.title, "qobuz/favourites/track/" + qobuzTrack.id));
        }
        return items;
    };

    self.api.getFavourites("tracks")
        .then(function (result) {
            defer.resolve(self.populatedNavigation(["list", "grid"], parseResult(result), "qobuz"));
        })
        .fail(function (e) { defer.reject(new Error()); });

    return defer.promise;
};

ControllerQobuz.prototype.userPlaylistsList = function () {
    var self = this;

    var defer = libQ.defer();

    var parseResult = function (result) {
        items = [];
        for (i = 0; i < result.playlists.items.length; i++) {
            var qobuzPlayList = result.playlists.items[i];
            items.push(this.folderListItem(qobuzPlayList.name, qobuzPlayList.owner.name, qobuzPlayList.name, "qobuz/favourites/playlist/" + qobuzPlayList.id));
        }
        return items;
    };

    self.api.getUserPlaylists()
        .then(function (result) {
            defer.resolve(self.populatedNavigation(["list", "grid"], parseResult(result), "qobuz"));
        })
        .fail(function (e) { defer.reject(new Error()); });

    return defer.promise;
};

ControllerSpop.prototype.explodeUri = function (uri) {

    var self = this;

    var defer = libQ.defer();

    var uriParts = uri.split('/');

    var response;

    if (uri.startsWith('qobuz/favourites/track')) {
        var trackId = uriParts[3];
        self.api.getTrackUrl(trackId, 6)
            .then(function (result) { defer.resolve({}); })
            .fail(function (e) { defer.reject(new Error()); });
    }

    if (uri.startsWith('qobuz/favourites/album')) {
        var albumId = uriParts[3];
        self.api.getAlbumTracks(albumId, 6)
            .then(function (result) { defer.resolve([]); })
            .fail(function (e) { defer.reject(new Error()); });
    }

    if (uri.startsWith('qobuz/favourites/playlists')) {
        var playlistId = uriParts[3];
        self.api.getPlayListTracks(playlistId, 6)
            .then(function (result) { defer.resolve([]); })
            .fail(function (e) { defer.reject(new Error()); });
    }
    return defer.promise;
};
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

    if (self.config.get('max_bitrate') !== '')
        self.samplerate = self.config.get('max_bitrate');
    else self.samplerate = "6";
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

/*index.js*/
ControllerSpop.prototype.getUIConfig = function () {
    var defer = libQ.defer();
    var self = this;

    var lang_code = this.commandRouter.sharedVars.get('language_code');

    self.commandRouter.i18nJson(__dirname + '/i18n/strings_' + lang_code + '.json',
		__dirname + '/i18n/strings_en.json',
		__dirname + '/UIConfig.json')
		.then(function (uiconf) {

		    uiconf.sections[0].content[0].value = self.config.get('username');
		    uiconf.sections[0].content[1].value = '*********';
		    uiconf.sections[0].content[2].value = self.config.get('max_bitrate');

		    defer.resolve(uiconf);
		})
		.fail(function () {
		    defer.reject(new Error());
		});

    return defer.promise;
};

ControllerQobuz.prototype.onVolumioStart = function () {
    var self = this;
    var configFile = this.commandRouter.pluginManager.getConfigurationFile(this.context, 'config.json');
    this.config = new (require('v-conf'))();
    this.config.loadFile(configFile);

    if (self.config.get('max_bitrate') !== '')
        self.samplerate = self.config.get('max_bitrate');
    else self.samplerate = "6";
};
/*end index.js*/

/*config.json*/
{
    "enabled": {
        "type": "boolean",
        "value": false
    },
  "username": {
      "type": "string",
      "value": ""
  },
  "password": {
      "type": "string",
      "value": ""
  },
  "user_auth_token": {
      "type": "string",
      "value": ""
  },
  "max_bitrate": {
      "type": "boolean",
      "value": "string"
  }
}
/*end config.json*/

/*UIConfig.json*/
{
    "page": {
        "label": "TRANSLATE.QOBUZ_CONFIGURATION"
    },
  "sections": [
   {
       "id": "section_account",
       "element": "section",
       "label": "TRANSLATE.QOBUZ_ACCOUNT",
       "icon": "fa-plug",
       "onSave": {"type":"controller", "endpoint":"music_service/qobuz", "method":"saveQobuzConfiguration"},
       "saveButton": {
           "label": "TRANSLATE.SAVE",
           "data": [
             "username",
             "password",
             "max_bitrate"
           ]
       },
       "content": [
         {
             "id": "username",
             "type":"text",
             "element": "input",
             "doc": "TRANSLATE.QOBUZ_USERNAME_DOC",
             "label": "TRANSLATE.QOBUZ_USERNAME",
             "value": ""
         },
         {
             "id": "password",
             "type":"password",
             "element": "input",
             "doc": "TRANSLATE.QOBUZ_PASSWORD_DOC",
             "label": "TRANSLATE.QOBUZ_PASSWORD",
             "value": "",
             'visibleIf': {'field': 'spotify_service', 'value': true}
         },
         {
             "id":"max_bitrate",
             "doc": "TRANSLATE.MAX_BITRATE_DOC",
             "label": "TRANSLATE.MAX_BITRATE",
             "element": "select",
             "value": { 'value': 6, 'label': 'CD - 16bits / 44.1kHz' },
             'options': [
                 {
                     'value': 5,
                     'label': 'MP3 - 320 kbps'
                 },
                 {
                     'value': 6,
                     'label': 'CD - 16bits / 44.1kHz'
                 },
                 {
                     'value': 7,
                     'label': 'HiRes - 24bits / up to 96 kHz'
                 },
                 {
                     'value': 27,
                     'label': 'HiRes - 24bits / up to 192 kHz'
                 }
             ]
         }
       ]
   }
  ]
}
/*end UIConfig.json*/
/*strings_en.json*/
{
    "QOBUZ_USERNAME": "Qobuz username",
    "QOBUZ_USERNAME_DOC": "This is the password of your Qobuz account",
    "QOBUZ_PASSWORD": "Qobuz password",
    "QOBUZ_PASSWORD_DOC": "Qobuz password",
    "MAX_BITRATE_DOC": "HiRes rates are only available to stream for users with a 'Sublime' subscription who have purchased that music in a HiRes format.",
    "SEARCH_RESULTS": "Number of results",
    "PLUGINS": "Last.fm",
    "LAST_FM_USERNAME": "Last.fm username",
    "LAST_FM_PASSWORD": "Last.fm password",
    "SEARCH_SONGS_SECTION": "Qobuz songs",
    "SEARCH_ALBUMS_SECTION": "Qobuz albums",
    "SEARCH_ARTISTS_SECTION": "Qobuz artists",
    "QOBUZ_CONFIGURATION": "Qobuz Configuration",
    "QOBUZ_ACCOUNT": "Qobuz account",
    "SAVE": "Save"

}

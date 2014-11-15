var express = require('express');
var http = require('http');
var path = require('path');

var file = require('./routes/file');
var www = require('./routes/web');
var api = require('./routes/api');
var watcher = require('./classes/watcher');

var app = express();

// all environments
app.set('port', process.env.PORT || 3001);
app.set('views', path.join(__dirname, 'views'));
app.set('view engine', 'jade');
app.use(express.favicon());
app.use(express.logger('dev'));
app.use(express.json());
app.use(express.urlencoded());
app.use(express.methodOverride());
app.use(app.router);
app.use(express.static(path.join(__dirname, 'public')));

// development only
if ('development' == app.get('env')) {
	app.use(express.errorHandler());
}


app.get('/', www.index);
app.get('/list/index', api.tableView); // http://localhost:3000/
app.get('/list/folders', api.listFolders); // http://localhost:3000/list/folders
app.get('/list/movies', api.listMovies); // http://localhost:3000/list/movies
app.get('/list/media/:year/:month', api.getMediaForDate); // http://localhost:3000/list/media/2012/01?title=poop
app.get('/getFile', file.getFile); // http://localhost:3000/getFile?Path=i:\\pictures\\jed-iphone\\home videos\\1970-02-01 lippold epic 2.mov&mode=thumbnail

http.createServer(app).listen(app.get('port'), function() {
	console.log('Express server listening on port ' + app.get('port'));
});

watcher.startWatch();


var chokidar = require('chokidar');
var fs = require('../classes/file');

function saveDirectoryListtoDB(path) {

}

// Watch a directory or file
exports.startWatch = function() {

	var watcher = chokidar.watch('/Users/Jed/Downloads', {
		ignored: /^\./,
		persistent: true,
		ignoreInitial: true
	});
	watcher
		.on('add', function(path) {
			//refresh parent dir
			console.log('File', path, 'has been added');
		})
		.on('addDir', function(path) {
			//refresh dir
			console.log('Directory', path, 'has been added');
		})
		.on('change', function(path) {
			//refresh parent dir
			console.log('File', path, 'has been changed');
		})
		.on('unlink', function(path) {
			console.log('File', path, 'has been removed');
		})
		.on('unlinkDir', function(path) {
			console.log('Directory', path, 'has been removed');
		})
		.on('error', function(error) {
			console.error('Error happened', error);
		});
};

exports.refreshDB = function() {

};

exports.refreshFolder = function(path) {
	fs.getFoldersInDirectory(path, function(directories) {
		saveDirectoryListtoDB(directories)
	});
};
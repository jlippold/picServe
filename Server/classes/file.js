var fs = require('fs');
var walk = require('walk');

exports.getFoldersInDirectory = function(path, onComplete) {
	var files = [];
	var walker = walk.walk(path, {
		followLinks: true
	});

	walker.on('directories', function(root, stat, next) {
		// Add this file to the list of files
		files.push(root);
		next();
	});

	walker.on('end', function() {
		onComplete(files);
		//console.log(files);
	});
};
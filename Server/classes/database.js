var config = {
	dbPath: './picServ.sqlite'
};

function initialize() {
	var fs = require("fs");
	if (!fs.existsSync(config.dbPath)) {
		console.log("Creating DB file.");
		fs.openSync(config.dbPath, "w");
		//create initial schema here;
	}
}

exports.getAllRowsForQuery = function(sql, params, success, error) {
	initialize();
	var sqlite3 = require("sqlite3").verbose();
	var db = new sqlite3.Database(config.dbPath);
	db.all(sql, params, function(err, row) {
		if (err) {
			console.log(err);
		}
		success(row);
	});
	db.close();
};
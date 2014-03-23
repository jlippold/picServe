var db = require("../classes/database");
var util = require("../classes/util");
var fs = require("fs");
var path = require("path");
var async = require("async");

exports.tableView = function(req, res) {
	var outputArray = [];
	var asyncTasks = [];

	asyncTasks.push(function(moveNext) { //add async function to array
		db.getAllRowsForQuery("SELECT FolderName FROM baseFolders order by FolderName", {}, function(result) {

			result.forEach(function(item) { //loop each record
				var div = "\\"; //path.sep;
				var pathArr = item.FolderName.split(div);
				outputArray.push({
					textLabel: pathArr[pathArr.length - 1],
					detailTextLabel: "",
					icon: "greyarrow",
					sectionHeader: "Folders",
					drillVal: item.FolderName
				});
			});
			moveNext();
		});

	});

	asyncTasks.push(function(moveNext) { //add async function to array
		outputArray.push({
			textLabel: "All Movies",
			detailTextLabel: "",
			icon: "greyarrow",
			sectionHeader: "Movies",
			drillVal: "Movies"
		});
		moveNext();
	});

	asyncTasks.push(function(moveNext) { //add async function to array
		var strSQL = "SELECT strftime('%Y', DateTaken) AS Year, strftime('%m', DateTaken) as month, COUNT(FullName) AS Pics " +
			"FROM         Pictures GROUP BY strftime('%Y', DateTaken), strftime('%m', DateTaken) " +
			"ORDER BY strftime('%Y', DateTaken) DESC, strftime('%m', DateTaken) DESC";
		db.getAllRowsForQuery(strSQL, {}, function(result) {
			result.forEach(function(item) { //loop each record
				var thisItem = {
					textLabel: util.monthNames()[parseFloat(item.month, 10) - 1] + " " + item.Year,
					detailTextLabel: item.Pics + " images",
					icon: "greyarrow",
					sectionHeader: "Folders",
					drillVal: item.Year + "-" + item.month
				};
				if (item.Year <= new Date().getFullYear()) { //no future dates
					outputArray.push(thisItem);
				}
			});
			moveNext();
		});

	});

	async.series(asyncTasks, function() {
		// All tasks are done now
		res.send(outputArray);
	});
};

exports.listFolders = function(req, res) {
	db.getAllRowsForQuery("SELECT * FROM folderProps order by FolderName", {}, function(result) {

		var outputArray = []; //hold the list of folders
		var asyncTasks = []; //hold array of async functions

		result.forEach(function(item) { //loop each record
			asyncTasks.push(function(moveNext) { //add async function to array
				fs.exists(item.FolderName, function(exists) {
					var div = "\\"; //path.sep;
					var pathArr = item.FolderName.split(div);
					var parentPathArr = item.FolderName.split(div).splice(0, pathArr.length - 1);

					outputArray.push({
						name: pathArr[pathArr.length - 1],
						fileCount: 90,
						fullPath: item.FolderName,
						parentName: parentPathArr[parentPathArr.length - 1],
						parentFull: parentPathArr.join(div),
						DateModified: item.DateModified,
						exists: exists
					});

					moveNext(); //tell them I am done
				});
			});
		});

		async.parallel(asyncTasks, function() {
			// All tasks are done now
			res.send(outputArray);
		});
	});
};

exports.getMediaForDate = function(req, res) {

	if (!req.query.title) {
		res.send({
			error: "missing parameter: t"
		});
	}
	db.getAllRowsForQuery("SELECT * FROM Pictures where strftime('%Y', DateTaken) = $y and strftime('%m', DateTaken) = $m ORDER BY DateTaken", {
		$y: req.params.year,
		$m: req.params.month
	}, function(result) {

		var outputArray = [];
		result.forEach(function(item) {

			var thisItem = {
				sectionHeader: req.query.t,
				image: "/getFile/?Path=" + item.FullName + "&mode=thumbnail",
				name: item.FileName,
				Path: item.FilePath,
				Type: "Image",
				UNCPath: "", //d.Add("UNCPath", String.Concat("\\", My.Computer.Name, "\", rst("FilePath").replace(":", "$") & "\" & rst("FileName")))
				DateCreated: item.DateTaken.toEpoch,
				cachePath: ""
			};
			if (item.FileName.endsWith(".mov")) {
				thisItem.Type = "Video";
			}
			outputArray.push(thisItem);

		});
		res.send(outputArray);
	});
};

exports.listMovies = function(req, res) {

	db.getAllRowsForQuery("SELECT strftime('%Y', DateTaken) AS Year, strftime('%m', DateTaken) as month, * FROM Pictures where FileName like '%.mov' ORDER BY DateTaken", {}, function(result) {

		var outputArray = [];
		result.forEach(function(item) {
			var thisItem = {
				sectionHeader: util.monthNames()[parseFloat(item.month, 10) - 1] + " " + item.Year,
				image: "/getFile/?Path=" + item.FullName + "&mode=thumbnail",
				name: item.Dimensions + " " + item.FileName,
				Path: item.FilePath,
				Type: "Video",
				UNCPath: "", //String.Concat("\\", My.Computer.Name, "\", rst("FilePath").replace(":", "$") & "\" & rst("FileName")))
				DateCreated: item.DateTaken.toEpoch,
				cachePath: ""
			};
			outputArray.push(thisItem);

		});
		res.send(outputArray);
	});
};


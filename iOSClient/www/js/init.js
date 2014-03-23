var init = {
	serverAddress: "192.168.1.0",
	serverKey: "",
	uploadPort: 0,
	wifiOnly: true,
	backButtonPath: "",
	basePaths: [],
	PhoneGapReady: function() {
		document.addEventListener("deviceready", init.onDeviceReady, false);
		document.addEventListener("resume", init.onResume, false);
		document.addEventListener("pause", init.onBackground, false);
		//window.onorientationchange = util.detectOrientation;
		//window.onresize = util.detectOrientation;
		window.onerror = function(msg, url, line) {
			console.log("\n\n" + msg + "\n" + url + "\nline " + line + "\n\n\n");
		};
	},
	showLoadError: function() {
		navigator.notification.confirm(
			'The server is not responding in time. Make sure the server application is running and the correct IP is entered in settings', // message

			function(i) {
				if (i === 1) {
					init.loadRoot(init.urlFromPath("", "getRoot"));
				} else {
					window.open("preferences://");
				}
			},
			'picServe', // title
			'Retry' // buttonLabels
		);
	},
	onDeviceReady: function() {
		window.scrollTo(0, 0);
		//init.loadImageView(init.urlFromPath("", ""));
		settings.get(function() {
			init.loadRoot(init.urlFromPath("", "getRoot"));
		});
	},
	urlFromPath: function(p, script) {
		var req = {};
		req.URL = "http://" + init.serverAddress + "/" + script + "/?Path=" + escape(p.replace(/\\/g, "\\\\")) + "&key=" + init.serverKey;
		req.Path = p;
		return req;
	},
	loadRoot: function(req) {
		util.doHud({
			show: true,
			labelText: "Loading Data",
			detailsLabelText: "Please Wait..."
		});

		init.getJsonFromServer(req.URL, function(tableView, isCached) {
			init.basePaths = [];
			$.each(tableView, function(i, x) {
				if (x.sectionHeader === "Folders") {
					init.basePaths.push(x.drillVal);
				}
			});

			var nt = window.plugins.NativeTable;
			nt.createTable({
				'height': $(window).height(),
				'showSearchBar': true,
				'showNavBar': true,
				'navTitle': "picServe",
				'navBarColor': 'black',
				'showRightButton': true,
				'RightButtonText': 'Cache'
			});
			nt.onRightButtonTap(function() {
				var actions = [];
				var actionSheet = window.plugins.actionSheet;
				actions.push("Upload Pictures");
				actions.push("Clear Item Cache");
				actions.push("Cancel");
				actionSheet.create({
					title: 'picServe',
					items: actions,
					destructiveButtonIndex: (actions.length - 1)
				}, function(buttonValue, buttonIndex) {
					if (buttonIndex == -1 || buttonIndex == (actions.length - 1)) {
						return;
					} else {
						if (buttonIndex === 0) {
							sync.beginSync();
						}
						if (buttonIndex === 1) {
							cache.clear();
							util.doAlert("Cache Cleared");
						}
					}
				});

			});
			nt.setRowSelectCallBackFunction(function(rowId) {
				var item = tableView[rowId];
				nt.hideTable(function() {
					util.doHud({
						show: true,
						labelText: "Loading Data...",
						detailsLabelText: "Please Wait..."
					});
					if (item.sectionHeader === "Folders") {
						init.loadImageView(init.urlFromPath(item.drillVal, "", true));
					} else if (item.sectionHeader === "Movies") {
						init.loadMovies();
					} else {
						var param = {};
						param.text = item.drillVal;
						param.label = item.textLabel;

						init.loadDynamic(param);
					}
				});
			});
			nt.setTableData(tableView);
			util.doHud({
				show: false
			});
			nt.showTable(function() {});
		});

	},
	loadMovies: function() {
		util.doHud({
			show: true,
			labelText: "Loading Data",
			detailsLabelText: "Please Wait..."
		});

		var req = init.urlFromPath("", "getMovies");

		init.getJsonFromServer(req.URL, function(images, isCached) {

			var ic = window.plugins.ImageCollection;

			ic.setImageViewData(images);
			ic.createImageView({
				'navTitle': "Movies",
				'showBackButton': true,
				'backButtonText': "picServ",
				'showRightButton': false,
				'zipcache': "",
				'zipAddress': ""
			});
			ic.onBackButtonTap(function() {
				ic.hideImageView(function() {
					init.loadRoot(init.urlFromPath("", "getRoot"));
				});
			});
			ic.setRowSelectCallBackFunction(function(rowId) {

				init.playVideo(images[rowId].image.replace("&mode=thumbnail", ""), images[rowId].name, images[rowId].UNCPath);

			});

			ic.showImageView(function() {
				util.doHud({
					show: false
				});
			});

		});

	},
	loadDynamic: function(param) {
		util.doHud({
			show: true,
			labelText: "Loading Data",
			detailsLabelText: "Please Wait..."
		});

		var req = init.urlFromPath("", "getDynamic");

		init.getJsonFromServer(req.URL + "&v=" + param.text + "&t=" + param.label, function(images, isCached) {

			var ic = window.plugins.ImageCollection;

			ic.setImageViewData(images);
			ic.createImageView({
				'navTitle': param.label,
				'showBackButton': true,
				'backButtonText': "picServ",
				'showRightButton': false,
				'zipcache': "",
				'zipAddress': ""
			});
			ic.onBackButtonTap(function() {
				ic.hideImageView(function() {
					init.loadRoot(init.urlFromPath("", "getRoot"));
				});
			});
			ic.setRowSelectCallBackFunction(function(rowId) {
				var item = images[rowId];
				var iv = window.plugins.ImageView;

				if (item.Type == "Image") {
					//show image
					var imageList = [];
					var path = item.image;
					var idx = 0;
					$.each(images, function(i, x) {
						if (x.Type === "Image") {
							imageList.push({
								'url': x.image.replace("&mode=thumbnail", ""),
								'cachePath': x.cachePath.toString()
							});
							if (x.image === path) {
								idx = imageList.length - 1;
							}
						}
					});
					iv.createImageView({
						'index': idx
					});
					iv.setImageViewData(imageList);
					iv.showImageView();
				}

				if (item.Type == "Video") {
					init.playVideo(item.image.replace("&mode=thumbnail", ""), item.name.replace(".MOV", ".m4v"), item.UNCPath);
				}

			});

			ic.showImageView(function() {
				util.doHud({
					show: false
				});
			});

		});

	},
	loadImageView: function(req) {

		util.doHud({
			show: true,
			labelText: "Loading Data",
			detailsLabelText: "Please Wait..."
		});

		init.getJsonFromServer(req.URL, function(x, isCached) {

			var images = [];
			var backPath = req.Path;
			var backText = "";
			var navTitle = (req.Path === "" ? "Pictures" : req.Path.substring(req.Path.lastIndexOf("\\") + 1));

			if (backPath !== "") {

				var pathToCheck = backPath;
				var isRoot = false;
				jQuery.each(init.basePaths, function() {
					if (this == pathToCheck) {
						isRoot = true;
					}
				});

				if (isRoot) {
					backPath = "";
					backText = "picServ";
				} else {
					backPath = backPath.substring(0, backPath.lastIndexOf("\\"));
					backText = backPath.substring(backPath.lastIndexOf("\\") + 1);
					if (backText.length > 8) {
						backText = backText.substring(0, 7) + "...";
					}
				}

			}

			$.each(x, function(i, item) {
				images.push({
					'sectionHeader': item.Heading,
					'image': (item.ItemType == "Folder" ? "www/img/folder.png" : init.urlFromPath(item.ItemPath, "getFile").URL + "&mode=thumbnail"),
					'name': item.ItemName,
					'Path': item.ItemPath.toString(),
					'Type': item.ItemType,
					'UNCPath': item.UNCPath,
					'DateCreated': item.DateCreated,
					'cachePath': escape(item.ItemPath.toString()).toLowerCase()
				});
			});
			images.sort(util.dynamicSort("-DateCreated"));

			var ic = window.plugins.ImageCollection;

			if (req.Path === "") {
				ic.onBackButtonTap(function() {});

				ic.onRightButtonTap(function() {

					navigator.notification.confirm(
						'Clear the Cache?', // message

						function(i) {
							if (i === 1) {
								cache.clear();
							}
						},
						'picServe', // title
						'Yes,No' // buttonLabels
					);

				});
			} else {

				ic.onRightButtonTap(function(idx) {
					idx = parseInt(idx, 10);
					//console.log(idx);
					var del = init.urlFromPath(images[idx].Path, "deleteFile").URL;
					images.splice(idx, 1);

					$.ajax({
						url: del,
						dataType: 'text',
						success: function(resp) {
							if (resp !== "success") {
								util.doAlert("Error Deleting: " + resp);
							}
						}
					});
				});

				ic.onBackButtonTap(function() {
					ic.hideImageView(function() {
						if (backPath === "") {
							init.loadRoot(init.urlFromPath("", "getRoot"));
						} else {
							init.loadImageView(init.urlFromPath(backPath, ""));
						}

					});
				});
			}

			ic.setImageViewData(images);
			ic.createImageView({
				'navTitle': navTitle,
				'showBackButton': req.Path === "" ? false : true,
				'backButtonText': backText,
				'showRightButton': req.Path === "" ? true : false,
				'zipcache': (isCached ? "" : escape(req.Path.replace(/\\/g, ".").replace(/:/g, "").toLowerCase()) + ".zip"),
				'zipAddress': (isCached ? "" : "http://" + init.serverAddress + '/zip/?key=' + init.serverKey)
			});
			ic.setRowSelectCallBackFunction(function(rowId) {
				var item = images[rowId];
				var iv = window.plugins.ImageView;

				if (item.Type == "Folder") {
					ic.hideImageView(function() {
						init.loadImageView(init.urlFromPath(item.Path, ""));
					});
				}

				if (item.Type == "Image") {
					//show image
					var imageList = [];
					var path = item.Path;
					var idx = 0;
					$.each(images, function(i, x) {
						if (x.Type === "Image") {
							imageList.push({
								'url': x.image.replace("&mode=thumbnail", "&downsize=true"),
								'cachePath': x.cachePath.toString()
							});
							if (x.Path == path) {
								idx = imageList.length - 1;
							}
						}
					});

					iv.createImageView({
						'index': idx
					});
					iv.setImageViewData(imageList);
					iv.showImageView();
				}

				if (item.Type == "Video") {
					init.playVideo(item.image.replace("&mode=thumbnail", ""), item.name, item.UNCPath);
				}
			});

			ic.showImageView(function() {

			});

		});

	},
	onResume: function() {
		settings.get();
	},
	onBackground: function() {

	},
	playVideo: function(videoPath, fileName, UNCPath) {
		var actionSheet = window.plugins.actionSheet;
		var actions = ["Stream", "Download", "Copy URL", "Copy UNC Path", "Cancel"];
		actionSheet.create({
			title: fileName,
			items: actions,
			destructiveButtonIndex: (actions.length - 1)
		}, function(buttonValue, buttonIndex) {
			if (buttonIndex == -1 || buttonIndex == (actions.length - 1)) {
				return;
			}
			if (buttonIndex === 3) { //copy UNC
				console.log(UNCPath);
				util.setStatusBarMessage("Copied: " + UNCPath);
				window.plugins.clipboardPlugin.setText(UNCPath);
				setTimeout(function() {
					util.setStatusBarForceClear();
				}, 1000);
			}
			if (buttonIndex === 2) { //copy url
				videoPath = videoPath.replace(/&/g, "%26");
				videoPath = videoPath.replace(/%5C/g, "||");
				videoPath = videoPath.replace(/%3A/g, ":");
				videoPath = videoPath.replace(/%20/g, "+");
				
				util.setStatusBarMessage("Copied: " + videoPath);
				window.plugins.clipboardPlugin.setText(videoPath);
				setTimeout(function() {
					util.setStatusBarForceClear();
				}, 1000);

			}
			if (buttonIndex === 1) { //download video
				var iv = window.plugins.ImageView;
				iv.playVideo({
					"video": videoPath,
					"title": fileName
				}, function(newPath) {
					util.setStatusBarMessage("Video saved to camera roll.");
					window.plugins.clipboardPlugin.setText(videoPath);
					setTimeout(function() {
						util.setStatusBarForceClear();
					}, 1000);
					return;
				});
			}
			if (buttonIndex === 0) { //stream
				util.doHud({
					show: true,
					labelText: "Loading Video",
					detailsLabelText: "Please Wait...",
					tappedEvent: function() {
						$("#vid").remove();
						util.doHud({
							show: false
						});
					}
				});
				$("#vid").remove();
				if ( navigator.userAgent.match(/iPad/i) ) {
					$("body").append("<video id='vid' style='' autoplay='autoplay' controls><source src='" + videoPath + "'></video>");
				} else {
					$("body").append("<video id='vid' style='visibility: hidden'><source src='" + videoPath + "'></video>");
				}
				
				$("#vid").bind("error", function() {
					util.doHud({
						show: false
					});
					$("#vid").remove();
					util.doAlert("Error playing video");
				});

	
				$("#vid").bind("webkitendfullscreen", function() {
					util.doHud({
						show: false
					});
					$("#vid").remove();
				});


				if ( navigator.userAgent.match(/iPad/i) ) {
					setTimeout(function(){ 
						$("#vid")[0].webkitEnterFullScreen();
						//$("#vid").remove();
					}, 250);
				} else {
					$("#vid")[0].play();
				}


			}

		});

	},
	getJsonFromServer: function(u, callback) {
		var isCached = false;
		cache.getJson(u, function(d) {
			if (d !== null) {
				isCached = true;
			}

			$.ajax({
				url: u,
				dataType: 'json',
				success: function(x) {
					cache.saveJson(u, x);
					if (isCached === false) {
						//console.log("live from server!");
						callback(x, isCached);
					} else {
						//console.log("cached save from server!");
					}
				},
				error: function() {
					if (isCached === false) {
						init.showLoadError();
					} else {
						util.doAlert("The server is not responding in time. Make sure the server application is running and the correct IP is entered in settings.");
					}
				}
			});

			if (isCached) {
				//console.log("cached");
				callback(d, isCached);
			}
		});

	}
};
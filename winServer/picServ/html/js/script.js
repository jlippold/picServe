$(function() {

	$(document).keydown(function(e) {
		var hasFancybox = false;
		if ($("body > div.fancybox-wrap").size() > 0) {
			hasFancybox = true;
		}

		if ((e.keyCode === 37 && hasFancybox) || (e.keyCode === 38 && hasFancybox)) {
			$("#prev").trigger("click");
			return false;
		}

		if ((e.keyCode === 39 && hasFancybox) || (e.keyCode === 40 && hasFancybox)) {
			$("#next").trigger("click");
			return false;
		}
		return true;
	});

	$("#pass").bind("click", function() {
		init.serverKey = prompt("Server Key: ");
		if (init.serverKey !== "") {
			localStorage.setItem("key", init.serverKey);
		}
		init.ready();
	});

	$("#browseByFolder").bind("click", function() {
		$(this).parents("ul").find("li").attr("class", "");
		$(this).parent().attr("class", "active");
		init.loadFolders(init.urlFromPath("", "getFolders"));
		return false;
	});

	$("#browseByDate").bind("click", function() {
		$(this).parents("ul").find("li").attr("class", "");
		$(this).parent().attr("class", "active");
		init.loadRoot(init.urlFromPath("", "getRoot"));
		return false;
	});

	init.ready();

});

var init = {
	serverAddress: "",
	serverKey: "",
	ready: function() {
		window.scrollTo(0, 0);
		if (localStorage.getItem("key")) {
			init.serverKey = localStorage.getItem("key");
		}
		if (init.serverKey === "") {
			init.serverKey = prompt("Server Key: ");
			init.ready();
			return;
		}
		localStorage.setItem("key", init.serverKey);

		$("#browseByDate").trigger("click");

	},
	urlFromPath: function(p, script) {
		var req = {};
		req.URL = "" + script + "/?Path=" + escape(p.replace(/\\/g, "\\\\")) + "&key=" + init.serverKey;
		req.Path = p;
		return req;
	},
	loadFolders: function(req) {
		window.scrollTo(0, 0);
		init.getJsonFromServer(req.URL, function(tableView, isCached) {
			$("#sideNav").empty();
			var li = "";
			$.each(tableView, function(i, x) {
				if (x.sectionHeader !== "Folders") {
					li += "<li><a href='#' data-id='" + i + "'>" + x.FullPath + "</a></li>";
				}
			});
			$("#sideNav").append(li);

			$("#sideNav a").bind("click", function() {
				var rowId = $(this).attr("data-id");
				var item = tableView[rowId];

				var param = {};
				param.override = "getFolder";
				param.text = item.QSPath;
				init.loadDynamic(param);

				return false;
			});

		});
	},
	loadRoot: function(req) {
		window.scrollTo(0, 0);
		init.getJsonFromServer(req.URL, function(tableView, isCached) {
			var li = "";
			$("#sideNav").empty();
			$.each(tableView, function(i, x) {
				if (x.sectionHeader !== "Folders") {
					li += "<li><a href='#' data-id='" + i + "'>" + x.textLabel + "</a></li>";
				}
			});
			$("#sideNav").append(li);

			$("#sideNav a").bind("click", function() {
				var rowId = $(this).attr("data-id");
				var item = tableView[rowId];
				var param = {};

				if (item.sectionHeader === "Folders") {
					init.loadImageView(init.urlFromPath(item.drillVal, "", true));
				} else if (item.sectionHeader === "Movies") {
					param.override = "getMovies";
					init.loadDynamic(param);
				} else {
					param.text = item.drillVal;
					param.label = item.textLabel;
					init.loadDynamic(param);
				}
				return false;
			});

		});
	},
	loadDynamic: function(param) {
		window.scrollTo(0, 0);

		var nURL = "";
		var req = {};

		if (param.override) {
			req = init.urlFromPath(((param.text) ? param.text : ""), param.override);
			nURL = req.URL;
		} else {
			req = init.urlFromPath("", "getDynamic");
			nURL = req.URL + "&v=" + param.text + "&t=" + param.label;
		}

		init.getJsonFromServer(nURL, function(images, isCached) {

			$("#images").empty();
			var ic = {};

			var li = "";
			$.each(images, function(i, item) {
				var big = item.image.replace("&mode=thumbnail", "");
				big = big.substr(0, 10) + big.substr(10).replace(/%5C/g, "||");
				big = big + "&mode=rotate";
				li += "<li><img class='lazy' data-path='" + item.Path + "\\\\" + item.name + "' data-type='" + item.Type + "' title='" + item.name + "' data-original='" + item.image + "' data-href='" + big + "' width='200' height='200'></li>";
			});

			$("#images").append(li);

			$("#images img.lazy").lazyload({
				threshold: 200,
				effect: "fadeIn"
			});

			$("#images img").each(function(i) {
				$(this).bind('click', function() {
					fancyBoxMe(i);
				});
			});

		});
	},
	getJsonFromServer: function(u, callback) {
		$.ajax({
			url: u,
			dataType: 'json',
			timeout: 5000,
			success: function(x) {
				callback(x, true);
			},
			error: function() {
				alert("Error loading data");
			}
		});
	}
};

function kill(e) {

	if (confirm("Are you sure you want to delete?")) {
		var p = $('#images img').eq(e).attr("data-path");
		var del = init.urlFromPath(p, "deleteFile").URL;
		$.ajax({
			url: del,
			dataType: 'text',
			timeout: 3000,
			success: function(resp) {
				if (resp == "success") {
					$.fancybox.close(true);
					$('#images img').eq(e).attr("src", "");
				} else {
					alert("Error Deleting: " + resp);
				}
			},
			error: function() {
				alert("deletion error");
			}
		});

	}

}

function fancyBoxMe(e) {

	var numElemets = $("#images img").size();
	if ((e + 1) === numElemets) {
		nexT = 0;
	} else {
		nexT = e + 1;
	}
	if (e === 0) {
		preV = (numElemets - 1);
	} else {
		preV = e - 1;
	}
	var tarGet = $('#images img').eq(e).data('href');
	var itemType = $('#images img').eq(e).attr("data-type");
	console.log(tarGet);
	if (itemType !== "Video") {
		$.fancybox({
			href: tarGet,
			type: "image",
			preload: 3,
			autoSize: false,
			helpers: {
				title: {
					type: 'inside'
				}
			},
			afterLoad: function() {
				this.title = 'Image ' + (e + 1) + ' of ' + numElemets + ' :: <a href="javascript:;" id="prev" onclick="fancyBoxMe(' + preV + ')">prev</a>&nbsp;&nbsp;&nbsp;<a href="javascript:;" id="next" onclick="fancyBoxMe(' + nexT + ')">next</a>&nbsp;&nbsp;&nbsp;<a onclick="rotate()" href="javascript:;">rotate</a>&nbsp;&nbsp;&nbsp;<a onclick="kill(' + e + ')" href="javascript:;">delete</a>';
			}
		}); // fancybox
	} else {

		$.fancybox({
			fitToView: false, // to show videos in their own size
			content: '<span></span>', // create temp content
			preload: 3,
			autoSize: false,
			width: 640,
			height: 480,
			helpers: {
				title: {
					type: 'inside'
				}
			},
			afterLoad: function() {
				// get dimensions from data attributes
				var $width = "640px"; // $(this.element).data('width');
				var $height = "480px"; //$(this.element).data('height');
				// replace temp content
				var c = '<div class="flowplayer" style="width: 640px; height: 480px">';
				c += '<video width="640" height="480"><source type="video/mp4" src="' + tarGet + '"></video>';
				c += "</div>";
				this.content = c; //'<object data="' + tarGet + '" width="640" height="480"></object>'; //<embed src='jwplayer/jwplayer.flash.swf?file=" + (tarGet).replace(/&/gi, "&amp;") + "&autostart=true&amp;wmode=opaque' type='application/x-shockwave-flash' width='" + $width + "' height='" + $height + "'></embed>";
				this.title = 'Image ' + (e + 1) + ' of ' + numElemets + ' :: <a href="javascript:;" onclick="fancyBoxMe(' + preV + ')">prev</a>&nbsp;&nbsp;&nbsp;<a href="javascript:;" onclick="fancyBoxMe(' + nexT + ')">next</a> <a onclick="rotateDiv()" href="javascript:;">Rotate</a>';

				setTimeout(function() {
					$(".flowplayer").flowplayer({
						swf: "flowplayer/flowplayer.swf"
					});
				}, 500);

			}
		}); // fancybox
	}

}

function rotateDiv() {
	var rotate = 0;
	var img = $("img.fancybox-image");
	if ($(img).attr("rotate")) {
		rotate = parseInt($(img).attr("rotate"), 10);
	}
	rotate += 90;
	if (rotate === 360) {
		rotate = 0;
	}
	$(img).attr("rotate", rotate);

	if (rotate === 90 || rotate === 270) {
		$(img).width($("div.fancybox-inner").height()).css({
			"margin": "auto",
			"transform": "rotate(" + rotate + "deg)"
		});
	} else {
		$(img).attr("style", "transform:rotate(" + rotate + "deg);");
	}

	$.fancybox.update();
	$.fancybox.reposition();
}
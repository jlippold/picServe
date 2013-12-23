var util = {
	showingHud: false,
	doHud: function(o) {
		if (util.showingHud && o.show) {
			return; //hud is already displayed, break the fuck out
		}
		var hud = window.plugins.progressHud;
		if (o.show) {
			util.showingHud = true;
			hud.show({
				mode: "text",
				labelText: o.labelText,
				detailsLabelText: o.detailsLabelText
			}, function() {}, (o.tappedEvent) ? o.tappedEvent : function() {});
		} else {
			util.showingHud = false;
			hud.hide();
		}
	},
	compare: function(a, b) {
		if (a.catIndex < b.catIndex)
			return -1;
		if (a.catIndex > b.catIndex)
			return 1;
		return 0;
	},
	isNumeric: function(n) {
		return !isNaN(parseFloat(n)) && isFinite(n);
	},
	ticksToMilliseconds: function(ticks) {
		return (ticks * 1 / 10000);
	},
	millisecondsToTicks: function(milliseconds) {
		return (milliseconds * 10000);
	},
	addMinutes: function(date, minutes) {
		return new Date(date.getTime() + minutes * 60000);
	},
	parseDate: function(str) {
		var mdy = str.split('/');
		return new Date(mdy[2], mdy[0] - 1, mdy[1]);
	},
	daydiff: function(first, second) {
		return (second - first) / (1000 * 60 * 60 * 24);
	},
	randomFromInterval: function(from, to) {
		return Math.floor(Math.random() * (to - from + 1) + from);
	},
	detectOrientation: function() {
		if (typeof window.onorientationchange != 'undefined') {
			util.doResize();
		}
	},
	executeObjC: function(url) {
		var iframe = document.createElement("IFRAME");
		iframe.setAttribute("src", url);
		document.documentElement.appendChild(iframe);
		iframe.parentNode.removeChild(iframe);
		iframe = null;
	},
	htmlEscape: function(str) {
		return String(str)
			.replace(/&/g, '&amp;')
			.replace(/"/g, '&quot;')
			.replace(/'/g, '&#39;')
			.replace(/</g, '&lt;')
			.replace(/>/g, '&gt;');
	},
	splash: function(t) {
		try {
			if (t == "hide") {
				navigator.splashscreen.hide();
			} else {
				navigator.splashscreen.show();
			}
		} catch (e) {}
	},
	setStatusBarMessage: function(text) {
		util.setStatusBarForceClear();
		var statusBar = window.plugins.CDVStatusBarOverlay;
		// Send a message to the statusbar
		statusBar.setStatusBar({
			"message": text,
			"animation": "Shrink",
			"showSpinner": true
		});
	},
	setStatusBarForceClear: function() {
		var statusBar = window.plugins.CDVStatusBarOverlay;
		statusBar.clearStatusBar();
	},
	getEpochTime: function() {
		var d = new Date();
		return Math.round(d.getTime());
	},
	epochToDateObject: function(n) {
		return new Date(n * 1000);
	},
	doAlert: function(msg) {
		util.doHud({
			show: false
		});
		try {
			navigator.notification.alert(msg, null, "picServe");
		} catch (e) {
			alert(msg);
		}
	},
	isWifi: function() {
		if (init.wifiOnly === false) {
			return true;
		}
		try {
			if (util.netState() == 'WiFi connection') {
				return true;
			} else {
				return false;
			}
		} catch (e) {
			return true;
		}
	},
	netState: function() {
		try {
			var networkState = navigator.connection.type;
			var states = {};
			states[Connection.UNKNOWN] = 'Unknown connection';
			states[Connection.ETHERNET] = 'Ethernet connection';
			states[Connection.WIFI] = 'WiFi connection';
			states[Connection.CELL_2G] = 'Cell 2G connection';
			states[Connection.CELL_3G] = 'Cell 3G connection';
			states[Connection.CELL_4G] = 'Cell 4G connection';
			states[Connection.NONE] = 'No network connection';
			return (states[networkState]);
		} catch (e) {
			return 'WiFi connection';
		}
	},
	dynamicSort: function(property) {
		var sortOrder = 1;
		if (property[0] === "-") {
			sortOrder = -1;
			property = property.substr(1, property.length - 1);
		}
		return function(a, b) {
			var result = (a[property] < b[property]) ? -1 : (a[property] > b[property]) ? 1 : 0;
			return result * sortOrder;
		};
	},
	shuffle: function(array) {
		var counter = array.length,
			temp, index;

		// While there are elements in the array
		while (counter > 0) {
			// Pick a random index
			index = (Math.random() * counter--) | 0;

			// And swap the last element with it
			temp = array[counter];
			array[counter] = array[index];
			array[index] = temp;
		}

		return array;
	}
};
$.fn.hasAttr = function(name) {
	return this.attr(name) !== undefined;
};
Date.prototype.timeNow = function() {
	var tod = "AM";
	var h = this.getHours();
	if (this.getHours() > 12) {
		tod = "PM";
		h = h - 12;
	}
	if (h === 0) {
		h = 12;
	}
	tod = h + ":" + ((this.getMinutes() < 10) ? "0" : "") + this.getMinutes() + " " + tod;
	if (tod == "0:00 AM") {
		return "";
	} else {
		return tod;
	}
};
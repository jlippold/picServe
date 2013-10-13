var settings = {
	get: function(callback) {
		//load from IOS prefs
		window.plugins.applicationPreferences.get('All', function(result) {
			var d = jQuery.parseJSON(result);
			if (d.ip.length <= 3 || d.port.length < 2 || d.password.length <= 1 || d.uploadport.length < 2) {
				//util.doAlert("Check Settings, Invalid Values");
			} else {
				if (d.wifi == 1) {
					init.wifiOnly = true;
				} else {
					init.wifiOnly = true;
				}
				init.serverAddress = d.ip + ":" + d.port;
				init.serverKey = d.password;
				init.uploadPort = d.uploadport;
			}
			if (callback !== undefined) {
				callback();
			}
		});

	}
};
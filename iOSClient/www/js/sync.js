var sync = {
	isRunning: false,
	beginSync: function() {
		if (init.uploadPort > 0) {
			var is = window.plugins.ImageSync;
			is.uploadPictures();
		}
	}
};
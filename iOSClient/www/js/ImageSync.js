(function(cordova) {
	function ImageSync() {
		this.imageUploadCompleted = null;
		this.gotSyncList = null;
	}
	ImageSync.prototype.uploadPictures = function(params) {
		cordova.exec("ImageSync.uploadPictures", params);
	};
	ImageSync.prototype._imageUploadCompleted = function(params) {
		if (this.imageUploadCompleted) {
			this.imageUploadCompleted(params);
		}
	};
	ImageSync.prototype.getSyncList = function(callback) {
		this.gotSyncList = callback;
		cordova.exec("ImageSync.getSyncList");
	};
	ImageSync.prototype._gotSyncList = function() {
		if (this.gotSyncList) {
			this.gotSyncList();
		}
	};

	cordova.addConstructor(function() {
		if (!window.plugins) window.plugins = {};
		window.plugins.ImageSync = new ImageSync();
	});

})(window.cordova || window.Cordova);
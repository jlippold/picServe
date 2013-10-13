(function(cordova) {

	function ImageView() {
		this.didDownloadVideo = null;
	}

	ImageView.prototype.createImageView = function(params) {
		cordova.exec("ImageView.createImageView", params);
	};

	ImageView.prototype.playVideo = function(params, cb) {
		this.didDownloadVideo = cb;
		cordova.exec("ImageView.playVideo", params);
	};

	ImageView.prototype._onVideoDownloaded = function(loc) {
		if (this.didDownloadVideo)
			this.didDownloadVideo(loc);
	};

	ImageView.prototype.showImageView = function() {
		cordova.exec("ImageView.showImageView");
	};

	ImageView.prototype.setImageViewData = function(images) {
		cordova.exec("ImageView.setImageViewData", images);
	};

	cordova.addConstructor(function() {
		if (!window.plugins) window.plugins = {};
		window.plugins.ImageView = new ImageView();
	});

})(window.cordova || window.Cordova);
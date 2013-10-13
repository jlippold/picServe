(function(cordova) {

	function ImageCollection() {
		this.callBackFunction = null;
		this.rightCallBackFunction = null;
		this.backCallBackFunction = null;
		this.didShowTable = null;
		this.didHideTable = null;
	}

	ImageCollection.prototype.createImageView = function(params) {
		cordova.exec("ImageCollection.createImageView", params);
	};

	ImageCollection.prototype.setImageViewData = function(images) {
		cordova.exec("ImageCollection.setImageViewData", images);
	};

	ImageCollection.prototype.showImageView = function(cb) {
		this.didShowTable = cb;
		cordova.exec("ImageCollection.showImageView");
	};
	ImageCollection.prototype._onTableShowComplete = function() {
		if (this.didShowTable)
			this.didShowTable();
	};

	ImageCollection.prototype.hideImageView = function(cb) {
		this.didHideTable = cb;
		cordova.exec("ImageCollection.hideImageView");
	};
	ImageCollection.prototype._onTableHideComplete = function() {
		if (this.didHideTable)
			this.didHideTable();
	};

	ImageCollection.prototype.setRowSelectCallBackFunction = function(callBkFunc) {
		this.callBackFunction = callBkFunc;
	};

	ImageCollection.prototype.onRightButtonTap = function(callBkFunc) {
		this.rightCallBackFunction = callBkFunc;
	};

	ImageCollection.prototype._onRightButtonTap = function(row) {
		if (this.rightCallBackFunction)
			this.rightCallBackFunction(row);
	};

	ImageCollection.prototype.onBackButtonTap = function(callBkFunc) {
		this.backCallBackFunction = callBkFunc;
	};

	ImageCollection.prototype._onBackButtonTap = function() {
		if (this.backCallBackFunction)
			this.backCallBackFunction();
	};

	ImageCollection.prototype._onImageViewRowSelect = function(rowId) {
		if (this.callBackFunction)
			this.callBackFunction(rowId);
	};

	cordova.addConstructor(function() {
		if (!window.plugins) window.plugins = {};
		window.plugins.ImageCollection = new ImageCollection();
	});

})(window.cordova || window.Cordova);
if (typeof String.prototype.endsWith !== 'function') {
	String.prototype.endsWith = function(suffix) {
		return this.indexOf(suffix, this.length - suffix.length) !== -1;
	};
}

if (typeof Date.prototype.toEpoch !== 'function') {
	Date.prototype.toEpoch = function() {
		return new this.getTime() / 1000;
	};
}

exports.monthNames = function() {
	return [
		"January", "February", "March",
		"April", "May", "June",
		"July", "August", "September",
		"October", "November", "December"
	]
};